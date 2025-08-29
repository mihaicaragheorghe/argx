namespace Argx.Parsing;

public sealed class Token
{
    public string Value { get; }

    public Token(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Token token) => token.Value;

    public override bool Equals(object? obj) => obj is Token token && Value == token.Value;

    public bool Equals(Token? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Token? left, Token? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Token? left, Token? right) => !left?.Equals(right) ?? right is not null;
}