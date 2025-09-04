using Argx.Parsing;

namespace Argx.Actions;

public class StoreFalseAction : ArgumentAction
{
    public override void Execute(
        Argument argument,
        ArgumentRepository repository,
        string dest,
        ReadOnlySpan<Token> tokens)
    {
    }
}