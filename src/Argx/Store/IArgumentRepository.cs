namespace Argx.Store;

public interface IArgumentRepository
{
    /// <summary>
    /// Sets the value for the specified argument.
    /// </summary>
    /// <param name="arg">The argument key (dest).</param>
    /// <param name="value">The value to set.</param>
    void Set(string arg, object value);

    /// <summary>
    /// Tries to get the value associated with the specified argument, cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="arg">The argument key (dest).</param>
    /// <param name="value">When this method returns, contains the value associated with the specified argument, cast to the specified type, if the argument exists; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the argument exists; otherwise, <c>false</c>.</returns>
    bool TryGetValue<T>(string arg, out T value);

    /// <summary>
    /// Tries to get the string value associated with the specified argument.
    /// </summary>
    /// <param name="arg">The argument key (dest).</param>
    /// <param name="value">When this method returns, contains the string value associated with the specified argument, if the argument exists; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the argument exists; otherwise, <c>false</c>.</returns>
    bool TryGetValue(string arg, out string? value);

    /// <summary>
    /// Determines whether the repository contains the specified argument.
    /// </summary>
    /// <param name="arg">The argument key (dest).</param>
    /// <returns><c>true</c> if the argument exists; otherwise, <c>false</c>.</returns>
    bool Contains(string arg);
}