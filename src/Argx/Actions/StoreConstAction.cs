using Argx.Parsing;

namespace Argx.Actions;

public class StoreConstAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> tokens)
    {
        if (argument.Arity != 0)
            throw new InvalidOperationException(
                $"Arity for 'store_const' must be 0. Use 'store', to store values. Argument: {argument.Name}");

        if (argument.ConstValue is null)
            throw new ArgumentException(
                $"Action 'store_const' requires 'ConstValue' to be set. Argument: {argument.Name}");

        repository.Set(argument.Dest, argument.ConstValue!);
    }
}