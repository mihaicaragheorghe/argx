using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Argx.Extensions;

internal static class TypeExtensions
{
    internal static Type? GetElementTypeIfEnumerable(this Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type == typeof(string))
        {
            return null;
        }

        Type? enumerableInterface = null;

        if (type.IsEnumerable())
        {
            enumerableInterface = type;
        }

        return enumerableInterface?.GenericTypeArguments switch
        {
            { Length: 1 } genericTypeArguments => genericTypeArguments[0],
            _ => null
        };
    }

    internal static bool IsEnumerable(this Type type)
    {
        if (type == typeof(string))
        {
            return false;
        }

        if (type == typeof(IDictionary<,>))
        {
            return false;
        }

        return type.IsArray || typeof(IEnumerable).IsAssignableFrom(type);
    }

    internal static bool TryGetNullableType(this Type type, [NotNullWhen(true)] out Type? nullableType)
    {
        nullableType = Nullable.GetUnderlyingType(type);
        return nullableType is not null;
    }

    internal static string GetFriendlyName(this Type type)
    {
        // TODO: add formats?
        if (type == typeof(bool))
            return "bool";
        if (type == typeof(string))
            return "string";
        if (type == typeof(int))
            return "int";
        if (type == typeof(long))
            return "long";
        if (type == typeof(short))
            return "short";
        if (type == typeof(uint))
            return "uint";
        if (type == typeof(ulong))
            return "ulong";
        if (type == typeof(ushort))
            return "ushort";
        if (type == typeof(float))
            return "float";
        if (type == typeof(double))
            return "double";
        if (type == typeof(decimal))
            return "decimal";
        if (type == typeof(Guid))
            return "guid";
        if (type == typeof(DateTime))
            return "DateTime";
        if (type == typeof(TimeSpan))
            return "TimeSpan";
        if (type.IsEnumerable())
            return $"{type.GetElementTypeIfEnumerable()!.GetFriendlyName()}[]";

        return type.Name;
    }
}