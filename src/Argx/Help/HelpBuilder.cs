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

    internal HelpBuilder AddUsage(IList<Argument> arguments, string prefix = "", string title = "Usage")
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(prefix))
        {
            sb.Append(prefix).Append(" ");
        }

        foreach (var opt in arguments.Where(a => !a.IsPositional))
        {
            var placeholder = opt.Name.TrimStart('-').ToUpper();
            var value = opt.Arity.IsFixed
                ? string.Join(" ", Enumerable.Repeat(placeholder, int.Parse(opt.Arity.Value)))
                : opt.Arity.Value switch
                {
                    Arity.Optional => $"[{placeholder}]",
                    Arity.Any or Arity.AtLeastOne => $"[{placeholder} ...]",
                    _ => ""
                };

            sb.Append('[');
            sb.Append(_config.PrintAliasInUsage && opt.Aliases?.Count > 0 ? opt.Aliases.First() : opt.Name);
            if (!string.IsNullOrEmpty(value)) sb.Append(" ").Append(value);
            sb.Append(']');
            sb.Append(' ');
        }

        foreach (var pos in arguments.Where(a => a.IsPositional))
        {
            sb.Append(pos.Name).Append(' ');
        }

        var content = sb.ToString().TrimEnd();
        var section = new HelpSection(title, content, _config.IndentSize, _config.MaxLineWidth);

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