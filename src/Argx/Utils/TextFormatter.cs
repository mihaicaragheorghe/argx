namespace Argx.Utils;

internal static class TextFormatter
{
    internal static IEnumerable<string> WrapText(string text, int maxWidth)
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