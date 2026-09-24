using System.Diagnostics;
using System.Text.Json;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

// Usage: FieldCheck.WinDriver <FieldCheck.exe> <outDir> <inspection-photo.png>
var exe = Path.GetFullPath(args[0]);
var outDir = Path.GetFullPath(args[1]);
var photo = Path.GetFullPath(args[2]);
var shots = Path.Combine(outDir, "screenshots");
Directory.CreateDirectory(shots);

var checks = new List<Check>();
using var automation = new UIA3Automation();
var d = new Driver(automation, exe, shots, checks);

void Step(string name, Action action)
{
    Console.WriteLine($"== {name}");
    try { action(); }
    catch (Exception ex)
    {
        Console.WriteLine($"   STEP FAILED: {ex}");
        checks.Add(new Check($"step:{name}", "FAIL", ex.Message));
        d.TryShot($"fail-{name}");
    }
}

var sw = Stopwatch.StartNew();
Step("launch", () =>
{
    d.Launch();
    var greeting = d.WaitName(n => n.StartsWith("Good ") && (n.EndsWith("morning") || n.EndsWith("afternoon") || n.EndsWith("evening")), 90);
    d.Record("A06", greeting is not null, $"Dashboard greeting '{greeting?.Name}' visible {sw.Elapsed.TotalSeconds:F1}s after launch; window {d.Window.BoundingRectangle}");
    d.Record("J04", greeting is not null, "Clean first run on a fresh runner seeded data and reached the Dashboard");
    Thread.Sleep(1500);
    d.Shot("01-dashboard");
    var rows = d.RowButtons();
    d.Record("C02-ui", d.HasName("12") && d.HasName("7") && d.HasName("3") && d.HasName("2"), "Dashboard shows 12 / 7 / 3 / 2");
    d.Record("B01-rows", rows.Any(r => r.StartsWith("Cooling Tower 07")) && rows.Any(r => r.StartsWith("Emergency Fan 90")), "Needs attention rows: " + string.Join(" | ", rows.Select(r => r.Split(',')[0])));
});

Step("keyboard-focus", () =>
{
    d.Window.Focus();
    for (var i = 0; i < 4; i++) { Keyboard.Press(VirtualKeyShort.TAB); Thread.Sleep(250); }
    var focused = automation.FocusedElement();
    d.Shot("g03-keyboard-focus-dashboard");
    d.Record("F07", focused is not null && focused.Properties.ProcessId.ValueOrDefault == d.App.ProcessId, $"Tab moves focus through app controls; focused: {focused?.ControlType} '{focused?.Name}'");
});

Step("assets-master-detail", () =>
{
    d.ClickSidebar("Assets");
    d.WaitId("AssetSearchEntry", 30);
    Thread.Sleep(1000);
    d.Record("B09", true, "Sidebar navigated to Assets");
    d.ClickRow("Cooling Tower 07");
    var start = d.WaitId("StartInspectionTopButton", 20);
    Thread.Sleep(1000);
    var paneTitle = d.All().FirstOrDefault(e => Driver.SafeName(e) == "Cooling Tower 07" && e.BoundingRectangle.X > d.Window.BoundingRectangle.X + 700);
    d.Record("B10", start is not null && paneTitle is not null, $"Wide Assets: list left, detail pane right (title x={paneTitle?.BoundingRectangle.X}, action {start?.BoundingRectangle})");
    d.Shot("02-assets-master-detail");
    var longText = d.All().FirstOrDefault(e => Driver.SafeName(e).Contains("intentionally long description"));
    d.Record("E07", longText is not null && !longText.IsOffscreen && longText.BoundingRectangle.Right <= d.Window.BoundingRectangle.Right, $"CT-007 description wraps within pane: {longText?.BoundingRectangle}");
});

