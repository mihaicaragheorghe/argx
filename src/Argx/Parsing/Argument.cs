using Argx.Actions;

namespace Argx.Parsing;

public class Argument
{
    public string Name { get; }

    public string? Alias { get; }

    public string Action { get; }

    public Type Type { get; }

    public string Dest { get; }

    public string? Usage { get; }

    public string? DefaultValue { get; }

    public object? ConstValue { get; }

    public string[]? Choices { get; }

    public int Arity { get; }

    public bool IsRequired { get; }

    public Argument(
        string name,
        string? alias = null,
        string? action = null,
        string? dest = null,
        string? usage = null,
        string? defaultVal = null,
        object? constValue = null,
        string[]? choices = null,
        bool isRequired = false,
        int? arity = null,
        Type? type = null)
    {
        Name = name;
        Alias = alias;
        Type = type ?? typeof(string);
        Action = action ?? ArgumentActions.Store;
        Dest = dest ?? name.Replace("--", string.Empty);
        Usage = usage;
        DefaultValue = defaultVal;
        ConstValue = constValue;
        Choices = choices;
        Arity = arity ?? ActionRegistry.DefaultArity(action ?? ArgumentActions.Store);
        IsRequired = isRequired;
    }
}