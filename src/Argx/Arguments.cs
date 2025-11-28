using System.Reflection;

using Argx.Attributes;
using Argx.Extensions;
using Argx.Parsing;

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

    /// <inheritdoc />
    public void Bind<T>(T instance) where T : class
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && !HasIgnoreAttribute(p));

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;

            if (typeof(T).TryGetNullableType(out var nullableType))
            {
                propertyType = nullableType;
            }

            if (!propertyType.IsComplexType() || propertyType.IsEnumerable())
            {
                SetProperty(instance, property);
            }
        }
    }

    /// <inheritdoc />
    public T Get<T>() where T : class, new()
    {
        var instance = new T();
        Bind(instance);
        return instance;
    }

    private void SetProperty<T>(T instance, PropertyInfo property)
    {
        var argName = GetArgumentName(property);

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

    private static bool HasIgnoreAttribute(PropertyInfo property)
    {
        return property.GetCustomAttribute<IgnoreAttribute>() != null;
    }

    private string GetArgumentName(PropertyInfo property)
    {
        var attr = property.GetCustomAttribute<ArgumentAttribute>();
        return attr?.Name ?? property.Name.PascalToKebabCase();
    }
}