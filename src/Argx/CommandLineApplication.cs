using Argx.Subcommands;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly ISubcomamndStore _subcommands;

    public CommandLineApplication()
    {
        _subcommands = new SubcommandStore();
    }

    public CommandLineApplication(ISubcomamndStore subcommands)
    {
        _subcommands = subcommands;
    }

    public void AddSubcommand(string name, AsyncSubcommandDelegate handler) => _subcommands.Register(name, handler);

    public void AddSubcommand(string name, SubcommandDelegate handler) => _subcommands.Register(name, handler);

    public async Task RunAsync(string[] args)
    {
        var subcommand = GetSubcommand(args);

        if (_subcommands.TryGetAsyncHandler(subcommand, out var asyncHandler))
        {
            await asyncHandler.Invoke(args[1..]);
        }
        else
        {
            RunSync(subcommand, args);
        }
    }

    public void Run(string[] args)
    {
        var subcommand = GetSubcommand(args);

        if (_subcommands.TryGetAsyncHandler(subcommand, out var asyncHandler))
        {
            asyncHandler.Invoke(args[1..]).GetAwaiter().GetResult();
        }
        else
        {
            RunSync(subcommand, args);
        }
    }

    private void RunSync(string subcommand, string[] args)
    {
        if (_subcommands.TryGetHandler(subcommand, out var handler))
        {
            handler.Invoke(args[1..]);
        }
        else
        {
            throw new InvalidOperationException($"Unknown subcommand: {subcommand}");
        }
    }

    private static string GetSubcommand(string[] args)
        => args.Length > 0 ? args[0] : throw new ArgumentException("No subcommand provided.", nameof(args));
}