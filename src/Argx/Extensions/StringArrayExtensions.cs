using Argx.Parsing;

namespace Argx.Extensions;

internal static class StringArrayExtensions
{
    internal static ReadOnlySpan<Token> Tokenize(this string[] arr)
    {
        var tokens = new Token[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            tokens[i] = new Token(value: arr[i], type: ParseTokenType(arr[i]), position: i);
        }

        return tokens;
    }

    private static TokenType ParseTokenType(string value)
    {
        if (value.IsSeparator())
            return TokenType.Separator;

        return value.IsOption() ? TokenType.Option : TokenType.Argument;
    }
}