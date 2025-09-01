using Argx.Binding;
using Argx.Parsing;

namespace Argx.Actions;

public class StoreAction : ArgumentAction
{
    public override void Execute(
        ReadOnlySpan<string> values,
        string dest,
        Type type,
        ArgumentStore store,
        int? arity = null,
        object? defaultValue = null,
        object? constValue = null,
        string[]? choices = null)
    {
        arity ??= 1; // default

        if (arity == 0)
        {
            throw new InvalidOperationException(
                "Arity for 'store' must be != 0. Use 'store_true', 'store_false' or 'store_const' to store constant values");
        }

        if (arity == 1)
        {
            var token = new Token(values[1]);
            var value = Convert(type, token);
            store.Add(dest, value);
            return;
        }

        if (!type.IsEnumerable())
        {
            throw new InvalidCastException("For 'store' action with arity > 1, the type has to be enumerable");
        }

        var tokens = GetTokens(values, (int)arity);
        var listValue = Convert(type, tokens);

        store.Add(dest, listValue);
    }

    private IReadOnlyList<Token> GetTokens(ReadOnlySpan<string> values, int c)
    {
        var tokens = new List<Token>();
        for (int i = 1; i <= c; i++)
        {
            tokens.Add(new Token(values[i]));
        }

        return tokens.AsReadOnly();
    }

    private object Convert(Type type, object obj)
    {
        var conversionResult = ArgumentConverter.ConvertToken(type, obj);
        if (conversionResult.IsError)
        {
            throw new InvalidCastException(conversionResult.Error);
        }

        return conversionResult.Value!;
    }
}