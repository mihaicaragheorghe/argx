using System.Text;

namespace Argx.Help;

internal class HelpSection
{
    internal string Title { get; }

    internal int IndentSize { get; set; } = 2;

    internal int MaxLineWidth { get; set; } = 80;

    internal string Content { get; private set; } = string.Empty;

    private readonly List<HelpSection> _children = [];

    internal HelpSection(string title)
    {
        Title = title;
    }

    internal HelpSection(string title, string content) : this(title)
    {
        Content = content;
    }

    internal HelpSection AddChild(HelpSection section)
    {
        _children.Add(section);
        return this;
    }

    internal IReadOnlyList<HelpSection> GetChildren() => _children.AsReadOnly();

    internal string Render(int indent = 0)
    {
        var sb = new StringBuilder();
        var ind = new string(' ', indent);

        if (!string.IsNullOrWhiteSpace(Title))
        {
            sb.AppendLine($"{ind}{Title}:");
            ind += new string(' ', IndentSize);
        }

        if (!string.IsNullOrWhiteSpace(Content))
        {
            foreach (var line in WrapText(Content, MaxLineWidth - indent))
            {
                sb.AppendLine(ind + line);
            }
        }

        foreach (var child in _children)
        {
            sb.AppendLine(string.Empty);
            sb.Append(child.Render(indent + IndentSize));
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

    internal void AppendColumns(IList<HelpRow> rows)
    {
        var sb = new StringBuilder();
        var padding = rows.Max(r => r.Left.Length) + 2; // 2 spaces between

        foreach (var row in rows)
        {
            var left = row.Left.PadRight(padding);
            sb.Append(left);

            var first = true;
            foreach (var line in WrapText(row.Right, MaxLineWidth - padding))
            {
                sb.AppendLine(line.PadLeft(first ? 0 : padding + line.Length));

                // do not add padding to the first line, because it's already added on the left side
                if (first) first = false;
            }
        }

        Content += sb.ToString().TrimEnd();
    }

    private static IEnumerable<string> WrapText(string text, int maxWidth)
    {
        foreach (var line in text.Split(Environment.NewLine))
        {
            var currLine = "";
            var words = line.Split(' ');

            foreach (var word in words)
            {
                if ((currLine + word).Length > maxWidth && !string.IsNullOrWhiteSpace(currLine))
                {
                    yield return currLine.TrimEnd();
                    currLine = "";
                }

                currLine += word + " ";
            }

            if (currLine.Length > 0) yield return currLine.TrimEnd();
        }
    }
}