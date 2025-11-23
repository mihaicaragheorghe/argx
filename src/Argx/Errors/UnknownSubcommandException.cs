namespace Argx.Errors;

internal sealed class UnknownSubcommandException : Exception
{
    internal UnknownSubcommandException(string subcommand)
        : base($"Unknown subcommand: {subcommand}")
    {
    }
}