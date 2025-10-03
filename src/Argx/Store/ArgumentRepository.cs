namespace Argx.Store;

public sealed class ArgumentRepository : IArgumentRepository
{
    private readonly Dictionary<string, object> _values = new();

    public void Set(string arg, object value)
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

    public bool TryGetValue(string arg, out string? value)
    {
        if (_values.TryGetValue(arg, out var obj))
        {
            value = obj.ToString();
            return true;
        }

        value = null;
        return false;
    }
}