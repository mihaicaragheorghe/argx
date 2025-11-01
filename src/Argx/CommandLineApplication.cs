using Argx.Subcommands;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly ISubcommandStore _subcommands;

    public CommandLineApplication()
    {
        _subcommands = new SubcommandStore();
    }

    public CommandLineApplication(ISubcommandStore subcommands)
    {
        _subcommands = subcommands;
    }

    public void AddSubcommand(string name, AsyncSubcommandDelegate handler)
        => _subcommands.Register(name, SubcommandDelegateFactory.Create(handler));

    public void AddSubcommand(string name, SubcommandDelegate handler)
        => _subcommands.Register(name, SubcommandDelegateFactory.Create(handler));

    public async Task RunAsync(string[] args)
    {
        var subcommand = GetSubcommand(args);

        if (_subcommands.TryGetHandler(subcommand, out var asyncHandler))
        {
            await asyncHandler.Invoke(args[1..]);
        }
        else
        {
            throw new InvalidOperationException($"Unknown subcommand: {subcommand}");
        }
    }

    public void Run(string[] args) => RunAsync(args).GetAwaiter().GetResult();

    private static string GetSubcommand(string[] args)
        => args.Length > 0 ? args[0] : throw new ArgumentException("No subcommand provided.", nameof(args));
}