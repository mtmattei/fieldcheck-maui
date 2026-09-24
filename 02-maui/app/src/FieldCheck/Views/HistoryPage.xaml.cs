using Microsoft.Maui.Layouts;

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
        SearchBox.WidthRequest = wide ? 415 : -1;
        SearchBox.Margin = wide ? new Thickness(0, 0, 28, 10) : new Thickness(0, 0, 0, 14);
        FlexLayout.SetBasis(SearchBox, wide ? FlexBasis.Auto : new FlexBasis(1, true));
        Rows.ItemTemplate = (DataTemplate)Resources[wide ? "WideRow" : "CompactRow"];
    }
}
