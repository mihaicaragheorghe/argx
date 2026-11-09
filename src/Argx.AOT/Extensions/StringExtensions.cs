using System.Text.RegularExpressions;

namespace Argx.Extensions;

internal static class StringExtensions
{
    internal static bool IsOption(this string s) => s.Length > 0 && s[0] == '-' && s != "--";

    internal static bool IsSeparator(this string s) => s == "--";

    internal static bool IsPositional(string s) => s.Length > 0 && s[0] != '-';

    internal static string PascalToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return Regex.Replace(
            value,
            "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
            "-$1",
            RegexOptions.Compiled)
            .Trim()
            .ToLower();
    }
}