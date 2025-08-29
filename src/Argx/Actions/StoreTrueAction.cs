using Argx.Parsing;

namespace Argx.Actions;

public class StoreTrueAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        if (argument.Arity != 0)
            throw new InvalidOperationException(
                $"Arity for 'store_true' must be 0. Use 'store', to store values. Argument: {argument.Name}");

        repository.Set(argument.Dest, true);
    }
}