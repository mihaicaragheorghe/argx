namespace Argx.Parsing;

internal class Argument
{
    internal string Name { get; private set; }

    internal string? Shorten { get; private set; }

    internal Type Type { get; private set; }

    internal string Action { get; private set; }

    internal string? Usage { get; private set; }

    internal string? DefaultValue { get; private set; }

    internal string? Value { get; private set; }

    internal bool IsRequired { get; private set; }

    internal Argument(
        string name,
        string? shorten = null,
        string action = "store",
        string? usage = null,
        string? defaultVal = null,
        bool isRequired = false,
        Type? type = null)
    {
        Name = name;
        Shorten = shorten;
        Usage = usage;
        Action = action;
        Value = defaultVal;
        DefaultValue = defaultVal;
        IsRequired = isRequired;
        Type = type ?? typeof(string);
    }

    internal void Set(string value)
    {
        Value = value;
    }
}