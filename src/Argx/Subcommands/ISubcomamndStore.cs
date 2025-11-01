using System.Diagnostics.CodeAnalysis;

namespace Argx.Subcommands;

public interface ISubcomamndStore
{
    void Register(string name, Delegate handler);

    bool TryGetHandler(string name, [MaybeNullWhen(false)] out SubcommandDelegate handler);

    bool TryGetAsyncHandler(string name, [MaybeNullWhen(false)] out AsyncSubcommandDelegate handler);
}
