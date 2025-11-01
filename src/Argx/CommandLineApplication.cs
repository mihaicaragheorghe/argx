using Argx.Subcommands;

namespace Argx;

public sealed class CommandLineApplication
{
    private readonly Dictionary<string, SubcommandDelegate> _subcommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AsyncSubcommandDelegate> _asyncSubcommands = new(StringComparer.OrdinalIgnoreCase);

    public void AddSubcommand(string name, AsyncSubcommandDelegate handler) => RegisterSubcommand(name, handler);
    public void AddSubcommand(string name, SubcommandDelegate handler) => RegisterSubcommand(name, handler);

    private void RegisterSubcommand(string name, Delegate handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Subcommand name cannot be null or whitespace.", nameof(name));
        }

        if (handler is AsyncSubcommandDelegate asyncHandler)
        {
            _asyncSubcommands[name] = asyncHandler;
        }
        else if (handler is SubcommandDelegate syncHandler)
        {
            _subcommands[name] = syncHandler;
        }
        else
        {
            throw new ArgumentException(
                $"Handler must be either {nameof(AsyncSubcommandDelegate)} or {nameof(SubcommandDelegate)}. Found: {handler.GetType()}");
        }
    }

    public async Task RunAsync(string[] args)
    {
        var subcommand = GetSubcommand(args);

        if (_asyncSubcommands.TryGetValue(subcommand, out var asyncHandler))
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

        if (_asyncSubcommands.TryGetValue(subcommand, out var asyncHandler))
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
        if (_subcommands.TryGetValue(subcommand, out var handler))
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