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

Consequently, local emulator startup and physical-device behavior are not claimed as passed. A separate GitHub Linux/Android workflow has been prepared to test installation, startup, mass calculation, target filling, isotope search, language switching and draft persistence. Its execution requires the repository workflow permission.
