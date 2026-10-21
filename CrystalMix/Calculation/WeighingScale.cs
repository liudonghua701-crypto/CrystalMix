namespace CrystalMix.Calculation;

public sealed record WeighingScaleResult(double OriginalTotal, double TotalMass,
    double ScaleFactor, int AnchorIndex, IReadOnlyList<RecipeResult> Materials);

public static class WeighingScale
{
    // 以一个实称质量为基准缩放整批，保留所有原料的摩尔份数。
    public static WeighingScaleResult Calculate(IReadOnlyList<RecipeInput> inputs,
        double originalTotal, int anchorIndex, double actualMass)
    {
        if (anchorIndex < 0 || anchorIndex >= inputs.Count)
            throw new ArgumentOutOfRangeException(nameof(anchorIndex));
        if (!double.IsFinite(actualMass) || actualMass <= 0)
            throw new ArgumentOutOfRangeException(nameof(actualMass));
        var original = RecipeCalculator.Calculate(inputs, originalTotal);
        var fraction = original[anchorIndex].MassFraction;
        var total = actualMass / fraction;
        var factor = total / originalTotal;
        if (!double.IsFinite(total) || total <= 0 || !double.IsFinite(factor) || factor <= 0)
            throw new ArgumentOutOfRangeException(nameof(actualMass));
        var scaled = RecipeCalculator.Calculate(inputs, total);
        if (scaled.Any(r => !double.IsFinite(r.RequiredMass) || r.RequiredMass <= 0) ||
            !double.IsFinite(scaled.Sum(r => r.RequiredMass)))
            throw new ArgumentOutOfRangeException(nameof(actualMass));
        return new(originalTotal, total, factor, anchorIndex, scaled);
    }
}
