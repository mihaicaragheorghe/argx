using Argx.Parsing;

namespace Argx.Actions;

internal class StoreTrueAction : ArgumentAction
{
    public override void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentStore store)
    {
        base.Execute(argument, invocation, values, store);

        store.Set(argument.Dest, true);
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