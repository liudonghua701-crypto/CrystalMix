using CrystalMix.Chemistry;

namespace CrystalMix.Calculation;

public sealed record RecipeInput(string Name, string Formula, double MolarRatio);

public sealed record RecipeResult(string Name, string Formula, double MolarRatio,
    double MolarMass, double RelativeMass, double MassFraction, double RequiredMass);

public static class RecipeCalculator
{
    public static IReadOnlyList<RecipeResult> Calculate(
        IReadOnlyList<RecipeInput> inputs, double totalMass)
    {
        if (inputs.Count == 0)
            throw new ArgumentException("At least one material is required.", nameof(inputs));
        if (!double.IsFinite(totalMass) || totalMass <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalMass));

        var parsed = inputs.Select(input =>
        {
            if (!double.IsFinite(input.MolarRatio) || input.MolarRatio <= 0)
                throw new ArgumentOutOfRangeException(nameof(input.MolarRatio));
            var molarMass = FormulaParser.Parse(input.Formula).MolarMass;
            return (Input: input, MolarMass: molarMass,
                RelativeMass: input.MolarRatio * molarMass);
        }).ToList();

        var sum = parsed.Sum(x => x.RelativeMass);
        if (!double.IsFinite(sum) || sum <= 0) throw new ArgumentOutOfRangeException(nameof(inputs));
        return parsed.Select(x =>
        {
            var fraction = x.RelativeMass / sum;
            return new RecipeResult(x.Input.Name, x.Input.Formula, x.Input.MolarRatio,
                x.MolarMass, x.RelativeMass, fraction, totalMass * fraction);
        }).ToList();
    }
}
