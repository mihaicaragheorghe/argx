using Argx.Parsing;

namespace Argx.Actions;

/// <summary>
/// Base class for argument actions.  
/// Provides a template for executing actions associated with arguments.
/// </summary>
public abstract class ArgumentAction
{
    public virtual void Execute(Argument argument, Token invocation, ReadOnlySpan<Token> values, IArgumentStore store)
    {
        if (string.IsNullOrEmpty(invocation.Value))
        {
            throw new InvalidOperationException("Invalid input: the first token must be the argument name.");
        }
    }

    /// <summary>
    /// Called when a new argument specification is added to the parser.
    /// It ensures that the action is valid and can be performed.
    /// Each action has its own validation rules and throws an exception if they are not met.
    /// </summary>
    /// <remarks>
    /// This helps catch configuration errors at startup, rather than at runtime during parsing.
    /// </remarks>
    /// <param name="argument"></param>
    public abstract void Validate(Argument argument);
}