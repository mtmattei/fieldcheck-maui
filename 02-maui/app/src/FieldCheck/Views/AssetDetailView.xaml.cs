namespace FieldCheck.Views;

public partial class AssetDetailView : ContentView
{
    public static readonly BindableProperty IsWideProperty =
        BindableProperty.Create(nameof(IsWide), typeof(bool), typeof(AssetDetailView), false,
            propertyChanged: (b, _, _) => ((AssetDetailView)b).ApplyLayout());

    public AssetDetailView()
    {
        InitializeComponent();
        ApplyLayout();
    }

    /// <summary>Wide layout: action beside the title. Compact layout: full-width action after the content.</summary>
    public bool IsWide
    {
        get => (bool)GetValue(IsWideProperty);
        set => SetValue(IsWideProperty, value);
    }

    private void ApplyLayout()
    {
        TopActionButton.IsVisible = IsWide;
        BottomActionButton.IsVisible = !IsWide;
        InfoGrid.Margin = new Thickness(0, IsWide ? 36 : 24, 0, 0);
    }
}
