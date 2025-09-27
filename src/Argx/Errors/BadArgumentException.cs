namespace Argx.Errors;

internal class BadArgumentException : Exception
{
    internal BadArgumentException(string arg, string message) : base($"Error: argument {arg}: {message}") { }
}