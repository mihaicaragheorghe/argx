namespace Argx.Help;

public class HelpConfiguration
{
    public string SectionSpacing { get; set; } = Environment.NewLine + Environment.NewLine;

    public int IndentSize { get; set; } = 2;

    public int MaxLineWidth { get; set; } = 80;

    public static HelpConfiguration Default()
    {
        return new HelpConfiguration();
    }
}