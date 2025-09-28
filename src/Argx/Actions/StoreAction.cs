using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;

namespace Argx.Actions;

internal class StoreAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        var name = tokens[0].Value;

        if (tokens.Length < 2)
            throw new ArgumentValueException(name, $"expected value");

        TokenConversionResult result = TokenConverter.ConvertTokens(argument.Type, tokens[1..]);

        if (result.IsError)
            throw new ArgumentValueException(name, $"expected type {argument.Type.GetFriendlyName()}. {result.Error}");

        repository.Set(argument.Dest, result.Value!);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity == 0)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity for 'store' must be != 0. Use 'store_true', 'store_false' or 'store_const' to store constant values.");
        }
    }
}