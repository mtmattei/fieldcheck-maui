using CommunityToolkit.Mvvm.ComponentModel;

namespace FieldCheck.Core.ViewModels;

public sealed partial class FilterOption(string label) : ObservableObject
{
    public const string All = "All";

    public string Label { get; } = label;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string AccessibilityLabel => $"Filter: {Label}";

    public static IReadOnlyList<FilterOption> Create(params string[] labels)
    {
        var options = labels.Select(l => new FilterOption(l)).ToList();
        options[0].IsSelected = true;
        return options;
    }
}
