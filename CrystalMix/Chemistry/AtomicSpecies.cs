namespace CrystalMix.Chemistry;

public static class AtomicSpecies
{
    // NIST 相对同位素质量；未收录的核素明确拒绝，不以整数质量数代替。
    // https://physics.nist.gov/PhysRefData/Compositions/index.html
    private static readonly Dictionary<string, double> Isotopes = new()
    {
        ["[1H]"] = 1.00782503223, ["[2H]"] = 2.01410177812,
        ["[3H]"] = 3.0160492779, ["[10B]"] = 10.01293695,
        ["[11B]"] = 11.00930536, ["[12C]"] = 12,
        ["[13C]"] = 13.00335483507, ["[14C]"] = 14.0032419884,
        ["[16O]"] = 15.99491461957, ["[17O]"] = 16.99913175650,
        ["[18O]"] = 17.99915961286, ["[54Fe]"] = 53.93960899,
        ["[56Fe]"] = 55.93493633, ["[57Fe]"] = 56.93539284,
        ["[58Fe]"] = 57.93327443
    };

    public static bool TryGetMass(string species, out double mass) =>
        Isotopes.TryGetValue(species, out mass) || PeriodicTable.TryGetAtomicWeight(species, out mass);

    public static string ElementSymbol(string species) => species.StartsWith('[')
        ? new string(species.Where(char.IsLetter).ToArray()) : species;

    public static string SupportedIsotopes => string.Join(", ", Isotopes.Keys);
}
