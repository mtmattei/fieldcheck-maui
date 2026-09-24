namespace FieldCheck.Core.Services;

/// <summary>Stores the data file in a directory, replacing it atomically on write.</summary>
public sealed class FileDataStore(string directory, string fileName = "fieldcheck-data.json") : IDataFileStore
{
    public string FilePath { get; } = Path.Combine(directory, fileName);

    public async Task<string?> ReadAsync(CancellationToken cancellationToken)
    {
        return File.Exists(FilePath) ? await File.ReadAllTextAsync(FilePath, cancellationToken) : null;
    }

    public async Task WriteAsync(string content, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);
        var temp = FilePath + ".tmp";
        await File.WriteAllTextAsync(temp, content, cancellationToken);
        File.Move(temp, FilePath, overwrite: true);
    }
}
