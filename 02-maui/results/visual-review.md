# Visual Review — FieldCheck (.NET MAUI)

## Status: NOT PERFORMED (no implementation screenshots)

No implementation screenshot could be captured. Android has no emulator/device in this environment (Google SDK host blocked, no KVM), and the Windows target cannot be built or run on Linux. H01–H06 are NOT TESTED. Nothing below is a screenshot comparison.

What follows is a **design-intent mapping**: how each reference view was translated into XAML, plus the divergences already known from the implementation. It exists so the comparison can be completed quickly on a capable machine.

## Deliberate divergences (known without rendering)

| Area | Reference | Implementation | Reason |
|---|---|---|---|
| Nav/search icons | Glyphs render as vertical-bar "tofu" boxes (Dashboard, History, search) | Line icons drawn as SVG: 2×2 grid (Dashboard), diamond (Assets, same as reference), list (History), magnifier (search) | The reference glyphs look like missing-font boxes. Reproducing them would ship broken-looking icons. |
| Dashboard "Needs attention" | 4 rows (CT-007, AHU-203, CNV-018, BLR-002); EF-090 absent although counted as Critical | 5 rows, Critical first: CT-007, EF-090, AHU-203, CNV-018, BLR-002 | Spec: list contains all Attention and Critical assets. Counts (7/3/2) match the reference. |
| History rows (Android) | Asset, then "ID · time" | Asset, "ID · time", then inspector on a third line | Spec §3.6 requires the inspector on each row. The Windows table already shows it. |
| History subtitle | "7 completed inspections" | "6 completed inspections" before any submit, 7 after | The reference is captured after one new inspection. |
| Inspection IDs | INS-24102 | INS-24092 for the first new record | IDs increment from the highest seeded ID. References allow differences for generated records. |
| Checklist boxes | Lime-filled box, ink check, ink border | Native CheckBox tinted Ink (ink fill, white check) | Keeps native keyboard focus and screen-reader semantics (F07/G01). A custom lime box would need a custom focusable control. |
| New inspection defaults | Shows a filled example (Attention, 27, all checked) | Starts empty: no condition, empty temperature, unchecked | These are required inputs. Prefilling them would bypass validation. |
| Android New inspection | No Notes field visible | Notes (optional, multiline) after Issue description | Spec §3.4 requires Notes. |
| Windows Cancel | Quiet "Cancel" text button | Same. On Android, Cancel is the header back arrow plus system back (no extra button, matching the reference) | — |
| Status bar | References show no Android status bar | Headers use top padding 40–44 px. With MAUI 10 edge-to-edge, final vertical offsets depend on the device's inset handling | Needs a runtime check. |

## Per-screen mapping

- **Dashboard (H01)**: FIELDCHECK micro label (Android only; the Windows sidebar carries the mark), "Good afternoon/morning/evening" 30 semibold, "Facility A · {weekday, MMM d}", 50–54 px total metric plus muted "assets", three colored counts, hairline, NEEDS ATTENTION rows with chip and chevron, full-width (Android) or 170 px (Windows) Accent "View all assets".
- **Assets (H02/H03 Windows)**: title and count, bordered search field (12 px radius), pill chips (Ink fill when selected), separated rows. At ≥ 760 px content width: a 460 px master pane on #F6F5F1 with a 1 px rule, the selected row on a Surface fill with a 4 px Accent marker, and the detail pane (title 30, chip, 200 px Accent "Start inspection" top-right, 2×2 info sheet, rules, description, latest inspection).
- **Asset detail (H03 Android)**: back plus "Asset detail" 30, then the same info sheet with a full-width Start inspection button after the content.
- **New inspection (H04)**: three segmented buttons (Ink fill when selected), operating-normally switch (Ink track, Accent thumb), temperature field, checklist, conditional issue description with an Attention-colored "Required" tag, notes, "+ Choose file" field, Accent submit. Wide layout: checklist in a right column at x = 380 + 92, rule, 820 px text areas, Cancel and Submit bottom-right.
- **Success (H05)**: 70 px lime circle with an ink check, "Inspection saved" 32, ID, rule, asset name with chip, date · inspector, local-storage note, then View asset (Accent) and View history (outlined). Stacked on Android, side by side on Windows (191 px each).
- **History (H06)**: title and count, search plus chips (one row on wide, stacked on narrow), Windows table columns ASSET / INSPECTION / INSPECTOR / RESULT with hairlines, Android stacked rows.

## To complete this review

1. On Windows: `dotnet build src/FieldCheck -f net10.0-windows10.0.19041.0 -c Release`, run, and capture at the default 1440×900 window.
2. On an Android emulator (Pixel-class 412×915 dp): install the Release APK and capture the six views.
3. Save the images to `results/screenshots/{android,windows}/` using the reference file names, then replace this document with the per-screenshot comparison.
