using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FieldCheck.Core.Models;

namespace FieldCheck.Core.ViewModels;

/// <summary>Semantic tone used by the views to pick status colors. Status is always also shown as text.</summary>
public enum Tone
{
    Success,
    Attention,
    Critical,
}

public static class Display
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    public static Tone ToTone(this AssetStatus status) => status switch
    {
        AssetStatus.Operational => Tone.Success,
        AssetStatus.Attention => Tone.Attention,
        _ => Tone.Critical,
    };

    public static Tone ToTone(this InspectionCondition condition) => condition.ToAssetStatus().ToTone();

    public static string LongDate(DateOnly date) => date.ToString("MMM d, yyyy", Culture);

    public static string LongDate(DateTime date) => date.ToString("MMM d, yyyy", Culture);

    public static string Timestamp(DateTime value, DateTime now)
    {
        var time = value.ToString("h:mm tt", Culture);
        if (value.Date == now.Date)
        {
            return $"Today, {time}";
        }

        return value.Year == now.Year
            ? $"{value.ToString("MMM d", Culture)}, {time}"
            : $"{value.ToString("MMM d, yyyy", Culture)}, {time}";
    }

    public static string Greeting(DateTime now) => now.Hour switch
    {
        < 12 => "Good morning",
        < 18 => "Good afternoon",
        _ => "Good evening",
    };

    public static string Plural(int count, string singular, string plural) =>
        $"{count.ToString(Culture)} {(count == 1 ? singular : plural)}";
}

public sealed partial class AssetItem(Asset asset) : ObservableObject
{
    public Asset Asset { get; } = asset;

    /// <summary>True for the row shown in the master/detail pane.</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string Id => Asset.Id;
    public string Name => Asset.Name;
    public string StatusText => Asset.Status.ToString();
    public Tone Tone => Asset.Status.ToTone();
    public string IdAndType => $"{Asset.Id} · {Asset.Type}";
    public string IdAndLocation => $"{Asset.Id} · {Asset.Location}";
    public string Location => Asset.Location;
    public string AccessibilityLabel => $"{Asset.Name}, {Asset.Id}, {Asset.Type}, {Asset.Location}, status {Asset.Status}";
}

public sealed record InspectionItem(Inspection Inspection, string Timestamp)
{
    public string Id => Inspection.Id;
    public string AssetName => Inspection.AssetName;
    public string Inspector => Inspection.Inspector;
    public string ConditionText => Inspection.Condition.ToString();
    public Tone Tone => Inspection.Condition.ToTone();
    public string IdAndTimestamp => $"{Inspection.Id} · {Timestamp}";
    public string AccessibilityLabel =>
        $"{Inspection.AssetName}, inspection {Inspection.Id}, {Timestamp}, inspector {Inspection.Inspector}, result {Inspection.Condition}";
}
