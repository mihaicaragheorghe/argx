using Argx.Actions;

namespace Argx;

public class Argument
{
    public string Name { get; }

    public AliasSet? Aliases { get; }

    public string Action { get; }

    public Type Type { get; }

    public string Dest { get; }

    public string? Usage { get; }

    public string? DefaultValue { get; }

    public object? ConstValue { get; }

    public string[]? Choices { get; }

    public Arity Arity { get; }

    public bool IsPositional { get; }

    public Argument(
        string name,
        string? action = null,
        string? dest = null,
        string? usage = null,
        string? defaultValue = null,
        string? arity = null,
        object? constValue = null,
        string[]? choices = null,
        bool isPositional = false,
        Type? type = null,
        params string[]? alias)
    {
        Name = name;
        Aliases = alias == null ? null : new AliasSet(alias);
        Type = type ?? typeof(string);
        Action = action ?? ArgumentActions.Store;
        Dest = dest ?? name.TrimStart('-');
        Usage = usage;
        DefaultValue = defaultValue ?? Name.TrimStart('-').ToUpper();
        ConstValue = constValue;
        Choices = choices;
        IsPositional = isPositional;
        Arity = arity is null
            ? new Arity(ActionRegistry.DefaultArity(action ?? ArgumentActions.Store))
            : new Arity(arity);
    }

    public string GetDisplayName()
    {
        return Name + (string.IsNullOrEmpty(Aliases?.ToString()) ? "" : $", {Aliases}");
    }
}