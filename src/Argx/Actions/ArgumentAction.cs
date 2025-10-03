using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

public abstract class ArgumentAction
{
    public virtual void Execute(Argument argument, IArgumentRepository repository, ReadOnlySpan<Token> values)
    {
        if (values.Length == 0)
        {
            throw new InvalidOperationException("Invalid input: the first token must be the argument name.");
        }
    }

    public abstract void Validate(Argument argument);
}