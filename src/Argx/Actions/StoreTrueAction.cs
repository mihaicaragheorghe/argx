using Argx.Parsing;

namespace Argx.Actions;

internal class StoreTrueAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        repository.Set(argument.Dest, true);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 0)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity for 'store_true' must be 0. Use 'store', to store values.");
        }
    }
}