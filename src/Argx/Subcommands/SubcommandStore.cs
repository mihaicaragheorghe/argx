using System.Diagnostics.CodeAnalysis;

namespace Argx.Subcommands;

internal class SubcommandStore : ISubcommandStore
{
    private readonly Dictionary<string, Subcommand> _subcommands = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Subcommand> GetRegisteredSubcommands()
    {
        return _subcommands.Values;
    }

    public Subcommand Register(string name, AsyncSubcommandDelegate handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var subcommand = new Subcommand(name, handler);
        _subcommands[name] = subcommand;
        return subcommand;
    }

    public bool TryGetHandler(string name, [MaybeNullWhen(false)] out AsyncSubcommandDelegate handler)
    {
        if (_subcommands.TryGetValue(name, out var subcommand))
        {
            handler = subcommand.Handler;
            return true;
        }

        handler = null;
        return false;
    }
}