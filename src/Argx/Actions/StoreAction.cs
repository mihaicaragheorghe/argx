using Argx.Binding;
using Argx.Parsing;

namespace Argx.Actions;

public class StoreAction : ArgumentAction
{
    public override void Execute(
        Argument argument,
        ArgumentStore store,
        string dest,
        ReadOnlySpan<Token> tokens)
    {
        var arity = argument.Arity ?? 1; // default

        if (arity == 0)
            throw new InvalidOperationException(
                "Arity for 'store' must be != 0. Use 'store_true', 'store_false' or 'store_const' to store constant values");

        TokenConversionResult result = TokenConverter.TryConvert(argument.Type, tokens.Slice(1));

        if (result.IsError)
            throw new InvalidCastException(result.Error);

        store.Add(dest, result.Value!);
    }
}