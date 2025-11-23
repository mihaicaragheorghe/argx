using System.Diagnostics.CodeAnalysis;

namespace Argx.Subcommands;

public interface ISubcommandStore
{
    void Register(string name, AsyncSubcommandDelegate handler);

    bool TryGetHandler(string name, [MaybeNullWhen(false)] out AsyncSubcommandDelegate handler);

    IEnumerable<string> GetRegisteredSubcommandNames();
}
