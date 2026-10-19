using System.Globalization;

namespace CrystalMix.Chemistry;

public sealed record FormulaResult(double MolarMass, IReadOnlyDictionary<string, double> Composition);

public static class FormulaParser
{
    public static FormulaResult Parse(string? formula)
    {
        var source = (formula ?? string.Empty).Trim().Replace(" ", string.Empty);
        if (source.Length == 0)
            throw new FormulaException("empty", 0);

        var total = new Dictionary<string, double>(StringComparer.Ordinal);
        var segments = source.Split(new[] { '·', '•' }, StringSplitOptions.None);
        var sourceOffset = 0;
        foreach (var segment in segments)
        {
            if (segment.Length == 0)
                throw new FormulaException("unexpected", sourceOffset);

            var reader = new Reader(segment, sourceOffset);
            var leadingMultiplier = reader.ReadOptionalNumber(1d);
            if (leadingMultiplier <= 0)
                throw new FormulaException("badNumber", sourceOffset);

            var composition = reader.ParseGroup(null);
            if (!reader.AtEnd)
                throw new FormulaException("unexpected", reader.AbsolutePosition, reader.Current.ToString());
            if (composition.Count == 0)
                throw new FormulaException("empty", sourceOffset);

            Merge(total, composition, leadingMultiplier);
            sourceOffset += segment.Length + 1;
        }

        double mass = 0;
        foreach (var pair in total)
        {
            if (!PeriodicTable.TryGetAtomicWeight(pair.Key, out var atomicWeight))
                throw new FormulaException("unknownElement", 0, pair.Key);
            mass += atomicWeight * pair.Value;
        }

        return new FormulaResult(mass, total);
    }

    private static void Merge(IDictionary<string, double> target,
        IReadOnlyDictionary<string, double> source, double multiplier)
    {
        foreach (var pair in source)
        {
            target.TryGetValue(pair.Key, out var existing);
            target[pair.Key] = existing + pair.Value * multiplier;
        }
    }

    private sealed class Reader
    {
        private readonly string _text;
        private readonly int _offset;
        private int _index;

        public Reader(string text, int offset)
        {
            _text = text;
            _offset = offset;
        }

        public bool AtEnd => _index >= _text.Length;
        public char Current => AtEnd ? '\0' : _text[_index];
        public int AbsolutePosition => _offset + _index;

        public Dictionary<string, double> ParseGroup(char? expectedCloser)
        {
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            while (!AtEnd)
            {
                var c = Current;
                if (c is ')' or ']')
                {
                    if (expectedCloser is null || c != expectedCloser)
                        throw new FormulaException("mismatchedBracket", AbsolutePosition, c.ToString());
                    _index++;
                    return result;
                }

                if (c is '(' or '[')
                {
                    _index++;
                    var closer = c == '(' ? ')' : ']';
                    var inner = ParseGroup(closer);
                    var multiplier = ReadOptionalNumber(1d);
                    ValidateMultiplier(multiplier);
                    Merge(result, inner, multiplier);
                    continue;
                }

                if (!char.IsUpper(c))
                    throw new FormulaException("expectedElement", AbsolutePosition, c.ToString());

                var start = _index++;
                while (!AtEnd && char.IsLower(Current))
                    _index++;
                var symbol = _text[start.._index];
                if (!PeriodicTable.TryGetAtomicWeight(symbol, out _))
                    throw new FormulaException("unknownElement", _offset + start, symbol);

                var count = ReadOptionalNumber(1d);
                ValidateMultiplier(count);
                result[symbol] = result.GetValueOrDefault(symbol) + count;
            }

            if (expectedCloser is not null)
                throw new FormulaException("missingBracket", AbsolutePosition, expectedCloser.ToString());
            return result;
        }

        public double ReadOptionalNumber(double defaultValue)
        {
            var start = _index;
            var seenDot = false;
            while (!AtEnd && (char.IsDigit(Current) || Current == '.'))
            {
                if (Current == '.')
                {
                    if (seenDot) break;
                    seenDot = true;
                }
                _index++;
            }

            if (start == _index)
                return defaultValue;

            var token = _text[start.._index];
            if (!double.TryParse(token, NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var value))
                throw new FormulaException("badNumber", _offset + start, token);
            return value;
        }

        private void ValidateMultiplier(double value)
        {
            if (!double.IsFinite(value) || value <= 0)
                throw new FormulaException("badNumber", AbsolutePosition);
        }
    }
}
