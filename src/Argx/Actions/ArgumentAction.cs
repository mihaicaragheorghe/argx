using Argx.Parsing;

namespace Argx.Actions;

public abstract class ArgumentAction
{
    public abstract void Execute(
        Argument argument,
        ArgumentRepository repository,
        string dest,
        ReadOnlySpan<Token> values);
}