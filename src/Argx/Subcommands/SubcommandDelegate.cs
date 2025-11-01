namespace Argx.Subcommands;

public delegate void SubcommandDelegate(string[] args);

public delegate Task AsyncSubcommandDelegate(string[] args);
