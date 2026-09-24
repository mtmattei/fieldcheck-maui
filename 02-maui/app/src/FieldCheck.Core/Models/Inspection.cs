namespace FieldCheck.Core.Models;

public enum InspectionCondition
{
    Good,
    Attention,
    Critical,
}

public sealed record Inspection
{
    public required string Id { get; init; }
    public required string AssetId { get; init; }
    public required string AssetName { get; init; }
    public DateTime Date { get; init; }
    public InspectionCondition Condition { get; init; }
    public bool OperatingNormally { get; init; }
    public double TemperatureC { get; init; }
    public required string Inspector { get; init; }
    public string? Notes { get; init; }
    public string? IssueDescription { get; init; }
    public bool? ChecklistComplete { get; init; }
    public string? AttachmentFileName { get; init; }
    public string? AttachmentPath { get; init; }
}

/// <summary>Validated user input for a new inspection; the repository assigns ID, date and inspector.</summary>
public sealed record InspectionDraft
{
    public required string AssetId { get; init; }
    public InspectionCondition Condition { get; init; }
    public bool OperatingNormally { get; init; }
    public double TemperatureC { get; init; }
    public string? Notes { get; init; }
    public string? IssueDescription { get; init; }
    public string? AttachmentFileName { get; init; }
    public string? AttachmentPath { get; init; }
}
