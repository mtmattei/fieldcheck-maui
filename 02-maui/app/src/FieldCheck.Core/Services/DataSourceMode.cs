namespace FieldCheck.Core.Services;

/// <summary>
/// Deterministic repository behavior used to verify loading/empty/error states.
/// Selected at startup (tests or the FIELDCHECK_DATA_MODE environment variable); never exposed in the UI.
/// </summary>
public enum DataSourceMode
{
    Normal,
    /// <summary>Repository reads return no assets or inspections.</summary>
    Empty,
    /// <summary>Every read fails.</summary>
    Error,
    /// <summary>The first read fails; later reads succeed (verifies Retry recovery).</summary>
    ErrorOnce,
}

public sealed class DataSourceOptions
{
    public DataSourceMode Mode { get; init; } = DataSourceMode.Normal;

    /// <summary>Artificial latency applied to reads, used to make the loading state observable.</summary>
    public TimeSpan ReadDelay { get; init; } = TimeSpan.Zero;

    public static DataSourceOptions FromEnvironment(string? mode, string? delayMs) => new()
    {
        Mode = Enum.TryParse<DataSourceMode>(mode, ignoreCase: true, out var parsed) ? parsed : DataSourceMode.Normal,
        ReadDelay = int.TryParse(delayMs, out var ms) && ms > 0 ? TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero,
    };
}
