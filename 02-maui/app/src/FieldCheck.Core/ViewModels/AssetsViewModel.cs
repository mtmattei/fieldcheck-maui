using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Core.ViewModels;

public sealed partial class AssetsViewModel(IFieldCheckRepository repository, INavigationService navigation, AssetDetailViewModel detail)
    : StatefulViewModel
{
    private IReadOnlyList<Asset> _all = [];

    /// <summary>Detail pane used by the wide (master/detail) layout.</summary>
    public AssetDetailViewModel Detail { get; } = detail;

    public ObservableCollection<AssetItem> Items { get; } = [];

    public IReadOnlyList<FilterOption> Filters { get; } = FilterOption.Create(FilterOption.All, nameof(AssetStatus.Operational), nameof(AssetStatus.Attention), nameof(AssetStatus.Critical));

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedFilter { get; set; } = FilterOption.All;

    [ObservableProperty]
    public partial string Subtitle { get; private set; } = string.Empty;

    /// <summary>Set by the view when the window is wide enough for master/detail.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDetailPane))]
    public partial bool IsWide { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDetailPane))]
    public partial AssetItem? SelectedItem { get; set; }

    public bool ShowDetailPane => IsWide && SelectedItem is not null;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedFilterChanged(string value)
    {
        foreach (var option in Filters)
        {
            option.IsSelected = option.Label == value;
        }

        ApplyFilter();
    }

    partial void OnIsWideChanged(bool value)
    {
        if (value)
        {
            EnsureWideSelection();
        }
    }

    partial void OnSelectedItemChanged(AssetItem? oldValue, AssetItem? newValue)
    {
        oldValue?.IsSelected = false;
        var value = newValue;
        if (value is null)
        {
            return;
        }

        if (IsWide)
        {
            value.IsSelected = true;
            if (Detail.AssetId != value.Id)
            {
                _ = Detail.LoadAsync(value.Id);
            }
        }
        else
        {
            // Single-pane: selection is a navigation gesture, not a persistent state.
            SelectedItem = null;
            _ = navigation.GoToAssetDetailAsync(value.Id);
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_all.Count == 0)
        {
            State = ViewState.Loading;
        }

        try
        {
            _all = await repository.GetAssetsAsync();
            Subtitle = Display.Plural(_all.Count, "equipment record", "equipment records");
            ApplyFilter();
            if (IsWide && SelectedItem is not null)
            {
                await Detail.LoadAsync(SelectedItem.Id);
            }
        }
        catch (DataLoadException)
        {
            _all = [];
            Items.Clear();
            State = ViewState.Error;
        }
    }

    /// <summary>Row activation: shows the asset in the detail pane (wide) or opens Asset Detail (narrow).</summary>
    [RelayCommand]
    private void OpenAsset(AssetItem? item) => SelectedItem = item;

    [RelayCommand]
    private void SelectFilter(FilterOption? filter) => SelectedFilter = filter?.Label ?? FilterOption.All;

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        SelectedFilter = FilterOption.All;
    }

    private void ApplyFilter()
    {
        if (State == ViewState.Error && _all.Count == 0)
        {
            return;
        }

        var query = SearchText.Trim();
        var matches = _all.Where(a =>
                (SelectedFilter == FilterOption.All || a.Status.ToString() == SelectedFilter) &&
                (query.Length == 0 ||
                 a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 a.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 a.Type.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 a.Location.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Select(a => new AssetItem(a))
            .ToList();

        var selectedId = SelectedItem?.Id;
        Items.Clear();
        foreach (var item in matches)
        {
            Items.Add(item);
        }

        State = _all.Count == 0 ? ViewState.Empty
            : Items.Count == 0 ? ViewState.NoResults
            : ViewState.Content;

        if (IsWide)
        {
            // Keep the same asset selected (with refreshed data) when it is still visible.
            SelectedItem = Items.FirstOrDefault(i => i.Id == selectedId) ?? Items.FirstOrDefault();
        }
    }

    private void EnsureWideSelection()
    {
        if (SelectedItem is null || Items.All(i => i.Id != SelectedItem.Id))
        {
            SelectedItem = Items.FirstOrDefault();
        }
    }
}
