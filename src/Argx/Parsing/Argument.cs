using Argx.Actions;

namespace Argx.Parsing;

internal class Argument
{
    internal string Name { get; }

    internal string? Alias { get; }

    internal string Action { get; }

    internal Type Type { get; }

    internal string? Usage { get; }

    internal string? DefaultValue { get; }

    internal string? ConstValue { get; }

    internal string[]? Choices { get; }

    internal bool IsRequired { get; }

    internal Argument(
        string name,
        string? alias = null,
        string? action = null,
        string? usage = null,
        string? defaultVal = null,
        string? constValue = null,
        string[]? choices = null,
        bool isRequired = false,
        Type? type = null)
    {
        Name = name;
        Alias = alias;
        Type = type ?? typeof(string);
        Action = action ?? ArgumentActions.Store;
        Usage = usage;
        DefaultValue = defaultVal;
        ConstValue = defaultVal;
        Choices = choices;
        IsRequired = isRequired;
    }
}