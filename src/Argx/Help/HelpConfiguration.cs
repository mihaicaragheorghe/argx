namespace Argx.Help;

public class HelpConfiguration
{
    public string SectionSpacing { get; set; } = Environment.NewLine + Environment.NewLine;

    public int IndentSize { get; set; } = DefaultIndentSize;

    public int MaxLineWidth { get; set; } = DefaultMaxLineWidth;

    public bool PrintAliasInUsage { get; set; } = false;

    public const int DefaultIndentSize = 2;

    public const int DefaultMaxLineWidth = 80;

    public static HelpConfiguration Default()
    {
        return new HelpConfiguration();
    }
}