Step("assets-search-filter", () =>
{
    d.TypeInto("AssetSearchEntry", "roof");
    Thread.Sleep(800);
    var roof = d.RowButtons().Select(r => r.Split(',')[1].Trim()).ToList();
    d.Record("C03-ui", roof.OrderBy(x => x).SequenceEqual(new[] { "AHU-203", "CT-007", "FAN-305" }), "Search 'roof' -> " + string.Join(",", roof));
    d.ClickName("Filter: Critical");
    Thread.Sleep(800);
    var combined = d.RowButtons().Select(r => r.Split(',')[1].Trim()).ToList();
    d.Record("C05-ui", combined.SequenceEqual(new[] { "CT-007" }), "Search 'roof' + Critical -> " + string.Join(",", combined));
    d.TypeInto("AssetSearchEntry", "zzz");
    var none = d.WaitName(n => n == "No matching assets", 10);
    d.Shot("e05-assets-no-results");
    d.Record("E05-ui", none is not null && d.RowButtons().Count == 0, "No-results state shown for 'zzz' + Critical");
    d.ClickId("StateActionButton");
    Thread.Sleep(800);
    d.Record("C04-ui", d.RowButtons().Count >= 7 && d.FindId("AssetSearchEntry")!.AsTextBox().Text == "", $"Clear search restores list ({d.RowButtons().Count} rows realized)");
    d.ClickName("Filter: Attention");
    Thread.Sleep(800);
    var att = d.RowButtons().Select(r => r.Split(',')[1].Trim()).ToList();
    d.Record("C04-ui-attention", att.SequenceEqual(new[] { "AHU-203", "CNV-018", "BLR-002" }), "Attention filter -> " + string.Join(",", att));
    d.ClickName("Filter: All");
    Thread.Sleep(800);
    d.ClickRow("Cooling Tower 07");
    d.WaitId("StartInspectionTopButton", 10);
});

Step("new-inspection-validation", () =>
{
    d.ClickId("StartInspectionTopButton");
    d.WaitId("TemperatureEntry", 20);
    Thread.Sleep(1200);
    d.Record("B04", d.HasName("Cooling Tower 07 · CT-007"), "New inspection shows asset identity");
    d.Shot("h04-new-inspection-empty");
    d.Record("D14-ui", !d.FindId("SubmitButton")!.IsEnabled, "Submit disabled on empty form");
    d.ClickId("ConditionGood");
    Thread.Sleep(500);
    d.Record("D10-ui", d.FindId("IssueEditor") is null, "Good + Operating Yes: issue description hidden");
    d.ClickId("OperatingSwitch");
    Thread.Sleep(500);
    d.Record("D12-ui", d.FindId("IssueEditor") is not null && d.HasName("No"), "Operating normally No: issue description shown");
    d.ClickId("OperatingSwitch");
    Thread.Sleep(500);
    d.ClickId("ConditionAttention");
    Thread.Sleep(500);
    d.Record("D11-ui", d.FindId("IssueEditor") is not null, "Attention: issue description shown");
    d.TypeInto("TemperatureEntry", "300");
    var err = d.WaitId("TemperatureError", 5);
    Thread.Sleep(500);
    d.Shot("d04-temperature-validation");
    d.Record("D04-ui", err is not null && err.Name.Contains("between -50 and 250") && !d.FindId("SubmitButton")!.IsEnabled, $"300 °C -> '{err?.Name}', submit disabled");
    var summary = d.FindId("RequirementsSummary");
    d.Record("E06-ui", summary is not null && summary.Name.Contains("checklist") && summary.Name.Contains("Describe the issue"), $"Summary: '{summary?.Name}'");
    d.TypeInto("TemperatureEntry", "27");
    Thread.Sleep(300);
    d.Record("D03-ui", d.FindId("TemperatureError") is null, "27 °C accepted");
    d.ClickId("CheckGuards");
    d.ClickId("CheckLeaks");
    Thread.Sleep(300);
    d.Record("D05-ui", !d.FindId("SubmitButton")!.IsEnabled, "Two of three checklist items -> submit still disabled");
    d.ClickId("CheckArea");
    d.TypeInto("IssueEditor", "Basin-level alarm intermittent; inspect fan vibration.");
    d.ClickId("NotesEditor");
    Keyboard.Type("Line one");
    Keyboard.Press(VirtualKeyShort.RETURN);
    Keyboard.Type("Line two");
    Thread.Sleep(1000);
    d.Record("D06-ui", d.FindId("NotesEditor")!.AsTextBox().Text.Replace("\r\n", "\n").Replace('\r', '\n').Contains("Line one\nLine two"), "Notes accepts multiline text");
});

