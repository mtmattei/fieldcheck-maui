using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Core.ViewModels;

public sealed partial class AssetDetailViewModel(IFieldCheckRepository repository, INavigationService navigation)
    : StatefulViewModel
{
    [ObservableProperty]
    public partial string? AssetId { get; private set; }

    [ObservableProperty]
    public partial Asset? Asset { get; private set; }

    [ObservableProperty]
    public partial Inspection? LatestInspection { get; private set; }

    public string Name => Asset?.Name ?? string.Empty;
    public string StatusText => Asset?.Status.ToString() ?? string.Empty;
    public Tone Tone => Asset?.Status.ToTone() ?? Tone.Success;
    public string Type => Asset?.Type ?? string.Empty;
    public string Location => Asset?.Location ?? string.Empty;
    public string Description => Asset?.Description ?? string.Empty;
    public string LastInspectionText => Asset is null ? string.Empty : Display.LongDate(Asset.LastInspection);

    public bool HasLatestInspection => LatestInspection is not null;
    public string LatestConditionText => LatestInspection is null ? string.Empty : $"{LatestInspection.Condition} condition";
    public Tone LatestTone => LatestInspection?.Condition.ToTone() ?? Tone.Success;

    /// <summary>Issue description when one was recorded, otherwise the inspection notes.</summary>
    public string LatestSummary => LatestInspection is null
        ? string.Empty
        : LatestInspection.IssueDescription ?? LatestInspection.Notes ?? "No notes recorded.";

    public string LatestMeta => LatestInspection is null
        ? string.Empty
        : $"{LatestInspection.Id} · {Display.LongDate(LatestInspection.Date)} · {LatestInspection.Inspector}";

    [RelayCommand]
    public async Task LoadAsync(string assetId)
    {
        var changedAsset = AssetId != assetId;
        AssetId = assetId;
        if (changedAsset || Asset is null)
        {
            State = ViewState.Loading;
        }

        try
        {
            var asset = await repository.GetAssetAsync(assetId);
            if (AssetId != assetId)
            {
                return; // A newer selection superseded this load.
            }

            var inspections = await repository.GetInspectionsAsync();
            Asset = asset;
            LatestInspection = inspections.FirstOrDefault(i => i.AssetId == assetId);
            State = asset is null ? ViewState.Empty : ViewState.Content;
        }
        catch (DataLoadException)
        {
            State = ViewState.Error;
        }

        OnPropertyChanged(string.Empty);
    }

    [RelayCommand]
    private Task RetryAsync() => AssetId is null ? Task.CompletedTask : LoadAsync(AssetId);

    [RelayCommand]
    private Task StartInspectionAsync() =>
        Asset is null ? Task.CompletedTask : navigation.GoToNewInspectionAsync(Asset.Id);

    [RelayCommand]
    private Task BackAsync() => navigation.GoBackAsync();
}
