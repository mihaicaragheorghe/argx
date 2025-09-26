using Argx.Parsing;

namespace Argx.Actions;

public class CountAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        if (argument.Arity != 0)
        {
            throw new InvalidOperationException(
                $"Invalid argument {argument.Name}: arity for 'count' action must be 0.");
        }

        if (repository.TryGetValue<int>(argument.Dest, out var value))
        {
            repository.Set(argument.Dest, value + 1);
        }

        repository.Set(argument.Dest, 1);
    }
}