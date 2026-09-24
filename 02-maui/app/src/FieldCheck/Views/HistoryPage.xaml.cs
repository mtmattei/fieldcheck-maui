using FieldCheck.Core.ViewModels;

namespace FieldCheck.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;
    private bool? _wide;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        SizeChanged += (_, _) => ApplyLayout(Width >= Breakpoints.WideContentWidth + 140);
        ApplyLayout(false);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    /// <summary>Table layout on wide windows; stacked rows when narrow so columns never clip.</summary>
    private void ApplyLayout(bool wide)
    {
        if (_wide == wide)
        {
            return;
        }

        _wide = wide;
        TableHeader.IsVisible = wide;
        // Wide: search and chips share one row. Narrow: chips move below the search field.
        SearchRow.ColumnDefinitions = wide ? [new ColumnDefinition(new GridLength(415)), new ColumnDefinition(GridLength.Star)] : [new ColumnDefinition(GridLength.Star)];
        SearchRow.RowDefinitions = wide ? [new RowDefinition(GridLength.Auto)] : [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)];
        Grid.SetColumn(ChipScroller, wide ? 1 : 0);
        Grid.SetRow(ChipScroller, wide ? 0 : 1);
        ChipScroller.Margin = wide ? new Thickness(28, 0, 0, 0) : new Thickness(0, 14, 0, 0);
        Rows.ItemTemplate = (DataTemplate)Resources[wide ? "WideRow" : "CompactRow"];
    }
}
