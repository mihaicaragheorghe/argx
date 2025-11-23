namespace Argx.Errors;

internal sealed class NoSubcommandException : Exception
{
    internal NoSubcommandException()
        : base("No subcommand provided.")
    {
    }
}