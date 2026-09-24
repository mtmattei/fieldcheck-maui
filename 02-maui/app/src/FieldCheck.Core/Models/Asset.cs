namespace FieldCheck.Core.Models;

public enum AssetStatus
{
    Operational,
    Attention,
    Critical,
}

public sealed record Asset
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Location { get; init; }
    public AssetStatus Status { get; init; }
    public DateOnly LastInspection { get; init; }
    public string Description { get; init; } = string.Empty;
}
