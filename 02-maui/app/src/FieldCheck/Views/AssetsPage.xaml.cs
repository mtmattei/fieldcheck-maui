using FieldCheck.Core.ViewModels;

namespace FieldCheck.Views;

public partial class AssetsPage : ContentPage
{
    public static readonly BindableProperty ShowChevronsProperty =
        BindableProperty.Create(nameof(ShowChevrons), typeof(bool), typeof(AssetsPage), true);

    private static readonly Color MasterBackground = Color.FromArgb("#F6F5F1");

    private readonly AssetsViewModel _viewModel;

    public AssetsPage(AssetsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        SizeChanged += (_, _) => ApplyLayout();
    }

    /// <summary>Rows show a chevron when activating them navigates to another page (single-pane layout).</summary>
    public bool ShowChevrons
    {
        get => (bool)GetValue(ShowChevronsProperty);
        set => SetValue(ShowChevronsProperty, value);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    private void ApplyLayout()
    {
        var wide = Width >= Breakpoints.WideContentWidth;
        _viewModel.IsWide = wide;
        ShowChevrons = !wide;

        RootGrid.ColumnDefinitions[0].Width = wide ? new GridLength(460) : GridLength.Star;
        RootGrid.ColumnDefinitions[1].Width = wide ? new GridLength(1) : new GridLength(0);
        RootGrid.ColumnDefinitions[2].Width = wide ? GridLength.Star : new GridLength(0);
        PaneRule.IsVisible = wide;
        MasterPane.BackgroundColor = wide ? MasterBackground : Colors.Transparent;
    }
}
