using FieldCheck.Core.ViewModels;

namespace FieldCheck.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh on every visit so counts reflect inspections saved elsewhere in the app.
        _viewModel.LoadCommand.Execute(null);
    }
}
