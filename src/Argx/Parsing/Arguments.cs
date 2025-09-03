namespace Argx.Parsing;

public class Arguments
{
    private readonly IList<IArgumentProvider> _providers;

    public Arguments(IList<IArgumentProvider> providers)
    {
        _providers = providers;
    }

    public string? this[string key] => GetValue(key);

    public string? GetValue(string arg)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryGetValue(arg, out var value))
            {
                return value;
            }
        }

        return null;
    }

    public T GetValue<T>(string arg) => TryGetValue<T>(arg, out T value) ? value : default!;

    public bool TryGetValue<T>(string arg, out T value)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryGetValue<T>(arg, out value))
            {
                return true;
            }
        }

        value = default!;
        return false;
    }
}