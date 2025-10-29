using Argx.Parsing;
using Argx.Store;

namespace Argx.Actions;

/// <summary>
/// Base class for argument actions.  
/// Provides a template for executing actions associated with arguments.
/// </summary>
public abstract class ArgumentAction
{
    public virtual void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentRepository store)
    {
        if (string.IsNullOrEmpty(invocation.Value))
        {
            throw new InvalidOperationException("Invalid input: the first token must be the argument name.");
        }
    }

    public abstract void Validate(Argument argument);
}