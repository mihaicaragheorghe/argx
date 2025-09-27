namespace Argx.Errors;

internal class ArgumentValueException : Exception
{
    internal ArgumentValueException(string arg, string message) : base($"Error: argument {arg}: {message}") { }
}