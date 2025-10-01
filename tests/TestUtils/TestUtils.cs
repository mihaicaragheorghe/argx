using Argx.Extensions;
using Argx.Parsing;

namespace Argx.Tests.TestUtils;

public static class Create
{
    public static ReadOnlySpan<Token> Tokens(params string[] tokens)
        => tokens.Tokenize();
}