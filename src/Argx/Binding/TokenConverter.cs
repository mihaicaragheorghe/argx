using Argx.Extensions;
using Argx.Parsing;
using Argx.Utils;

namespace Argx.Binding;

internal static partial class TokenConverter
{
    internal static TokenConversionResult ConvertTokens(Type type, ReadOnlySpan<Token> tokens)
        => tokens.Length switch
        {
            1 when !type.IsEnumerable() => TokenConverter.ConvertToken(type, tokens[0]),
            _ => ConvertSpan(type, tokens),
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

        return TokenConversionResult.Failure($"Failed to convert '{token}' to type {type.GetFriendlyName()}");
    }

    private static TokenConversionResult ConvertSpan(Type type, ReadOnlySpan<Token> tokens)
    {
        if (!type.IsEnumerable())
        {
            throw new InvalidOperationException(
                $"Invalid type {type.GetFriendlyName()} for {tokens.Length} tokens: it has to be an enumerable type");
        }

        var itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
        var values = CollectionUtils.CreateCollection(type, itemType, tokens.Length);
        var isArray = values is Array;

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            var result = ConvertToken(itemType, token);

            if (result.IsError)
            {
                return TokenConversionResult.Failure(
                    $"Failed to convert token '{token}' in collection type {itemType.GetFriendlyName()} to type {type.GetFriendlyName()}: {result.Error}");
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

    private static TokenConversionResult ConvertList(Type type, IReadOnlyList<Token> tokens)
    {
        return ConvertSpan(type, tokens.ToArray().AsSpan());
    }
}