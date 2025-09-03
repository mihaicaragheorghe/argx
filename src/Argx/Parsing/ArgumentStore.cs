namespace Argx.Parsing;

public sealed class ArgumentStore : IArgumentProvider
{
    private readonly Dictionary<string, object> _values = new();

    public void Add(string arg, object value)
    {
        _values[arg] = value;
    }

    public bool TryGetValue<T>(string arg, out T value)
    {
        if (_values.TryGetValue(arg, out var obj) && obj is T t)
        {
            value = t;
            return true;
        }

        value = default!;
        return false;
    }

    public bool TryGetValue(string arg, out string? value) => TryGetValue<string>(arg, out value);
}