using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace FieldCheck;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        _shell = shell;

        // Resize the window for the soft keyboard so every form field and action stays reachable.
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_shell) { Title = "FieldCheck" };
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            // Reference desktop viewport; users can resize down to the minimum.
            window.Width = 1440;
            window.Height = 900;
            window.MinimumWidth = 720;
            window.MinimumHeight = 560;
        }

        return window;
    }
}
