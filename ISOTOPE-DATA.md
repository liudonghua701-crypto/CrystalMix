# Isotope data and reproducibility

Snapshot retrieved: **2026-10-19**. The embedded catalog contains **3,498 records**: 3,486 ground-state mass entries and 12 long-lived nuclear isomers across the existing 118 element symbols. Element coverage does not imply stable nuclides for every element; this is not an exhaustive inventory of excited states or future measurements.

## Default selection and syntax

The default filter includes source-labeled `STABLE` nuclides and half-lives of at least one Julian year (31,557,600 seconds). Upper limits (`LT`/`LE`), unknown lifetimes and inferred lifetimes containing `#` are excluded from this default. Lower limits and approximate values retain their source qualifiers. The current default subset contains 407 records. The stable-only subset contains 244 source-labeled stable entries. Classification reflects the snapshot, not a proof of absolute nuclear stability.

The all-records view also includes short-lived and unknown-lifetime ground states. Use `[175Lu]` or `[175Lu]2O3` for a specific isotope. Use `[180mTa]` or `[178m2Hf]` for isomers: `m` means the first isomeric state and `m2` the second. Ordinary elements retain the original conventional atomic weights; specified nuclides use their own masses and remain separate composition keys.

Nuclear lifetime and chemical stability are different properties. The program does not infer the existence of a compound or the feasibility of a synthesis route from a nuclide's lifetime.

## Sources and conversions

- [NIST Atomic Weights and Isotopic Compositions](https://physics.nist.gov/PhysRefData/Compositions/index.html): 3,352 mass records. These values take precedence to preserve prior calculations; they are not claimed to be the newest evaluations for every nuclide.
- [IAEA LiveChart API](https://iaea-nds.github.io/lc_api_notebook/): 134 supplementary AME2020 mass records and ENSDF ground-state lifetime metadata. The `atomic_mass` field is in micro-u and is divided by 1,000,000.
- [NUBASE2020](https://www-nds.iaea.org/amdc/ame2020/nubase_3.mas20.txt): 12 isomers with half-lives of at least one year. Neutral-atom mass is calculated as `A + mass excess / 931494.10242`, with mass excess in keV and the CODATA2018 conversion constant. Other short-lived isomers are outside this import's scope.

Ta-180's roughly eight-hour ground state remains distinct from long-lived Ta-180m. The NUBASE lower bound for Ta-180m is retained as a lower bound, not relabeled as stable. Source `#` or systematic-estimate mass flags are stored in `Estimated` and shown in the catalog. Central masses are used for weighing calculations; evaluation uncertainties are not propagated. Original snapshots retain the source detail.

## Offline database

The application embeds `CrystalMix/Data/isotopes.json` and requires no runtime network access. The database reuses the existing 118-element symbol map and conventional-weight table. Source archives and import utilities are maintained locally rather than distributed with the application sources. Future data updates must preserve source attribution and review numerical changes.
