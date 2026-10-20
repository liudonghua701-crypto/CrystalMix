# Changelog

This file records user-visible changes and the actual development sequence. Version 1.1.0 is the first numbered GitHub release. Earlier development snapshots retained application version 1.0.0; the entries below identify them by commit instead of inventing retroactive release numbers.

## Android 1.0.0 — 2026-10-20

The Android edition has its own version sequence. Windows remains at 1.1.0.

- Added a native, offline Android application with Recipe, Target, Isotopes and Guide pages.
- Reused the existing C# chemistry/calculation sources and embedded isotope snapshot without generating a second element-weight table.
- Added touch-friendly editable material cards, target composition previews, confirmed recipe replacement, local draft storage and language switching.
- Added isotope search with stable-plus-one-year, stable-only and all-record filters, paged results, and direct insertion into the recipe.
- Added result copying, CSV text sharing and CSV file export through Android's system document picker.
- Included ARM64, ARM32 and x86-64 runtimes in a signed APK for Android 8.0 and later, with no network permission.
- Added an Android release build/signature-verification script and a cross-platform core suite with 7,040 assertions.
- Verified the published APK on a Linux-hosted Android 14 emulator: installation, startup, 10 g mass calculation, target filling, Lu-175 search/add, localized Help, and language/page restoration. Added a repeatable GitHub Actions workflow and screenshot evidence; physical-device and CSV interaction testing remain pending.
- Kept release signing keys and credentials private, outside tracked source files.

## 1.1.0 — 2026-10-19

### Added

- Original faceted-crystal identity with editable SVG, PNG preview and Windows icon sizes 16, 24, 32, 48, 64, 128 and 256 pixels. The executable, application windows and installed shortcuts use the application icon.
- Resizable, scrollable Help window, replacing the long message box.
- Separate Chinese and English help text selected by the current interface language, including isotope/isomer syntax, variable formulas, precursor examples and the arsenide balance conflict.
- Localization regression checks that reject Chinese prose in English Help; icon resource and application-version checks.
- English README covering installation, features, calculation examples, limits, project structure, source builds and verification.
- A public GitHub repository, a complete engineering source archive, a self-contained Windows x64 installer and release checksums.

### Changed

- Application and installed-app version set to 1.1.0.
- Help content explains the distinction between target composition and selected raw materials, and between nuclear lifetime and chemical stability.
- Release documentation distinguishes the downloadable installer from the source archive; end users need only the installer.

### Retained from the preceding development snapshots

- Ordinary formulas, nested groups, decimal stoichiometry, hydrates, specified isotopes and arithmetic doping expressions.
- Searchable 3,498-record isotope catalog with a stable-plus-one-year default subset of 407 records, source provenance, lifetime bounds and estimated-mass labels.
- Editable material tables, target-derived suggestions, precursor balancing, total-mass allocation, result copying and CSV export.
- Adaptive layouts, custom installation folders and repaired uninstall behavior.

## Earlier development history — application version 1.0.0

### 7a591fe — Full isotope catalog and balance diagnostics

- Replaced the small hand-maintained isotope subset with source-backed mass and lifetime data: 3,486 ground-state records plus 12 long-lived isomers across 118 element symbols.
- Added the searchable isotope browser, stable-only/default/all filters, explicit nuclear-isomer syntax, and direct material insertion.
- Preserved ordinary-element weights and the existing isotope masses; archived source snapshots with hashes and an offline reproducible importer.
- Added element-conservation explanations for incompatible precursor bases, including the As excess for Ba1-xKxFeAs with BaAs, Fe2As and KAs.
- Tested every catalog record as an element and compound, plus mass closure and multiple doping values; improved large-font catalog scrolling and minimum column widths.

### 972f5e1 — Lutetium isotope support

- Added Lu-175 and Lu-176 to the then-limited isotope subset and tested elemental, oxide, mixed-isotope and variable formulas.
- Clarified errors for unsupported nuclides instead of substituting integer mass numbers.

### 525e173 — Folder-browser correction

- Fixed a PowerShell automatic-variable collision that caused the missing `Text` property error when selecting a drive such as D:.
- Drive-root selections now resolve to a dedicated application subfolder; cancelling preserves the previous path.

### c631348 — Installation paths and uninstall repair

- Added editable/browsable installation folders, including paths with spaces and Chinese characters.
- Reworked the uninstall entry to invoke PowerShell directly, validate the registered folder, handle locked files, and preserve unrelated user files.
- Added focused installer, folder-dialog and uninstall-entry regressions.

### 344d166 — Isotopes, doping and adaptive layout

- Added element-name recognition, explicit bracket isotope syntax, variable target formulas and user-selected precursor balancing.
- Kept target composition separate from actual editable materials; preserved the table when target parsing fails.
- Improved font/DPI layout behavior and wrapping table headers.
- Standardized installer script encodings and expanded build, parser and installation checks.

### 84af1cf — Source baseline

- Preserved the existing Windows Forms calculator as the first local Git baseline before subsequent changes.
- Baseline functions included material molar ratios, formula molar masses, batch weighing, bilingual UI and CSV output.

## Scope and known limits

- Windows x64 is the release target. The package is unsigned.
- Calculations are theoretical composition balances, not synthesis-route validation or uncertainty propagation.
- The isotope database is a dated source snapshot; short-lived isomers beyond the documented subset are not imported.
- Existing installer dialogs remain Chinese; application Help is independently localized for Chinese and English.
