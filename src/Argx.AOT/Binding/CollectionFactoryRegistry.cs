using System.Collections;

namespace Argx.Binding;

/// <summary>
/// Registers AOT-safe factories for the collection types used by a parser.
/// </summary>
public sealed class CollectionFactoryRegistry
{
    private readonly Dictionary<Type, Func<int, IList>> _factories = [];

    /// <summary>
    /// Creates a registry containing the built-in argument element types and their nullable forms.
    /// </summary>
    public CollectionFactoryRegistry()
    {
        Register<string>();
        RegisterValueType<bool>();
        RegisterValueType<int>();
        RegisterValueType<long>();
        RegisterValueType<short>();
        RegisterValueType<uint>();
        RegisterValueType<ulong>();
        RegisterValueType<ushort>();
        RegisterValueType<decimal>();
        RegisterValueType<double>();
        RegisterValueType<float>();
        RegisterValueType<Guid>();
        RegisterValueType<DateTime>();
        RegisterValueType<DateTimeOffset>();
        RegisterValueType<TimeSpan>();
    }

    /// <summary>
    /// Registers arrays, lists, and the IEnumerable, IList, and ICollection interfaces for an element type.
    /// Repeated registration is safe. This does not register token conversion for the element type.
    /// </summary>
    /// <typeparam name="TElement">The collection element type.</typeparam>
    public void Register<TElement>()
    {
        Func<int, IList> arrayFactory = static capacity => new TElement[capacity];
        _factories[typeof(TElement[])] = arrayFactory;
        _factories[typeof(IEnumerable<TElement>)] = arrayFactory;
        _factories[typeof(IList<TElement>)] = arrayFactory;
        _factories[typeof(ICollection<TElement>)] = arrayFactory;
        _factories[typeof(List<TElement>)] = static capacity => new List<TElement>(capacity);
    }

    internal Func<int, IList> GetFactory(Type type)
    {
        if (_factories.TryGetValue(type, out var factory))
        {
            return factory;
        }

        throw new ArgumentException(
            $"Collection type '{type}' is not registered. For arrays, List<T>, IEnumerable<T>, IList<T>, or ICollection<T>, " +
            "register the element type with ArgumentParserConfiguration.Collections.Register<TElement>() before adding the argument.");
    }

    private void RegisterValueType<TElement>() where TElement : struct
    {
        Register<TElement>();
        Register<TElement?>();
    }
}
