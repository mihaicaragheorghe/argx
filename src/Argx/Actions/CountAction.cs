using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class CountAction : ArgumentAction
{
    public override void Execute(Argument arg, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        base.Execute(arg, invocation, values, store);

        if (store.TryGetValue<int>(arg.Dest, out var value))
        {
            store.Set(arg.Dest, value + 1);
        }

        store.Set(arg.Dest, 1);
    }

    public override void Validate(Argument argument)
    {
        if (argument.Arity != 0)
        {
            throw new ArgumentException($"Argument {argument.Name}: arity for 'count' action must be 0.");
        }
    }
}