using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Core.ViewModels;

public sealed partial class DashboardViewModel(IFieldCheckRepository repository, INavigationService navigation, IClock clock)
    : StatefulViewModel
{
    public const string FacilityName = "Facility A";

    public string Greeting => Display.Greeting(clock.Now);

    public string ContextLine => $"{FacilityName} · {clock.Now.ToString("dddd, MMM d", Display.Culture)}";

    [ObservableProperty]
    public partial int TotalCount { get; private set; }

    [ObservableProperty]
    public partial int OperationalCount { get; private set; }

    [ObservableProperty]
    public partial int AttentionCount { get; private set; }

    [ObservableProperty]
    public partial int CriticalCount { get; private set; }

    [ObservableProperty]
    public partial bool HasNeedsAttention { get; private set; }

    public string TotalLabel => TotalCount == 1 ? "asset" : "assets";

    public ObservableCollection<AssetItem> NeedsAttention { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        OnPropertyChanged(nameof(Greeting));
        OnPropertyChanged(nameof(ContextLine));
        if (TotalCount == 0)
        {
            State = ViewState.Loading;
        }

        try
        {
            var assets = await repository.GetAssetsAsync();
            TotalCount = assets.Count;
            OperationalCount = assets.Count(a => a.Status == AssetStatus.Operational);
            AttentionCount = assets.Count(a => a.Status == AssetStatus.Attention);
            CriticalCount = assets.Count(a => a.Status == AssetStatus.Critical);
            OnPropertyChanged(nameof(TotalLabel));

            NeedsAttention.Clear();
            foreach (var asset in assets
                         .Where(a => a.Status != AssetStatus.Operational)
                         .OrderByDescending(a => a.Status))
            {
                NeedsAttention.Add(new AssetItem(asset));
            }

            HasNeedsAttention = NeedsAttention.Count > 0;
            State = assets.Count == 0 ? ViewState.Empty : ViewState.Content;
        }
        catch (DataLoadException)
        {
            State = ViewState.Error;
        }
    }

    [RelayCommand]
    private Task OpenAssetAsync(AssetItem? item) =>
        item is null ? Task.CompletedTask : navigation.GoToAssetDetailAsync(item.Id);

    [RelayCommand]
    private Task ViewAllAssetsAsync() => navigation.GoToAssetsAsync();
}
