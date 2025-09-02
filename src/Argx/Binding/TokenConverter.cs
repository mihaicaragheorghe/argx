using System.Collections;
using Argx.Parsing;

namespace Argx.Binding;

internal static partial class TokenConverter
{
    internal static TokenConversionResult TryConvert(Type type, object? obj)
    {
        try
        {
            return ConvertObject(type, obj);
        }
        catch (Exception ex)
        {
            return TokenConversionResult.Failure(ex.Message);
        }
    }

    internal static TokenConversionResult TryConvert(Type type, ReadOnlySpan<Token> tokens)
    {
        try
        {
            return ConvertTokens(type, tokens);
        }
        catch (Exception ex)
        {
            return TokenConversionResult.Failure(ex.Message);
        }
    }

    internal static TokenConversionResult ConvertTokens(Type type, ReadOnlySpan<Token> tokens)
        => tokens.Length switch
        {
            1 when !type.IsEnumerable() => TokenConverter.ConvertToken(type, tokens[0]),
            _ => TokenConverter.ConvertSpan(type, tokens),
        };

    internal static TokenConversionResult ConvertObject(Type type, object? obj)
        => obj switch
        {
            Token single => ConvertToken(type, single),
            IReadOnlyList<Token> list => ConvertList(type, list),
            _ => throw new InvalidCastException($"Cannot convert '{obj}' to type {type}")
        };

    private static TokenConversionResult ConvertToken(Type type, Token token)
    {
        if (type.TryGetNullableType(out var nullableType))
        {
            return ConvertToken(nullableType, token);
        }

        if (!StringConverters.TryGetValue(type, out var tryConvert))
        {
            throw new NotSupportedException($"The type {type} is not supported");
        }

        if (tryConvert(token.Value, out var converted))
        {
            return TokenConversionResult.Success(converted);
        }

        return TokenConversionResult.Failure($"Failed to convert token '{token}' to type {type}");
    }

    private static TokenConversionResult ConvertSpan(Type type, ReadOnlySpan<Token> tokens)
    {
        // TODO: remove extra allocation
        var list = new List<Token>(tokens.ToArray()).AsReadOnly();
        return ConvertList(type, list);
    }

    private static TokenConversionResult ConvertList(Type type, IReadOnlyList<Token> tokens)
    {
        if (!type.IsEnumerable())
        {
            throw new InvalidCastException(
                $"Cannot convert {tokens} to type {type}, it has to be an enumerable type");
        }

        var itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
        var values = CreateCollection(type, itemType, tokens.Count);
        var isArray = values is Array;

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var result = ConvertToken(itemType, token);

            if (result.IsError)
            {
                return TokenConversionResult.Failure(
                    $"Cannot convert token '{token}' in collection of type {itemType} to type {type}. {result.Error}");
            }

            if (isArray)
            {
                values[i] = result.Value;
            }
            else
            {
                values.Add(result.Value);
            }
        }

        return TokenConversionResult.Success(values);
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