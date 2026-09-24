using FieldCheck.Services;
using FieldCheck.Views;

namespace FieldCheck;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(ShellNavigationService.AssetDetailRoute, typeof(AssetDetailPage));
        Routing.RegisterRoute(ShellNavigationService.NewInspectionRoute, typeof(NewInspectionPage));
        Routing.RegisterRoute(ShellNavigationService.InspectionSuccessRoute, typeof(InspectionSuccessPage));
    }
}
