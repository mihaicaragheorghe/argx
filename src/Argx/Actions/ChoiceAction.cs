using Argx.Parsing;

namespace Argx.Actions;

public class ChoiceAction : ArgumentAction
{
    public override void Execute(
        Argument argument,
        ArgumentRepository repository,
        string dest,
        ReadOnlySpan<Token> tokens)
    {
    }
}