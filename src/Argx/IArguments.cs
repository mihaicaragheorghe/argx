namespace Argx;

/// <summary>
/// Represents the parsed command-line arguments.
/// Provides methods to access argument values in a type-safe manner.
/// </summary>
public interface IArguments
{
    /// <summary>
    /// Any extra arguments that were not matched by the parser.
    /// </summary>
    public List<string> Extras { get; }

    /// <summary>
    /// Gets the value associated with the specified argument key (dest).
    /// </summary>
    /// <param name="key">The argument key (dest).</param>
    /// <returns>
    /// The value associated with the specified key, or <c>null</c> if the key does not exist.
    /// </returns>
    public string? this[string key] { get; }

    /// <summary>
    /// Gets the value associated with the specified argument.
    /// </summary>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns>
    /// The value associated with the specified argument, or <c>null</c> if the argument does not exist.
    /// </returns>
    public string? GetValue(string arg);

    /// <summary>
    /// Gets the value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns>
    /// The value associated with the specified argument, cast to the specified type, or the default value of <typeparamref name="T"/> if the argument does not exist.
    /// </returns>
    public T GetValue<T>(string arg);

    /// <summary>
    /// Tries to get the value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <param name="value">When this method returns, contains the value associated with the specified argument, cast to the specified type, if the argument exists; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the argument exists; otherwise, <c>false</c>.</returns>
    public bool TryGetValue<T>(string arg, out T value);

    /// <summary>
    /// Gets the required value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns>The value associated with the specified argument, cast to the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified argument does not exist.</exception>
    public T GetRequired<T>(string arg);

    /// <summary>
    /// Binds the argument values to the properties of the specified instance.
    /// </summary>
    /// <remarks>
    /// The properties of the instance should have public setters.
    /// By default, the property name converted to kebab-case will be used as the argument name,
    /// but this can be overridden using the <see cref="Attributes.ArgumentAttribute"/>.
    /// Properties marked with the <see cref="Attributes.IgnoreAttribute"/> will be ignored during binding
    /// </remarks>
    /// <typeparam name="T">The type of the instance to bind the argument values to.</typeparam>
    /// <param name="instance">The instance to bind the argument values to.</param>
    void Bind<T>(T instance) where T : class;

    /// <summary>
    /// Creates a new instance of type <typeparamref name="T"/>, binds the argument values to its properties, and returns it.
    /// </summary>
    /// <remarks>
    /// The properties of the instance should have public setters.
    /// By default, the property name converted to kebab-case will be used as the argument name,
    /// but this can be overridden using the <see cref="Attributes.ArgumentAttribute"/>.
    /// Properties marked with the <see cref="Attributes.IgnoreAttribute"/> will be ignored during binding
    /// </remarks>
    /// <typeparam name="T">The type of the instance to create and bind the argument values to.</typeparam>
    /// <returns>The created and bound instance of type <typeparamref name="T"/>.</returns>
    T Get<T>() where T : class, new();
}