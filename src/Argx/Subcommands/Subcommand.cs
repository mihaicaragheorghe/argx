namespace Argx.Subcommands;

public class Subcommand
{
    public string Name { get; private set; }

    public string Usage { get; private set; } = null!;

    public AsyncSubcommandDelegate Handler { get; }

    public Subcommand(string name, AsyncSubcommandDelegate handler)
    {
        Name = name;
        Handler = handler;
    }

    public Subcommand(string name, string usage, AsyncSubcommandDelegate handler)
        : this(name, handler)
    {
        Usage = usage;
    }

    public Subcommand WithUsage(string usage)
    {
        Usage = usage;
        return this;
    }
}