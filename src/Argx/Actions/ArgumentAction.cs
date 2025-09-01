using Argx.Parsing;

namespace Argx.Actions;

public abstract class ArgumentAction
{
    public ArgumentAction() { }

    public abstract void Execute(
        ReadOnlySpan<string> values,
        string dest,
        Type type,
        ArgumentStore store,
        int? narg = null,
        object? defaultValue = null,
        object? constValue = null,
        string[]? choices = null);
}