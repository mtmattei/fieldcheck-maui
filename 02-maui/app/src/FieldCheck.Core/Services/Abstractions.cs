using FieldCheck.Core.Models;

namespace FieldCheck.Core.Services;

public interface IFieldCheckRepository
{
    Task<IReadOnlyList<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inspection>> GetInspectionsAsync(CancellationToken cancellationToken = default);
    Task<Asset?> GetAssetAsync(string assetId, CancellationToken cancellationToken = default);
    Task<Inspection?> GetInspectionAsync(string inspectionId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new inspection and updates the asset. Throws <see cref="DataPersistenceException"/> when the write fails.</summary>
    Task<Inspection> AddInspectionAsync(InspectionDraft draft, CancellationToken cancellationToken = default);
}

/// <summary>Reads and writes the local data file.</summary>
public interface IDataFileStore
{
    Task<string?> ReadAsync(CancellationToken cancellationToken);
    Task WriteAsync(string content, CancellationToken cancellationToken);
}

/// <summary>Provides the bundled seed fixtures (assets.json / inspections.json).</summary>
public interface ISeedDataSource
{
    Task<string> ReadAssetsJsonAsync(CancellationToken cancellationToken);
    Task<string> ReadInspectionsJsonAsync(CancellationToken cancellationToken);
}

public interface IClock
{
    DateTime Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}

public interface INavigationService
{
    Task GoToAssetDetailAsync(string assetId);
    Task GoToAssetsAsync();
    Task GoToHistoryAsync();
    Task GoToNewInspectionAsync(string assetId);
    /// <summary>Replaces the inspection form with the success page.</summary>
    Task GoToInspectionSuccessAsync(string inspectionId);
    Task GoBackAsync();
}

public sealed record PickedFile(string FileName, string LocalPath);

public interface IFilePickerService
{
    /// <summary>Opens the platform picker. Returns null when the user cancels.</summary>
    Task<PickedFile?> PickPhotoOrFileAsync();
}

public sealed class DataPersistenceException(string message, Exception? inner = null) : Exception(message, inner);

public sealed class DataLoadException(string message, Exception? inner = null) : Exception(message, inner);
