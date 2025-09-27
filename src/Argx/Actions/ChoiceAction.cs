using Argx.Errors;
using Argx.Parsing;

namespace Argx.Actions;

public class ChoiceAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        var name = tokens[0].Value;

        if (argument.Arity != 1)
            throw new InvalidOperationException($"Arity for 'choice' must be 1. Use 'store', to store collections");

        if (argument.Choices?.Length == 0)
            throw new InvalidOperationException(
                $"Action 'choice' requires a list of choices to be set, but was empty for argument {name}");

        if (tokens.Length < 2)
            throw new BadArgumentException(name, "expected one value");

        var value = tokens[1].Value;
        var allowed = string.Join(", ", argument.Choices);

        if (!allowed.Contains(value))
            throw new BadArgumentException(name, $"invalid choice: {value}, chose from {allowed}");

        repository.Set(argument.Dest, value);
    }
}