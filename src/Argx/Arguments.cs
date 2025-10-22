using Argx.Store;

namespace Argx;

/// <summary>
/// Represents the parsed command-line arguments.
/// Provides methods to access argument values in a type-safe manner.
/// </summary>
public class Arguments
{
    private readonly IArgumentRepository _repository;

    /// <summary>
    /// Any extra arguments that were not matched by the parser.
    /// </summary>
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

    /// <summary>
    /// Gets the value associated with the specified argument key (dest).
    /// </summary>
    /// <param name="key">The argument key (dest).</param>
    /// <returns>
    /// The value associated with the specified key, or <c>null</c> if the key does not exist.
    /// </returns>
    public string? this[string key] => GetValue(key);

    /// <summary>
    /// Gets the value associated with the specified argument.
    /// </summary>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns>
    /// The value associated with the specified argument, or <c>null</c> if the argument does not exist.
    /// </returns>
    public string? GetValue(string arg)
    {
        if (_repository.TryGetValue(arg, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Gets the value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns>
    /// The value associated with the specified argument, cast to the specified type, or the default value of <typeparamref name="T"/> if the argument does not exist.
    /// </returns>
    public T GetValue<T>(string arg) => TryGetValue(arg, out T value) ? value : default!;

    /// <summary>
    /// Tries to get the value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <param name="value">When this method returns, contains the value associated with the specified argument, cast to the specified type, if the argument exists; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the argument exists; otherwise, <c>false</c>.</returns>
    public bool TryGetValue<T>(string arg, out T value) => _repository.TryGetValue(arg, out value);

    /// <summary>
    /// Gets the required value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns>The value associated with the specified argument, cast to the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified argument does not exist.</exception>
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