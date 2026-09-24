using FieldCheck.Core.Services;

namespace FieldCheck.Tests;

public sealed class FixtureSeedDataSource(string directory) : ISeedDataSource
{
    public static string DefaultDirectory => Path.Combine(AppContext.BaseDirectory, "fixtures");

    public Task<string> ReadAssetsJsonAsync(CancellationToken cancellationToken) =>
        File.ReadAllTextAsync(Path.Combine(directory, "assets.json"), cancellationToken);

    public Task<string> ReadInspectionsJsonAsync(CancellationToken cancellationToken) =>
        File.ReadAllTextAsync(Path.Combine(directory, "inspections.json"), cancellationToken);
}

public sealed class FixedClock(DateTime now) : IClock
{
    public DateTime Now { get; set; } = now;
}

/// <summary>Wraps a real store and can be switched to fail writes.</summary>
public sealed class FlakyStore(IDataFileStore inner) : IDataFileStore
{
    public bool FailWrites { get; set; }
    public int Writes { get; private set; }

    public Task<string?> ReadAsync(CancellationToken cancellationToken) => inner.ReadAsync(cancellationToken);

    public async Task WriteAsync(string content, CancellationToken cancellationToken)
    {
        if (FailWrites)
        {
            throw new IOException("Disk full");
        }

        Writes++;
        await inner.WriteAsync(content, cancellationToken);
    }
}

public sealed class RecordingNavigation : INavigationService
{
    public List<string> Calls { get; } = [];

    /// <summary>Completes navigations only when released, to simulate slow page transitions.</summary>
    public TaskCompletionSource? Gate { get; set; }

    public Task GoToAssetDetailAsync(string assetId) => Record($"detail:{assetId}");
    public Task GoToAssetsAsync() => Record("assets");
    public Task GoToHistoryAsync() => Record("history");
    public Task GoToNewInspectionAsync(string assetId) => Record($"new:{assetId}");
    public Task GoToInspectionSuccessAsync(string inspectionId) => Record($"success:{inspectionId}");
    public Task GoBackAsync() => Record("back");

    private Task Record(string call)
    {
        Calls.Add(call);
        return Gate?.Task ?? Task.CompletedTask;
    }
}

public sealed class FakeFilePicker : IFilePickerService
{
    public Func<Task<PickedFile?>> Behavior { get; set; } = () => Task.FromResult<PickedFile?>(null);
    public int Calls { get; private set; }

    public Task<PickedFile?> PickPhotoOrFileAsync()
    {
        Calls++;
        return Behavior();
    }
}

/// <summary>Creates an isolated repository over a temp app-storage directory seeded from the real fixtures.</summary>
public sealed class TestContext : IDisposable
{
    public static readonly DateTime Now = new(2026, 9, 23, 14, 41, 0);

    public TestContext(DataSourceMode mode = DataSourceMode.Normal)
    {
        Directory.CreateDirectory(StorageDirectory);
        Store = new FlakyStore(new FileDataStore(StorageDirectory));
        Repository = CreateRepository(mode);
    }

    public string StorageDirectory { get; } = Path.Combine(Path.GetTempPath(), "fieldcheck-tests", Guid.NewGuid().ToString("N"));
    public FlakyStore Store { get; }
    public FixedClock Clock { get; } = new(Now);
    public JsonFieldCheckRepository Repository { get; }
    public RecordingNavigation Navigation { get; } = new();
    public FakeFilePicker Picker { get; } = new();

    /// <summary>A new repository over the same storage, equivalent to an app process restart.</summary>
    public JsonFieldCheckRepository CreateRepository(DataSourceMode mode = DataSourceMode.Normal) =>
        new(Store, new FixtureSeedDataSource(FixtureSeedDataSource.DefaultDirectory), Clock, new DataSourceOptions { Mode = mode });

    public void Dispose()
    {
        try
        {
            Directory.Delete(StorageDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
