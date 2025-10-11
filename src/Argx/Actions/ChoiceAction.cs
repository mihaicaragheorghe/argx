using Argx.Errors;
using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class ChoiceAction : ArgumentAction
{
    public override void Execute(Argument arg, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(arg, invocation, values, store);

        if (values.Length == 0)
        {
            throw new ArgumentValueException(invocation, "expected value");
        }

        var value = values[0].Value;
        var allowed = string.Join(", ", arg.Choices!);

        if (!allowed.Contains(value))
        {
            throw new ArgumentValueException(invocation, $"invalid choice: {value}, chose from {allowed}");
        }

        store.Set(arg.Dest, value);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 1)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity for 'choice' must be 1, use 'store' to store collections");
        }

        if (argument.Choices?.Length == 0)
        {
            throw new ArgumentException($"Argument {argument.Name}: choices required for action 'choice'");
        }
    }
}