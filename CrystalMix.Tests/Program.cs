using CrystalMix.Calculation;
using CrystalMix.Chemistry;

var tests = new List<(string Name, Action Run)>
{
    ("Element Fe", () => Near(FormulaParser.Parse("Fe").MolarMass, 55.845)),
    ("Binary FeSe", () => Near(FormulaParser.Parse("FeSe").MolarMass, 134.816)),
    ("Parentheses Ca(OH)2", () => Near(FormulaParser.Parse("Ca(OH)2").MolarMass, 74.092)),
    ("Nested brackets K4[Fe(CN)6]", () => Near(FormulaParser.Parse("K4[Fe(CN)6]").MolarMass, 368.3462)),
    ("Decimal Fe1.03Te", () => Near(FormulaParser.Parse("Fe1.03Te").MolarMass, 185.12035)),
    ("Hydrate CuSO4·5H2O", () => Near(FormulaParser.Parse("CuSO4·5H2O").MolarMass, 249.677, 1e-6)),
    ("Leading hydrate coefficient", () => Near(FormulaParser.Parse("2H2O").MolarMass, 36.03)),
    ("Unknown element rejected", () => Throws<FormulaException>(() => FormulaParser.Parse("Xx2"))),
    ("Mismatched bracket rejected", () => Throws<FormulaException>(() => FormulaParser.Parse("Ca(OH]2"))),
    ("Recipe mass closes", () =>
    {
        var result = RecipeCalculator.Calculate(new[]
        {
            new RecipeInput("Iron", "Fe", 1),
            new RecipeInput("Selenium", "Se", 1)
        }, 10);
        Near(result.Sum(x => x.RequiredMass), 10);
        Near(result[0].RequiredMass, 10 * 55.845 / (55.845 + 78.971));
    }),
    ("Arbitrary molar ratios", () =>
    {
        var result = RecipeCalculator.Calculate(new[]
        {
            new RecipeInput("A", "Ba", 1),
            new RecipeInput("B", "FeAs", 2),
        }, 25);
        Near(result.Sum(x => x.MassFraction), 1);
        Near(result.Sum(x => x.RequiredMass), 25);
    })
};

var failed = 0;
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS  {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.WriteLine($"FAIL  {test.Name}: {ex.Message}");
    }
}

Console.WriteLine($"{tests.Count - failed}/{tests.Count} tests passed.");
return failed == 0 ? 0 : 1;

static void Near(double actual, double expected, double tolerance = 1e-8)
{
    if (Math.Abs(actual - expected) > tolerance)
        throw new Exception($"Expected {expected:R}; got {actual:R}.");
}

static void Throws<T>(Action action) where T : Exception
{
    try
    {
        action();
    }
    catch (T)
    {
        return;
    }
    throw new Exception($"Expected {typeof(T).Name}.");
}
