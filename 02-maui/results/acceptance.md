# FieldCheck MAUI — Acceptance Results

Status: **COMPLETE**. PASS 95 · FAIL 0 · NOT TESTED 0 (of 95; A08 is Uno-only and not applicable).

Runtime evidence comes from the final CI run on commit `d37afd2`:

- Run: https://github.com/mtmattei/fieldcheck-maui/actions/runs/36049012411
- Windows: 48/48 end-to-end checks.
- Android: 43/43 end-to-end checks.
- Unit tests: 58/58.
- Raw check files, logs and UI dumps: `results/ci/`.

Runtime verification uses GitHub-hosted runners, made available by a recorded human intervention (`interventions.md`).

| ID | Status | Evidence |
|---|---|---|
| A01 | PASS | net10.0 / net10.0-android / net10.0-windows10.0.19041.0; Microsoft.Maui.Controls 10.0.110. Built on CI and locally. |
| A02 | PASS | results/environment.json records the container SDK 10.0.112 plus the CI SDK 10.0.401, workloads maui-android 36.1.69 and maui-windows 10.0.20, and runner OS versions. |
| A03 | PASS | CI ubuntu-latest with the official Android SDK (platforms;android-36, build-tools;36.0.0): `dotnet build -f net10.0-android -c Release`, 0 warnings / 0 errors, signed APK + AAB, 117 s. Run https://github.com/mtmattei/fieldcheck-maui/actions/runs/36049012411. |
| A04 | PASS | CI windows-latest (Server 2025): `dotnet build -f net10.0-windows10.0.19041.0 -c Release`, 0 warnings / 0 errors, 103 s. First success in run 3; final in https://github.com/mtmattei/fieldcheck-maui/actions/runs/36049012411. |
| A05 | PASS | Android emulator driver (results/ci/android/android-checks.json): Release APK installed on an API 34 pixel_6 emulator; Dashboard shown ('Good evening'), am start TotalTime 2269 ms. |
| A06 | PASS | Windows CI driver (results/ci/windows/windows-checks.json): Release FieldCheck.exe launched at 1440x900; Dashboard greeting visible 2-3 s after launch. |
| A07 | PASS | Warnings: 0 in Core, MAUI net10.0 + XAML SourceGen, Android Release (CI), Windows Release (CI) and tests. |
| B01 | PASS | Dashboard rendered on both targets (screenshots 01-dashboard) with the reference hierarchy: greeting, 12-asset metric, 3 counts, rule, Needs attention rows, Accent CTA. |
| B02 | PASS | Assets rendered and driven on both targets (android 02-assets, windows 02-assets-master-detail). |
| B03 | PASS | Asset Detail rendered (android 03-asset-detail; windows detail pane + f04-asset-detail-720x600 page). |
| B04 | PASS | New Inspection rendered and filled on both (04-new-inspection / 03-new-inspection); asset identity line verified. |
| B05 | PASS | Success rendered on both (05-inspection-success / 04-inspection-success) showing INS-24092. |
| B06 | PASS | History rendered on both (06-history / 05-history). |
| B07 | PASS | Android emulator driver (results/ci/android/android-checks.json): bottom tabs reached Assets, History and Dashboard (B07, B07-history, B07-dashboard). |
| B08 | PASS | Android emulator driver (results/ci/android/android-checks.json): system back from New inspection -> Asset detail -> Assets; app not exited (B08, F06). |
| B09 | PASS | Windows CI driver (results/ci/windows/windows-checks.json): sidebar navigated to Dashboard, Assets and History repeatedly. |
| B10 | PASS | Windows CI driver (results/ci/windows/windows-checks.json): at 1440x900 the Assets list is on the left and the CT-007 detail pane (with Start inspection) on the right. |
| B11 | PASS | Cancel (Windows Cancel button; Android header back) returned without saving; history stayed at 7 on both. Unit test also PASS. |
| B12 | PASS | View asset returned to the asset showing the new status/date on both targets; View history opened History with the new record first (Windows, INS-24093). |
| C01 | PASS | Unit test (12 seeded assets); UI shows '12 equipment records' on both targets. |
| C02 | PASS | Unit test (counts change after an inspection); UI shows 12/7/3/2 on both. |
| C03 | PASS | UI on both: 'roof' -> AHU-203, CT-007, FAN-305; unit theory covers name/ID/type/location case-insensitively. |
| C04 | PASS | Windows UI: Attention filter -> AHU-203, CNV-018, BLR-002; unit test covers all statuses. |
| C05 | PASS | UI on both: 'roof' + Critical -> CT-007. |
| C06 | PASS | UI on both: INS-24092 (Today) first, then seed records newest-first. |
| C07 | PASS | UI: 'booster' -> INS-24091 (Android), 'ahu-203' -> INS-24086 (Windows). |
| C08 | PASS | UI: Critical -> INS-24072 (Android); Good -> INS-24091, INS-24044 (Windows). |
| C09 | PASS | UI on both: History shows 7 completed inspections after submit. |
| C10 | PASS | Runtime on both: force-stop (Android) / process kill (Windows) + relaunch, INS-24092 still present with 7 inspections. |
| C11 | PASS | Unit test hashes the fixtures before/after save. Fixtures are packaged read-only and copied to app storage. |
| C12 | PASS | INS-24092 then INS-24093 after restart (Windows runtime); unit test covers uniqueness and persistence. |
| D01 | PASS | Good/Attention/Critical selectable on both (UI) + unit test. |
| D02 | PASS | Operating normally switch toggled Yes/No on both. |
| D03 | PASS | 27 accepted on both; -50 accepted in the Windows second inspection; unit theory covers both bounds. |
| D04 | PASS | 300 °C shows 'Temperature must be between -50 and 250 °C.' with submit disabled on both (d04 screenshots). |
| D05 | PASS | Windows: two of three checked keeps submit disabled; unit theory covers each item. |
| D06 | PASS | Windows UI read back 'Line one\nLine two'; Android UI dump shows the two-line notes value. |
| D07 | PASS | System picker opened: Windows FileOpenPicker (d07-file-picker-open), Android DocumentsUI (d07-picker-open). |
| D08 | PASS | Selecting mock-data/inspection-photo.png showed 'inspection-photo.png' plus a thumbnail on both. |
| D09 | PASS | Picker cancel (Escape / Back) left the form intact with no attachment on both. |
| D10 | PASS | Good + Yes hides Issue description on both (UI) + unit theory. |
| D11 | PASS | Attention shows Issue description on both. |
| D12 | PASS | Operating normally No shows Issue description on both. |
| D13 | PASS | Unit test; UI requirement summary lists 'Describe the issue' (Windows E06). |
| D14 | PASS | Submit disabled on the empty form and enabled once valid on both. |
| D15 | PASS | Triple click/tap on Submit created exactly one inspection on both (no INS-24093 in that run). |
| D16 | PASS | History grew from 6 to 7 after one submit on both. |
| D17 | PASS | Success shows INS-24092, Cooling Tower 07, Attention, date · Alex Morgan on both. |
| D18 | PASS | After View asset: last inspection = today, 'Attention condition', and the issue text shown on both. |
| E01 | PASS | Windows runtime with FIELDCHECK_READ_DELAY_MS=5000 shows 'Loading dashboard…' (e01-loading). VM starts in Loading (unit test). |
| E02 | PASS | Windows runtime FIELDCHECK_DATA_MODE=Empty: dashboard 'No assets yet' and history 'No inspections yet' (e02 screenshots). |
| E03 | PASS | Windows runtime ErrorOnce: 'Couldn't load assets' with a user-safe message and Retry (e03-error-dashboard). |
| E04 | PASS | Retry recovered to the populated dashboard (e04-retry-recovered); unit test also PASS. |
| E05 | PASS | 'No matching assets' with a Clear action on both (e05 screenshots); distinct from the Empty state. |
| E06 | PASS | Per-field messages and the submit summary ('To submit: …') shown on Windows; unit tests. |
| E07 | PASS | CT-007 long description wraps without clipping in the Android detail page and the Windows detail pane. |
| E08 | PASS | Unit tests: a write failure reports an error, keeps the entries and does not navigate; retry succeeds. Not inducible on runners without fault injection. |
| F01 | PASS | Android pixel_6 = 1080x2400 @420 dpi = 411x914 dp (412x915-class); all six views captured without clipping. |
| F02 | PASS | Android: keyboard shown while typing; after dismissal every field and Submit reachable by scrolling. |
| F03 | PASS | Windows 1440x900 capture matches the reference composition (sidebar, 460 px master, detail pane). |
| F04 | PASS | Windows resized to 1000x800, 900x760, 720x600: at 720 the master/detail collapses to single pane; History switches to stacked rows (f04 screenshots). |
| F05 | PASS | Android font_scale 1.3: dashboard and full form reachable (f05 screenshots). Windows text scaling not separately exercised. |
| F06 | PASS | Android system back follows the navigation stack; success replaces the form. |
| F07 | PASS | Windows Tab moves focus into app controls (sidebar items, rows, inputs); driver interactions use keyboard input. |
| F08 | PASS | Windows FileOpenPicker with an extension filter; Android SAF/DocumentsUI with MIME filter; both return a copy in app storage. |
| G01 | PASS | UIA names / content-desc verified in dumps: row buttons ('Cooling Tower 07, CT-007, …, status Critical'), chips ('Filter: Critical'), 'Attach photo or file', 'Cancel inspection and go back', checkbox names. |
| G02 | PASS | Status chips always carry text ('Status: Critical' semantic plus visible label). |
| G03 | PASS | Windows focus rectangle clearly visible (g03-keyboard-focus-dashboard); inputs thicken to a 2 px Ink frame on focus. |
| G04 | PASS | Android dumps: checkboxes 116x116 px, header icon 116x116 px, condition buttons 116 px tall (= 44 dp at 420 dpi); primary buttons 54 dp. |
| G05 | PASS | Validation text sits directly under the field in Critical color with a red frame (d04 screenshots); not color-only. |
| G06 | PASS | Computed contrast: Ink/Canvas 15.8, Muted/Canvas 4.7, Ink/Accent 15.6, chips 5.7/5.5, Attention chip 4.3 (supplied palette). |
| H01 | PASS | results/screenshots/{android,windows}/01-dashboard.png; differences in visual-review.md. |
| H02 | PASS | android/02-assets.png, windows/02-assets-master-detail.png. |
| H03 | PASS | android/03-asset-detail.png, windows/02-assets-master-detail.png (detail pane). |
| H04 | PASS | android/04-new-inspection.png (+ -lower), windows/03-new-inspection.png. |
| H05 | PASS | android/05-inspection-success.png, windows/04-inspection-success.png. |
| H06 | PASS | android/06-history.png, windows/05-history.png. |
| I01 | PASS | CommunityToolkit.Mvvm view models in FieldCheck.Core; pages only bind. |
| I02 | PASS | Repository/data store in Core; views never touch persistence. |
| I03 | PASS | Validation and state live in VMs; code-behind is limited to layout breakpoints, query forwarding and refresh-on-appear. |
| I04 | PASS | No dead or duplicate implementations; FlexLayout variants were replaced, not kept. |
| I05 | PASS | No test shortcuts; data modes are env-var repository options with no UI. The CI drivers exercise the real Release binaries. |
| I06 | PASS | 2 runtime packages; test/CI-only: xunit stack and FlaUI.UIA3 5.0.0 (harness, not shipped). See dependencies.txt. |
| I07 | PASS | 58 xUnit tests pass locally and on the Windows runner; plus 48 Windows and 43 Android end-to-end UI checks. |
| I08 | PASS | No tests weakened. Driver fixes corrected wrong selectors, each backed by a UI dump or screenshot, and never relaxed an app expectation. |
| J01 | PASS | No placeholder/debug UI in any screenshot. |
| J02 | PASS | Every button exercised by the drivers or bound to a tested command (Remove attachment covered by unit test). |
| J03 | PASS | Final run: Android crash buffer empty; Windows driver completed all steps with no process exit. Two crashes found earlier were fixed (see implementation-notes). |
| J04 | PASS | Fresh runner/emulator: first run seeded data and reached the Dashboard on both. |
| J05 | PASS | Kill + relaunch preserved the submitted inspection on both. |
| J06 | PASS | Android HOME + resume kept the form input; Windows minimize/restore stayed responsive. |
| J07 | PASS | Picker cancel on both at runtime; failed/superseded picker paths covered by unit tests. |
| J08 | PASS | No secrets; INTERNET permission removed; CI uses only the built-in GITHUB_TOKEN. |
| J09 | PASS | Primary workflow Dashboard -> Assets -> Detail -> Start -> validate/submit -> Success -> View asset -> History -> relaunch executed end-to-end on both targets. |
| J10 | PASS | All runtime verification used Release builds (APK from bin/Release, exe from bin/Release). |
