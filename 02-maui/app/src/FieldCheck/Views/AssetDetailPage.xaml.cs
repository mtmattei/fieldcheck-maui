using FieldCheck.Core.ViewModels;

namespace FieldCheck.Views;

public partial class AssetDetailPage : ContentPage, IQueryAttributable
{
    private readonly AssetDetailViewModel _viewModel;

    public AssetDetailPage(AssetDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        SizeChanged += (_, _) => DetailView.IsWide = Width >= Breakpoints.WideContentWidth;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id) && id is string assetId)
        {
            _viewModel.LoadCommand.Execute(assetId);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Returning from a saved inspection must show the new status and last-inspection date.
        if (_viewModel.AssetId is { } assetId && _viewModel.Asset is not null)
        {
            _viewModel.LoadCommand.Execute(assetId);
        }
    }
}
