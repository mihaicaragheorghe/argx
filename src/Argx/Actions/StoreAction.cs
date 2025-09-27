using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;

namespace Argx.Actions;

public class StoreAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        var name = tokens[0].Value;

        if (argument.Arity == 0)
            throw new InvalidOperationException(
                $"Arity for 'store' must be != 0. Use 'store_true', 'store_false' or 'store_const' to store constant values. Argument: {name}");

        if (tokens.Length < 2)
            throw new BadArgumentException(name, $"expected {(argument.Arity > 1 ? "at least " : "")}one value");

        TokenConversionResult result = TokenConverter.ConvertTokens(argument.Type, tokens[1..]);

        if (result.IsError)
            throw new BadArgumentException(name, $"expected type {argument.Type.GetFriendlyName()}. {result.Error}");

        repository.Set(argument.Dest, result.Value!);
    }
}