namespace Argx.Parsing;

public sealed class Token
{
    internal const int ImplicitPosition = -1;

    public TokenType Type { get; }

    public string Value { get; }

    internal int Position { get; }

    public Token(string value, TokenType type, int position)
    {
        Value = value;
        Type = type;
        Position = position;
    }

    public bool IsImplicit => Position == ImplicitPosition;

    public override string ToString() => Value;

    public static implicit operator string(Token token) => token.Value;

    public override bool Equals(object? obj) => obj is Token token && Value == token.Value;

    public bool Equals(Token? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Token? left, Token? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Token? left, Token? right) => !left?.Equals(right) ?? right is not null;
}