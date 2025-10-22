namespace Argx.Actions;

internal static class ActionRegistry
{
    private static Dictionary<string, ArgumentAction>? s_registry;

    private static Dictionary<string, ArgumentAction> Registry
        => s_registry ??= new Dictionary<string, ArgumentAction>
        {
            [ArgumentActions.Store] = new StoreAction(),
            [ArgumentActions.StoreConst] = new StoreConstAction(),
            [ArgumentActions.StoreTrue] = new StoreTrueAction(),
            [ArgumentActions.StoreFalse] = new StoreFalseAction(),
            [ArgumentActions.Count] = new CountAction(),
            [ArgumentActions.Append] = new AppendAction(),
            [ArgumentActions.NoAction] = new NoAction(),
        };

    internal static int DefaultArity(string action)
        => action switch
        {
            ArgumentActions.Store => 1,
            ArgumentActions.Append => 1,
            _ => 0
        };

    internal static void Add(string name, ArgumentAction action)
    {
        Registry.Add(name, action);
    }

    internal static bool TryGetHandler(string name, out ArgumentAction? action)
        => Registry.TryGetValue(name, out action);
}