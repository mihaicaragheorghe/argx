using System.Text;

using Argx.Utils;

namespace Argx.Help;

internal class HelpSection
{
    internal string Title { get; }

    internal string Content { get; private set; } = string.Empty;

    private readonly int _indentSize;

    private readonly int _maxLineWidth;

    private readonly List<HelpSection> _children = [];

    internal HelpSection(
        string title,
        int indentSize = HelpConfiguration.DefaultIndentSize,
        int maxLineWidth = HelpConfiguration.DefaultMaxLineWidth)
    {
        Title = title;
        _indentSize = indentSize;
        _maxLineWidth = maxLineWidth;
    }

    internal HelpSection(
        string title, string content,
        int indentSize = HelpConfiguration.DefaultIndentSize,
        int maxLineWidth = HelpConfiguration.DefaultMaxLineWidth)
        : this(title, indentSize, maxLineWidth)
    {
        Content = content;
    }

    internal void AddChild(HelpSection section)
    {
        _children.Add(section);
    }

    internal IReadOnlyList<HelpSection> GetChildren() => _children.AsReadOnly();

    internal string Render(int indent = 0)
    {
        var sb = new StringBuilder();
        var ind = new string(' ', indent);

        if (!string.IsNullOrWhiteSpace(Title))
        {
            sb.AppendLine($"{ind}{Title}:");
            ind += new string(' ', _indentSize);
        }

        if (!string.IsNullOrWhiteSpace(Content))
        {
            foreach (var line in TextFormatter.WrapText(Content, _maxLineWidth - indent))
            {
                sb.AppendLine(ind + line);
            }
        }

        foreach (var child in _children)
        {
            sb.AppendLine(string.Empty);
            sb.Append(child.Render(indent + _indentSize));
        }

        return sb.ToString().TrimEnd();
    }

    internal void AppendText(string text)
    {
        Content += text;
    }

    internal void AppendLine(string line)
    {
        Content += Environment.NewLine + line;
    }

    internal void AppendRows(IList<TwoColumnRow> rows, int spacing = 2)
    {
        var sb = new StringBuilder();
        var padding = rows.Max(r => r.Left.Length) + spacing;

        foreach (var row in rows)
        {
            var left = row.Left.PadRight(padding);
            sb.Append(left);

            var first = true;
            foreach (var line in TextFormatter.WrapText(row.Right, _maxLineWidth - padding))
            {
                sb.AppendLine(line.PadLeft(first ? 0 : padding + line.Length));

                // do not add padding to the first line, because it's already added on the left side
                if (first) first = false;
            }
        }

        Content += sb.ToString().TrimEnd();
    }
}