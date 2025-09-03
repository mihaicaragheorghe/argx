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
        if (argument.Arity == 0)
            throw new InvalidOperationException(
                $"Arity for 'store' must be != 0. Use 'store_true', 'store_false' or 'store_const' to store constant values. Argument: {argument.Name}");

        TokenConversionResult result = TokenConverter.ConvertTokens(argument.Type, tokens[1..]);

        if (result.IsError)
            throw new InvalidCastException(
                $"Could not convert argument {argument.Name} to type {argument.Type} in order to store. {result.Error}");

        store.Add(dest, result.Value!);
    }
}