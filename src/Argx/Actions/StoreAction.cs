using Argx.Binding;
using Argx.Errors;
using Argx.Extensions;
using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class StoreAction : ArgumentAction
{
    public override void Execute(Argument arg, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(arg, invocation, values, store);

        if (arg.ConstValue == null && values.Length < 1)
        {
            throw new ArgumentValueException(invocation, "expected value");
        }

        if (values.Length == 0)
        {
            store.Set(arg.Dest, arg.ConstValue!);
            return;
        }

        if (arg.Choices?.Length > 0)
        {
            foreach (var token in values)
            {
                if (!arg.Choices.Contains(token.Value))
                {
                    throw new ArgumentValueException(invocation,
                        $"invalid choice '{token}', expected one of: {string.Join(", ", arg.Choices)}");
                }
            }
        }

        TokenConversionResult result = TokenConverter.ConvertTokens(arg.ValueType, values);

        if (result.IsError)
        {
            throw new ArgumentValueException(invocation, $"expected type {arg.ValueType.GetFriendlyName()}. {result.Error}");
        }

        store.Set(arg.Dest, result.Value!);
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