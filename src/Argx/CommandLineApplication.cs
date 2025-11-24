using Argx.Abstractions;
using Argx.Errors;
using Argx.Help;
using Argx.Subcommands;
using Argx.Utils;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly ISubcommandStore _subcommands;
    private readonly IEnvironment _env;
    private readonly bool _exitOnError = true;

    public string? Name { get; }
    public string? Usage { get; }
    public string? Description { get; }
    public string? Epilogue { get; }

    public CommandLineApplication(
        string? name = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        bool exitOnError = true)
    {
        Name = name;
        Usage = usage;
        Description = description;
        Epilogue = epilogue;
        _exitOnError = exitOnError;
        _env = new EnvironmentControl();
        _subcommands = new SubcommandStore();
        _subcommands.Register("help", _ =>
            {
                WriteHelp(Console.Out);
                return Task.CompletedTask;
            })
            .WithUsage("Display help information about this application");
    }

    internal CommandLineApplication(
        ISubcommandStore subcommands,
        IEnvironment? environment = null,
        string? name = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        bool exitOnError = true)
        : this(name, description, usage, epilogue, exitOnError)
    {
        _env = environment ?? new EnvironmentControl();
        _subcommands = subcommands;
    }

    public Subcommand AddSubcommand(string name, AsyncSubcommandDelegate handler)
        => _subcommands.Register(name, SubcommandDelegateFactory.Create(handler));

    public Subcommand AddSubcommand(string name, SubcommandDelegate handler)
        => _subcommands.Register(name, SubcommandDelegateFactory.Create(handler));

    public async Task RunAsync(string[] args)
    {
        try
        {
            await RunAsyncImpl(args);
        }
        catch (UnknownSubcommandException ex)
        {
            if (_exitOnError)
            {
                Console.WriteLine($"{GetName()}: {ex.Message}. See 'help' for a list of available commands.");
                _env.Exit(1);
            }
            else throw;
        }
        catch (NoSubcommandException)
        {
            if (_exitOnError)
            {
                WriteHelp(Console.Out);
                _env.Exit(1);
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

    internal void WriteHelp(TextWriter writer)
    {
        var builder = new HelpBuilder();

        if (!string.IsNullOrEmpty(Name))
        {
            builder.AddSection(Name, Description ?? string.Empty);
        }
        else if (!string.IsNullOrEmpty(Description))
        {
            builder.AddText(Description);
        }

        if (string.IsNullOrEmpty(Usage))
        {
            builder.AddSection("Usage", $"{GetName()} <command> [<arguments>]");
        }
        else
        {
            builder.AddSection("Usage", Usage);
        }

        var rows = _subcommands.GetRegisteredSubcommands()
            .Select(sc => new TwoColumnRow(Left: sc.Name, Right: sc.Usage ?? string.Empty))
            .OrderBy(r => r.Left)
            .ToList();

        builder.AddRows("Available commands", rows);

        builder.AddText("Use '<command> --help' to get more information about a specific command.");

        if (!string.IsNullOrEmpty(Epilogue))
        {
            builder.AddSection(string.Empty, Epilogue);
        }

        writer.WriteLine(builder.Build());
    }

    private string GetName()
        => string.IsNullOrEmpty(Name)
            ? Path.GetFileName(_env.GetCommandLineArgs()[0])
            : Name;
}