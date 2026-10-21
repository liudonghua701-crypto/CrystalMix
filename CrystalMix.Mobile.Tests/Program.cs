using CrystalMix;
using CrystalMix.Chemistry;
using CrystalMix.Calculation;
using System.Text.RegularExpressions;

int checks = WeighingTests.Run();
void Require(bool value, string detail) { checks++; if (!value) throw new Exception(detail); }
void Near(double a, double b) => Require(double.IsFinite(a) && Math.Abs(a - b) < 1e-8, $"{a} != {b}");
void Reject(Action action) { try { action(); } catch (Exception e) when (e is ArgumentException or FormulaException or FormatException) { checks++; return; } throw new Exception("Invalid input accepted"); }
Require(IsotopeCatalog.All.Count == 3498, "Catalog coverage");
Require(IsotopeCatalog.All.Count(i => i.IsStableOrLongLived()) == 407, "Default filter");
foreach (var isotope in IsotopeCatalog.All)
{
    Near(FormulaParser.Parse(isotope.Species + "2O").MolarMass, 2 * isotope.Mass + 15.999);
    Require(ElementNames.Get(isotope.Species) == ElementNames.Get(isotope.Symbol), "Isotope name");
}
Near(FormulaParser.Parse("[175Lu]2O3").MolarMass, 397.8785504);
Near(FormulaParser.Parse("CuSO4·5H2O").Composition["H"], 10);
Near(FormulaParser.Parse("K4[Fe(CN)6]").Composition["C"], 6);
Near(FormulaParser.Parse("Fe1.03Te").Composition["Fe"], 1.03);
Near(TargetFormula.Evaluate("[178m2Hf]{1-x}Hf{x}O2", .25).Composition["[178m2Hf]"], .75);
foreach (var x in new[] { 0d, .1, .4, .5, 1d })
{
    string[] basis = { "BaAs", "Fe2As", "KAs" };
    var recipe = TargetRecipe.Suggest(TargetFormula.Evaluate("Ba1-xKxFe2As2", x), basis);
    Near(recipe.Where(r => r.Formula == "BaAs").Sum(r => r.MolarRatio), 1-x);
    Near(recipe.Where(r => r.Formula == "Fe2As").Sum(r => r.MolarRatio), 1);
    Near(recipe.Where(r => r.Formula == "KAs").Sum(r => r.MolarRatio), x);
    Near(RecipeCalculator.Calculate(recipe, 12.34).Sum(r => r.RequiredMass), 12.34);
    Reject(() => TargetRecipe.Suggest(TargetFormula.Evaluate("Ba1-xKxFeAs", x), basis));
}
foreach (var formula in new[] { "", "Xx", "Fe()", "[999Lu]", "[180m9Ta]", "Ca(OH", "CuSO4··H2O" })
    Reject(() => FormulaParser.Parse(formula));
Reject(() => TargetFormula.Evaluate("Ba{x/0}", .4));
Reject(() => TargetFormula.Evaluate("Ba1-xKx", double.NaN));
Reject(() => RecipeCalculator.Calculate(new[] { new RecipeInput("", "Fe", -1) }, 10));
Reject(() => RecipeCalculator.Calculate(new[] { new RecipeInput("", "Fe", 1) }, double.PositiveInfinity));
var help = new Localizer { Language = UiLanguage.English }["helpBody"] + AdvancedHelp.Get(UiLanguage.English);
Require(!Regex.IsMatch(help, @"\p{IsCJKUnifiedIdeographs}"), "English help language leak");
Console.WriteLine($"PASS: {checks} mobile shared-core assertions; all 3498 catalog records checked.");
