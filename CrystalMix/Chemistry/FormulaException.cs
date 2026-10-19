namespace CrystalMix.Chemistry;

public sealed class FormulaException : Exception
{
    public FormulaException(string code, int position, string? detail = null)
        : base(detail ?? code)
    {
        Code = code;
        Position = position;
        Detail = detail;
    }

    public string Code { get; }
    public int Position { get; }
    public string? Detail { get; }
}
