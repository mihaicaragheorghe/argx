using Argx.Abstractions;
using Argx.Errors;
using Argx.Help;
using Argx.Subcommands;
using Argx.Utils;

namespace Argx;

/// <summary>
/// Represents a command line application.
/// </summary>
public sealed class CommandLineApplication : ICommandLineApplication
{
    /// <inheritdoc/>
    public string? Name { get; }

    /// <inheritdoc/>
    public string? Usage { get; }

    /// <inheritdoc/>
    public string? Description { get; }

    /// <inheritdoc/>
    public string? Epilogue { get; }

    private readonly ISubcommandStore _subcommands;
    private readonly IEnvironment _env;
    private readonly bool _exitOnError = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineApplication"/> class.
    /// </summary>
    /// <param name="name">
    /// The name of the command line application.
    /// If not provided, the application will infer the name using argument zero from the environment.
    /// </param>
    /// <param name="usage">
    /// The usage information for the command line application.
    /// If not provided, a default usage string will be generated based on registered subcommands.
    /// </param>
    /// <param name="description">The description of the command line application.</param>
    /// <param name="epilogue">The epilogue text for the command line application.</param>
    /// <param name="exitOnError">Indicates whether the application should exit on error or throw exceptions.</param>
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

    /// <inheritdoc/>
    public Subcommand AddSubcommand(string name, AsyncSubcommandDelegate handler)
        => _subcommands.Register(name, SubcommandDelegateFactory.Create(handler));

    /// <inheritdoc/>
    public Subcommand AddSubcommand(string name, SubcommandDelegate handler)
        => _subcommands.Register(name, SubcommandDelegateFactory.Create(handler));

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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