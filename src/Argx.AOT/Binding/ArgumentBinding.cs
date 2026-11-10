using System.Collections.Concurrent;

namespace Argx.Binding;

public interface IArgumentBindingResolver
{
    void Bind(IArguments args, object instance, string prefix);
}

public abstract class ArgumentBindingContext : IArgumentBindingResolver
{
    public abstract void Bind(IArguments args, object instance, string prefix);
    public abstract void RegisterCollections();
    public abstract void Register();
}

public sealed class ArgumentBinding<T> where T : class
{
    private readonly Action<IArguments, T, string, IArgumentBindingResolver> _bind;
    private readonly Func<T>? _create;
    private readonly IArgumentBindingResolver _resolver;

    public ArgumentBinding(Action<IArguments, T, string, IArgumentBindingResolver> bind,
        Func<T>? create, IArgumentBindingResolver resolver)
    {
        _bind = bind;
        _create = create;
        _resolver = resolver;
    }

    public T Create() => _create is null
        ? throw new MissingMethodException($"No accessible parameterless constructor for {typeof(T)}.")
        : _create();

    public void Bind(IArguments args, T instance) => _resolver.Bind(args, instance, "");

    public void BindDeclared(IArguments args, T instance, string prefix, IArgumentBindingResolver resolver)
        => _bind(args, instance, prefix, resolver);
}

public sealed class ArgumentBindingRegistry : IArgumentBindingResolver
{
    private readonly ConcurrentDictionary<Type, Entry> _bindings = new();
    public static ArgumentBindingRegistry Default { get; } = new();

    private sealed class Entry(object metadata, Action<IArguments, object, string> bind)
    {
        public object Metadata { get; } = metadata;
        public Action<IArguments, object, string> Bind { get; } = bind;
    }

    public void Register<T>(ArgumentBinding<T> binding) where T : class
    {
        var entry = _bindings.GetOrAdd(typeof(T), _ => new Entry(binding,
            (args, value, prefix) => binding.BindDeclared(args, (T)value, prefix, this)));
        if (!ReferenceEquals(entry.Metadata, binding))
            throw new InvalidOperationException($"Conflicting binding registration for {typeof(T)}.");
    }

    public ArgumentBinding<T> Get<T>() where T : class => _bindings.TryGetValue(typeof(T), out var entry)
        ? (ArgumentBinding<T>)entry.Metadata
        : throw Missing(typeof(T));

    public void Bind(IArguments args, object instance, string prefix)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        if (!_bindings.TryGetValue(instance.GetType(), out var entry)) throw Missing(instance.GetType());
        entry.Bind(args, instance, prefix);
    }

    private static InvalidOperationException Missing(Type type) => new(
        $"No generated binding for {type}. Include the type in an ArgxBinding context and call context.Register().");
}
