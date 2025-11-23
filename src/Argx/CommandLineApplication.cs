using Argx.Errors;
using Argx.Help;
using Argx.Subcommands;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly ISubcommandStore _subcommands;
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
        _subcommands = new SubcommandStore();
        _subcommands.Register("help", _ =>
            {
                WriteHelp(Console.Out);
                return Task.CompletedTask;
            })
            .WithUsage("Display help information about this application");
    }

    public CommandLineApplication(
        ISubcommandStore subcommands,
        string? name = null,
        string? description = null,
        string? usage = null,
        string? epilogue = null,
        bool exitOnError = true)
        : this(name, description, usage, epilogue, exitOnError)
    {
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
            Console.WriteLine($"{GetName()}: {ex.Message}. See 'help' for a list of available subcommands.");

            if (_exitOnError)
            {
                Environment.Exit(1);
            }
            else throw;
        }
        catch (NoSubcommandException ex)
        {
            Console.WriteLine($"{GetName()}: {ex.Message}. See 'help' for a list of available subcommands.");

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
            builder.AddSection("Usage", $"{GetName()} <subcommand> [<args>]");
        }
        else
        {
            builder.AddSection("Usage", Usage);
        }

        var rows = _subcommands.GetRegisteredSubcommands()
            .Select(sc => new TwoColumnRow(Left: sc.Name, Right: sc.Usage ?? string.Empty))
            .OrderBy(r => r.Left)
            .ToList();

        builder.AddRows("Available subcommands", rows);

        if (!string.IsNullOrEmpty(Epilogue))
        {
            builder.AddSection(string.Empty, Epilogue);
        }

        writer.WriteLine(builder.Build());
    }

    private string GetName()
        => string.IsNullOrEmpty(Name)
            ? Path.GetFileName(Environment.GetCommandLineArgs()[0])
            : Name;
}