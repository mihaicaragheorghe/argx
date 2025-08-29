using Argx.Parsing;

namespace Argx.Actions;

public abstract class ArgumentAction
{
    public abstract void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> values);
}