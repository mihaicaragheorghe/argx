using Argx.Parsing;

namespace Argx.Actions;

public class ChoiceAction : ArgumentAction
{
    public override void Execute(
        ReadOnlySpan<string> values,
        string dest,
        Type type,
        ArgumentStore store,
        int? narg = null,
        object? defaultValue = null,
        object? constValue = null,
        string[]? choices = null)
    {
        throw new NotImplementedException();
    }
}