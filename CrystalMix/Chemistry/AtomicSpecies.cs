namespace CrystalMix.Chemistry;

public static class AtomicSpecies
{
    // 元素平均原子量沿用原表；指定核素质量来自内嵌的可追溯数据快照。
    public static bool TryGetMass(string species, out double mass) =>
        (mass = IsotopeCatalog.Get(species)?.Mass ?? 0) > 0 || PeriodicTable.TryGetAtomicWeight(species, out mass);

    public static string ElementSymbol(string species) => species.StartsWith('[')
        ? IsotopeCatalog.Get(species)?.Symbol ?? species : species;

    public static string SupportedIsotopes => string.Join(", ", IsotopeCatalog.All.Select(i => i.Species));
}
