using Argx.Errors;
using Argx.Help;
using Argx.Subcommands;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly ISubcommandStore _subcommands;
    private readonly bool _exitOnError = true;
    private readonly string? _description;
    private readonly string? _usage;

    public string Name => field ?? Environment.GetCommandLineArgs()[0];

    public CommandLineApplication(
        string? name = null,
        string? description = null,
        string? usage = null,
        bool exitOnError = true)
    {
        Name = name;
        _usage = usage;
        _description = description;
        _exitOnError = exitOnError;
        _subcommands = new SubcommandStore();
    }

    public CommandLineApplication(
        ISubcommandStore subcommands,
        string? name = null,
        string? description = null,
        string? usage = null,
        bool exitOnError = true)
        : this(name, description, usage, exitOnError)
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
            Console.WriteLine($"{Name}: {ex.Message}. See --help for a list of available subcommands.");

            if (_exitOnError)
            {
                Environment.Exit(1);
            }
            else throw;
        }
        catch (NoSubcommandException ex)
        {
            Console.WriteLine($"{Name}: {ex.Message}. See --help for a list of available subcommands.");

            if (_exitOnError)
            {
                Environment.Exit(1);
            }
            else throw;
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

    private void WriteHelp(TextWriter writer)
    {
        var builder = new HelpBuilder(HelpConfiguration.Default());

        builder.AddSection(Name, _description ?? string.Empty);

        builder.AddSection("Usage", string.IsNullOrEmpty(_usage) ? $"{Name} <subcommand> [<args>]" : _usage);

        var subcommandNames = _subcommands.GetRegisteredSubcommandNames().ToList();
        if (subcommandNames.Any())
        {
            builder.AddSection("Available Subcommands", string.Join(Environment.NewLine, subcommandNames));
        }

        writer.WriteLine(builder.Build());
    }
}