using FieldCheck.Core.ViewModels;

namespace FieldCheck.Views;

public partial class NewInspectionPage : ContentPage, IQueryAttributable
{
    private readonly NewInspectionViewModel _viewModel;
    private bool? _wide;

    public NewInspectionPage(NewInspectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        SizeChanged += (_, _) => ApplyLayout(Width >= 900);
        ApplyLayout(false);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("assetId", out var id) && id is string assetId)
        {
            _ = _viewModel.LoadAsync(assetId);
        }
    }

    /// <summary>
    /// Pure layout: compact is one continuous column (reference Android); wide places the checklist beside
    /// the primary inputs and right-aligns the actions (reference Windows). The same controls are reused.
    /// </summary>
    private void ApplyLayout(bool wide)
    {
        if (_wide == wide)
        {
            return;
        }

        _wide = wide;
        FormGrid.ColumnDefinitions.Clear();
        FormGrid.RowDefinitions.Clear();

        if (wide)
        {
            FormGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(380)));
            FormGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(92)));
            FormGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (var i = 0; i < 6; i++)
            {
                FormGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            Place(SectionPrimary, 0, 0, 1, new Thickness(0));
            Place(SectionChecklist, 0, 2, 1, new Thickness(0));
            Place(SectionRule, 1, 0, 3, new Thickness(0, 28, 0, 32));
            Place(SectionIssue, 2, 0, 3, new Thickness(0, 0, 0, 28), width: 820);
            Place(SectionNotes, 3, 0, 3, new Thickness(0, 0, 0, 28), width: 820);
            Place(SectionAttach, 4, 0, 3, new Thickness(0), width: 360);
            Place(SectionActions, 5, 0, 3, new Thickness(0, 36, 0, 0));

            ActionsGrid.ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(195))];
            Grid.SetColumn(CancelButton, 1);
            Grid.SetColumn(SubmitButton, 2);
        }
        else
        {
            FormGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (var i = 0; i < 7; i++)
            {
                FormGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            Place(SectionPrimary, 0, 0, 1, new Thickness(0, 0, 0, 20));
            Place(SectionChecklist, 1, 0, 1, new Thickness(0, 0, 0, 16));
            Place(SectionRule, 2, 0, 1, new Thickness(0));
            Place(SectionIssue, 3, 0, 1, new Thickness(0, 0, 0, 20));
            Place(SectionNotes, 4, 0, 1, new Thickness(0, 0, 0, 20));
            Place(SectionAttach, 5, 0, 1, new Thickness(0));
            Place(SectionActions, 6, 0, 1, new Thickness(0, 22, 0, 0));

            ActionsGrid.ColumnDefinitions = [new ColumnDefinition(GridLength.Star)];
            Grid.SetColumn(CancelButton, 0);
            Grid.SetColumn(SubmitButton, 0);
        }

        SectionRule.IsVisible = wide;
    }

    private static void Place(View view, int row, int column, int columnSpan, Thickness margin, double width = -1)
    {
        Grid.SetRow(view, row);
        Grid.SetColumn(view, column);
        Grid.SetColumnSpan(view, columnSpan);
        view.Margin = margin;
        view.WidthRequest = width;
        view.HorizontalOptions = width > 0 ? LayoutOptions.Start : LayoutOptions.Fill;
    }
}
