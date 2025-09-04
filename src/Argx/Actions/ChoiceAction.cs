using Argx.Parsing;

namespace Argx.Actions;

public class ChoiceAction : ArgumentAction
{
    public override void Execute(
        Argument argument,
        IArgumentRepository repository,
        string dest,
        ReadOnlySpan<Token> tokens)
    {
    }
}