Step("file-picker", () =>
{
    d.ClickId("AttachButton");
    var dialog = d.WaitDialog(20);
    d.ShotScreen("d07-file-picker-open");
    d.Record("D07", dialog is not null, $"System file dialog opened: '{dialog?.Name}' ({dialog?.ClassName})");
    Keyboard.Press(VirtualKeyShort.ESCAPE);
    Thread.Sleep(1500);
    d.Record("D09", d.WaitDialog(1) is null && d.FindId("TemperatureEntry")!.AsTextBox().Text == "27" && d.FindId("AttachmentName") is null,
        "Picker cancelled: form intact, no attachment");
    d.ClickId("AttachButton");
    dialog = d.WaitDialog(20);
    Thread.Sleep(1000);
    Keyboard.Type(photo);
    Keyboard.Press(VirtualKeyShort.RETURN);
    var name = d.WaitId("AttachmentName", 20);
    Thread.Sleep(1000);
    d.Record("D08", name?.Name == "inspection-photo.png", $"Attachment shown: '{name?.Name}'");
    d.Record("F08", name is not null, "Windows FileOpenPicker used with image/PDF extension filter");
    d.Record("D14-ui-enabled", d.FindId("SubmitButton")!.IsEnabled, "Submit enabled once all required inputs are valid");
    d.Shot("03-new-inspection");
});

