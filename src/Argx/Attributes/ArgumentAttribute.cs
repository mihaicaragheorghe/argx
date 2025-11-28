namespace Argx.Attributes;

/// <summary>
/// Specifies the CLI argument name for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ArgumentAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}