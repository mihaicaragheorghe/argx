using System.Text;

namespace Argx.Help;

internal class HelpBuilder
{
    private readonly List<HelpSection> _sections = [];

    private readonly HelpConfiguration _config;

    internal HelpBuilder(HelpConfiguration config)
    {
        _config = config;
    }

    internal HelpBuilder AddSection(string title, string content)
    {
        _sections.Add(new HelpSection(title, content, _config.IndentSize, _config.MaxLineWidth));
        return this;
    }

    internal HelpBuilder AddText(string text)
    {
        _sections.Add(new HelpSection(string.Empty, text, _config.IndentSize, _config.MaxLineWidth));
        return this;
    }

    internal HelpBuilder AddArguments(IList<Argument> arguments, string title = "Arguments")
    {
        var section = new HelpSection(title, _config.IndentSize, _config.MaxLineWidth);

        var rows = arguments
            .Select(a => new TwoColumnRow(Left: a.GetDisplayName(), Right: a.Usage ?? string.Empty))
            .OrderBy(r => r.Left)
            .ToList();

        section.AppendColumns(rows);
        _sections.Add(section);
        return this;
    }

    internal string Build()
    {
        var sb = new StringBuilder();

        foreach (var section in _sections.Select(s => s.Render()))
        {
            sb.Append(section);
            sb.Append(_config.SectionSpacing);
        }

        return sb.ToString().TrimEnd();
    }
}