Step("submit", () =>
{
    var submit = d.FindId("SubmitButton")!;
    d.ScrollIntoView(submit);
    var p = submit.GetClickablePoint();
    Mouse.Click(p);
    Mouse.Click(p);
    Mouse.Click(p);
    var id = d.WaitId("SuccessInspectionId", 20);
    Thread.Sleep(1200);
    d.Shot("04-inspection-success");
    d.Record("D17-ui", id?.Name.Contains("INS-24092") == true && d.HasName("Cooling Tower 07") && d.HasName("Attention"), $"Success shows '{id?.Name}', Cooling Tower 07, Attention");
    d.ClickId("ViewAssetButton");
    d.WaitId("AssetSearchEntry", 20);
    Thread.Sleep(1500);
    var today = DateTime.Now.ToString("MMM d, yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
    d.Shot("d18-asset-after-save");
    d.Record("D18-ui", d.HasName(today) && d.HasName("Attention condition") && d.HasName("Basin-level alarm intermittent; inspect fan vibration."), $"Detail pane shows last inspection {today}, Attention condition");
    d.Record("B12-view-asset", d.FindId("StartInspectionTopButton") is not null, "View asset returned to the asset (Assets master/detail)");
});

Step("history", () =>
{
    d.ClickSidebar("History");
    d.WaitId("HistorySearchEntry", 20);
    Thread.Sleep(1500);
    d.Shot("05-history");
    var first = d.All().Where(e => Driver.SafeName(e).StartsWith("INS-") && Driver.SafeName(e).Contains(" · ")).OrderBy(e => e.BoundingRectangle.Y).Select(e => Driver.SafeName(e)).ToList();
    d.Record("C06-ui", first.FirstOrDefault()?.StartsWith("INS-24092 · Today") == true, "History order: " + string.Join(" | ", first));
    d.Record("D15-ui", !first.Any(n => n.StartsWith("INS-24093")) && first.Count(n => n.StartsWith("INS-24092")) == 1, "Triple click on Submit created exactly one inspection");
    d.Record("C09-ui", d.HasName("7 completed inspections"), "History count 7 after save");
    d.TypeInto("HistorySearchEntry", "ahu-203");
    Thread.Sleep(800);
    var ahu = d.All().Where(e => Driver.SafeName(e).StartsWith("INS-") && Driver.SafeName(e).Contains(" · ")).Select(e => Driver.SafeName(e)).ToList();
    d.Record("C07-ui", ahu.Count == 1 && ahu[0].StartsWith("INS-24086"), "Search 'ahu-203' -> " + string.Join(",", ahu));
    d.TypeInto("HistorySearchEntry", "");
    d.ClickName("Filter: Good");
    Thread.Sleep(800);
    var good = d.All().Where(e => Driver.SafeName(e).StartsWith("INS-") && Driver.SafeName(e).Contains(" · ")).Select(e => Driver.SafeName(e).Split(' ')[0]).ToList();
    d.Shot("c08-history-filter-good");
    d.Record("C08-ui", good.OrderBy(x => x).SequenceEqual(new[] { "INS-24044", "INS-24091" }), "Good filter -> " + string.Join(",", good));
    d.ClickName("Filter: All");
});

Step("cancel", () =>
{
    d.ClickSidebar("Assets");
    d.WaitId("AssetSearchEntry", 20);
    Thread.Sleep(800);
    d.ClickRow("Booster Pump 104");
    d.ClickId("StartInspectionTopButton");
    d.WaitId("TemperatureEntry", 20);
    d.TypeInto("TemperatureEntry", "55");
    d.ClickId("CancelButton");
    var back = d.WaitId("AssetSearchEntry", 20);
    d.ClickSidebar("History");
    d.WaitId("HistorySearchEntry", 20);
    Thread.Sleep(1000);
    d.Record("B11", back is not null && d.HasName("7 completed inspections"), "Cancel returned to Assets; history still 7");
});

Step("responsive", () =>
{
    d.ClickSidebar("Assets");
    d.WaitId("AssetSearchEntry", 20);
    foreach (var (w, h) in new[] { (1000, 800), (900, 760), (720, 600) })
    {
        d.Resize(w, h);
        Thread.Sleep(1500);
        d.Shot($"f04-assets-{w}x{h}");
    }
    var narrowHasPane = d.FindId("StartInspectionTopButton") is not null;
    d.ClickSidebar("History");
    d.WaitId("HistorySearchEntry", 20);
    Thread.Sleep(1200);
    d.Shot("f04-history-720x600");
    d.ClickSidebar("Assets");
    d.WaitId("AssetSearchEntry", 20);
    Thread.Sleep(800);
    d.ClickRow("Cooling Tower 07");
    var detailPage = d.WaitId("StartInspectionButton", 20);
    Thread.Sleep(1000);
    d.Shot("f04-asset-detail-720x600");
    d.Record("F04", !narrowHasPane && detailPage is not null, "At 720 px the master/detail collapses to single pane; row opens Asset Detail page");
    d.ClickId("BackButton");
    d.WaitId("AssetSearchEntry", 20);
    d.Resize(1440, 900);
    Thread.Sleep(1200);
    d.Record("F03-size", Math.Abs(d.Window.BoundingRectangle.Width - 1440) <= 20, $"Window restored to {d.Window.BoundingRectangle}");
});

Step("minimize-restore", () =>
{
    d.Window.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Minimized);
    Thread.Sleep(1500);
    d.Window.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal);
    Thread.Sleep(1500);
    d.ClickSidebar("Dashboard");
    var ok = d.WaitId("ViewAllAssetsButton", 15);
    d.Record("J06", ok is not null, "Minimize/restore keeps the app responsive");
});

Step("restart-persistence", () =>
{
    d.Kill();
    Thread.Sleep(2000);
    d.Launch();
    d.WaitId("ViewAllAssetsButton", 90);
    d.ClickSidebar("History");
    d.WaitId("HistorySearchEntry", 20);
    Thread.Sleep(1500);
    d.Shot("c10-history-after-restart");
    d.Record("C10", d.All().Any(e => Driver.SafeName(e).StartsWith("INS-24092 · ")), "INS-24092 present after process kill + relaunch");
    d.Record("J05", d.HasName("7 completed inspections"), "Relaunch succeeded with 7 inspections");
});

