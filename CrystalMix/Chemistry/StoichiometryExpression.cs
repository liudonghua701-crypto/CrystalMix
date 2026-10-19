using System.Globalization;

namespace CrystalMix.Chemistry;

// 只解析数值、x 和四则运算，不执行脚本或任意代码。
internal sealed class StoichiometryExpression(string text, double x)
{
    private int index;
    private int depth;
    public double Evaluate()
    {
        if (text.Length > 256 || !double.IsFinite(x)) throw Error();
        var value = Sum();
        if (index != text.Length || !double.IsFinite(value)) throw Error();
        return value;
    }
    private FormatException Error() => new("无效的下标表达式 / Invalid subscript expression: " + text);
    private double Sum()
    {
        var value = Product();
        while (index < text.Length && text[index] is '+' or '-')
        {
            var op = text[index++];
            var right = Product();
            value = op == '+' ? value + right : value - right;
        }
        return value;
    }
    private double Product()
    {
        var value = Atom();
        while (index < text.Length && text[index] is '*' or '/')
        {
            var op = text[index++];
            var right = Atom();
            if (op == '/' && right == 0) throw Error();
            value = op == '*' ? value * right : value / right;
        }
        return value;
    }
    private double Atom()
    {
        if (++depth > 32 || index >= text.Length) throw Error();
        double value;
        var c = text[index];
        if (c is '+' or '-') { index++; value = (c == '-' ? -1 : 1) * Atom(); }
        else if (c == 'x') { index++; value = x; }
        else if (c == '(')
        {
            index++; value = Sum();
            if (index >= text.Length || text[index++] != ')') throw Error();
        }
        else
        {
            var start = index;
            while (index < text.Length && (char.IsAsciiDigit(text[index]) || text[index] == '.')) index++;
            if (!double.TryParse(text[start..index], NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out value)) throw Error();
        }
        depth--;
        return value;
    }
}
