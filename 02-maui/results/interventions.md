# Human Interventions

## 1. GitHub Actions runners made available (2026-09-24 ~17:40 UTC)

**Recorded in timing.jsonl:** `human_intervention_ci_runners_available` and `run_resumed` at 17:42:07 UTC. The run had stopped as BLOCKED at `run_finish`, 16:00:14 UTC.

**What the human said:** the repository can run GitHub Actions. GitHub-hosted runners provide `windows-latest` and Linux runners with KVM for the Android emulator, plus the official Android SDK. The agent may use them to build, launch and verify the Windows and Android targets.

**Why it was needed:** the original Claude Code container could not build or run WinUI (Linux host). It also could not download the official Android SDK or emulator (`dl.google.com` denied) and had no `/dev/kvm`.

**How it was used:**
- `.github/workflows/maui-verify.yml` runs on each push to the run branch. It has two jobs:
  - `windows`: `windows-latest`. Installs the `maui` workload, runs the xUnit tests, builds a Release `net10.0-windows10.0.19041.0` win-x64 build, then runs the FlaUI UI driver in `02-maui/ci/windows`.
  - `android`: `ubuntu-latest` with KVM. Uses the official SDK (`platforms;android-36`, `build-tools;36.0.0`) and the `maui-android` workload to build the Release APK. Then it runs the adb/uiautomator driver in `02-maui/ci/android` on an API 34 x86_64 `pixel_6` emulator.
- Each job force-pushes its logs, screenshots and check JSON to a results branch (`maui-ci-windows`, `maui-ci-android`). The agent fetches those branches and reads them.
- CI time is part of the measured run. Wall-clock between 16:00:14 (BLOCKED finish) and 17:42:07 (resume) was spent waiting for the human and is not agent working time.

**Effect on comparability:** runtime verification now happens on GitHub-hosted runners instead of the agent's own machine. On CI, the .NET SDK is the runner's 10.0.4xx band, not the container's 10.0.112. The MAUI package version (10.0.110, pinned by PackageReference) is unchanged.
