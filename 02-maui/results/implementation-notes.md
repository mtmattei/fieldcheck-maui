# Implementation Notes — FieldCheck (.NET MAUI 10)

## Solution layout (`app/`)

```
FieldCheck.sln
src/FieldCheck.Core/        net10.0 — models, repository, view models (no MAUI dependency)
src/FieldCheck/             MAUI app — net10.0-android (+ net10.0-windows10.0.19041.0 on Windows hosts)
tests/FieldCheck.Tests/     xUnit, net10.0 — repository + view-model tests over the real fixtures
```

## Architecture

- **MVVM**: CommunityToolkit.Mvvm source generators (partial `[ObservableProperty]`, `[RelayCommand]`). View models live in Core, so they are testable without a device. Views bind with compiled bindings (`x:DataType`) and XAML source generation (`MauiXamlInflator=SourceGen`, strict compilation).
  - Decision: split Core from the app. Reason: view-model and persistence tests run on any host, which proved decisive here. Tradeoff: navigation, file picking and the seed source are behind small interfaces (`INavigationService`, `IFilePickerService`, `ISeedDataSource`, `IDataFileStore`, `IClock`).
- **Persistence**: `JsonFieldCheckRepository` holds one JSON document (`fieldcheck-data.json` in `FileSystem.AppDataDirectory`). On first run it seeds from the bundled fixtures (`MauiAsset` links to `mock-data/*.json`, which are never written). Writes are atomic (temp file then move). New state is adopted in memory only after the write succeeds, so a failed save cannot report success or leave memory ahead of disk. JSON uses System.Text.Json source generation, which is trim-safe for Android Release.
- **IDs**: `INS-` + (highest numeric suffix + 1). The first new record is INS-24092. IDs persist, so they stay stable and unique.
- **Asset update on save**: status maps Good→Operational, Attention→Attention, Critical→Critical, and `lastInspection` becomes today.
- **Deterministic states**: `DataSourceOptions` (Normal / Empty / Error / ErrorOnce, plus optional read delay) is injected into the repository. The app reads `FIELDCHECK_DATA_MODE` and `FIELDCHECK_READ_DELAY_MS` from the environment at startup. No debug UI exists.
- **Navigation**: one `Shell` with a `FlyoutItem` (AsMultipleItems) of three tabs.
  - Android: bottom tab bar, flyout disabled. A custom `ShellRenderer` bottom-nav tracker draws the small Accent indicator.
  - Windows: locked 232 px flyout used as the sidebar, with a custom item template (Accent marker plus Canvas surface when selected), the product mark header and the technician footer. The tab bar is hidden.
  - Detail routes are pushed (`assetdetail`, `newinspection`). Success replaces the form (`../inspectionsuccess`), so both system back and "View asset" return to the asset, never to a submitted form.
- **Adaptive layout** (logical px of page content):
  - Assets becomes master/detail at ≥ 760, about a 1000 px window minus the sidebar.
  - The inspection form uses two columns at ≥ 900.
  - The History table appears at ≥ 900, otherwise stacked rows.
  - The Windows minimum window is 720×560.
  - Layout switching is view-only code-behind; the VM receives only `IsWide`.
- **Rows**: CollectionView with `SelectionMode=None` and a transparent full-row `Button` as the activation target. Rows are focusable, keyboard-activatable and screen-reader-labelled. WinUI list selection would follow focus and navigate on arrow keys.
- **File picker**: `FilePicker.Default.PickAsync` with image/PDF types. The picked file is copied into app storage so the path outlives the picker grant. Cancel (null), superseded picker (`TaskCanceledException`) and failures are handled in the VM, which keeps the form usable.
- **Duplicate submission**: `AsyncRelayCommand` disables `CanExecute` while running, plus an `Interlocked` gate for activations that arrive before the UI observes the change.
- **Inputs**: native Entry/Editor chrome (Android underline, WinUI border) is removed through handler mappings so only the FieldCheck frame shows. `FocusFrameBehavior` draws a 2 px Ink frame on focus to keep a visible keyboard focus indicator.

## Build/verification log (chronological)

Times for #1 and #14 match `timing.jsonl` milestones; other times are approximate (session order, minute precision).

| # | Time (UTC) | Command | Result |
|---|---|---|---|
| 1 | 15:40:51 | build Core | success |
| 2 | 15:42 | build Core (FilterOption) | success |
| 3 | 15:44 | build Core (AssetItem observable) | success |
| 4 | 15:47 | build app `-f net10.0` | **failed**: `Layout` class name clashed with `VisualElement.Layout`; missing `Microsoft.Maui.Layouts` using |
| 5 | 15:47 | build app `-f net10.0` | success (XAML SourceGen, 0 warnings) |
| 6 | 15:48 | rebuild `--no-incremental` to confirm SourceGen output | success |
| 7 | 15:48 | Android Release | **failed**: XA5300, no Android SDK directory |
| 8 | 15:49 | Android Release with Ubuntu API 23 jar | **failed**: APT2260, AndroidX values-v31 resources not in API 23 |
| 9 | 15:49 | Android `-t:Compile` | success (managed Android assembly incl. renderer) |
| 10 | 15:51 | test project | **failed**: missing global `using Xunit` |
| 11 | 15:52 | `dotnet test` | 58/58 passed |
| 12 | 15:53 | build app `-f net10.0` (focus behavior) | success |
| 13 | 15:54 | net10.0 + Android compile | success |
| 14 | 15:56 | Android Release with API 36 framework jar | **success**: 0 warnings, signed APK + AAB |
| 15 | 15:57 | `dotnet test` | 58/58 passed |

Totals: 15 build/test invocations, 4 failed (1 code, 1 test-project config, 2 environment/SDK). Runtime launches: 0 (impossible in this environment).

## Corrective iterations

1. `AssetsViewModel` selection-suppression flag replaced with an "already loaded" check before writing any build. Review-only fix.
2. `Layout` helper renamed to `Breakpoints` (build error #4).
3. Behaviors set through a Style setter (a read-only collection, unreliable at runtime) moved to handler mappings.
4. `MauiAsset` LogicalName backslashes changed to `/` so the APK asset path is `assets/seed/*.json`. Verified in the APK listing.

## Significant environment issues

- The MAUI workload install garbage-collected its own net10 packs (user-local install over the distro SDK). Fixed with `dotnet workload repair`.
- Network policy denies `dl.google.com`, so there is no official Android SDK, emulator or platform-tools. The build SDK was composed from Maven Central (`org.robolectric:android-all` API 36) and Ubuntu packages. Maven Central rate-limited (HTTP 429) twice before the download succeeded.
- `/dev/kvm` is absent, so no Android emulator could run even with SDK access.
- The Linux host cannot build or run WinUI.

## Documentation / MCP usage

- Microsoft Learn MCP: 3 searches (MAUI unit-testing TFM pattern, FilePicker semantics, what's new in MAUI 10).
- nuget.org flat-container API: latest stable versions.
- Claude Code remote docs: environment network page (blocked-host guidance).
- No Uno-specific skills/MCPs used (forbidden for this run). No installed MAUI-specific skill exists.
