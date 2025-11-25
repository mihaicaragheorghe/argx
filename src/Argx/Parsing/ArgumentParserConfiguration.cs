using Argx.Help;

namespace Argx.Parsing;

/// <summary>
/// Configuration settings for the Argument Parser.
/// </summary>
public class ArgumentParserConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically add a help argument (-h/--help).
    /// </summary>
    public bool AddHelpArgument { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the parser should exit on error.
    /// </summary>
    public bool ExitOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets the exit code to use when an error occurs and ExitOnError is true.
    /// </summary>
    public int ErrorExitCode { get; set; } = 1;

    public static ArgumentParserConfiguration Default() => new();
}