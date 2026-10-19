using System.Globalization;
using System.Text.RegularExpressions;

namespace CrystalMix.Chemistry;

public static class TargetFormula
{
    public static FormulaResult Evaluate(string formula, double x)
    {
        if (!double.IsFinite(x) || x < 0 || x > 1)
            throw new FormatException("x 必须在 0 到 1 之间 / x must be between 0 and 1.");
        if (formula.Length > 4096) throw new FormatException("Formula too long.");
        var source = formula.Replace(" ", "");
        var evaluated = Regex.Replace(source,
            @"(\[\d+[A-Z][a-z]?\]|[A-Z][a-wyz]?|[)\]])(\{[^{}]*\}|[0-9.x+*/-]+)", match =>
            {
                var expression = match.Groups[2].Value.Trim('{', '}');
                var count = new StoichiometryExpression(expression, x).Evaluate();
                if (count < 0 || count > 1e12)
                    throw new FormatException("下标超出范围 / Subscript out of range.");
                return match.Groups[1].Value + count.ToString("0.############################", CultureInfo.InvariantCulture);
            });
        return FormulaParser.Parse(evaluated, allowZero: true);
    }
}
