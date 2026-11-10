namespace Argx.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ArgxBindingAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
    public string? Name { get; set; }
}
