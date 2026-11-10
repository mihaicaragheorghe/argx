namespace Argx.Errors;

/// <summary>
/// Exception thrown when an unknown subcommand is provided.
/// </summary>
public sealed class UnknownSubcommandException(string subcommand)
    : Exception($"Unknown subcommand: {subcommand}")
{
}