using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class CountAction : ArgumentAction
{
    public override void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(argument, invocation, values, store);

        if (store.TryGetValue<int>(argument.Dest, out var value))
        {
            store.Set(argument.Dest, value + 1);
            return;
        }

        store.Set(argument.Dest, 1);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 0)
        {
            throw new ArgumentException($"Argument {argument.Name}: arity for 'count' action must be 0.");
        }
    }
}