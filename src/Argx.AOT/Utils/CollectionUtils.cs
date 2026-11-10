using System.Collections;

using Argx.Binding;

namespace Argx.Utils;

internal static class CollectionUtils
{
    // Used by standalone actions and converters; never exposed for registration.
    private static readonly CollectionFactoryRegistry DefaultFactories = new();

    internal static IList CreateCollection(Type type, int capacity, Func<int, IList>? factory = null)
        => (factory ?? DefaultFactories.GetFactory(type))(capacity);
}
