using Argx.Help;

namespace Argx.Parsing;

public class ArgumentParserConfiguration
{
    public bool AddHelpArgument { get; set; } = true;

    public bool ExitOnError { get; set; } = true;

    public HelpConfiguration HelpConfiguration { get; set; } = HelpConfiguration.Default();
}