# Scale a recipe from an actual weighed mass

Available in Android 1.2.0 and Windows 1.2.0. Use this when a chunk, pellet or other material cannot conveniently be weighed to the original target mass.

1. Prepare a valid recipe with the desired material formulas, molar parts and batch mass.
2. On Android, press **Use actual mass** below the measured material. On Windows, press **Use actual mass** and choose the measured material from the list.
3. Enter the actual mass in **grams**. The preview shows the scale factor, old/new batch totals and old/new mass for every material.
4. Choose **Apply scale**. The recipe keeps its formulas and molar parts and updates the batch mass and weighing targets. **Cancel** leaves the recipe unchanged.

If a material was planned at 2.000 g but actually weighs 2.080 g, the scale factor is 2.080 / 2.000 = 1.04. Another material planned at 3.000 g becomes 3.120 g. The 5.000 g batch becomes 5.200 g.

For anchor material j, the new mass for material i is:

`new_mass_i = actual_mass_j * mass_fraction_i / mass_fraction_j`

This preserves the selected molar ratios, including compounds and explicitly specified isotopes. Duplicate formulas are separate rows; select the correct row. Inputs must be finite and positive. Invalid or overflowing inputs cannot be applied.

Each operation uses **one measured material as its basis**. It does not lock several measured masses or calculate how much to add to material already on a balance. Other rows show new total targets, not additional amounts. Recheck other already-weighed materials against those targets. Later edits or another scaling operation calculate a new plan; the previous anchor is not permanently locked.

Scaling does not change the target formula, choose a synthesis route, or compensate for purity, isotope enrichment or processing losses. Android saves the updated batch total in the existing private recipe draft. CSV exports use the updated targets.
