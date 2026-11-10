using System.Collections;

namespace Argx;

/// <summary>
/// Represents a set of argument aliases.
/// </summary>
public class AliasSet : IEnumerable<string>
{
    private readonly HashSet<string> _aliases = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="AliasSet"/> class.
    /// </summary>
    /// <param name="aliases">The aliases to include in the set.</param>
    public AliasSet(string[] aliases)
    {
        foreach (var alias in aliases)
        {
            Add(alias);
        }
    }

    /// <inheritdoc cref="HashSet{T}.Contains"/>
    public bool Contains(string alias) => _aliases.Contains(alias);

    /// <inheritdoc cref="HashSet{T}.Count"/>
    public int Count => _aliases.Count;

    /// <summary>
    /// Gets the first alias in the set.
    /// </summary>
    public string First() => _aliases.First();

    /// <summary>
    /// Adds an alias to the set.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the alias is null or empty, or if it does not start with '-'.
    /// </exception>
    /// <remarks>
    /// Aliases must start with a '-' character (e.g., '-h' or '--help').<br/>
    /// If alias already exists in the set, it will not be added again.
    /// </remarks>
    public void Add(string alias)
    {
        if (!IsValid(alias))
        {
            throw new ArgumentException($"Argument {alias}: alias must start with '-'");
        }

        _aliases.Add(alias);
    }

    private static bool IsValid(string alias) => !string.IsNullOrWhiteSpace(alias) && alias[0] == '-';

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator() => _aliases.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public override string ToString() => string.Join(", ", _aliases);
}