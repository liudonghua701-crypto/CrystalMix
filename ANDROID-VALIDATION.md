# Android 1.0.0 verification

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