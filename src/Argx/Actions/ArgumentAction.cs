using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

public abstract class ArgumentAction
{
    public abstract void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> values);

    public abstract void Validate(Argument argument);
}