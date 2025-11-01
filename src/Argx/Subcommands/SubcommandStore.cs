using System.Diagnostics.CodeAnalysis;

namespace Argx.Subcommands;

internal class SubcommandStore : ISubcomamndStore
{
    private readonly Dictionary<string, SubcommandDelegate> _subcommands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AsyncSubcommandDelegate> _asyncSubcommands = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string name, Delegate handler)
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

    public bool TryGetAsyncHandler(string name, [MaybeNullWhen(false)] out AsyncSubcommandDelegate handler)
    {
        return _asyncSubcommands.TryGetValue(name, out handler);
    }

    public bool TryGetHandler(string name, [MaybeNullWhen(false)] out SubcommandDelegate handler)
    {
        return _subcommands.TryGetValue(name, out handler);
    }
}