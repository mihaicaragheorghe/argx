using System.Reflection;

using Argx.Attributes;

namespace Argx.Extensions;

/// <summary>
/// Extension methods for the <see cref="IArguments"/> interface.
/// </summary>
public static class ArgumentsExtensions
{
    /// <summary>
    /// Binds the argument values to the properties of the specified instance.
    /// </summary>
    /// <remarks>
    /// The properties of the instance should have public setters.
    /// By default, the property name converted to kebab-case will be used as the argument name,
    /// but this can be overridden using the <see cref="ArgumentAttribute"/>.
    /// Properties marked with the <see cref="IgnoreAttribute"/> will be ignored during binding
    /// </remarks>
    /// <typeparam name="T">The type of the instance to bind the argument values to.</typeparam>
    /// <param name="args">The <see cref="IArguments"/> instance.</param>
    /// <param name="instance">The instance to bind the argument values to.</param>
    public static void Bind<T>(this IArguments args, T instance) where T : class
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
                args.SetProperty(instance, property);
            }
        }
    }

    /// <summary>
    /// Creates a new instance of type <typeparamref name="T"/>, binds the argument values to its properties, and returns it.
    /// </summary>
    /// <remarks>
    /// The properties of the instance should have public setters.
    /// By default, the property name converted to kebab-case will be used as the argument name,
    /// but this can be overridden using the <see cref="ArgumentAttribute"/>.
    /// Properties marked with the <see cref="IgnoreAttribute"/> will be ignored during binding
    /// </remarks>
    /// <typeparam name="T">The type of the instance to create and bind the argument values to.</typeparam>
    /// <param name="args">The <see cref="IArguments"/> instance.</param>
    /// <returns>The created and bound instance of type <typeparamref name="T"/>.</returns>
    public static T Get<T>(this IArguments args) where T : class, new()
    {
        var instance = new T();
        args.Bind(instance);
        return instance;
    }

    private static void SetProperty<T>(this IArguments args, T instance, PropertyInfo property)
    {
        var argName = GetArgumentName(property);

        var tryGetMethod = typeof(IArguments).GetMethod(nameof(args.TryGetValue))!
            .MakeGenericMethod(property.PropertyType);

        var parameters = new object[] { argName, null! };
        var success = (bool)tryGetMethod.Invoke(args, parameters)!;

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

    private static string GetArgumentName(PropertyInfo property)
    {
        var attr = property.GetCustomAttribute<ArgumentAttribute>();
        return attr?.Name ?? property.Name.PascalToKebabCase();
    }
}