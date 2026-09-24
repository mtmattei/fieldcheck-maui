using FieldCheck.Core.Services;

namespace FieldCheck.Services;

public sealed class ShellNavigationService : INavigationService
{
    public const string AssetDetailRoute = "assetdetail";
    public const string NewInspectionRoute = "newinspection";
    public const string InspectionSuccessRoute = "inspectionsuccess";

    private static Shell Shell => Shell.Current;

    public Task GoToAssetDetailAsync(string assetId) =>
        Shell.GoToAsync(AssetDetailRoute, new ShellNavigationQueryParameters { ["id"] = assetId });

    public Task GoToAssetsAsync() => Shell.GoToAsync("//main/assets");

    public async Task GoToHistoryAsync()
    {
        // Leave the current tab at its root so returning to it shows the asset, not a stale success page.
        if (Shell.Navigation.NavigationStack.Count > 1)
        {
            await Shell.Navigation.PopAsync(false);
        }

        await Shell.GoToAsync("//main/history");
    }

    public Task GoToNewInspectionAsync(string assetId) =>
        Shell.GoToAsync(NewInspectionRoute, new ShellNavigationQueryParameters { ["assetId"] = assetId });

    public Task GoToInspectionSuccessAsync(string inspectionId) =>
        Shell.GoToAsync($"../{InspectionSuccessRoute}", new ShellNavigationQueryParameters { ["id"] = inspectionId });

    public Task GoBackAsync() =>
        Shell.Navigation.NavigationStack.Count > 1 ? Shell.GoToAsync("..") : Task.CompletedTask;
}
