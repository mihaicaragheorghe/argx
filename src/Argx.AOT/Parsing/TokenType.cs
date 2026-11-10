namespace Argx.Parsing;

/// <summary>
/// Represents the type of a parsed command-line token.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// A positional argument (e.g., <c>input.txt</c>).
    /// </summary>
    Argument,

    /// <summary>
    /// An optional argument or flag (e.g., <c>--verbose</c> or <c>-v</c>).
    /// </summary>
    Option,

    /// <summary>
    /// A separator token, typically <c>--</c>, indicating the end of options.
    /// </summary>
    Separator
}