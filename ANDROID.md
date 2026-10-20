# CrystalMix for Android

CrystalMix Android is a native, offline application that reuses the Windows calculator's C# chemistry and calculation sources. It does not require a browser, a server, or a separate .NET installation on the phone.

## Install

This is an initial preview: calculation and signature checks have passed, but device startup verification is pending because the local Windows emulator crashes before boot. Download `CrystalMix-Android-1.0.0.apk` from the **Android 1.0.0** GitHub pre-release, open it on your phone, and allow installation from that browser or file manager when Android requests it. The minimum version is **Android 8.0 (API 26)**. One APK contains ARM64, ARM32 and x86-64 native runtimes. This is a directly installable APK, not a Play Store listing.

The app requests no network permission. Its isotope data and calculations work offline. Android may display a warning for an app installed outside a store; use the published checksum to identify the released file.

## Mobile workflow

- **Recipe:** edit material cards, optional names, molar parts and total mass; calculate weighing masses and mass fractions. Copy results, share CSV text, or save a UTF-8 CSV file with the system document picker.
- **Target:** evaluate a target composition and x, then specify actual precursors or leave the basis blank for elements. Generate an editable recipe after confirming replacement of existing material cards. Invalid targets preserve the current recipe.
- **Isotopes:** search all 3,498 records by symbol, isotope syntax or Chinese/English element name. The default stable-plus-one-year subset has 407 records. Stable-only and all-record filters are also available. Results are paged in groups of 20.
- **Guide:** read help in the selected language. The top language button switches English and Chinese. Formula examples include `[175Lu]2O3`, `[180mTa]`, and `Ba1-xKxFe2As2`.

Draft materials, target settings, language and the current page are saved privately on the device when the app is paused or navigation is used. Uninstalling removes these drafts. Copy or export important recipes first. Text sizes follow Android font scaling and pages scroll vertically.

Target preview updates as you type; applying it to the raw-material table is an explicit action on mobile. Element-conservation checks do not validate a synthesis route, reaction yield, purity, isotope enrichment or material losses. See [isotope provenance](ISOTOPE-DATA.md) for catalog coverage and limitations.

## Build

Requirements: Windows, the .NET 8 SDK with the `android` workload, Android SDK API 34/build tools and a compatible JDK 17. The repository-local tooling convention is `.tools/dotnet`, `.tools/android-sdk` and `.tools/android-jdk`; tooling is not included in source archives.

```powershell
dotnet workload install android
dotnet build CrystalMix.Android -t:InstallAndroidDependencies -f net8.0-android `
  -p:AndroidSdkDirectory="E:\Android\sdk" -p:JavaSdkDirectory="E:\Android\jdk" `
  -p:AcceptAndroidSdkLicenses=True
powershell.exe -File .\build-android.ps1 -SdkDirectory "E:\Android\sdk" `
  -JavaDirectory "E:\Android\jdk" -KeyStore "E:\Private\release.keystore" `
  -PasswordFile "E:\Private\store-password.txt" -KeyPasswordFile "E:\Private\key-password.txt"
```

For a new independent signing identity, run `powershell.exe -File tools/new-android-signing-key.ps1` after installing the JDK. This refuses to overwrite existing signing files. The keystore alias must be `crystalmix`. Use a private release keystore with at least a 2048-bit RSA key and a suitably long validity period. Store and key passwords are supplied through **separate files**, even when their contents are identical: the Android signer reads password files sequentially. Never commit a keystore or password file.

`build-android.ps1` runs the cross-platform core tests, publishes the APK, aligns it, applies the release signature with the official Android signing tool, verifies the signature, and writes `dist/ANDROID-SHA256SUMS.txt`. The source and data are compiled directly from the shared desktop files; the mobile UI lives in `CrystalMix.Android/`.

For this project's release, the private signing files are kept in the ignored `.signing/` directory on the owner's machine. Back up this directory securely and separately from the repository. Future updates must use the same signing key and a higher Android `ApplicationVersion` to install over existing versions. The source ZIP deliberately excludes signing keys, passwords, `.git`, caches, binaries and emulator images.

## Tests

```powershell
dotnet run --project CrystalMix.Mobile.Tests -c Release
```

The mobile shared-core suite performs 7,040 assertions, including all isotope compounds, isotope names, hydrate/nested formulas, isomers, doping endpoints and intermediate values, exact precursor balance, inconsistent targets, invalid input, mass closure and English Help language isolation. Desktop tests remain separate and unchanged.

APK signature, manifest, ABI and runtime checks are recorded in the Android release validation report. A simulator cannot establish compatibility with every handset or vendor-specific Android build.
