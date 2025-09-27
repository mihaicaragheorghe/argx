using Argx.Parsing;

namespace Argx.Extensions;

internal static class StringArrayExtensions
{
    internal static ReadOnlySpan<Token> Tokenize(this string[] arr)
    {
        var tokens = new Token[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            tokens[i] = new Token(arr[i]);
        }

        return tokens.AsSpan();
    }
}