using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;

namespace Argx.Actions;

internal class StoreAction : ArgumentAction
{
    public override void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentStore store)
    {
        base.Execute(argument, invocation, values, store);

        if (argument.ConstValue == null && values.Length < 1)
        {
            throw new ArgumentValueException(invocation, "expected value");
        }

        if (values.Length == 0)
        {
            store.Set(argument.Dest, argument.ConstValue!);
            return;
        }

        if (argument.Choices?.Length > 0)
        {
            foreach (var token in values)
            {
                if (!argument.Choices.Contains(token.Value))
                {
                    throw new ArgumentValueException(invocation,
                        $"invalid choice '{token}', expected one of: {string.Join(", ", argument.Choices)}");
                }
            }
        }

        TokenConversionResult result = TokenConverter.ConvertTokens(argument.ValueType, values);

        if (result.IsError)
        {
            throw new ArgumentValueException(invocation, $"expected type {argument.ValueType.GetFriendlyName()}. {result.Error}");
        }

        store.Set(argument.Dest, result.Value!);
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

        if (argument.Arity.AcceptsMultipleValues && !argument.ValueType.IsEnumerable())
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: type must be enumerable for arity > 1, {Arity.Any} or {Arity.AtLeastOne}, consider {Arity.Optional}");
        }
    }
}