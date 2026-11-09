namespace Argx.Errors;

/// <summary>
/// Exception thrown when no subcommand is provided.
/// </summary>
public sealed class NoSubcommandException : Exception
{
    public NoSubcommandException()
        : base("No subcommand provided.")
    {
    }
}