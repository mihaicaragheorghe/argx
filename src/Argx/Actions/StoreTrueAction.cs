using Argx.Parsing;

namespace Argx.Actions;

public class StoreTrueAction : ArgumentAction
{
    public override void Execute(
        Argument argument,
        IArgumentRepository repository,
        string dest,
        ReadOnlySpan<Token> tokens)
    {
    }
}