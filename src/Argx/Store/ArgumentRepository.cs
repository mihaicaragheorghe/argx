namespace Argx.Store;

/// <summary>
/// In-memory repository for storing parsed argument values.
/// </summary>
public sealed class ArgumentRepository : IArgumentRepository
{
    private readonly Dictionary<string, object> _values = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Set(string arg, object value)
    {
        _values[arg] = value;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool Contains(string arg) => _values.ContainsKey(arg);
}