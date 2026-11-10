namespace Argx.Attributes;

/// <summary>
/// Specifies that this property should be ignored during binding.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreAttribute : Attribute
{
}
