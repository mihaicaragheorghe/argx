namespace Argx.Parsing;

public sealed class ArgumentStore
{
    private readonly Dictionary<string, object> _values = new();

    public void Add(string arg, object val)
    {
        _values[arg] = val;
    }

    public string? Get(string arg)
    {
        if (!_values.TryGetValue(arg, out var val))
            return null;

        if (val is string s)
            return s;

        return val.ToString();
    }

    public T Get<T>(string arg)
    {
        if (_values.TryGetValue(arg, out var val) && val is T t)
            return t;

        return default!;
    }
}