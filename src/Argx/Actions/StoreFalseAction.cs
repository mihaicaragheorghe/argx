using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class StoreFalseAction : ArgumentAction
{
    public override void Execute(Argument arg, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(arg, invocation, values, store);

        store.Set(arg.Dest, false);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 0)
        {
            throw new ArgumentException(
                $"Argument {argument.Name}: arity for 'store_false' must be 0. Use 'store', to store values.");
        }
    }
}