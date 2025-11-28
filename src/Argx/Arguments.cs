using Argx.Extensions;
using Argx.Parsing;
using Argx.Utils;

namespace Argx;

/// <inheritdoc cref="IArguments"/>
public class Arguments : IArguments
{
    private readonly IArgumentStore _store;

    /// <inheritdoc />
    public List<string> Extras { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Arguments"/> class.
    /// </summary>
    /// <param name="store">The argument store to use for retrieving argument values.</param>
    public Arguments(IArgumentStore store)
    {
        _store = store;
        Extras = [];
    }

    /// <inheritdoc />
    public string? this[string key] => GetValue(key);

    /// <inheritdoc />
    public string? GetValue(string arg)
    {
        if (_store.TryGetValue(arg, out var value))
        {
            return value;
        }

        return null;
    }

    /// <inheritdoc />
    public T GetValue<T>(string arg) => TryGetValue(arg, out T value) ? value : default!;

    /// <inheritdoc />
    public bool TryGetValue<T>(string arg, out T value) => _store.TryGetValue(arg, out value);

    /// <inheritdoc />
    public T GetRequired<T>(string arg)
    {
        if (_store.TryGetValue<T>(arg, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Missing required value for argument '{arg}' (expected type: {typeof(T).Name}).");
    }

    public void Bind<T>(T instance) where T : class
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        var properties = typeof(T).GetWritableProperties();

        foreach (var property in properties)
        {
            var argName = property.Name.PascalToKebabCase();

            var tryGetMethod = typeof(IArguments).GetMethod(nameof(TryGetValue))!
                .MakeGenericMethod(property.PropertyType);

            var parameters = new object[] { argName, null! };
            var success = (bool)tryGetMethod.Invoke(this, parameters)!;

            if (success)
            {
                var value = parameters[1];
                property.SetValue(instance, value);
            }
        }
    }
}