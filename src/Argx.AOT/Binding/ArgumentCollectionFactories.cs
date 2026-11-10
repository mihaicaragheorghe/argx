using System.Collections;
using System.Collections.Concurrent;

namespace Argx.Binding;

/// <summary>
/// Registers AOT-safe factories for argument collections with additional element types.
/// </summary>
public static class ArgumentCollectionFactories
{
    private static readonly ConcurrentDictionary<Type, Func<int, IList>> Factories = new();

    /// <summary>
    /// Registers arrays, lists, and enumerable collection interfaces for element type <typeparamref name="T"/>.
    /// Register before parsing arguments. This does not register a token converter for the element type.
    /// </summary>
    public static void Register<T>()
    {
        Func<int, IList> arrayFactory = capacity => new T[capacity];
        Factories[typeof(T[])] = arrayFactory;
        Factories[typeof(IEnumerable<T>)] = arrayFactory;
        Factories[typeof(IList<T>)] = arrayFactory;
        Factories[typeof(ICollection<T>)] = arrayFactory;
        Factories[typeof(List<T>)] = capacity => new List<T>(capacity);
    }

    internal static bool TryCreate(Type type, int capacity, out IList collection)
    {
        if (Factories.TryGetValue(type, out var factory))
        {
            collection = factory(capacity);
            return true;
        }

        collection = null!;
        return false;
    }
}
