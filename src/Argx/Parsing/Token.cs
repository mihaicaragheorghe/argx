namespace Argx.Parsing;

public sealed class Token
{
    public string Value { get; }

    public Token(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}