# FieldCheck — .NET MAUI Run: Final Summary

## Completion status: **COMPLETE**

All 95 applicable acceptance criteria pass: **95 PASS · 0 FAIL · 0 NOT TESTED**. A08 is Uno-only and not applicable.

Runtime verification ran on GitHub-hosted runners after a recorded human intervention (`interventions.md`). The original container could not build or run Windows, or run an Android emulator.

The final evidence comes from CI run [36049012411](https://github.com/mtmattei/fieldcheck-maui/actions/runs/36049012411) on commit `d37afd2`:

| Target | Evidence |
|---|---|
| Windows (Server 2025, Release exe, 1440×900) | 48/48 end-to-end UI checks |
| Android (API 34 emulator, pixel_6, Release APK) | 43/43 end-to-end UI checks |
| Unit tests | 58/58 xUnit tests, locally and on the Windows runner |

## Versions

| Item | Version |
|---|---|
| .NET MAUI (Microsoft.Maui.Controls) | **10.0.110** (latest stable at run start) |
| .NET SDK | 10.0.112 (container) · 10.0.401 (CI runners) |
| .NET runtime | 10.0.12 |
| Workloads | maui-android: Android pack 36.1.69, target API 36 · maui-windows: manifest 10.0.20 |
| Android SDK (CI) | Official platforms;android-36, build-tools;36.0.0; emulator API 34 x86_64 |
| Windows | Windows Server 2025 Datacenter 10.0.26100, Windows App SDK self-contained |
| CommunityToolkit.Mvvm | 8.4.2 |
| Tests / harness | xunit 2.9.3; FlaUI.UIA3 5.0.0 (CI only) |
| Claude Code | 2.1.281; session model `claude-opus-5-5` |

## Timeline

Controller marks come from `timing.jsonl`. Actual first-success times come from the CI run records.

| Milestone | UTC | Notes |
|---|---|---|
| run_start | 15:30:46 | |
| capability_discovery_complete | 15:35:32 | |
| first_source_edit (K01) | 15:37:47 | +7:01 |
| first_build_attempt (K02) | 15:40:51 | +10:05 |
| first_android_build_success (K03) | 15:56:50 | Composed SDK in the container. First official-SDK build: CI run 1, 17:45 |
| verification_start | 15:57:53 | |
| run_finish (BLOCKED) | 16:00:14 | Container could not launch either target |
| human_intervention_ci_runners_available / run_resumed | 17:42:07 | Waiting time 16:00→17:42 was not agent work |
| first Windows build success (K03) | ~18:10 (CI run 3) | Controller mark written later at 19:17:16 |
| first Windows + Android launch success (K04) | ~18:33 (CI run 4) | Controller marks written later at 19:17:16 |
| core_complete (K05) | 19:46:29 | Final all-green CI run |
| run_finish | see the last `timing.jsonl` entry | |

Agent working wall-clock: 29:28 in phase 1, plus 2:04 from resume to core_complete in phase 2.

## Builds, tests, defects

| Metric | Result |
|---|---|
| Build/test invocations (K09) | Container: 15, 4 failed. CI: 11 runs, 3 cancelled. Windows builds 1 failed / 8 succeeded; Android builds 9/9 succeeded. |
| Build time (K13) | Android Release 117 s (CI) / ~2 min (container). Windows Release 103 s (CI). |
| Startup (K13) | Android `am start` TotalTime ≈ 2.3 s cold, ≈ 1.1–1.8 s relaunch. Windows greeting visible ≈ 2–3 s after process start. |
| Runtime defects (K10) | 5 found only by running the app. Crash at startup (both platforms). Android `FlexLayout` arrange crash. Android tab bar visible on pushed pages. Windows native navigation pill shown beside the Accent marker. Windows App Runtime dependency. All fixed and re-verified. |
| Known remaining defects | None. The visual differences listed in `visual-review.md` are deliberate (spec-required content, native accessibility) or cosmetic. |

## Complexity (K12)

- App + core: about 2.1k lines of C# and 1.1k lines of XAML. Tests: 764 lines.
- CI harness: Windows FlaUI driver plus the Android Python driver.
- Runtime packages: 2 (Microsoft.Maui.Controls, CommunityToolkit.Mvvm).

## Architecture

MVVM (CommunityToolkit.Mvvm) with a MAUI-free `FieldCheck.Core` library holding models, the JSON repository and all view models. The repository seeds once from the fixtures, writes atomically and commits in memory only after the write succeeds.

The MAUI app uses Shell:

- **Android**: bottom tabs with a custom Accent indicator renderer.
- **Windows**: a locked 232 px flyout as the sidebar.
- **Adaptive layout**: Assets master/detail at ≥ 1000 px window width; two-column inspection form; History table on wide windows.
- **Platform services**: Shell navigation, MAUI FilePicker (copying the pick into app storage), and a packaged seed reader.

Details are in `implementation-notes.md`.

## Tools / MCP / skills used

- **Microsoft Learn MCP**: MAUI unit-testing pattern, FilePicker semantics, Shell tab bar visibility.
- **GitHub MCP**: workflow run and job status.
- **Claude Code remote docs**: network policy guidance.
- **CLI**: dotnet CLI (template, workload install/repair, build, test).
- **CI**: GitHub Actions with windows-latest, and ubuntu-latest with KVM running reactivecircus/android-emulator-runner.
- **UI drivers**: FlaUI/UIA3; adb + uiautomator.
- **Other sources**: Ubuntu apt and Maven Central, for the phase-1 composed SDK only.
- **Skills**: none invoked. No MAUI-specific skill is installed, and Uno skills/MCP are excluded by the run rules.

## Token usage / cost

Not available to the agent: session tools expose no token or cost counters. **Attach external session telemetry** as `results/external-session-usage.txt` (see `METRICS_CAPTURE.md`). No estimate is given.

## Result files

- **Run records**: `results/environment.json`, `capabilities.md`, `timing.jsonl`, `interventions.md`
- **Acceptance**: `results/acceptance.json`, `acceptance.md`
- **Review and notes**: `results/visual-review.md`, `implementation-notes.md`, `dependencies.txt`
- **Screenshots**: `results/screenshots/android/*.png` (412×915, plus `-1080x2400` originals) and `results/screenshots/windows/*.png` (1440×900)
- **Raw CI output**: `results/ci/{android,windows}/` (check JSON, driver logs, build logs, logcat, event log, UI dumps)
- **App and harness**: application in `app/FieldCheck.sln`; CI harness in `ci/` and `.github/workflows/maui-verify.yml`
