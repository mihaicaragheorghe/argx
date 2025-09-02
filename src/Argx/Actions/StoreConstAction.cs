using Argx.Parsing;

namespace Argx.Actions;

public class StoreConstAction : ArgumentAction
{
    public override void Execute(
        Argument argument,
        ArgumentStore store,
        string dest,
        ReadOnlySpan<Token> tokens)
    {
    }
}