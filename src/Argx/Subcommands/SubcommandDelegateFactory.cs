namespace Argx.Subcommands;

internal class SubcommandDelegateFactory
{
    internal static AsyncSubcommandDelegate Create(Delegate handler)
    {
        if (handler is AsyncSubcommandDelegate asyncHandler)
        {
            return asyncHandler;
        }
        else if (handler is SubcommandDelegate syncHandler)
        {
            return args =>
            {
                syncHandler(args);
                return Task.CompletedTask;
            };
        }
        else
        {
            throw new ArgumentException(
                $"Handler must be either {nameof(AsyncSubcommandDelegate)} or {nameof(SubcommandDelegate)}. Found: {handler.GetType()}");
        }
    }
}