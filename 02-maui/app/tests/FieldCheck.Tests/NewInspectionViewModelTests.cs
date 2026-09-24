using FieldCheck.Core.Models;
using FieldCheck.Core.Services;
using FieldCheck.Core.ViewModels;

namespace FieldCheck.Tests;

public class NewInspectionViewModelTests
{
    private static async Task<NewInspectionViewModel> Form(TestContext ctx, string assetId = "CT-007")
    {
        var vm = new NewInspectionViewModel(ctx.Repository, ctx.Navigation, ctx.Picker);
        await vm.LoadAsync(assetId);
        return vm;
    }

    private static void FillValidGood(NewInspectionViewModel vm)
    {
        vm.SelectConditionCommand.Execute("Good");
        vm.OperatingNormally = true;
        vm.TemperatureText = "27";
        vm.GuardsSecure = vm.NoVisibleLeaks = vm.AreaClear = true;
    }

    [Fact]
    public async Task Shows_selected_asset_identity_and_starts_invalid()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);

        Assert.Equal("Cooling Tower 07 · CT-007", vm.AssetLine);
        Assert.Null(vm.Condition);
        Assert.True(vm.OperatingNormally);
        Assert.False(vm.SubmitCommand.CanExecute(null));
        Assert.Equal(["Select a condition", "Enter a temperature from -50 to 250 °C", "Confirm 3 checklist items"], vm.MissingRequirements);
        Assert.Null(vm.TemperatureError); // untouched fields do not shout
    }

    [Fact]
    public async Task Condition_supports_good_attention_critical()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        foreach (var (name, check) in new (string, Func<bool>)[] { ("Good", () => vm.IsGood), ("Attention", () => vm.IsAttention), ("Critical", () => vm.IsCritical) })
        {
            vm.SelectConditionCommand.Execute(name);
            Assert.True(check());
            Assert.Equal(Enum.Parse<InspectionCondition>(name), vm.Condition);
        }
    }

    [Theory]
    [InlineData("Good", true, false)]
    [InlineData("Attention", true, true)]
    [InlineData("Critical", true, true)]
    [InlineData("Good", false, true)]
    [InlineData("Critical", false, true)]
    public async Task Issue_description_visibility_follows_condition_and_operating_state(string condition, bool operating, bool required)
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        vm.SelectConditionCommand.Execute(condition);
        vm.OperatingNormally = operating;
        Assert.Equal(required, vm.IsIssueDescriptionRequired);
        Assert.Equal(operating ? "Yes" : "No", vm.OperatingNormallyText);
    }

    [Fact]
    public async Task Issue_description_is_required_when_shown()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        Assert.True(vm.SubmitCommand.CanExecute(null));

        vm.OperatingNormally = false;
        Assert.False(vm.SubmitCommand.CanExecute(null));
        Assert.Contains("Describe the issue", vm.MissingRequirements);

        vm.IssueDescription = "   ";
        Assert.Equal("Describe the issue before submitting.", vm.IssueDescriptionError);
        vm.IssueDescription = "Bearing noise";
        Assert.Null(vm.IssueDescriptionError);
        Assert.True(vm.SubmitCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("-50", true)]
    [InlineData("250", true)]
    [InlineData("0", true)]
    [InlineData("27.5", true)]
    [InlineData("-4,5", true)]
    [InlineData("-50.1", false)]
    [InlineData("250.01", false)]
    [InlineData("300", false)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData("1e3", false)]
    public async Task Temperature_accepts_minus_50_through_250_inclusive(string text, bool valid)
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        vm.TemperatureText = text;

        Assert.Equal(valid, vm.SubmitCommand.CanExecute(null));
        Assert.Equal(valid, vm.TemperatureError is null);
        if (!valid)
        {
            Assert.Contains("emperature", vm.TemperatureError);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Each_checklist_item_is_individually_required(int unchecked_)
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        switch (unchecked_)
        {
            case 0: vm.GuardsSecure = false; break;
            case 1: vm.NoVisibleLeaks = false; break;
            default: vm.AreaClear = false; break;
        }

        Assert.False(vm.SubmitCommand.CanExecute(null));
        Assert.Equal("All three checklist items must be confirmed.", vm.ChecklistError);
        Assert.Contains("Confirm 1 checklist item", vm.MissingRequirements);
    }

    [Fact]
    public async Task Submit_creates_exactly_one_inspection_and_navigates_to_success()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        vm.SelectConditionCommand.Execute("Attention");
        vm.TemperatureText = "27";
        vm.GuardsSecure = vm.NoVisibleLeaks = vm.AreaClear = true;
        vm.IssueDescription = "Basin-level alarm intermittent; inspect fan vibration.";
        vm.Notes = "Line one\nLine two";

        await vm.SubmitCommand.ExecuteAsync(null);

        var inspections = await ctx.CreateRepository().GetInspectionsAsync();
        Assert.Equal(7, inspections.Count);
        var saved = inspections[0];
        Assert.Equal(("INS-24092", "CT-007", InspectionCondition.Attention, 27d), (saved.Id, saved.AssetId, saved.Condition, saved.TemperatureC));
        Assert.Equal("Line one\nLine two", saved.Notes);
        Assert.Equal("Basin-level alarm intermittent; inspect fan vibration.", saved.IssueDescription);
        Assert.Equal(["success:INS-24092"], ctx.Navigation.Calls);
    }

    [Fact]
    public async Task Repeated_activation_during_save_cannot_duplicate()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        ctx.Navigation.Gate = new TaskCompletionSource(); // hold the save "in progress"

        var first = vm.SubmitCommand.ExecuteAsync(null);
        Assert.True(vm.IsSubmitting);
        Assert.False(vm.SubmitCommand.CanExecute(null)); // disabled while executing
        Assert.False(vm.CancelCommand.CanExecute(null));
        var second = vm.SubmitCommand.ExecuteAsync(null);
        vm.SubmitCommand.Execute(null);

        ctx.Navigation.Gate.SetResult();
        await Task.WhenAll(first, second);

        Assert.Equal(7, (await ctx.Repository.GetInspectionsAsync()).Count);
        Assert.Single(ctx.Navigation.Calls);
        Assert.False(vm.IsSubmitting);
    }

    [Fact]
    public async Task Cancel_returns_without_saving()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        await vm.CancelCommand.ExecuteAsync(null);

        Assert.Equal(["back"], ctx.Navigation.Calls);
        Assert.Equal(6, (await ctx.CreateRepository().GetInspectionsAsync()).Count);
    }

    [Fact]
    public async Task Persistence_failure_reports_error_and_keeps_form()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        ctx.Store.FailWrites = true;

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.NotNull(vm.SubmitError);
        Assert.Empty(ctx.Navigation.Calls);
        Assert.Equal("27", vm.TemperatureText);
        Assert.True(vm.SubmitCommand.CanExecute(null)); // user can retry

        ctx.Store.FailWrites = false;
        await vm.SubmitCommand.ExecuteAsync(null);
        Assert.Equal(["success:INS-24092"], ctx.Navigation.Calls);
    }

    [Fact]
    public async Task Picker_selection_shows_filename_and_is_saved_with_inspection()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);
        ctx.Picker.Behavior = () => Task.FromResult<PickedFile?>(new PickedFile("inspection-photo.png", "/data/attachments/abc.png"));

        await vm.ChooseFileCommand.ExecuteAsync(null);
        Assert.True(vm.HasAttachment);
        Assert.Equal("inspection-photo.png", vm.AttachmentName);

        await vm.SubmitCommand.ExecuteAsync(null);
        var saved = (await ctx.Repository.GetInspectionsAsync())[0];
        Assert.Equal("inspection-photo.png", saved.AttachmentFileName);
        Assert.Equal("/data/attachments/abc.png", saved.AttachmentPath);
    }

    [Fact]
    public async Task Picker_cancel_and_failure_leave_form_usable()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        FillValidGood(vm);

        ctx.Picker.Behavior = () => Task.FromResult<PickedFile?>(null); // user cancelled
        await vm.ChooseFileCommand.ExecuteAsync(null);
        Assert.False(vm.HasAttachment);
        Assert.Null(vm.AttachmentError);
        Assert.False(vm.IsPicking);

        ctx.Picker.Behavior = () => Task.FromException<PickedFile?>(new TaskCanceledException());
        await vm.ChooseFileCommand.ExecuteAsync(null);
        Assert.Null(vm.AttachmentError);

        ctx.Picker.Behavior = () => Task.FromException<PickedFile?>(new UnauthorizedAccessException());
        await vm.ChooseFileCommand.ExecuteAsync(null);
        Assert.NotNull(vm.AttachmentError);
        Assert.False(vm.IsPicking);
        Assert.True(vm.SubmitCommand.CanExecute(null));

        ctx.Picker.Behavior = () => Task.FromResult<PickedFile?>(new PickedFile("a.png", "/x/a.png"));
        await vm.ChooseFileCommand.ExecuteAsync(null);
        Assert.Null(vm.AttachmentError);
        vm.RemoveAttachmentCommand.Execute(null);
        Assert.False(vm.HasAttachment);
    }

    [Fact]
    public async Task Asset_detail_and_success_reflect_the_new_inspection()
    {
        using var ctx = new TestContext();
        var vm = await Form(ctx);
        vm.SelectConditionCommand.Execute("Attention");
        vm.TemperatureText = "27";
        vm.GuardsSecure = vm.NoVisibleLeaks = vm.AreaClear = true;
        vm.IssueDescription = "Basin-level alarm intermittent.";
        await vm.SubmitCommand.ExecuteAsync(null);

        var success = new InspectionSuccessViewModel(ctx.Repository, ctx.Navigation);
        await success.LoadAsync("INS-24092");
        Assert.Equal(("INS-24092", "Cooling Tower 07", "Attention", "Sep 23, 2026 · Alex Morgan"),
            (success.InspectionId, success.AssetName, success.ConditionText, success.MetaLine));

        await success.ViewAssetCommand.ExecuteAsync(null);
        await success.ViewHistoryCommand.ExecuteAsync(null);
        Assert.Equal(["success:INS-24092", "back", "history"], ctx.Navigation.Calls);

        var detail = new AssetDetailViewModel(ctx.Repository, ctx.Navigation);
        await detail.LoadAsync("CT-007");
        Assert.Equal("Attention", detail.StatusText);
        Assert.Equal("Sep 23, 2026", detail.LastInspectionText);
        Assert.Equal("Attention condition", detail.LatestConditionText);
        Assert.Equal("Basin-level alarm intermittent.", detail.LatestSummary);
    }

    [Fact]
    public async Task Load_failure_shows_error_state()
    {
        using var ctx = new TestContext(DataSourceMode.Error);
        var vm = await Form(ctx);
        Assert.Equal(ViewState.Error, vm.State);
        Assert.False(vm.SubmitCommand.CanExecute(null));
    }
}
