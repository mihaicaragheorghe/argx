using System.Diagnostics.CodeAnalysis;

namespace Argx.Subcommands;

public interface ISubcommandStore
{
    Subcommand Register(string name, AsyncSubcommandDelegate handler);

    bool TryGetHandler(string name, [MaybeNullWhen(false)] out AsyncSubcommandDelegate handler);

    IEnumerable<Subcommand> GetRegisteredSubcommands();
}
