using CrystalMix.Calculation;
using CrystalMix.Chemistry;

var tests = new List<(string Name, Action Run)>
{
    ("Bilingual names", () => { if (ElementNames.Get("Fe") != "铁 / Iron" || ElementNames.Get("[57Fe]") != "铁 / Iron" || ElementNames.Get("Og") != "鿫 / Oganesson") throw new Exception("Names"); }),
    ("Pure isotope", () => Near(FormulaParser.Parse("[57Fe]").MolarMass, 56.93539284)),
    ("Isotope compound", () => Near(FormulaParser.Parse("[2H]2O").MolarMass, 20.02720355624)),
    ("Mixed isotope composition", () => { var c = FormulaParser.Parse("Fe[57Fe]2O3").Composition; Near(c["Fe"], 1); Near(c["[57Fe]"], 2); }),
    ("Unknown isotope", () => Throws<FormulaException>(() => FormulaParser.Parse("[999Fe]"))),
    ("Empty group", () => Throws<FormulaException>(() => FormulaParser.Parse("Fe()"))),
    ("Doping endpoints and intermediate x", () => {
        foreach (var x in new[] { 0d, .1, .4, .5, 1d }) {
            var t = TargetFormula.Evaluate("Ba1-xKxFe2As2", x);
            Near(t.Composition.GetValueOrDefault("Ba"), 1-x); Near(t.Composition.GetValueOrDefault("K"), x);
            Near(t.Composition["Fe"], 2); Near(t.Composition["As"], 2);
            Near(RecipeCalculator.Calculate(TargetRecipe.Suggest(t), 10).Sum(r => r.RequiredMass), 10);
            var parents = TargetRecipe.Suggest(t, new[] { "BaFe2As2", "KFe2As2" });
            Near(parents.Where(p => p.Formula == "BaFe2As2").Sum(p => p.MolarRatio), 1-x);
            Near(parents.Where(p => p.Formula == "KFe2As2").Sum(p => p.MolarRatio), x);
        }
    }),
    ("Explicit variable arithmetic", () => Near(TargetFormula.Evaluate("Ba{1-x}K{(2*x)/2}Fe2As2", .4).Composition["K"], .4)),
    ("Variable isotope compound", () => Near(TargetFormula.Evaluate("[57Fe]{1-x}Fe{x}Se", .25).Composition["[57Fe]"], .75)),
    ("Invalid variables", () => {
        foreach (var f in new[] { "Ba{y}Fe2", "Ba{1/0}", "Ba{x-1}", "Ba{x+}", "Ba{}", "Ba{(x}" })
            Throws<FormatException>(() => TargetFormula.Evaluate(f, .4));
        Throws<FormatException>(() => TargetFormula.Evaluate("Ba1-xKx", 1.1));
        Throws<FormatException>(() => TargetFormula.Evaluate("Ba", double.NaN));
    }),
    ("Unbalanced or dependent materials", () => {
        var t = TargetFormula.Evaluate("BaFe2As2", 0);
        Throws<ArgumentException>(() => TargetRecipe.Suggest(t, new[] { "Ba", "Fe" }));
        Throws<ArgumentException>(() => TargetRecipe.Suggest(t, new[] { "Ba", "Ba", "FeAs" }));
    }),
    ("Isotope mass closure", () => Near(RecipeCalculator.Calculate(new[] { new RecipeInput("", "[57Fe]", 2), new RecipeInput("", "[2H]2O", 3) }, 12.34).Sum(p => p.RequiredMass), 12.34)),
    ("All existing elements have bilingual names", () => {
        var weights = (IReadOnlyDictionary<string, double>)typeof(PeriodicTable).GetField("Weights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.GetValue(null)!;
        foreach (var symbol in weights.Keys) if (string.IsNullOrWhiteSpace(ElementNames.Get(symbol))) throw new Exception(symbol);
    }),
    ("Malformed formula regression", () => {
        foreach (var f in new[] { "", "fe", "Fe0", "Fe-1", "[57Fe", "[57Xx]", "Fe..2", "CuSO4·", "()", "H2O)", "Fe2+" })
            Throws<FormulaException>(() => FormulaParser.Parse(f));
    }),
    ("Target ordinary formula regression", () => {
        foreach (var f in new[] { "FeSe", "Ca(OH)2", "K4[Fe(CN)6]", "CuSO4·5H2O", "Fe1.03Te", "[2H]2O" })
            Near(TargetFormula.Evaluate(f, .4).MolarMass, FormulaParser.Parse(f).MolarMass);
    }),
    ("Isotope grouped hydrate", () => Near(FormulaParser.Parse("Ca([2H]O)2·2[2H]2O").Composition["[2H]"], 6)),
    ("Zero target rejected", () => Throws<FormulaException>(() => TargetFormula.Evaluate("Fe{x}", 0))),
    ("Nonfinite recipe rejected", () => Throws<ArgumentOutOfRangeException>(() => RecipeCalculator.Calculate(new[] {new RecipeInput("", "Fe", double.MaxValue)}, 1))),
    ("Bracket count is not an isotope", () => Near(FormulaParser.Parse("[Fe57]").MolarMass, 57 * 55.845)),
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

if (args.Contains("--ui")) tests.Add(("UI layout and target workflow", UiChecks.Run));

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
    if (!double.IsFinite(actual) || Math.Abs(actual - expected) > tolerance)
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
