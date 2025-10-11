using Argx.Extensions;
using Argx.Parsing;

namespace Argx.Tests.TestUtils;

public static class Create
{
    public static ReadOnlySpan<Token> Tokens(params string[] tokens)
        => tokens.Tokenize();

    public static Token Token(string token) => new(token, TokenType.Argument, 0);
}