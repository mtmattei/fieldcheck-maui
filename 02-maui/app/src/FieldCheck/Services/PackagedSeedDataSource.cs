using FieldCheck.Core.Services;

namespace FieldCheck.Services;

/// <summary>Reads the fixtures bundled as read-only MauiAssets.</summary>
public sealed class PackagedSeedDataSource : ISeedDataSource
{
    public Task<string> ReadAssetsJsonAsync(CancellationToken cancellationToken) => ReadAsync("seed/assets.json");

    public Task<string> ReadInspectionsJsonAsync(CancellationToken cancellationToken) => ReadAsync("seed/inspections.json");

    private static async Task<string> ReadAsync(string name)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(name);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
