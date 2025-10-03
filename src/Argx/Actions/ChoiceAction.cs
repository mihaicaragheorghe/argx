using Argx.Errors;
using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class ChoiceAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        base.Execute(argument, repository, tokens);
        
        var name = tokens[0].Value;

        if (tokens.Length < 2)
        {
            throw new ArgumentValueException(name, "expected value");
        }

        var value = tokens[1].Value;
        var allowed = string.Join(", ", argument.Choices!);

        if (!allowed.Contains(value))
        {
            throw new ArgumentValueException(name, $"invalid choice: {value}, chose from {allowed}");
        }

        repository.Set(argument.Dest, value);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 1)
        {
            throw new ArgumentException($"Argument {argument.Name}: arity for 'choice' must be 1, use 'store' to store collections");
        }

        if (argument.Choices?.Length == 0)
        {
            throw new ArgumentException($"Argument {argument.Name}: choices required for action 'choice'");
        }
    }
}