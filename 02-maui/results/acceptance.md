# FieldCheck MAUI — Acceptance Results

Status: **BLOCKED** (environment). PASS 50 · FAIL 0 · NOT TESTED 45 (of 95).

PASS entries are backed by builds, automated tests or deterministic code checks. Criteria needing a rendered UI, a platform picker, a device/emulator or Windows are NOT TESTED, with the reason.

| ID | Status | Evidence / reason |
|---|---|---|
| A01 | PASS | All projects target .NET 10 (net10.0, net10.0-android; net10.0-windows10.0.19041.0 on Windows hosts); Microsoft.Maui.Controls 10.0.110. Verified by builds. |
| A02 | PASS | results/environment.json records SDK 10.0.112, runtime 10.0.12, MAUI 10.0.110, maui-android workload 10.0.20 manifest, Android pack 36.1.69. |
| A03 | PASS | `dotnet build -f net10.0-android -c Release` succeeded: 0 errors, 0 warnings, signed APK + AAB produced (targetSdk 36). CAVEAT: official SDK host is blocked, so the SDK was composed: platforms/android-36/android.jar = org.robolectric:android-all:16-robolectric-13921718 (full API 36 framework incl. resources.arsc), build-tools = Ubuntu 29.0.3. Re-run with the official SDK before publication. |
| A04 | NOT TESTED | Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| A05 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| A06 | NOT TESTED | Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| A07 | PASS | Warnings recorded: Core (TreatWarningsAsErrors) 0; MAUI net10.0 compile incl. XAML SourceGen 0; Android Release 0; tests 0. |
| B01 | NOT TESTED | DashboardPage implemented and XAML-compiled; rendering vs reference not verifiable. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B02 | NOT TESTED | AssetsPage implemented and compiled; not rendered. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B03 | NOT TESTED | AssetDetailPage/AssetDetailView implemented and compiled; not rendered. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B04 | NOT TESTED | NewInspectionPage implemented and compiled; not rendered. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B05 | NOT TESTED | InspectionSuccessPage implemented and compiled; not rendered. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B06 | NOT TESTED | HistoryPage implemented and compiled; not rendered. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B07 | NOT TESTED | Shell bottom tabs (Dashboard/Assets/History) implemented. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B08 | NOT TESTED | Shell push stack assetdetail -> newinspection; header back and system back pop. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B09 | NOT TESTED | Shell locked flyout sidebar on WinUI implemented. Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| B10 | NOT TESTED | AssetsPage switches to 460px master + detail pane at content width >= 760 (≈1000px window). VM logic PASS in tests; layout Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| B11 | NOT TESTED | VM test Cancel_returns_without_saving PASS (navigates back, 6 inspections persisted). Runtime navigation not verifiable. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| B12 | NOT TESTED | VM test: ViewAsset -> GoBack (form already replaced by success), ViewHistory -> //main/history. Shell execution not verifiable. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| C01 | PASS | Test Seeds_all_twelve_assets_and_six_inspections_from_fixtures. |
| C02 | PASS | Test Dashboard_counts_are_derived_from_data (12/7/3/2, then 8/3/1 after a Good inspection on CT-007). |
| C03 | PASS | Theory Assets_search_matches_name_id_type_location (case-insensitive name, ID, type, location). |
| C04 | PASS | Test Assets_status_filter_and_search_combine (Operational 7, Attention 3, Critical 2). |
| C05 | PASS | Same test: Critical + 'roof' -> CT-007; Critical + 'pump' -> NoResults. |
| C06 | PASS | Tests Inspections_are_newest_first and History_is_newest_first_searchable_and_filterable. |
| C07 | PASS | History search by asset name ('booster') and ID ('cnv-018'); location is not matched. |
| C08 | PASS | History Good/Critical filters verified. |
| C09 | PASS | Added_inspection_persists_across_restart_and_updates_asset writes the JSON data file to disk via FileDataStore (atomic temp+move). |
| C10 | NOT TESTED | Data-layer restart (new repository instance over the same storage file) PASS in tests; true process kill + relaunch on a device not possible. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| C11 | PASS | Saving_does_not_mutate_fixture_files compares SHA-256 of the fixtures before/after a save; fixtures are bundled read-only (MauiAsset) and copied to app storage. |
| C12 | PASS | Generated_ids_are_unique_sequential_and_stable_after_restart (INS-24092, 24093, then 24094 after restart). |
| D01 | PASS | Condition_supports_good_attention_critical. |
| D02 | PASS | OperatingNormally Yes/No toggled in Issue_description_visibility tests. |
| D03 | PASS | Temperature theory: -50, 0, 27.5, -4,5, 250 accepted. |
| D04 | PASS | -50.1, 250.01, 300, abc, empty, 1e3 rejected with a message; submit disabled (VM level; message rendering not verified). |
| D05 | PASS | Each_checklist_item_is_individually_required (theory over the three items). |
| D06 | PASS | Multiline notes persisted verbatim ('Line one\nLine two'); Editor control in UI. |
| D07 | NOT TESTED | MauiFilePickerService uses FilePicker.Default.PickAsync (Android SAF / Windows FileOpenPicker). Platform picker cannot be opened here. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| D08 | NOT TESTED | VM shows filename + thumbnail binding; tested with a fake picker. Platform selection of inspection-photo.png not possible. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| D09 | NOT TESTED | VM: null result (cancel), TaskCanceledException and failures leave form usable (test PASS). Platform picker cancel not exercised. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| D10 | PASS | Good + Yes -> hidden (theory). |
| D11 | PASS | Attention/Critical -> shown (theory). |
| D12 | PASS | Operating normally No -> shown (theory). |
| D13 | PASS | Issue_description_is_required_when_shown (whitespace rejected). |
| D14 | PASS | SubmitCommand.CanExecute false until condition, temperature, 3 checklist items and (when shown) issue description are valid. |
| D15 | PASS | Repeated_activation_during_save_cannot_duplicate: CanExecute false while running + interlocked gate; 3 activations -> 1 inspection. |
| D16 | PASS | Submit_creates_exactly_one_inspection_and_navigates_to_success (count 6 -> 7). |
| D17 | PASS | Success VM exposes INS-24092, Cooling Tower 07, Attention, 'Sep 23, 2026 · Alex Morgan' (VM test; page rendering not verified). |
| D18 | PASS | Asset_detail_and_success_reflect_the_new_inspection: status Attention, last inspection Sep 23, 2026, latest summary = issue description. |
| E01 | PASS | All list/detail VMs start in Loading and show a StatePanel spinner; FIELDCHECK_READ_DELAY_MS makes it observable. VM state tested; rendering not verified. |
| E02 | PASS | DataSourceMode.Empty -> ViewState.Empty on Dashboard/Assets/History (tests). |
| E03 | PASS | DataSourceMode.Error -> ViewState.Error with user-safe message (tests). |
| E04 | PASS | Assets_error_state_retries_and_recovers (ErrorOnce -> Retry -> 12 assets). |
| E05 | PASS | NoResults distinct from Empty (Assets_empty_repository_is_distinct_from_no_results; search no-match -> NoResults with Clear action). |
| E06 | PASS | MissingRequirements/RequirementsSummary list what to fix; per-field messages for temperature, checklist, issue description. |
| E07 | NOT TESTED | CT-007 description is in a wrapping Label inside a ScrollView (WordWrap, max width 620). Rendering not verifiable. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| E08 | PASS | Persistence_failure_reports_error_and_keeps_form + Write_failure_throws_and_leaves_data_unchanged: no navigation to success, entries kept, retry succeeds. |
| F01 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| F02 | NOT TESTED | Form is in a ScrollView; Android WindowSoftInputModeAdjust.Resize set. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| F03 | NOT TESTED | Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| F04 | NOT TESTED | Adaptive rules implemented (master/detail >=760 content px, History table >=900, form two-column >=900, 720px min window). Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| F05 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| F06 | NOT TESTED | Shell back stack; success replaces form so back returns to asset. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| F07 | NOT TESTED | All interactive rows are real Buttons (focusable), inputs are native. Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| F08 | NOT TESTED | Android image/* + application/pdf MIME types; Windows extension list. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| G01 | NOT TESTED | SemanticProperties.Description/Hint set on icon buttons, row activators, chips, inputs, switch, checkboxes. Accessibility tree not inspectable without a running target. |
| G02 | PASS | Every status is shown via StatusChip text or a text label ('Critical condition'); tone color is never the only signal (code + VM Tone/Text pairs). |
| G03 | NOT TESTED | Native WinUI focus visuals kept on buttons; FocusFrameBehavior thickens the input frame on focus. Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| G04 | NOT TESTED | Button styles set MinimumHeight/Width 44; primary buttons 54; icon buttons 44x44; checkboxes min 44. Not measured at runtime. |
| G05 | NOT TESTED | Errors are text directly below the field plus a red frame, and listed in the submit summary. Rendering not verified. |
| G06 | PASS | Computed WCAG ratios: Ink/Canvas 15.8, Muted/Canvas 4.7, Ink/Accent 15.6, Success chip 5.7, Critical chip 5.5, Attention chip 4.3 (supplied palette), Surface/Ink 17.7. |
| H01 | NOT TESTED | No screenshot possible. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| H02 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| H03 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| H04 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| H05 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| H06 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| I01 | PASS | CommunityToolkit.Mvvm view models in FieldCheck.Core; pages only bind and forward query parameters/size changes. |
| I02 | PASS | IFieldCheckRepository/JsonFieldCheckRepository/IDataFileStore in Core; views never touch persistence. |
| I03 | PASS | Validation, filtering, state and submission live in VMs; code-behind limited to layout breakpoints, query forwarding and OnAppearing refresh. |
| I04 | PASS | No dead code or duplicate implementations (reviewed; template MainPage/fonts/iOS/Mac folders removed). |
| I05 | PASS | No test shortcuts; data modes are injectable repository behavior with no UI switch (env var only). |
| I06 | PASS | Runtime deps: Microsoft.Maui.Controls 10.0.110, CommunityToolkit.Mvvm 8.4.2. Test deps: xunit 2.9.3, runner 3.1.5, Test.Sdk 18.10.1. Recorded in dependencies.txt. |
| I07 | PASS | 58 xUnit tests over repository and all six view models (dotnet test: 58 passed, 0 failed). |
| I08 | PASS | No tests removed or weakened; all passed on first execution. |
| J01 | PASS | grep: no lorem/TODO/placeholder content; no debug UI. |
| J02 | PASS | All 22 Buttons and 2 ImageButtons bound to commands (StatePanel action hidden when no command); commands covered by VM tests. |
| J03 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| J04 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| J05 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| J06 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| J07 | NOT TESTED | VM cancel/failure paths PASS in tests; platform picker not exercisable. No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. |
| J08 | PASS | No secrets/credentials in source; INTERNET permission removed; APK signed with the SDK-generated debug key (not committed). |
| J09 | NOT TESTED | No Android emulator/device (dl.google.com blocked, no KVM) and no Windows host in this environment; runtime UI cannot be launched. Windows target cannot be built or run on this Linux host (maui-windows workload / WinUI XAML compiler are Windows-only). |
| J10 | NOT TESTED | Android Release build produced, but no release runtime verification was possible. |

K-metrics are reported in `final-summary.md`.
