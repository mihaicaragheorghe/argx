namespace Argx.Parsing;

internal sealed class Token
{
    internal string Value { get; }

    internal Token(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}