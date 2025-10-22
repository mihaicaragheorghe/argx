namespace Argx.Parsing;

/// <summary>
/// Represents a token in the command-line input.
/// </summary>
public sealed class Token
{
    internal const int ImplicitPosition = -1;

    /// <summary>
    /// The type of the token.
    /// </summary>
    public TokenType Type { get; }

    /// <summary>
    /// The string value of the token.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The position of the token in the input.
    /// </summary>
    internal int Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> class.
    /// </summary>
    /// <param name="value">The string value of the token.</param>
    /// <param name="type">The type of the token.</param>
    /// <param name="position">The position of the token in the input.</param>
    public Token(string value, TokenType type, int position)
    {
        Value = value;
        Type = type;
        Position = position;
    }

    internal bool IsImplicit => Position == ImplicitPosition;

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Token"/> to its string value.
    /// </summary>
    /// <param name="token">The token to convert.</param>
    /// <returns>The string value of the token.</returns>
    public static implicit operator string(Token token) => token.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Token token && Value == token.Value;

    /// <summary>
    /// Determines whether this token is equal to another token.
    /// </summary>
    /// <param name="other">The other token to compare.</param>
    /// <returns>><c>true</c> if the tokens have the same value; otherwise, <c>false</c>.</returns>
    public bool Equals(Token? other) => other is not null && Value == other.Value;

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Determines whether two <see cref="Token"/> instances are equal.
    /// </summary>
    /// <param name="left">The left token.</param>
    /// <param name="right">The right token.</param>
    /// <returns>><c>true</c> if the tokens have the same value; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Token? left, Token? right) => left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines whether two <see cref="Token"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left token.</param>
    /// <param name="right">The right token.</param>
    /// <returns><c>true</c> if the tokens have different values; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Token? left, Token? right) => !left?.Equals(right) ?? right is not null;
}