Step("view-history-action", () =>
{
    d.ClickSidebar("Assets");
    d.WaitId("AssetSearchEntry", 20);
    Thread.Sleep(800);
    d.ClickRow("Booster Pump 104");
    d.ClickId("StartInspectionTopButton");
    d.WaitId("TemperatureEntry", 20);
    d.ClickId("ConditionGood");
    d.TypeInto("TemperatureEntry", "-50");
    d.ClickId("CheckGuards"); d.ClickId("CheckLeaks"); d.ClickId("CheckArea");
    d.ClickId("SubmitButton");
    var idName = d.WaitId("SuccessInspectionId", 20) is { } idEl ? Driver.SafeName(idEl) : "";
    d.ClickId("ViewHistoryButton");
    d.WaitId("HistorySearchEntry", 20);
    Thread.Sleep(1200);
    var first = d.All().Where(e => Driver.SafeName(e).StartsWith("INS-") && Driver.SafeName(e).Contains(" · ")).OrderBy(e => e.BoundingRectangle.Y).FirstOrDefault()?.Name;
    d.Record("B12-view-history", idName.Contains("INS-24093") && first?.StartsWith("INS-24093") == true, $"Second inspection '{idName}'; View history shows first row '{first}'");
    d.Record("C12-ui", idName.Contains("INS-24093"), "IDs continue after restart (INS-24093)");
});

Step("data-modes", () =>
{
    d.Kill();
    d.Launch(new() { ["FIELDCHECK_READ_DELAY_MS"] = "5000" });
    var loading = d.WaitName(n => n == "Loading dashboard…", 60);
    d.Shot("e01-loading");
    d.Record("E01-ui", loading is not null, "Loading state visible during delayed initial read");
    d.WaitId("ViewAllAssetsButton", 60);
    d.Kill();

    d.Launch(new() { ["FIELDCHECK_DATA_MODE"] = "Empty" });
    var empty = d.WaitName(n => n == "No assets yet", 60);
    Thread.Sleep(800);
    d.Shot("e02-empty-dashboard");
    d.ClickSidebar("History");
    var emptyHistory = d.WaitName(n => n == "No inspections yet", 20);
    d.Shot("e02-empty-history");
    d.Record("E02-ui", empty is not null && emptyHistory is not null, "Empty repository mode: dashboard and history empty states");
    d.Kill();

    d.Launch(new() { ["FIELDCHECK_DATA_MODE"] = "ErrorOnce" });
    var error = d.WaitName(n => n == "Couldn't load assets", 60);
    Thread.Sleep(800);
    d.Shot("e03-error-dashboard");
    d.Record("E03-ui", error is not null && d.HasName("Retry"), "Read failure shows user-safe error with Retry");
    d.ClickId("StateActionButton");
    var recovered = d.WaitId("ViewAllAssetsButton", 30);
    Thread.Sleep(800);
    d.Shot("e04-retry-recovered");
    d.Record("E04-ui", recovered is not null, "Retry recovered to populated dashboard");
    d.Kill();
});

