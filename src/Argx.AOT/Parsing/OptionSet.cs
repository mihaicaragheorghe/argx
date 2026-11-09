namespace Argx.Parsing;

internal class OptionSet
{
    private readonly Dictionary<string, Argument> _optLookup = [];

    private readonly Dictionary<string, string> _aliasLookup = [];

    internal Argument? Get(string key)
    {
        if (_optLookup.TryGetValue(key, out var argument)
            || _aliasLookup.TryGetValue(key, out var name) && _optLookup.TryGetValue(name, out argument))
        {
            return argument;
        }

        return null;
    }

    internal void Add(Argument argument)
    {
        _optLookup[argument.Name] = argument;

        if (argument.Aliases?.Count > 0)
        {
            foreach (var alias in argument.Aliases)
            {
                _aliasLookup[alias] = argument.Name;
            }
        }
    }

    internal List<Argument> ToList() => [.. _optLookup.Values];

    internal Argument? GetByAlias(string alias) => _optLookup.GetValueOrDefault(alias);
}