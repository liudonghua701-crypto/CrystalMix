using CrystalMix.Calculation;

public static class WeighingTests
{
    public static int Run()
    {
        int checks = 0;
        void Near(double a, double b)
        {
            checks++;
            if (!double.IsFinite(a) || Math.Abs(a-b) > 1e-10 * Math.Max(1, Math.Abs(b)))
                throw new Exception($"Weighing mismatch: {a} != {b}");
        }
        void Reject(Action test)
        {
            try { test(); } catch (ArgumentException) { checks++; return; }
            throw new Exception("Invalid weighing accepted");
        }
        var inputs = new[] { new RecipeInput("chunk", "Fe", 1), new RecipeInput("powder", "Se", 1) };
        var original = RecipeCalculator.Calculate(inputs, 10);
        foreach (double factor in new[] { .5, 1, 1.125, 2 })
        foreach (int anchor in new[] { 0, 1 })
        {
            var actual = original[anchor].RequiredMass * factor;
            var result = WeighingScale.Calculate(inputs, 10, anchor, actual);
            Near(result.TotalMass, 10*factor); Near(result.ScaleFactor, factor);
            Near(result.Materials[anchor].RequiredMass, actual);
            Near(result.Materials.Sum(r => r.RequiredMass), result.TotalMass);
            for (int i=0; i<inputs.Length; i++)
            {
                Near(result.Materials[i].MolarRatio, inputs[i].MolarRatio);
                Near(result.Materials[i].RequiredMass, original[i].RequiredMass*factor);
            }
        }
        var isotopes = new[] { new RecipeInput("", "[175Lu]2O3", 2), new RecipeInput("", "Fe2O3", 3) };
        var isotopeResult = WeighingScale.Calculate(isotopes, 10, 0, 1.234);
        Near(isotopeResult.Materials[0].RequiredMass, 1.234);
        Near(isotopeResult.Materials.Sum(r => r.RequiredMass), isotopeResult.TotalMass);
        var duplicates = new[] { inputs[0], inputs[0] with { MolarRatio = 2 } };
        Near(WeighingScale.Calculate(duplicates, 10, 1, 4).Materials[0].RequiredMass, 2);
        Near(WeighingScale.Calculate(new[] { inputs[0] }, 10, 0, 3).TotalMass, 3);
        foreach(double bad in new[] { 0, -1, double.NaN, double.PositiveInfinity, double.MaxValue })
            Reject(() => WeighingScale.Calculate(inputs, 10, 0, bad));
        Reject(() => WeighingScale.Calculate(inputs, 10, -1, 2));
        Reject(() => WeighingScale.Calculate(inputs, 10, 2, 2));
        Reject(() => WeighingScale.Calculate(inputs, 0, 0, 2));
        Reject(() => WeighingScale.Calculate(Array.Empty<RecipeInput>(), 10, 0, 2));
        Near(original.Sum(r => r.RequiredMass), 10);
        return checks;
    }
}
