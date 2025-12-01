using System.Text;

namespace Argx.Help;

internal class HelpBuilder
{
    private readonly List<HelpSection> _sections = [];

    internal HelpBuilder AddSection(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(content))
        {
            return this;
        }

        _sections.Add(new HelpSection(title, content, HelpConfiguration.IndentSize, HelpConfiguration.MaxLineWidth));
        return this;
    }

    internal HelpBuilder AddText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return this;

        _sections.Add(new HelpSection(string.Empty, text, HelpConfiguration.IndentSize, HelpConfiguration.MaxLineWidth));
        return this;
    }

    internal HelpBuilder AddArguments(IList<Argument> arguments, string title = "Arguments")
    {
        if (arguments.Count == 0) return this;

        var rows = arguments
            .Select(a => new TwoColumnRow(Left: a.GetDisplayName(), Right: a.Usage ?? string.Empty))
            .OrderBy(r => r.Left)
            .ToList();

        AddRows(title, rows);
        return this;
    }

    internal HelpBuilder AddUsage(IList<Argument> arguments, string? prefix = null, string title = "Usage")
    {
        var sb = new StringBuilder();
        var section = new HelpSection(title, HelpConfiguration.IndentSize, HelpConfiguration.MaxLineWidth);

        if (string.IsNullOrEmpty(prefix))
        {
            var arg = Environment.GetCommandLineArgs()[0];

            prefix = Path.IsPathFullyQualified(arg) ? Path.GetFileName(arg) : arg;
        }

        foreach (var opt in arguments.Where(a => !a.IsPositional))
        {
            var placeholder = opt.Metavar;
            var value = opt.Arity.IsFixed
                ? string.Join(" ", Enumerable.Repeat(placeholder, int.Parse(opt.Arity.Value)))
                : opt.Arity.Value switch
                {
                    Arity.Optional => $"[{placeholder}]",
                    Arity.Any or Arity.AtLeastOne => $"[{placeholder} ...]",
                    _ => ""
                };

            sb.Append('[');
            sb.Append(HelpConfiguration.UseAliasInUsageText && opt.Aliases?.Count > 0 ? opt.Aliases.First() : opt.Name);
            if (!string.IsNullOrEmpty(value)) sb.Append(' ').Append(value);
            sb.Append("] ");
        }

        foreach (var pos in arguments.Where(a => a.IsPositional))
        {
            if (pos.Arity.IsFixed)
            {
                for (int i = 0; i < int.Parse(pos.Arity.Value); i++)
                {
                    sb.Append('<').Append(pos.Name).Append('>').Append(' ');
                }
                continue;
            }

            var value = pos.Arity.Value switch
            {
                Arity.Optional => $"[<{pos.Name}>]",
                Arity.Any => $"[<{pos.Name}> ...]",
                Arity.AtLeastOne => $"<{pos.Name}> [<{pos.Name}> ...]",
                _ => ""
            };

            sb.Append(value).Append(' ');
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            section.AppendRows([new TwoColumnRow(Left: prefix, Right: sb.ToString().TrimEnd())], spacing: 1);
        }
        else
        {
            section.AppendText(sb.ToString().TrimEnd());
        }

        _sections.Add(section);
        return this;
    }

    internal HelpBuilder AddRows(string title, IList<TwoColumnRow> rows)
    {
        if (rows.Count == 0) return this;

        var section = new HelpSection(title, HelpConfiguration.IndentSize, HelpConfiguration.MaxLineWidth);
        section.AppendRows(rows);
        _sections.Add(section);
        return this;
    }

    internal string Build()
    {
        var sb = new StringBuilder();

        foreach (var section in _sections.Select(s => s.Render()))
        {
            sb.Append(section);
            sb.Append(HelpConfiguration.SectionSpacing);
        }

        return sb.ToString().TrimEnd();
    }
}