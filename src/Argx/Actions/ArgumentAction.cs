using Argx.Parsing;

namespace Argx.Actions;

public abstract class ArgumentAction
{
    public abstract void Execute(
        Argument argument,
        ArgumentStore store,
        string dest,
        ReadOnlySpan<Token> values);
}