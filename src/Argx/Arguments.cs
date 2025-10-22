using Argx.Store;

namespace Argx;

/// <inheritdoc cref="IArguments"/>
public class Arguments : IArguments
{
    private readonly IArgumentRepository _repository;

    /// <inheritdoc />
    public List<string> Extras { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Arguments"/> class.
    /// </summary>
    /// <param name="repository">The argument repository to use for retrieving argument values.</param>
    public Arguments(IArgumentRepository repository)
    {
        _repository = repository;
        Extras = [];
    }

    /// <inheritdoc />
    public string? this[string key] => GetValue(key);

    /// <inheritdoc />
    public string? GetValue(string arg)
    {
        if (_repository.TryGetValue(arg, out var value))
        {
            return value;
        }

        return null;
    }

    /// <inheritdoc />
    public T GetValue<T>(string arg) => TryGetValue(arg, out T value) ? value : default!;

    /// <inheritdoc />
    public bool TryGetValue<T>(string arg, out T value) => _repository.TryGetValue(arg, out value);

    /// <inheritdoc />
    public T GetRequired<T>(string arg)
    {
        if (_repository.TryGetValue<T>(arg, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Missing required value for argument '{arg}' (expected type: {typeof(T).Name}).");
    }
}