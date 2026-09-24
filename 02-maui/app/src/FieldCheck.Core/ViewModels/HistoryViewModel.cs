using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Core.ViewModels;

public sealed partial class HistoryViewModel(IFieldCheckRepository repository, IClock clock) : StatefulViewModel
{
    private IReadOnlyList<Inspection> _all = [];

    public ObservableCollection<InspectionItem> Items { get; } = [];

    public IReadOnlyList<FilterOption> Filters { get; } = FilterOption.Create(FilterOption.All, nameof(InspectionCondition.Good), nameof(InspectionCondition.Attention), nameof(InspectionCondition.Critical));

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedFilter { get; set; } = FilterOption.All;

    [ObservableProperty]
    public partial string Subtitle { get; private set; } = string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedFilterChanged(string value)
    {
        foreach (var option in Filters)
        {
            option.IsSelected = option.Label == value;
        }

        ApplyFilter();
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
            _all = await repository.GetInspectionsAsync();
            Subtitle = Display.Plural(_all.Count, "completed inspection", "completed inspections");
            ApplyFilter();
        }
        catch (DataLoadException)
        {
            _all = [];
            Items.Clear();
            State = ViewState.Error;
        }
    }

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
        var now = clock.Now;
        Items.Clear();
        foreach (var inspection in _all
                     .Where(i =>
                         (SelectedFilter == FilterOption.All || i.Condition.ToString() == SelectedFilter) &&
                         (query.Length == 0 ||
                          i.AssetName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                          i.AssetId.Contains(query, StringComparison.OrdinalIgnoreCase)))
                     .OrderByDescending(i => i.Date))
        {
            Items.Add(new InspectionItem(inspection, Display.Timestamp(inspection.Date, now)));
        }

        State = _all.Count == 0 ? ViewState.Empty
            : Items.Count == 0 ? ViewState.NoResults
            : ViewState.Content;
    }
}
