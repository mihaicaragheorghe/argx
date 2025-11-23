using Argx.Errors;
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
        try
        {
            await RunAsyncImpl(args);
        }
        catch (UnknownSubcommandException ex)
        {
            Console.WriteLine($"{ex.Message}. See --help for a list of available subcommands.");
            Environment.Exit(1); // TODO: configuration
        }
        catch (NoSubcommandException ex)
        {
            Console.WriteLine($"{ex.Message} See --help for a list of available subcommands.");
            Environment.Exit(1); // TODO: configuration
        }
    }

    internal async Task RunAsyncImpl(string[] args)
    {
        var subcommand = GetSubcommand(args);

        if (_subcommands.TryGetHandler(subcommand, out var asyncHandler))
        {
            await asyncHandler.Invoke(args[1..]);
        }
        else
        {
            throw new UnknownSubcommandException(subcommand);
        }
    }

    public void Run(string[] args) => RunAsync(args).GetAwaiter().GetResult();

    private static string GetSubcommand(string[] args)
        => args.Length > 0 ? args[0] : throw new NoSubcommandException();
}