# CrystalMix 1.1.0 release verification

Date: 2026-10-19. Target: self-contained Windows x64.

## Scope

- Added the original crystal icon to the application executable and windows; installed shortcuts reference that executable icon.
- Split extended Help into Chinese and English text and replaced the message box with a resizable scrolling window.
- Updated the application and installer registration to version 1.1.0.
- Reworked README, changelog and isotope provenance documentation in English.

## Automated verification

- Core suite: 38/38 passed.
- Full suite including interactive desktop checks: 41/41 passed.
- English Help is checked for Chinese character leakage; both language versions retain isotope, isomer, doping and balance-conflict examples.
- Help window read-only text, scrolling, Windows line breaks and both languages are exercised at enlarged font size.
- The embedded ICO is checked for seven image sizes; assembly version is checked against 1.1.
- Existing full-catalog, mass-closure, parser, doping, precursor-balance and font-scaling regressions remain enabled.

UI evidence is generated in `dist/ui-checks`, including `help-English.png` and `help-Chinese.png`. These are local build outputs rather than source dependencies.

## Distribution

The GitHub release provides the installer, a source ZIP created from the tagged Git tree, and SHA-256 checksums. The source archive includes code, tests, installer scripts, icon sources and isotope provenance snapshots. It excludes local credentials, caches, `.git`, and generated binaries. The repository retains the complete existing commit history.

The package is unsigned. Font-scale checks do not cover every physical display/DPI combination. The application is self-contained; source builds still require the .NET 8 SDK and restore packs. Existing installer dialogs remain Chinese.

## Package smoke results

The installer payload contained all six expected files with matching hashes. The self-contained application window started and responded, and the Chinese installation confirmation rendered correctly and exited cleanly on cancellation. File version is 1.1.0.0. These checks do not replace the user's existing installation.
