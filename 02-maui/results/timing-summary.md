# Timing Summary (from timing.jsonl, UTC, 2026-09-24)

| Phase | From → to | Duration |
|---|---|---|
| 1. Container build (ended BLOCKED) | 15:30:46 → 16:00:14 | 29:28 |
| Waiting for human intervention | 16:00:14 → 17:42:07 | 1:41:53 (not agent work) |
| 2. CI verification | 17:42:07 → 19:49:28 | 2:07:21 |
| **Total wall-clock** | 15:30:46 → 19:49:28 | **4:18:42** |
| **Agent working time** | phases 1 + 2 | **2:36:49** |

| Milestone | UTC | Wall | Working |
|---|---|---|---|
| capability_discovery_complete | 15:35:32 | 0:04:46 | 0:04:46 |
| first_source_edit (K01) | 15:37:47 | 0:07:01 | 0:07:01 |
| first_build_attempt (K02) | 15:40:51 | 0:10:05 | 0:10:05 |
| first_android_build_success, composed SDK (K03) | 15:56:50 | 0:26:04 | 0:26:04 |
| first Android build, official SDK (CI run 1) | 17:45:07 | 2:14:21 | 0:32:28 |
| first Windows build success (CI run 3) (K03) | ~18:10 | ~2:39 | ~0:57 |
| first Windows / Android launch success (CI run 4) (K04) | 18:32:50 / 18:34:10 | 3:02 / 3:03 | 1:20 / 1:22 |
| core_complete (K05) | 19:46:29 | 4:15:43 | 2:33:50 |
| run_finish | 19:49:28 | 4:18:42 | 2:36:49 |

## Notes

- The `timing.jsonl` entries `first_windows_build_success`, `first_windows_launch_success` and `first_android_launch_success` were written late, at 19:17:16. The actual first occurrences are the CI times in the table above, taken from the GitHub Actions run records. The log was not rewritten, per the benchmark rules.
- The first Windows build time is approximate. CI run 3 started at 18:04:11, and the exact build-step completion time was not captured.
- GitHub runner time overlaps phase 2 and is not counted separately.
- Session activity after 19:49:28 is post-run Q&A and is excluded.
