using System.Globalization;
using System.Text.Json;
using FieldCheck.Core.Models;

namespace FieldCheck.Core.Services;

/// <summary>
/// Local JSON repository. On first run the bundled fixtures are copied into app storage;
/// afterwards all reads and writes use the app-storage file only, so fixtures are never mutated.
/// </summary>
public sealed class JsonFieldCheckRepository(
    IDataFileStore store,
    ISeedDataSource seed,
    IClock clock,
    DataSourceOptions options) : IFieldCheckRepository
{
    public const string InspectorName = "Alex Morgan";
    private const string InspectionIdPrefix = "INS-";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private FieldCheckData? _data;
    private bool _failedOnce;

    public async Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default)
    {
        var data = await ReadAsync(cancellationToken);
        return data.Assets.ToList();
    }

    public async Task<IReadOnlyList<Inspection>> GetInspectionsAsync(CancellationToken cancellationToken = default)
    {
        var data = await ReadAsync(cancellationToken);
        return data.Inspections.OrderByDescending(i => i.Date).ToList();
    }

    public async Task<Asset?> GetAssetAsync(string assetId, CancellationToken cancellationToken = default)
    {
        var data = await ReadAsync(cancellationToken);
        return data.Assets.FirstOrDefault(a => string.Equals(a.Id, assetId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Inspection?> GetInspectionAsync(string inspectionId, CancellationToken cancellationToken = default)
    {
        var data = await ReadAsync(cancellationToken);
        return data.Inspections.FirstOrDefault(i => i.Id == inspectionId);
    }

    public async Task<Inspection> AddInspectionAsync(InspectionDraft draft, CancellationToken cancellationToken = default)
    {
        var data = await ReadAsync(cancellationToken);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var asset = data.Assets.FirstOrDefault(a => a.Id == draft.AssetId)
                ?? throw new DataPersistenceException($"Asset {draft.AssetId} was not found.");

            var now = clock.Now;
            var inspection = new Inspection
            {
                Id = NextInspectionId(data.Inspections),
                AssetId = asset.Id,
                AssetName = asset.Name,
                Date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second),
                Condition = draft.Condition,
                OperatingNormally = draft.OperatingNormally,
                TemperatureC = draft.TemperatureC,
                Inspector = InspectorName,
                Notes = string.IsNullOrWhiteSpace(draft.Notes) ? null : draft.Notes.Trim(),
                IssueDescription = string.IsNullOrWhiteSpace(draft.IssueDescription) ? null : draft.IssueDescription.Trim(),
                ChecklistComplete = true,
                AttachmentFileName = draft.AttachmentFileName,
                AttachmentPath = draft.AttachmentPath,
            };
            var updatedAsset = asset with
            {
                Status = draft.Condition.ToAssetStatus(),
                LastInspection = DateOnly.FromDateTime(now),
            };

            // Build the next state separately and only adopt it after the write succeeds,
            // so a failed save leaves memory and disk consistent.
            var next = new FieldCheckData
            {
                Assets = data.Assets.Select(a => a.Id == asset.Id ? updatedAsset : a).ToList(),
                Inspections = [inspection, .. data.Inspections],
            };

            try
            {
                await store.WriteAsync(Serialize(next), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DataPersistenceException("The inspection could not be saved to local storage.", ex);
            }

            _data = next;
            return inspection;
        }
        finally
        {
            _gate.Release();
        }
    }

    internal static string NextInspectionId(IEnumerable<Inspection> existing)
    {
        var max = existing
            .Select(i => i.Id.StartsWith(InspectionIdPrefix, StringComparison.Ordinal)
                && int.TryParse(i.Id.AsSpan(InspectionIdPrefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return InspectionIdPrefix + (max + 1).ToString(CultureInfo.InvariantCulture);
    }

    private async Task<FieldCheckData> ReadAsync(CancellationToken cancellationToken)
    {
        if (options.ReadDelay > TimeSpan.Zero)
        {
            await Task.Delay(options.ReadDelay, cancellationToken);
        }

        switch (options.Mode)
        {
            case DataSourceMode.Error:
                throw new DataLoadException("Local data could not be read.");
            case DataSourceMode.ErrorOnce when !_failedOnce:
                _failedOnce = true;
                throw new DataLoadException("Local data could not be read.");
            case DataSourceMode.Empty:
                return new FieldCheckData();
        }

        if (_data is not null)
        {
            return _data;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            _data ??= await LoadOrSeedAsync(cancellationToken);
            return _data;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<FieldCheckData> LoadOrSeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            var existing = await store.ReadAsync(cancellationToken);
            if (existing is not null)
            {
                return JsonSerializer.Deserialize(existing, FieldCheckJsonContext.Default.FieldCheckData)
                    ?? throw new DataLoadException("Local data file is empty.");
            }

            var seeded = new FieldCheckData
            {
                Assets = JsonSerializer.Deserialize(
                    await seed.ReadAssetsJsonAsync(cancellationToken), FieldCheckJsonContext.Default.ListAsset) ?? [],
                Inspections = JsonSerializer.Deserialize(
                    await seed.ReadInspectionsJsonAsync(cancellationToken), FieldCheckJsonContext.Default.ListInspection) ?? [],
            };
            await store.WriteAsync(Serialize(seeded), cancellationToken);
            return seeded;
        }
        catch (Exception ex) when (ex is not DataLoadException and not OperationCanceledException)
        {
            throw new DataLoadException("Local data could not be read.", ex);
        }
    }

    private static string Serialize(FieldCheckData data) =>
        JsonSerializer.Serialize(data, FieldCheckJsonContext.Default.FieldCheckData);
}
