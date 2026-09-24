# Capability Discovery — FieldCheck (.NET MAUI run)

Discovery performed 2026-09-24 15:30–15:45 UTC, before any application source was written.

## Agent Skills (installed)

| Skill | Relevant? | Use |
|---|---|---|
| `anthropic-skills:dotnet-csharp` | Yes (generic .NET/C#) | General C# guidance (async, nullability, disposal). |
| `anthropic-skills:ui-craft` | Partially | Generic UI critique method; the reference PNGs are normative, so no redesign use. |
| `anthropic-skills:*uno*`, `winui-xaml`, `mvux`, `xaml-*` | Excluded | Uno Platform/WinUI-specific. RUN_PROMPT forbids Uno-specific tooling in the MAUI run. |
| `run`, `code-review`, `simplify` | Generic | Available; `run` cannot launch a MAUI target on this Linux host. |
| No .NET MAUI-specific skill is installed. | — | — |

## MCP servers

| Server | Relevant? | Use |
|---|---|---|
| `Microsoft_Learn` (docs search/fetch/code samples) | **Yes** | Used for MAUI 10 unit-testing guidance (net10.0 TFM pattern), FilePicker API semantics (null on cancel, TaskCanceledException), MAUI 10 what's-new (CollectionView over ListView, MessagingCenter removal → CommunityToolkit.Mvvm messenger). |
| `uno` (Uno Platform docs) | Excluded | Uno-specific; forbidden for this run. |
| `github`, `Claude_Code_Remote` | Infra | Repo/branch operations and environment docs only. |
| Google Drive, Gmail, HubSpot, Claude_Docs | No | Unrelated to the task. |
| No MAUI app-runtime/UI-automation MCP is available. | — | — |

## Official documentation / resources

- learn.microsoft.com/dotnet/maui (view=net-maui-10.0): unit testing, FilePicker, what's new in .NET 10.
- nuget.org package index: latest stable `Microsoft.Maui.Controls` = **10.0.110**, `CommunityToolkit.Mvvm` = **8.4.2**, `xunit` = 2.9.3, `Microsoft.NET.Test.Sdk` = 18.10.1, `xunit.runner.visualstudio` = 3.1.5.

## CLI / templates / tooling

- `dotnet` SDK 10.0.112 (Ubuntu build). No workloads preinstalled.
- `dotnet workload install maui-android` → succeeded from nuget.org; needed `dotnet workload repair` because the first install garbage-collected the net10 Android/MAUI packs. Result: maui-android 10.0.20 manifest, Android SDK pack 36.1.69, MAUI templates available (`dotnet new maui`).
- `maui-windows` workload: not installable on Linux.
- Android SDK: Google distribution host blocked by network policy. Ubuntu archive `android-sdk-platform-23` + `android-sdk-build-tools 29.0.3` installed as the only reachable SDK source (API 23 android.jar; the net10.0-android build requires API 36).
- Java: OpenJDK 21.0.10.

## UI/runtime verification tools

| Tool | Availability |
|---|---|
| Android emulator | **Unavailable** — cannot be downloaded (dl.google.com denied) and no KVM/hardware virtualization on host. |
| Physical Android device / adb | Unavailable (no platform-tools, no USB). |
| Windows desktop runtime | **Unavailable** — Linux host. |
| Appium / WinAppDriver | Not applicable without a runnable target. |
| xUnit on `net10.0` | **Available** — used to run domain, repository and view-model tests on Linux. |
| Playwright/Chromium | Installed but irrelevant (no web target in a MAUI run). |

## Consequence

Android + Windows runtime launch, screenshot capture and on-device workflow verification cannot be performed in this environment. Everything that can run on Linux (net10.0 compile of the shared app code and XAML, automated tests, Android compile as far as the available SDK permits) is exercised; the rest is recorded as NOT TESTED with the reason.
