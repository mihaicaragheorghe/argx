namespace Argx.Errors;

/// <summary>
/// Exception thrown when an argument value is invalid.
/// </summary>
public class ArgumentValueException(string arg, string message)
    : Exception($"Error: argument {arg}: {message}")
{
}