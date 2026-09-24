using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace FieldCheck;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;

        // Resize the window for the soft keyboard so every form field and action stays reachable.
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // The Shell is created here, after InitializeComponent has loaded the app resources its XAML references.
        var window = new Window(new AppShell()) { Title = "FieldCheck" };
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
