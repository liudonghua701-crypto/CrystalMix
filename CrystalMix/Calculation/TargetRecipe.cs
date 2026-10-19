using CrystalMix.Chemistry;

namespace CrystalMix.Calculation;

public static class TargetRecipe
{
    // 只有用户选定的原料列独立且能精确配平时才返回建议；不推断合成路线。
    public static IReadOnlyList<RecipeInput> Suggest(FormulaResult target, string[]? materials = null)
    {
        materials = materials is { Length: > 0 } ? materials : target.Composition.Keys.ToArray();
        if (materials.Length > 32) throw new ArgumentException("最多 32 种原料 / At most 32 materials.");
        var parsed = materials.Select(f => FormulaParser.Parse(f).Composition).ToArray();
        var species = target.Composition.Keys.Concat(parsed.SelectMany(p => p.Keys)).Distinct().ToArray();
        int n = materials.Length;
        var a = new double[species.Length, n];
        var b = species.Select(s => target.Composition.GetValueOrDefault(s)).ToArray();
        for (int r = 0; r < species.Length; r++)
            for (int c = 0; c < n; c++) a[r, c] = parsed[c].GetValueOrDefault(species[r]);
        var conflict = BalanceDiagnostics.Find(a, b, species);
        if (conflict is not null) throw new ArgumentException(conflict);
        var system = new double[n, n + 1];
        for (int i = 0; i < n; i++)
            for (int r = 0; r < species.Length; r++)
            {
                for (int j = 0; j < n; j++) system[i, j] += a[r, i] * a[r, j];
                system[i, n] += a[r, i] * b[r];
            }
        for (int c = 0; c < n; c++)
        {
            int pivot = c;
            for (int r = c + 1; r < n; r++)
                if (Math.Abs(system[r, c]) > Math.Abs(system[pivot, c])) pivot = r;
            if (Math.Abs(system[pivot, c]) < 1e-10)
                throw new ArgumentException("所选原料配比不唯一，请减少相关原料 / Selected materials are dependent; reduce the basis.");
            for (int j = c; j <= n; j++)
                (system[c, j], system[pivot, j]) = (system[pivot, j], system[c, j]);
            var divisor = system[c, c];
            for (int j = c; j <= n; j++) system[c, j] /= divisor;
            for (int r = 0; r < n; r++)
            {
                if (r == c) continue;
                var factor = system[r, c];
                for (int j = c; j <= n; j++) system[r, j] -= factor * system[c, j];
            }
        }
        var ratios = Enumerable.Range(0, n).Select(i => system[i, n]).ToArray();
        if (ratios.Any(v => !double.IsFinite(v) || v < -1e-9)) throw Unbalanced();
        for (int r = 0; r < species.Length; r++)
            if (Math.Abs(Enumerable.Range(0, n).Sum(c => a[r, c] * ratios[c]) - b[r]) > 1e-8 * Math.Max(1, b[r]))
                throw Unbalanced();
        return materials.Select((f, i) => new RecipeInput(ElementNames.Get(f) ?? f, f, Math.Max(0, ratios[i])))
            .Where(p => p.MolarRatio > 1e-12).ToArray();
    }
    private static ArgumentException Unbalanced() => new("所选原料不能配平目标组成 / Selected materials cannot balance the target composition.");
}
