# Android verification

## Android 1.2.1 — opt-in precursor constraints

Verified on 2026-10-23 in [Android 14 run 39951156744](https://github.com/liudonghua701-crypto/CrystalMix/actions/runs/39951156744). With the saved BaAs/Fe2As/KAs preset present, the switch and input started disabled; Fe2O3 generated elemental Fe and O without applying that preset. Explicit opt-in enabled the input and the arsenide recipe regression passed. Screenshots were inspected. The full run also passed actual-weighing and workspace regressions. Shared-core assertions: 7,118; APK signatures v2/v3 passed. Existing drafts without the new boolean property deserialize to false. Physical-phone and CSV interaction limits remain unchanged.

## Android 1.2.0 — actual weighed mass

Measured-mass preview, invalid input, Cancel and Apply passed on Android 14, together with the existing workspace regressions. See [the 1.2.0 verification report](WEIGHING-VALIDATION.md) and [run 39590243343](https://github.com/liudonghua701-crypto/CrystalMix/actions/runs/39590243343). Shared-core coverage is now 7,118 assertions. The new Apply operation updates batch mass while preserving formulas and molar parts.

## Android 1.1.0 — single-workspace interface

Verified on 2026-10-21 using [Actions run 39560691248](https://github.com/liudonghua701-crypto/CrystalMix/actions/runs/39560691248). The APK tested under the rc1 tag is identical to the final 1.1.0 APK.

- Native Android 14 installation and startup passed.
- The initial 10 g result appeared automatically. Editing total mass to 20 g and editing molar parts required no Calculate button or page navigation.
- Expanding Target, setting x = 0.25 and confirming generation populated BaAs with 0.75 molar parts while retaining the 20 g batch.
- Adding [175Lu] through the isotope dialog returned directly to the material workspace.
- Contextual Help and language switching worked; the target-generated recipe and total mass survived backgrounding, force-stop and relaunch.
- At 360 x 800 / 160 dpi with font scale 1.3, all three bottom actions remained within the viewport with at least 48 px touch height. Screenshots were inspected for wrapping and readability; the material list remains scrollable.
- Shared chemistry regression: 7,040 assertions passed, including all 3,498 isotope records. APK signatures v2/v3 passed with the original release certificate.

Physical phones, CSV chooser/share-sheet interactions and every possible display setting are not covered by these checks. The release remains a preview. Check release SHA256SUMS for the exact binary hash.

## Android 1.0.0 verification

Date: 2026-10-20. Package: `io.github.liudonghua701.crystalmix`.

## Automated calculation checks

- Mobile shared-core suite: **7,040 assertions passed**, including every one of the 3,498 isotope records.
- Desktop regression suite: **38/38 passed** after adding the Android project.
- The Android project compiles the existing chemistry and calculation files directly. Element-weight and isotope data are not regenerated or duplicated.

## Package inspection

- Android version name 1.0.0, version code 1.
- Minimum Android API 26; target API 34.
- Native runtimes: `arm64-v8a`, `armeabi-v7a`, `x86_64`.
- No network or storage permission requested. File saving uses the system document picker.
- Backup disabled; recipe drafts remain in private app preferences.
- Release build has no debuggable manifest flag.
- Official Android signer verification passes APK Signature Scheme v2 and v3.
- Release certificate SHA-256: `0a6363205484dfda06b70493be30052bb92a143feff03138fe0dec06900f5e72`.

Signing material stays in the ignored local `.signing` directory; public files contain only the certificate fingerprint. Distribution checksums are provided separately with the release.

## Local emulator limitation

The Windows host could not run the Android 34 emulator to completion. The emulator's own native process exited with Windows access-violation status `0xC0000005` before the Android system became usable, including a retry with software acceleration and a single virtual CPU. This occurred before APK installation and is not an observed application crash.

Local Windows emulator startup and physical-device behavior are not claimed as passed. Linux-hosted emulator verification subsequently succeeded, as detailed below.

## Successful Android emulator run

[GitHub Actions run 39412813563](https://github.com/liudonghua701-crypto/CrystalMix/actions/runs/39412813563) completed successfully on 2026-10-20 using the published Android 1.0.0 APK, Android 14 / API 34 x86_64, hardware virtualization, and a 480 x 1000 display at 160 dpi.

- APK installation and application startup passed; the process remained running.
- Default Fe/Se recipe calculated a total of 10 g.
- Ba1-xKxFe2As2 at x = 0.4 displayed its evaluated composition; confirmed generation populated BaAs-containing material cards.
- Searching 175Lu found [175Lu] and its Add action completed.
- English Help contained no Chinese prose; switching to Chinese displayed Chinese Help.
- Language and selected Help page persisted after backgrounding, force-stop and relaunch.
- No AndroidRuntime fatal exception appeared in the collected log.
- Calculation, isotope and both Help screenshots were visually inspected. Text wrapped and remained readable at the tested display size.

APK SHA-256: `060b3913c7ac254125142531c19c00c2eb94290ecd8e1a7985789c124b62c363`.

This smoke test does not establish physical ARM-device compatibility, all font-scale/window-size combinations, CSV chooser/sharing behavior, or persistence of every recipe field. The release remains a preview. The workflow and test script are tracked in the repository; release attachments include a verification report and screenshots. No private signing material is uploaded.