<p align="center"><img src="CrystalMix/Assets/CrystalMix.png" width="128" alt="CrystalMix crystal icon"></p>

# CrystalMix

**A Windows desktop calculator for crystal-growth material ratios, isotope-aware compositions, and weighing plans.**

CrystalMix converts user-selected raw materials and molar parts into molar masses, mass fractions, and weighing masses for a specified batch. It also evaluates target compositions and checks whether a selected precursor basis can reproduce them. The interface offers Chinese and English; the Help window follows the selected language.

## Adjusting to an actual weighed mass

Both editions support **Use actual mass**: enter the measured mass of one material, preview the scaled targets, and apply while retaining all molar ratios. This updates the batch total as well. See [the weighing guide](WEIGHING.md) for an example and the distinction between new targets and already-weighed amounts.

## Android APK

A native, offline Android preview is available for **Android 8.0 and later**. Installation, startup and core workflows passed on an Android 14 emulator; see [verification status](ANDROID-VALIDATION.md). Physical-device testing remains pending. It shares the chemistry and calculation sources with the Windows edition and provides a single weighing workspace with live row results, collapsible target settings, a contextual isotope picker, bilingual Help and CSV export.

[Download Android APK](https://github.com/liudonghua701-crypto/CrystalMix/releases/download/android-v1.2.0/CrystalMix-Android-1.2.0.apk) · [Android installation, features and builds](ANDROID.md)

## Download and install (Windows)

**[Download the Windows x64 installer](https://github.com/liudonghua701-crypto/CrystalMix/releases/latest/download/CrystalMix-Setup-x64.exe)** · [All releases](https://github.com/liudonghua701-crypto/CrystalMix/releases) · [Changelog](CHANGELOG.md)

1. Download **CrystalMix-Setup-x64.exe** from the release assets.
2. Run it, confirm installation, and choose a dedicated installation folder.
3. Launch CrystalMix from the desktop or Start menu shortcut.

The installer includes the self-contained Windows x64 application. End users do **not** need Git, the source archive, a .NET SDK, or a separately installed .NET runtime. Runtime calculations and the isotope catalog work offline. The installer itself does not download application components.

Installation is per-user and does not require administrator rights when the selected folder is writable. Paths with spaces and Chinese characters are supported. Choosing a drive root in the folder browser selects a dedicated subfolder, such as `D:\CrystalMix`. Uninstall through Windows **Installed apps**. To move an existing installation, uninstall its old location before installing elsewhere. Do not manually move an installed folder.

The package is currently unsigned; Windows may display a publisher or reputation prompt. Published SHA-256 checksums identify the exact release files. The tested target is Windows x64; ARM-native and non-Windows builds are not provided.

## Features

- Convert any positive molar parts into normalized mass fractions and batch weighing masses.
- Recognize all 118 element symbols and provide Chinese/English element names.
- Parse elements, compounds, decimal stoichiometry, nested groups, and hydrate middle dots.
- Calculate specified isotopes in elemental materials and compounds, retaining isotope identity throughout composition calculations.
- Search an embedded catalog of **3,498 records** spanning 118 element symbols: 3,486 ground-state mass entries and 12 long-lived isomers.
- Default to **stable nuclides plus half-lives of at least one year**; switch to stable-only or all available records. The current default subset has 407 records.
- Evaluate variable target formulas such as `Ba1-xKxFe2As2`, with `0 <= x <= 1`.
- Suggest elemental amounts or balance a user-selected, independent precursor basis; explain inconsistent element constraints.
- Keep the target composition separate from the editable raw-material table.
- Copy calculated results and export UTF-8 CSV files.
- Adapt table headers, controls, and scrolling to window size and font scaling.
- Provide a scrollable Help window in the selected language and a multi-resolution application icon.

## Quick start

1. Enter one actual material formula and its molar parts per row. A custom material name is optional.
2. Set **Total batch mass** in grams.
3. Select **Calculate recipe**. Each contribution is `molar parts × molar mass`; CrystalMix normalizes the contributions and allocates the batch mass.
4. Use **Copy results** or **Export CSV** after a successful calculation.

Molar parts are relative amounts; they do not need to sum to one. For example, Fe:Se parts of 1:1 mean equal mole amounts, not equal masses.

| Input | Meaning |
| --- | --- |
| `Fe`, `FeSe` | Element or compound |
| `Ca(OH)2`, `K4[Fe(CN)6]` | Grouped and nested composition |
| `Fe1.03Te` | Decimal stoichiometry |
| `CuSO4·5H2O` | Hydrate; use the middle dot `·` |
| `[175Lu]`, `[175Lu]2O3` | Specified isotope, alone or in a compound |
| `[180mTa]`, `[178m2Hf]` | Explicit nuclear isomer |

Element symbols are case-sensitive. The period `.` is a decimal point, not a hydrate separator. Unsupported nuclides and malformed formulas are rejected rather than silently approximated.

## Targets, precursors, and variables

Enter a target such as `Ba1-xKxFe2As2`, set `x = 0.4`, and optionally specify the actual materials. A blank material basis suggests elemental amounts. For the same target:

| Selected materials, in order | Suggested molar parts |
| --- | --- |
| `BaFe2As2, KFe2As2` | `0.6 : 0.4` |
| `BaAs, Fe2As, KAs` | `0.6 : 1 : 0.4` |

These are composition balances, **not unique synthesis recipes**. The solver requires an independent basis and a nonnegative exact solution; dependent, incompatible, or negative-use solutions are rejected.

For `Ba1-xKxFeAs`, the same three arsenides cannot balance. Matching Ba, Fe and K forces parts `(1-x):0.5:x`, which supply **1.5 As** per target unit instead of **1 As**. CrystalMix reports the element constraint rather than changing your target formula.

Use braces for more complex subscripts: `Ba{1-x}K{x}Fe2As2`, `Fe{(1+x)/2}` or `[175Lu]{1-x}[176Lu]{x}O`. Arithmetic supports `+`, `-`, `*`, `/` and parentheses. Zero target components are omitted; invalid or nonfinite expressions are rejected.

Changing the target, variable value, or selected basis refills the material table. You can then freely edit material formulas and molar parts without changing the target composition. Invalid target input preserves the previously entered table.

## Isotope data and scientific scope

Open **Isotopes** to search by symbol, Chinese/English element name, or bracket syntax, inspect masses and half-lives, and add a selected record. Ordinary `Lu` uses the existing conventional atomic weight; `[175Lu]` uses its specified isotope mass. Mixed ordinary and isotope species remain distinct.

Mass records come from NIST, supplemented by IAEA/AME2020; ground-state lifetime metadata comes from IAEA/ENSDF, with long-lived isomers from NUBASE2020. Estimated masses and lifetime bounds remain identified. See [Isotope data and reproducibility](ISOTOPE-DATA.md) for coverage, units, source snapshots, and import rules.

Coverage of 118 elements does not imply that every element has a stable isotope. The catalog is a dated data snapshot, not a claim to include every nuclear excited state. Nuclear lifetime does not establish chemical stability. Ta-180 and Ta-180m are separate entries.

Calculations use neutral-atom masses and theoretical stoichiometry. They do not automatically account for purity, isotope-enrichment fractions, oxidation, moisture, volatilization, flux, byproducts, reaction yield, or source-data uncertainty. A stoichiometric solution does not establish reaction feasibility.

## Build from source

Requirements: Windows x64, PowerShell 5.1, and the **.NET 8 SDK**. Git is optional if using the source ZIP. Windows IExpress, used to assemble the installer, is expected at `%WINDIR%\System32\iexpress.exe`.

```powershell
git clone https://github.com/liudonghua701-crypto/CrystalMix.git
cd CrystalMix
.\build-release.ps1
```

The script restores/builds the project, runs core tests, verifies installer encodings, publishes a self-contained `win-x64` executable, and creates the single-file installer. A first source build needs access to the .NET restore feeds unless the required packs are already cached. Output:

- `dist/publish/CrystalMix.exe`: self-contained application.
- `dist/CrystalMix-Setup-x64.exe`: installer, also copied to the project root.
- `dist/SHA256.txt`: installer checksum.

Build scripts prefer `.tools/dotnet/dotnet.exe` when present, otherwise `dotnet` from PATH. Local tool caches and build outputs are excluded from Git and the source ZIP. The project has no third-party test-framework dependency.

## Tests and release checks

```powershell
dotnet run --project CrystalMix.Tests -c Release
# Requires an interactive Windows desktop:
dotnet run --project CrystalMix.Tests -c Release -- --ui
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\smoke-release.ps1
```

Tests cover element names, ordinary formulas, all catalog isotope formulas and compounds, isomers, variable expressions, multiple doping values, rejected input, total mass closure, balance conflicts, and parser regressions. UI checks exercise target editing, language-specific help, font scaling, and isotope selection. Screenshots are saved under `dist/ui-checks`. Font-size simulations do not replace testing every physical monitor/DPI combination.

The release smoke check extracts the package, verifies all six payload files, starts and cleanly closes the application, and checks the Chinese installer prompt before cancelling. It does not replace an existing installation.

Installer-specific checks are available in `test-installer.ps1`, `test-install-dialog.ps1`, `test-install-browse.ps1`, and `test-uninstall-entry.ps1`. The full installer regression performs actual installation/uninstallation in a test location and restores previously registered information in its cleanup block. Run it only when intentionally testing those changes.

## Project layout

| Path | Purpose |
| --- | --- |
| `CrystalMix/` | Windows Forms application, chemistry model, calculations and localization |
| `CrystalMix/Assets/` | Editable SVG, PNG preview and multi-resolution ICO |
| `CrystalMix/Data/isotopes.json` | Embedded, offline isotope snapshot |
| `CrystalMix.Tests/` | Calculation, localization and desktop UI checks |
| `installer/` | Install/uninstall scripts and IExpress configuration |
| `tools/` | Reproducible icon generation and isotope import |
| `data-sources/` | Archived source data and checksum manifest |
| `build-release.ps1` | Test, publish and package pipeline |

The release source ZIP contains tracked engineering files, including test code, installer scripts, original icon assets and source-data snapshots. It excludes `.git`, credentials, development tool caches, and generated binaries. Clone the repository if you need commit history; download the installer if you only want to use the application.

## Maintenance and release history

See [CHANGELOG.md](CHANGELOG.md) for version 1.1.0 and the earlier commit-by-commit development history. Earlier validation reports are historical snapshots, not claims about the current installer checksum. Release assets include checksums for the current installer and source archive.

Rebuild the icon with `powershell.exe -File tools/build-icon.ps1`. Recreate the isotope JSON with `powershell.exe -File tools/import-isotopes.ps1`; the importer verifies the archived source hashes and uses a fixed PowerShell serialization runtime.

Source files and documentation use UTF-8. Installer PowerShell scripts use UTF-8 BOM for Windows PowerShell; CMD launchers and IExpress configuration remain ASCII. Installation/uninstallation text is handled as Unicode. Uninstallation validates the registered folder and preserves unrelated user files; locked files cause a recoverable error instead of silently deleting the uninstall entry.

## License

See [LICENSE](LICENSE). Scientific data sources retain their own provenance, documented separately in [ISOTOPE-DATA.md](ISOTOPE-DATA.md).
