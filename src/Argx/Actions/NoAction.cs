using Argx.Parsing;

namespace Argx.Actions;

internal class NoAction : ArgumentAction
{
    public override void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentStore store)
    {
    }

    public override void Validate(Argument argument)
    {
    }
}