namespace Argx.Help;

/// <summary>
/// Configuration settings for generating help messages.
/// </summary>
public class HelpConfiguration
{
    /// <summary>
    /// The spacing between sections in the help message. Defaults to two new lines.
    /// </summary>
    public string SectionSpacing { get; set; } = Environment.NewLine + Environment.NewLine;

    /// <summary>
    /// The number of spaces to indent help message lines. Defaults to 2.
    /// </summary>
    public int IndentSize { get; set; } = DefaultIndentSize;

    /// <summary>
    /// The maximum line width for help messages. Defaults to 80. Text will be wrapped accordingly.
    /// </summary>
    public int MaxLineWidth { get; set; } = DefaultMaxLineWidth;

    /// <summary>
    /// Indicates whether to use argument aliases in the usage text instead of the primary argument names (when available).
    /// Defaults to false.
    /// </summary>
    public bool UseAliasInUsageText { get; set; } = false;

    /// <summary>
    /// The default indent size for help message lines.
    /// </summary>
    public const int DefaultIndentSize = 2;

    /// <summary>
    /// The default maximum line width for help messages.
    /// </summary>
    public const int DefaultMaxLineWidth = 80;

    /// <summary>
    /// Creates a new instance of <see cref="HelpConfiguration"/> with default settings.
    /// </summary>
    public static HelpConfiguration Default()
    {
        return new HelpConfiguration();
    }
}