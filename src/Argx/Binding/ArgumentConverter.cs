using System.Collections;

using Argx.Parsing;

namespace Argx.Binding;

internal static partial class ArgumentConverter
{
    public static object? ConvertToken(Type type, object? obj) =>
        obj switch
        {
            Token single => ConvertToken(type, single),
            IReadOnlyList<Token> list => ConvertTokens(type, list),
            _ => throw new InvalidCastException($"Cannot convert token '{obj}' to type {type}")
        };

    private static object? ConvertToken(Type type, Token token)
    {
        if (type.TryGetNullableType(out var nullableType))
        {
            return ConvertToken(nullableType, token);
        }

        if (StringConverters.TryGetValue(type, out var tryConvert)
            && tryConvert(token.Value, out var converted))
        {
            return converted;
        }

        throw new InvalidCastException($"Cannot convert token '{token}' to type {type}");
    }

    private static object ConvertTokens(Type type, IReadOnlyList<Token> tokens)
    {
        var itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
        var values = CreateCollection(type, itemType, tokens.Count);
        var isArray = values is Array;

        for (var i = 0; i < tokens.Count; i++)
        {
            var converted = ConvertToken(itemType, tokens[i]);

            if (isArray)
            {
                values[i] = converted;
            }
            else
            {
                values.Add(converted);
            }
        }

        return values;
    }

    private static IList CreateCollection(Type type, Type itemType, int capacity = 0)
    {
        if (type.IsArray)
        {
            return CreateArray(itemType, capacity);
        }

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();

            if (genericType == typeof(IEnumerable<>) ||
                genericType == typeof(IList<>) ||
                genericType == typeof(ICollection<>))
            {
                return CreateArray(itemType, capacity);
            }

            if (genericType == typeof(List<>))
            {
                return CreateList(type);
            }
        }

        throw new ArgumentException($"Type {type} cannot be created.");
    }


    private static Array CreateArray(Type itemType, int capacity) => Array.CreateInstance(itemType, capacity);

    private static IList CreateList(Type listType) => (IList)Activator.CreateInstance(listType);
}