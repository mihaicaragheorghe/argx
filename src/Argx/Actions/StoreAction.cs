using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class StoreAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        var name = tokens[0].Value;

        if (argument.ConstValue == null && tokens.Length < 2)
        {
            throw new ArgumentValueException(name, $"expected value");
        }

        if (tokens.Length == 1)
        {
            repository.Set(argument.Dest, argument.ConstValue!);
            return;
        }

        TokenConversionResult result = TokenConverter.ConvertTokens(argument.Type, tokens[1..]);

        if (result.IsError)
        {
            throw new ArgumentValueException(name, $"expected type {argument.Type.GetFriendlyName()}. {result.Error}");
        }

        repository.Set(argument.Dest, result.Value!);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity == 0)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity for 'store' must be != 0. Use 'store_true', 'store_false' or 'store_const' to store constant values.");
        }

        if (argument.ConstValue != null && !argument.Arity.IsOptional)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity must be {Arity.Optional} or {Arity.Any} to supply a const value");
        }

        if (argument.Arity.AcceptsMultipleValues && !argument.Type.IsEnumerable())
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: type must be enumerable for arity > 1, {Arity.Any} or {Arity.AtLeastOne}, consider {Arity.Optional}");
        }
    }
}