namespace Argx.Subcommands;

internal class SubcommandDelegateFactory
{
    internal static AsyncSubcommandDelegate Create(Delegate handler)
    {
        switch (handler)
        {
            case AsyncSubcommandDelegate asyncHandler:
                return asyncHandler;

            case SubcommandDelegate syncHandler:
                {
                    return args =>
                    {
                        syncHandler(args);
                        return Task.CompletedTask;
                    };
                }

            default:
                throw new ArgumentException($"Cannot create subcommand delegate from {handler.GetType()}");
        }
    }
}