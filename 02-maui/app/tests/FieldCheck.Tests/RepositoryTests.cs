using System.Security.Cryptography;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Tests;

public class RepositoryTests
{
    private static InspectionDraft Draft(string assetId = "CT-007", InspectionCondition condition = InspectionCondition.Attention) => new()
    {
        AssetId = assetId,
        Condition = condition,
        OperatingNormally = true,
        TemperatureC = 27,
        IssueDescription = "Basin-level alarm intermittent; inspect fan vibration.",
    };

    [Fact]
    public async Task Seeds_all_twelve_assets_and_six_inspections_from_fixtures()
    {
        using var ctx = new TestContext();

        var assets = await ctx.Repository.GetAssetsAsync();
        var inspections = await ctx.Repository.GetInspectionsAsync();

        Assert.Equal(12, assets.Count);
        Assert.Equal(["PMP-104", "AHU-203", "CMP-012", "CT-007", "PNL-044", "CNV-018", "FAN-305", "BLR-002", "MTR-219", "PMP-220", "DB-011", "EF-090"], assets.Select(a => a.Id));
        Assert.Equal(6, inspections.Count);
        Assert.True(File.Exists(Path.Combine(ctx.StorageDirectory, "fieldcheck-data.json")));
    }

    [Fact]
    public async Task Inspections_are_newest_first()
    {
        using var ctx = new TestContext();
        var inspections = await ctx.Repository.GetInspectionsAsync();
        Assert.Equal(["INS-24091", "INS-24044", "INS-24086", "INS-24065", "INS-24058", "INS-24072"], inspections.Select(i => i.Id));
    }

    [Fact]
    public async Task Added_inspection_persists_across_restart_and_updates_asset()
    {
        using var ctx = new TestContext();
        var saved = await ctx.Repository.AddInspectionAsync(Draft());

        var restarted = ctx.CreateRepository();
        var inspections = await restarted.GetInspectionsAsync();
        var asset = await restarted.GetAssetAsync("CT-007");

        Assert.Equal(saved.Id, inspections[0].Id);
        Assert.Equal("Alex Morgan", inspections[0].Inspector);
        Assert.Equal("Cooling Tower 07", inspections[0].AssetName);
        Assert.Equal(InspectionCondition.Attention, inspections[0].Condition);
        Assert.Equal(7, inspections.Count);
        Assert.Equal(AssetStatus.Attention, asset!.Status);
        Assert.Equal(new DateOnly(2026, 9, 23), asset.LastInspection);
    }

    [Fact]
    public async Task Generated_ids_are_unique_sequential_and_stable_after_restart()
    {
        using var ctx = new TestContext();
        var first = await ctx.Repository.AddInspectionAsync(Draft());
        var second = await ctx.Repository.AddInspectionAsync(Draft("PMP-104", InspectionCondition.Good));

        Assert.Equal("INS-24092", first.Id);
        Assert.Equal("INS-24093", second.Id);

        var restarted = ctx.CreateRepository();
        var third = await restarted.AddInspectionAsync(Draft("AHU-203"));
        var ids = (await restarted.GetInspectionsAsync()).Select(i => i.Id).ToList();

        Assert.Equal("INS-24094", third.Id);
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.Contains("INS-24092", ids);
    }

    [Fact]
    public async Task Saving_does_not_mutate_fixture_files()
    {
        var dir = FixtureSeedDataSource.DefaultDirectory;
        string Hash(string f) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(dir, f))));
        var before = (Hash("assets.json"), Hash("inspections.json"));

        using var ctx = new TestContext();
        await ctx.Repository.AddInspectionAsync(Draft());

        Assert.Equal(before, (Hash("assets.json"), Hash("inspections.json")));
    }

    [Fact]
    public async Task Write_failure_throws_and_leaves_data_unchanged()
    {
        using var ctx = new TestContext();
        await ctx.Repository.GetAssetsAsync();
        ctx.Store.FailWrites = true;

        await Assert.ThrowsAsync<DataPersistenceException>(() => ctx.Repository.AddInspectionAsync(Draft()));

        Assert.Equal(6, (await ctx.Repository.GetInspectionsAsync()).Count);
        Assert.Equal(AssetStatus.Critical, (await ctx.Repository.GetAssetAsync("CT-007"))!.Status);
        Assert.Equal(6, (await ctx.CreateRepository().GetInspectionsAsync()).Count);
    }

    [Fact]
    public async Task Empty_mode_returns_no_records()
    {
        using var ctx = new TestContext(DataSourceMode.Empty);
        Assert.Empty(await ctx.Repository.GetAssetsAsync());
        Assert.Empty(await ctx.Repository.GetInspectionsAsync());
    }

    [Fact]
    public async Task Error_mode_fails_reads_and_error_once_recovers()
    {
        using var failing = new TestContext(DataSourceMode.Error);
        await Assert.ThrowsAsync<DataLoadException>(() => failing.Repository.GetAssetsAsync());
        await Assert.ThrowsAsync<DataLoadException>(() => failing.Repository.GetAssetsAsync());

        using var once = new TestContext(DataSourceMode.ErrorOnce);
        await Assert.ThrowsAsync<DataLoadException>(() => once.Repository.GetAssetsAsync());
        Assert.Equal(12, (await once.Repository.GetAssetsAsync()).Count);
    }

    [Fact]
    public async Task Corrupt_data_file_surfaces_as_load_error()
    {
        using var ctx = new TestContext();
        await File.WriteAllTextAsync(Path.Combine(ctx.StorageDirectory, "fieldcheck-data.json"), "{ not json");
        await Assert.ThrowsAsync<DataLoadException>(() => ctx.Repository.GetAssetsAsync());
    }

    [Fact]
    public void Environment_options_parse_modes_and_default_to_normal()
    {
        Assert.Equal(DataSourceMode.ErrorOnce, DataSourceOptions.FromEnvironment("erroronce", null).Mode);
        Assert.Equal(DataSourceMode.Normal, DataSourceOptions.FromEnvironment(null, null).Mode);
        Assert.Equal(DataSourceMode.Normal, DataSourceOptions.FromEnvironment("bogus", "x").Mode);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), DataSourceOptions.FromEnvironment("empty", "1500").ReadDelay);
    }
}
