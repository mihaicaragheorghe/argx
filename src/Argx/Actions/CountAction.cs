using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class CountAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        if (repository.TryGetValue<int>(argument.Dest, out var value))
        {
            repository.Set(argument.Dest, value + 1);
        }

        repository.Set(argument.Dest, 1);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 0)
        {
            throw new ArgumentException($"Argument {argument.Name}: arity for 'count' action must be 0.");
        }
    }
}