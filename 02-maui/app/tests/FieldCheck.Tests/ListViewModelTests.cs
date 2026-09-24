using FieldCheck.Core.Services;
using FieldCheck.Core.ViewModels;

namespace FieldCheck.Tests;

public class ListViewModelTests
{
    [Fact]
    public async Task Dashboard_counts_are_derived_from_data()
    {
        using var ctx = new TestContext();
        var vm = new DashboardViewModel(ctx.Repository, ctx.Navigation, ctx.Clock);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Content, vm.State);
        Assert.Equal((12, 7, 3, 2), (vm.TotalCount, vm.OperationalCount, vm.AttentionCount, vm.CriticalCount));
        Assert.Equal(["CT-007", "EF-090", "AHU-203", "CNV-018", "BLR-002"], vm.NeedsAttention.Select(a => a.Id));
        Assert.Equal("Good afternoon", vm.Greeting);
        Assert.Equal("Facility A · Wednesday, Sep 23", vm.ContextLine);

        await ctx.Repository.AddInspectionAsync(new() { AssetId = "CT-007", Condition = Core.Models.InspectionCondition.Good, OperatingNormally = true, TemperatureC = 20 });
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal((8, 3, 1), (vm.OperationalCount, vm.AttentionCount, vm.CriticalCount));
    }

    [Fact]
    public async Task Dashboard_opens_asset_detail_and_all_assets()
    {
        using var ctx = new TestContext();
        var vm = new DashboardViewModel(ctx.Repository, ctx.Navigation, ctx.Clock);
        await vm.LoadCommand.ExecuteAsync(null);
        await vm.OpenAssetCommand.ExecuteAsync(vm.NeedsAttention[0]);
        await vm.ViewAllAssetsCommand.ExecuteAsync(null);
        Assert.Equal(["detail:CT-007", "assets"], ctx.Navigation.Calls);
    }

    [Theory]
    [InlineData(DataSourceMode.Empty, ViewState.Empty)]
    [InlineData(DataSourceMode.Error, ViewState.Error)]
    public async Task Dashboard_shows_empty_and_error_states(DataSourceMode mode, ViewState expected)
    {
        using var ctx = new TestContext(mode);
        var vm = new DashboardViewModel(ctx.Repository, ctx.Navigation, ctx.Clock);
        Assert.Equal(ViewState.Loading, vm.State);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(expected, vm.State);
    }

    private static async Task<AssetsViewModel> LoadedAssets(TestContext ctx, bool wide = false)
    {
        var vm = new AssetsViewModel(ctx.Repository, ctx.Navigation, new AssetDetailViewModel(ctx.Repository, ctx.Navigation)) { IsWide = wide };
        await vm.LoadCommand.ExecuteAsync(null);
        return vm;
    }

    [Theory]
    [InlineData("cooling", "CT-007")]      // name, case-insensitive
    [InlineData("ct-007", "CT-007")]       // ID
    [InlineData("SMOKE EXHAUST", "EF-090")] // type
    [InlineData("stair core", "EF-090")]   // location
    public async Task Assets_search_matches_name_id_type_location(string query, string expectedId)
    {
        using var ctx = new TestContext();
        var vm = await LoadedAssets(ctx);
        vm.SearchText = query;
        Assert.Equal([expectedId], vm.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task Assets_status_filter_and_search_combine()
    {
        using var ctx = new TestContext();
        var vm = await LoadedAssets(ctx);
        Assert.Equal("12 equipment records", vm.Subtitle);

        vm.SelectFilterCommand.Execute(vm.Filters.Single(f => f.Label == "Operational"));
        Assert.Equal(7, vm.Items.Count);
        Assert.True(vm.Filters.Single(f => f.Label == "Operational").IsSelected);
        Assert.False(vm.Filters[0].IsSelected);

        vm.SelectFilterCommand.Execute(vm.Filters.Single(f => f.Label == "Attention"));
        Assert.Equal(["AHU-203", "CNV-018", "BLR-002"], vm.Items.Select(i => i.Id));

        vm.SelectFilterCommand.Execute(vm.Filters.Single(f => f.Label == "Critical"));
        Assert.Equal(["CT-007", "EF-090"], vm.Items.Select(i => i.Id));

        vm.SearchText = "roof";
        Assert.Equal(["CT-007"], vm.Items.Select(i => i.Id));

        vm.SearchText = "pump";
        Assert.Equal(ViewState.NoResults, vm.State);
        Assert.Empty(vm.Items);

        vm.ClearSearchCommand.Execute(null);
        Assert.Equal(12, vm.Items.Count);
        Assert.Equal(ViewState.Content, vm.State);
    }

    [Fact]
    public async Task Assets_narrow_selection_navigates_wide_selection_loads_detail_pane()
    {
        using var ctx = new TestContext();
        var narrow = await LoadedAssets(ctx);
        narrow.OpenAssetCommand.Execute(narrow.Items[3]);
        Assert.Equal(["detail:CT-007"], ctx.Navigation.Calls);
        Assert.Null(narrow.SelectedItem);

        var wide = await LoadedAssets(ctx, wide: true);
        Assert.Equal("PMP-104", wide.SelectedItem!.Id); // first row selected by default
        wide.OpenAssetCommand.Execute(wide.Items[3]);
        await Task.Delay(50);
        Assert.Equal("CT-007", wide.Detail.AssetId);
        Assert.Equal("Cooling Tower 07", wide.Detail.Name);
        Assert.True(wide.Items[3].IsSelected);
        Assert.False(wide.Items[0].IsSelected);
        Assert.True(wide.ShowDetailPane);
        Assert.Single(ctx.Navigation.Calls); // no page navigation in master/detail
    }

    [Fact]
    public async Task Assets_error_state_retries_and_recovers()
    {
        using var ctx = new TestContext(DataSourceMode.ErrorOnce);
        var vm = await LoadedAssets(ctx);
        Assert.Equal(ViewState.Error, vm.State);

        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(ViewState.Content, vm.State);
        Assert.Equal(12, vm.Items.Count);
    }

    [Fact]
    public async Task Assets_empty_repository_is_distinct_from_no_results()
    {
        using var ctx = new TestContext(DataSourceMode.Empty);
        var vm = await LoadedAssets(ctx);
        Assert.Equal(ViewState.Empty, vm.State);
        vm.SearchText = "x";
        Assert.Equal(ViewState.Empty, vm.State);
    }

    [Fact]
    public async Task History_is_newest_first_searchable_and_filterable()
    {
        using var ctx = new TestContext();
        var vm = new HistoryViewModel(ctx.Repository, ctx.Clock);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal("6 completed inspections", vm.Subtitle);
        Assert.Equal(["INS-24091", "INS-24044", "INS-24086", "INS-24065", "INS-24058", "INS-24072"], vm.Items.Select(i => i.Id));
        Assert.Equal("INS-24091 · Sep 18, 9:24 AM", vm.Items[0].IdAndTimestamp);
        Assert.Equal("Alex Morgan", vm.Items[0].Inspector);

        vm.SearchText = "booster";
        Assert.Equal(["INS-24091"], vm.Items.Select(i => i.Id));
        vm.SearchText = "cnv-018";
        Assert.Equal(["INS-24065"], vm.Items.Select(i => i.Id));
        vm.SearchText = "Mechanical Room A"; // location is not a history search field
        Assert.Equal(ViewState.NoResults, vm.State);

        vm.ClearSearchCommand.Execute(null);
        vm.SelectFilterCommand.Execute(vm.Filters.Single(f => f.Label == "Good"));
        Assert.Equal(["INS-24091", "INS-24044"], vm.Items.Select(i => i.Id));
        vm.SelectFilterCommand.Execute(vm.Filters.Single(f => f.Label == "Critical"));
        Assert.Equal(["INS-24072"], vm.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task History_shows_new_inspection_first_with_today_timestamp()
    {
        using var ctx = new TestContext();
        await ctx.Repository.AddInspectionAsync(new() { AssetId = "CT-007", Condition = Core.Models.InspectionCondition.Attention, OperatingNormally = true, TemperatureC = 27, IssueDescription = "x" });
        var vm = new HistoryViewModel(ctx.Repository, ctx.Clock);
        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal("7 completed inspections", vm.Subtitle);
        Assert.Equal("INS-24092 · Today, 2:41 PM", vm.Items[0].IdAndTimestamp);
        Assert.Equal("Attention", vm.Items[0].ConditionText);
    }

    [Theory]
    [InlineData(DataSourceMode.Empty, ViewState.Empty)]
    [InlineData(DataSourceMode.Error, ViewState.Error)]
    public async Task History_shows_empty_and_error_states(DataSourceMode mode, ViewState expected)
    {
        using var ctx = new TestContext(mode);
        var vm = new HistoryViewModel(ctx.Repository, ctx.Clock);
        await vm.LoadCommand.ExecuteAsync(null);
        Assert.Equal(expected, vm.State);
    }

    [Fact]
    public async Task Asset_detail_shows_long_description_and_latest_inspection()
    {
        using var ctx = new TestContext();
        var vm = new AssetDetailViewModel(ctx.Repository, ctx.Navigation);
        await vm.LoadAsync("CT-007");

        Assert.Equal(ViewState.Content, vm.State);
        Assert.Equal("Sep 8, 2026", vm.LastInspectionText);
        Assert.Contains("long-content behavior", vm.Description);
        Assert.Equal("Critical condition", vm.LatestConditionText);
        Assert.Equal("Fan vibration above normal. Unit requires maintenance review.", vm.LatestSummary);

        await vm.StartInspectionCommand.ExecuteAsync(null);
        await vm.BackCommand.ExecuteAsync(null);
        Assert.Equal(["new:CT-007", "back"], ctx.Navigation.Calls);
    }

    [Fact]
    public async Task Asset_detail_without_inspection_and_unknown_asset()
    {
        using var ctx = new TestContext();
        var vm = new AssetDetailViewModel(ctx.Repository, ctx.Navigation);
        await vm.LoadAsync("PNL-044");
        Assert.False(vm.HasLatestInspection);

        await vm.LoadAsync("NOPE-1");
        Assert.Equal(ViewState.Empty, vm.State);
    }
}
