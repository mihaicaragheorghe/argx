namespace Argx.Subcommands;

/// <summary>
/// Represents a subcommand of a command line application.
/// </summary>
public class Subcommand
{
    /// <summary>
    /// The name of the subcommand.
    /// This is the string used to invoke the subcommand.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The usage information for the subcommand.
    /// Will be displayed in help text.
    /// </summary>
    public string Usage { get; private set; } = null!;

    /// <summary>
    /// The handler delegate for the subcommand.
    /// </summary>
    public AsyncSubcommandDelegate Handler { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Subcommand"/> class.
    /// </summary>
    /// <param name="name">The name of the subcommand.</param>
    /// <param name="handler">The handler delegate for the subcommand.</param>
    public Subcommand(string name, AsyncSubcommandDelegate handler)
    {
        Name = name;
        Handler = handler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Subcommand"/> class with usage information.
    /// </summary>
    /// <param name="name">The name of the subcommand.</param>
    /// <param name="usage">The usage information for the subcommand.</param>
    /// <param name="handler">The handler delegate for the subcommand.</param> 
    public Subcommand(string name, string usage, AsyncSubcommandDelegate handler)
        : this(name, handler)
    {
        Usage = usage;
    }

    /// <summary>
    /// Sets the usage information for the subcommand.
    /// </summary>
    /// <param name="usage">The usage information.</param>
    /// <returns>The subcommand instance.</returns>
    public Subcommand WithUsage(string usage)
    {
        Usage = usage;
        return this;
    }
}