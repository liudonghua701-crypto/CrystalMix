using System.Text.Json;

namespace CrystalMix.Chemistry;

public sealed record IsotopeInfo(string Species, string Symbol, int AtomicNumber, int MassNumber,
    double Mass, bool Stable, double? HalfLifeSeconds, string HalfLifeText, string LifeQualifier,
    bool Estimated, string MassSource, string LifeSource)
{
    public bool IsStableOrLongLived(double minimumSeconds = IsotopeCatalog.YearSeconds) => Stable ||
        (HalfLifeSeconds >= minimumSeconds && LifeQualifier is not "LT" and not "LE" && !HalfLifeText.Contains('#'));
}

public static class IsotopeCatalog
{
    public const double YearSeconds = 365.25 * 86400;
    private static readonly IReadOnlyDictionary<string, IsotopeInfo> Lookup = Load();
    public static IReadOnlyCollection<IsotopeInfo> All { get; } = Array.AsReadOnly(Lookup.Values
        .OrderBy(i => i.AtomicNumber).ThenBy(i => i.MassNumber).ThenBy(i => i.Species).ToArray());
    public static IsotopeInfo? Get(string species) => Lookup.GetValueOrDefault(species);

    private static IReadOnlyDictionary<string, IsotopeInfo> Load()
    {
        using var stream = typeof(IsotopeCatalog).Assembly.GetManifestResourceStream("CrystalMix.Data.isotopes.json")
            ?? throw new InvalidDataException("Isotope catalog resource missing.");
        var rows = JsonSerializer.Deserialize<IsotopeInfo[]>(stream)
            ?? throw new InvalidDataException("Empty isotope catalog.");
        foreach (var row in rows)
            if (!PeriodicTable.TryGetAtomicWeight(row.Symbol, out _) || !double.IsFinite(row.Mass) || row.Mass <= 0
                || row.MassNumber < row.AtomicNumber || Math.Abs(row.Mass - row.MassNumber) > 1)
                throw new InvalidDataException("Invalid isotope record: " + row.Species);
        return rows.ToDictionary(r => r.Species, StringComparer.Ordinal);
    }
}
