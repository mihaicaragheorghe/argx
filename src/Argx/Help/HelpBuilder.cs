using System.Text;

namespace Argx.Help;

internal class HelpBuilder
{
    private readonly List<HelpSection> _sections = [];

    internal string SectionSpacing { get; set; } = Environment.NewLine + Environment.NewLine;

    internal HelpBuilder AddSection(string title, string content)
    {
        _sections.Add(new HelpSection(title, content));
        return this;
    }

    internal HelpBuilder AddText(string text)
    {
        _sections.Add(new HelpSection(string.Empty, text));
        return this;
    }

    internal HelpBuilder AddArguments(IList<Argument> arguments, string title = "Arguments")
    {
        var section = new HelpSection(title);

        var rows = arguments
            .Select(a => new HelpRow(Left: a.GetDisplayName(), Right: a.Usage ?? string.Empty))
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
            sb.Append(SectionSpacing);
        }

        return sb.ToString().TrimEnd();
    }
}