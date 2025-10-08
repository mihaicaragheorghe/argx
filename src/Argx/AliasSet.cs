namespace Argx;

public class AliasSet
{
    private readonly HashSet<string> _aliases = new(StringComparer.Ordinal);

    public AliasSet(string[] aliases)
    {
        foreach (var alias in aliases)
        {
            Add(alias);
        }
    }

    public bool Contains(string alias) => _aliases.Contains(alias);

    public int Count => _aliases.Count;

    public string First() => _aliases.First();

    public void Add(string alias)
    {
        if (!IsValid(alias))
        {
            throw new ArgumentException($"Argument {alias}: alias must start with '-'");
        }

        _aliases.Add(alias);
    }

    private static bool IsValid(string alias) => !string.IsNullOrWhiteSpace(alias) && alias[0] == '-';

    public override string ToString() => string.Join(", ", _aliases);
}