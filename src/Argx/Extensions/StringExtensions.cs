namespace Argx.Extensions;

internal static class StringExtensions
{
    internal static bool IsOption(this string s) => s.Length > 0 && s[0] == '-' && s != "--";

    internal static bool IsSeparator(this string s) => s == "--";
    
    internal static bool IsPositional(string s) => s.Length > 0 && s[0] != '-';
}