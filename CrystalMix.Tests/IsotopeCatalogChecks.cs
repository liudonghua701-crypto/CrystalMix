using CrystalMix;
using CrystalMix.Chemistry;
using CrystalMix.Calculation;

internal static class IsotopeCatalogChecks
{
    public static void AllRecords()
    {
        var all = IsotopeCatalog.All;
        Require(all.Count == 3498 && all.Select(i => i.Symbol).Distinct().Count() == 118, "Catalog coverage");
        Require(all.Count(i => i.Stable) == 244, "Stable snapshot count");
        foreach (var i in all)
        {
            Near(FormulaParser.Parse(i.Species).MolarMass, i.Mass);
            Near(FormulaParser.Parse(i.Species + "2O").MolarMass, 2 * i.Mass + FormulaParser.Parse("O").MolarMass);
            Require(ElementNames.Get(i.Species) == ElementNames.Get(i.Symbol), i.Species + " name");
            var recipe = TargetRecipe.Suggest(TargetFormula.Evaluate(i.Species + "2O", .4));
            Near(RecipeCalculator.Calculate(recipe, 12.34).Sum(r => r.RequiredMass), 12.34);
        }
    }
    public static void Filters()
    {
        Require(IsotopeCatalog.All.Where(i => i.Stable).All(i => i.IsStableOrLongLived()), "Stable filter");
        Require(IsotopeCatalog.Get("[176Lu]")!.IsStableOrLongLived(), "Long lived Lu");
        Require(!IsotopeCatalog.Get("[180Ta]")!.IsStableOrLongLived(), "Ground state Ta");
        Require(IsotopeCatalog.Get("[180mTa]")!.IsStableOrLongLived(), "Isomer Ta");
        Require(IsotopeBrowser.Find("镥", 0).Any(i => i.Species == "[175Lu]") && IsotopeBrowser.Find("镥", 0).All(i => i.Symbol == "Lu"), "Chinese search");
        Require(!IsotopeBrowser.Find("[180Ta]", 0).Any() && IsotopeBrowser.Find("[180Ta]", 2).Count() == 1, "All filter");
        Require(!IsotopeBrowser.Find("Oganesson", 0).Any() && IsotopeBrowser.Find("Oganesson", 2).Any(), "No stable Og");
        var sample = IsotopeCatalog.Get("[176Lu]")!;
        Require(!(sample with { LifeQualifier = "LT" }).IsStableOrLongLived(), "Upper bound excluded");
        Require(!(sample with { HalfLifeSeconds = null }).IsStableOrLongLived(), "Unknown excluded");
        Require((sample with { HalfLifeSeconds = IsotopeCatalog.YearSeconds }).IsStableOrLongLived(), "Inclusive year boundary");
        Near(TargetFormula.Evaluate("[178m2Hf]{1-x}Hf{x}O2", .25).Composition["[178m2Hf]"], .75);
        try { FormulaParser.Parse("[180m9Ta]"); throw new Exception("Unknown isomer accepted"); }
        catch (FormulaException) { }
    }
    public static void ArsenideBalance()
    {
        foreach (double x in new[] { 0d, .1, .4, .5, 1d })
        {
            string[] basis = { "BaAs", "Fe2As", "KAs" };
            try { TargetRecipe.Suggest(TargetFormula.Evaluate("Ba1-xKxFeAs", x), basis); throw new Exception("Impossible balance accepted"); }
            catch (ArgumentException ex) { Require(ex.Message.Contains("As") && ex.Message.Contains("1.5"), ex.Message); }
            var recipe = TargetRecipe.Suggest(TargetFormula.Evaluate("Ba1-xKxFe2As2", x), basis);
            Near(recipe.Where(r => r.Formula == "BaAs").Sum(r => r.MolarRatio), 1-x);
            Near(recipe.Single(r => r.Formula == "Fe2As").MolarRatio, 1);
            Near(recipe.Where(r => r.Formula == "KAs").Sum(r => r.MolarRatio), x);
        }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Near(double actual, double expected) => Require(double.IsFinite(actual) && Math.Abs(actual - expected) < 1e-8, $"{actual} != {expected}");
}
