using Argx.Subcommands;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly Dictionary<string, SubcommandDelegate> _subcommands = new(StringComparer.OrdinalIgnoreCase);

    public void AddSubcommand<T>(string name, T handler) where T : Delegate
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Subcommand name cannot be null or whitespace.", nameof(name));
        }

        _subcommands[name] = SubcommandDelegateFactory.Create(handler);
    }

    public async Task RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("No subcommand provided.", nameof(args));
        }

        var subcommand = args[0];

        if (!_subcommands.TryGetValue(subcommand, out var handler))
        {
            throw new InvalidOperationException($"Subcommand '{subcommand}' not found.");
        }

        Environment.ExitCode = await handler.Invoke(args[1..]);
    }

    public void Run(string[] args) => RunAsync(args).GetAwaiter().GetResult();
}