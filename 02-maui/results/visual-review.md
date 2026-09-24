# Visual Review — FieldCheck (.NET MAUI)

Implementation screenshots come from the final CI run (commit `d37afd2`, run 36049012411):

- **Android**: API 34 `pixel_6` emulator, 1080×2400 px at 420 dpi = 411×914 dp. Files in `screenshots/android/<name>.png` are downscaled to 412×915 for side-by-side comparison. Full resolution is in `<name>-1080x2400.png`. Screenshots include the system status bar and gesture bar, which the references omit.
- **Windows**: Windows Server 2025 runner, 100 % scale, app window 1440×900 including the title bar, captured with UIA (`screenshots/windows/<name>.png`).

Font rasterization differs from the references: Roboto on Android, Segoe UI Variable on Windows, while the references use a generic grotesque. Per the acceptance rules this is not scored.

## Android (412×915)

| Ref | Implementation | Structure / composition | Spacing / alignment | Typography | Color / borders / radii | Sizing / density | Notes |
|---|---|---|---|---|---|---|---|
| 01-dashboard | 01-dashboard.png | Match: micro label, greeting, context line, 12 assets metric, 3 colored counts, rule, NEEDS ATTENTION rows (name / ID·type / location, chip, chevron), bottom nav with Accent indicator | Content starts ~30 px lower because of the status bar; 20 px gutters match | Titles render **bold** vs the reference's semibold | Tokens match; chips soft-fill with 14 px radius | Rows ~94 px vs ~92 px | List has **5** rows (spec: all Attention + Critical); the reference shows 4 and omits EF-090. The "View all assets" button is below the fold because of the 5th row. |
| 02-assets | 02-assets.png | Match: title + count, bordered search (12 px radius) with icon, 4 pill chips (Ink selected), separated rows with chip + chevron | Match | Bold title | Match | 6 rows visible (reference 6) | Search icon is a magnifier (reference glyph renders as a missing-font box). |
| 03-asset-detail | 03-asset-detail.png | Match: back + "Asset detail", name, chip, 2×2 info sheet, rules, description, latest inspection, full-width Accent Start inspection | Match | Bold header | Match | Start inspection sits below the fold (reached by scrolling, `e07-asset-detail-scrolled.png`) because an extra meta line (ID · date · inspector) is shown under the latest inspection. | Long CT-007 text wraps cleanly. |
| 04-new-inspection | 04-new-inspection.png, 04-new-inspection-lower.png | Match: segmented condition (Ink selected), operating switch (Ink track / lime thumb), temperature field, checklist, conditional Issue description with "Required", attach field, Accent submit | Match | Match | Checkboxes are native (Ink fill, white check) vs the reference's lime box with ink check | Form is longer because Notes (spec-required) sits between Issue description and Attach | The reference shows no Notes field; the spec requires it. |
| 05-inspection-success | 05-inspection-success.png | Match: lime circle + check, "Inspection saved", ID, rule, asset + chip, date · inspector, local-storage note, stacked View asset / View history | Match | Match | Match | Match | Near-identical composition. |
| 06-history | 06-history.png | Match: title + count, search, chips, rows with asset, ID · time, chip | Match | Match | Match | Rows are taller: a third line shows the **inspector** (spec §3.6 requires it; the reference omits it) | Bottom nav with Accent indicator on History. |

## Windows (1440×900)

| Ref | Implementation | Structure / composition | Spacing / alignment | Typography | Color / borders / radii | Sizing / density | Notes |
|---|---|---|---|---|---|---|---|
| 01-dashboard | 01-dashboard.png | Match: 232 px white sidebar with FIELDCHECK + lime F mark, 3 items with Accent marker on the active one, footer Alex Morgan / Facility A; content with greeting, metric, counts, rule, rows (chip column + chevron), 170 px Accent CTA | Content offset ~30 px lower (window title bar) | Bold titles | Match | 5 rows (see Android note) | The window title bar and system caption buttons are native chrome absent from the reference. |
| 02-assets-master-detail | 02-assets-master-detail.png | Match: 460 px master pane (lighter surface, 1 px rule) with search, chips, rows; selected row on a Surface fill with Accent marker; detail pane with title, chip, top-right Accent Start inspection, info sheet, description, latest inspection | Match | Match | Match | Master rows ~73 px (reference ~84 px): slightly denser | A native WinUI scroll indicator line appears in the master list while scrolling. |
| 03-new-inspection | 03-new-inspection.png | Match: two-column top (condition / operating / temperature left, checklist right), rule, 820 px issue + notes editors, 360 px attach field, Cancel + Submit bottom-right | Match | Match | Native Ink checkboxes (see Android) | With an attachment preview row added, Submit sits just below the 900 px fold (reachable by scroll). Reference fits because it has no attachment preview. | Shell adds a native back arrow in the title bar on pushed pages; it works, it is not in the reference. |
| 04-inspection-success | 04-inspection-success.png | Match: centered 413 px column, lime circle, title, ID, rule, asset + chip, meta, note, side-by-side 191 px buttons | Match | Match | Match | Match | Near-identical. |
| 05-history | 05-history.png | Match: title + count, 415 px search + chips in one row, table header ASSET / INSPECTION / INSPECTOR / RESULT, hairline rows | Match | Match | Match | Rows ~67 px vs ~66 px | Near-identical. |

## Responsive / state captures (supporting evidence)

- **Windows**:
  - `f04-assets-1000x800`, `f04-assets-900x760`, `f04-assets-720x600`: master/detail collapses below ~1000 px window width.
  - `f04-history-720x600`: stacked rows below the table breakpoint.
  - `f04-asset-detail-720x600`: detail as its own page.
  - Data states: `e01-loading`, `e02-empty-*`, `e03-error-dashboard`, `e04-retry-recovered`, `e05-assets-no-results`.
  - Also: `d04-temperature-validation`, `d07-file-picker-open`, `g03-keyboard-focus-dashboard` (visible focus rectangle).
- **Android**:
  - `f05-*-font-1.3`: 130 % system font scale.
  - Picker: `d07-picker-open`, `d08-picker-file` (DocumentsUI).
  - Also: `d04-temperature-validation`, `e05-no-results`, `c10-history-after-restart`.

## Summary of material differences

No structural divergence or redesign. The deliberate differences either follow the spec or keep native accessibility:

- 5 Needs-attention rows.
- Notes field present.
- Inspector shown in History rows.
- Native checkboxes.
- SVG line icons where the reference glyphs are missing-font boxes.

Cosmetic differences:

- Bold where the reference uses semibold titles.
- The native Windows title bar and back arrow.
- A slightly denser Windows master list.
- The Submit / Start inspection buttons land just below the fold in two views, because of the extra spec-required content.
