using FieldCheck.Core.ViewModels;

namespace FieldCheck.Views;

public partial class InspectionSuccessPage : ContentPage, IQueryAttributable
{
    private readonly InspectionSuccessViewModel _viewModel;

    public InspectionSuccessPage(InspectionSuccessViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id) && id is string inspectionId)
        {
            _ = _viewModel.LoadAsync(inspectionId);
        }
    }
}
