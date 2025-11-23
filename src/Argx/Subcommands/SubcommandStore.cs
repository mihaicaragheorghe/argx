using System.Diagnostics.CodeAnalysis;

namespace Argx.Subcommands;

internal class SubcommandStore : ISubcommandStore
{
    private readonly Dictionary<string, AsyncSubcommandDelegate> _subcommands = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> GetRegisteredSubcommandNames()
    {
        return _subcommands.Keys;
    }

    public void Register(string name, AsyncSubcommandDelegate handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        _subcommands[name] = handler;
    }

    public bool TryGetHandler(string name, [MaybeNullWhen(false)] out AsyncSubcommandDelegate handler)
    {
        return _subcommands.TryGetValue(name, out handler);
    }
}