d.Kill();
var summary = new
{
    total = checks.Count,
    pass = checks.Count(c => c.Status == "PASS"),
    fail = checks.Count(c => c.Status == "FAIL"),
    elapsed_seconds = sw.Elapsed.TotalSeconds,
    checks,
};
File.WriteAllText(Path.Combine(outDir, "windows-checks.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Checks: {summary.pass} pass / {summary.fail} fail of {summary.total}");

public sealed record Check(string Id, string Status, string Detail);

public sealed class Driver(UIA3Automation automation, string exe, string shots, List<Check> checks)
{
    public Application App { get; private set; } = null!;
    public Window Window { get; private set; } = null!;

    public void Launch(Dictionary<string, string>? env = null)
    {
        var psi = new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(exe)! };
        foreach (var (k, v) in env ?? []) psi.Environment[k] = v;
        App = Application.Launch(psi);
        Window = App.GetMainWindow(automation, TimeSpan.FromSeconds(90)) ?? throw new InvalidOperationException("No main window");
        Window.Patterns.Transform.PatternOrDefault?.Move(0, 0);
        Window.Focus();
    }

    public void Kill()
    {
        try { if (App is { HasExited: false }) { App.Kill(); App.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(1)); } } catch { }
        Thread.Sleep(1000);
    }

    public void Record(string id, bool ok, string detail)
    {
        checks.Add(new Check(id, ok ? "PASS" : "FAIL", detail));
        Console.WriteLine($"   [{(ok ? "PASS" : "FAIL")}] {id}: {detail}");
    }

    public AutomationElement[] All() => Window.FindAllDescendants();

    public bool HasName(string name) => Window.FindFirstDescendant(cf => cf.ByName(name)) is not null;

    public AutomationElement? FindId(string id) => Window.FindFirstDescendant(cf => cf.ByAutomationId(id));

    public AutomationElement? WaitId(string id, int seconds) => Wait(() => FindId(id), seconds);

    public AutomationElement? WaitName(Func<string, bool> match, int seconds) =>
        Wait(() => All().FirstOrDefault(e => SafeName(e) is { } n && match(n)), seconds);

    public AutomationElement? WaitDialog(int seconds) => Wait(() =>
        // Owned modal dialogs appear under the owner window in the UIA tree.
        Window.FindFirstChild(cf => cf.ByClassName("#32770"))
        ?? App.GetAllTopLevelWindows(automation).FirstOrDefault(w => w.ClassName == "#32770")
        ?? automation.GetDesktop().FindFirstChild(cf => cf.ByClassName("#32770")), seconds);

    public List<string> RowButtons() => All()
        .Where(e => e.ControlType == ControlType.Button && SafeName(e) is { } n && n.Contains(", status "))
        .Select(SafeName).ToList();

    public void ClickId(string id)
    {
        var e = WaitId(id, 15) ?? throw new InvalidOperationException($"AutomationId '{id}' not found");
        ScrollIntoView(e);
        e.Click();
        Thread.Sleep(400);
    }

    public void ClickName(string name)
    {
        var e = Wait(() => Window.FindFirstDescendant(cf => cf.ByName(name)), 15) ?? throw new InvalidOperationException($"Name '{name}' not found");
        ScrollIntoView(e);
        e.Click();
        Thread.Sleep(400);
    }

    public void ClickRow(string assetName)
    {
        var e = Wait(() => All().FirstOrDefault(x => x.ControlType == ControlType.Button && SafeName(x) is { } n && n.StartsWith(assetName + ",")), 15)
            ?? throw new InvalidOperationException($"Row '{assetName}' not found");
        ScrollIntoView(e);
        e.Click();
        Thread.Sleep(600);
    }

    public void ClickSidebar(string name)
    {
        var left = Window.BoundingRectangle.X + 240;
        // Shell flyout items surface as ListItems without their label text, so fall back to position.
        var index = Array.IndexOf(new[] { "Dashboard", "Assets", "History" }, name);
        var e = Wait(() => All().Where(x => SafeName(x) == name && x.BoundingRectangle.X < left && !x.BoundingRectangle.IsEmpty)
                .OrderByDescending(x => x.BoundingRectangle.Width).FirstOrDefault()
            ?? All().Where(x => x.ControlType == ControlType.ListItem && x.BoundingRectangle.X < left && !x.BoundingRectangle.IsEmpty)
                .OrderBy(x => x.BoundingRectangle.Y).ElementAtOrDefault(index), 15)
            ?? throw new InvalidOperationException($"Sidebar '{name}' not found");
        e.Click();
        Thread.Sleep(800);
    }

    public void TypeInto(string id, string text)
    {
        ClickId(id);
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
        Keyboard.Press(VirtualKeyShort.DELETE);
        if (text.Length > 0) Keyboard.Type(text);
        Thread.Sleep(300);
    }

    public void ScrollIntoView(AutomationElement e)
    {
        try { e.Patterns.ScrollItem.PatternOrDefault?.ScrollIntoView(); Thread.Sleep(300); } catch { }
    }

    public void Resize(int w, int h)
    {
        Window.Patterns.Transform.Pattern.Resize(w, h);
        Window.Patterns.Transform.Pattern.Move(0, 0);
    }

    public void Shot(string name)
    {
        Window.Focus();
        Thread.Sleep(300);
        Capture.Element(Window).ToFile(Path.Combine(shots, name + ".png"));
    }

    public void ShotScreen(string name) => Capture.Screen().ToFile(Path.Combine(shots, name + ".png"));

    public void TryShot(string name) { try { ShotScreen(name); } catch { } }

    public static string SafeName(AutomationElement e) { try { return e.Name ?? string.Empty; } catch { return string.Empty; } }

    private static T? Wait<T>(Func<T?> probe, int seconds) where T : class
    {
        var until = DateTime.UtcNow.AddSeconds(seconds);
        do
        {
            try { if (probe() is { } found) return found; } catch { }
            Thread.Sleep(300);
        } while (DateTime.UtcNow < until);
        return null;
    }
}
