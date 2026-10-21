# Measured-mass scaling verification — 1.2.0

Verified on 2026-10-21 for Android 1.2.0 and Windows 1.2.0.

- Shared mobile core: **7,118 assertions passed**, including 78 new measured-mass checks and all 3,498 isotope records.
- Windows suite with UI checks: **43/43 passed**. The measured-mass dialog rejected zero/NaN, previewed other materials, preserved the recipe on Cancel, and applied a 5 g Fe anchor without changing the 1:1 Fe/Se molar ratio. A bounds assertion confirms the Apply button is inside the dialog; the screenshot was visually inspected.
- Final Windows self-contained x64 installer: all six extracted files match the payload, the app starts and responds, and the Chinese installer prompt renders correctly and cancels cleanly.
- Android APK signatures v2/v3 verified with the existing signing identity. Version code 3 permits upgrades from earlier releases.
- [Android 14 emulator run 39590243343](https://github.com/liudonghua701-crypto/CrystalMix/actions/runs/39590243343) passed measured-mass preview, zero rejection, Cancel preserving the 10 g batch, and Apply changing the Fe target to 5 g and total to approximately 12.07055242 g. Screenshots were inspected.
- The same Android run passed workspace regression checks for live edits, target filling, isotope insertion, Help, language switching, recipe restoration, and a 360dp-wide display at 1.3x font scale.

The final Android APK is identical to the tested android-v1.2.0-rc1 asset. Release checksum files identify all distribution files. Complete source archives exclude signing keys, passwords and local toolchain caches.

Tests do not cover physical Android phones or CSV chooser/share-sheet interaction. Actual weighing uses one anchor at a time; it does not lock multiple measured materials or compute incremental additions. Android remains a preview.
