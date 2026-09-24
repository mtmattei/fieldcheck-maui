using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Core.ViewModels;

public sealed partial class InspectionSuccessViewModel(IFieldCheckRepository repository, INavigationService navigation)
    : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspectionId), nameof(AssetName), nameof(ConditionText), nameof(Tone), nameof(MetaLine))]
    public partial Inspection? Inspection { get; private set; }

    public string InspectionId => Inspection?.Id ?? string.Empty;
    public string AssetName => Inspection?.AssetName ?? string.Empty;
    public string ConditionText => Inspection?.Condition.ToString() ?? string.Empty;
    public Tone Tone => Inspection?.Condition.ToTone() ?? Tone.Success;
    public string MetaLine => Inspection is null ? string.Empty : $"{Display.LongDate(Inspection.Date)} · {Inspection.Inspector}";

    public async Task LoadAsync(string inspectionId)
    {
        try
        {
            Inspection = await repository.GetInspectionAsync(inspectionId);
        }
        catch (DataLoadException)
        {
            Inspection = null;
        }
    }

    /// <summary>Returns to the asset the inspection was started from (the form was already replaced).</summary>
    [RelayCommand]
    private Task ViewAssetAsync() => navigation.GoBackAsync();

    [RelayCommand]
    private Task ViewHistoryAsync() => navigation.GoToHistoryAsync();
}
