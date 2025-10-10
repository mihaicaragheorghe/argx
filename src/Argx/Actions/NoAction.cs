using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

internal class NoAction : ArgumentAction
{
    public override void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> values)
    {
    }

    public override void Validate(Argument argument)
    {
    }
}