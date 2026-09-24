using FieldCheck.Core.Services;
using FieldCheck.Core.ViewModels;
using FieldCheck.Services;
using FieldCheck.Views;

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
}
