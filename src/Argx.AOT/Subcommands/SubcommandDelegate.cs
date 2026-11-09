namespace Argx.Subcommands;

/// <summary>
/// Delegate for handling a subcommand.
/// </summary>
/// <param name="args">The arguments passed to the subcommand.</param>
public delegate void SubcommandDelegate(string[] args);

/// <summary>
/// Asynchronous delegate for handling a subcommand.
/// </summary>
/// <param name="args">The arguments passed to the subcommand.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task AsyncSubcommandDelegate(string[] args);
