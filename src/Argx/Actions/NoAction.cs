using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class NoAction : ArgumentAction
{
    public override void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
    }

    public override void Validate(Argument argument)
    {
    }
}