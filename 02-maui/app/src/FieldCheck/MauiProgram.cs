using FieldCheck.Controls;
using FieldCheck.Core.Services;
using FieldCheck.Core.ViewModels;
using FieldCheck.Services;
using FieldCheck.Views;
using Microsoft.Maui.Handlers;

namespace FieldCheck;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
#if ANDROID
        builder.ConfigureMauiHandlers(handlers =>
            handlers.AddHandler<Shell, FieldCheck.Platforms.Android.FieldCheckShellRenderer>());
#endif

        ConfigureInputChrome();

        var services = builder.Services;

        // Data: fixtures seed app storage once; FIELDCHECK_DATA_MODE / FIELDCHECK_READ_DELAY_MS select the
        // deterministic empty/error/loading verification modes without any in-app debug UI.
        services.AddSingleton(DataSourceOptions.FromEnvironment(
            Environment.GetEnvironmentVariable("FIELDCHECK_DATA_MODE"),
            Environment.GetEnvironmentVariable("FIELDCHECK_READ_DELAY_MS")));
        services.AddSingleton<IDataFileStore>(_ => new FileDataStore(FileSystem.AppDataDirectory));
        services.AddSingleton<ISeedDataSource, PackagedSeedDataSource>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IFieldCheckRepository, JsonFieldCheckRepository>();

        // Platform services
        services.AddSingleton(FilePicker.Default);
        services.AddSingleton<IFilePickerService, MauiFilePickerService>();
        services.AddSingleton<INavigationService, ShellNavigationService>();

        // View models and views
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AssetsViewModel>();
        services.AddTransient<AssetDetailViewModel>();
        services.AddTransient<NewInspectionViewModel>();
        services.AddTransient<InspectionSuccessViewModel>();
        services.AddTransient<HistoryViewModel>();

        services.AddSingleton<AppShell>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<AssetsPage>();
        services.AddTransient<AssetDetailPage>();
        services.AddTransient<NewInspectionPage>();
        services.AddTransient<InspectionSuccessPage>();
        services.AddTransient<HistoryPage>();

        return builder.Build();
    }

    /// <summary>
    /// Inputs are framed by a FieldCheck Border (DESIGN_SPEC: labeled fields with visible borders), so the native
    /// underline (Android) and TextBox border (Windows) are removed to avoid a double frame.
    /// </summary>
    private static void AttachFocusFrame(object view)
    {
        if (view is InputView input && !input.Behaviors.OfType<FocusFrameBehavior>().Any())
        {
            input.Behaviors.Add(new FocusFrameBehavior());
        }
    }

    private static void ConfigureInputChrome()
    {
        EntryHandler.Mapper.AppendToMapping("FieldCheckChrome", (handler, entry) =>
        {
            AttachFocusFrame(entry);
#if ANDROID
            handler.PlatformView.BackgroundTintList = global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
        });
        EditorHandler.Mapper.AppendToMapping("FieldCheckChrome", (handler, editor) =>
        {
            AttachFocusFrame(editor);
#if ANDROID
            handler.PlatformView.BackgroundTintList = global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.Transparent);
#elif WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
#endif
        });
    }
}
