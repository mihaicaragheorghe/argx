namespace Argx.Parsing;

internal class PositionalList : List<Argument>
{
    private int _position;

    internal bool ReachedEnd => _position >= Count;

    /// <summary>
    /// Returns the current element and advances the position.
    /// </summary>
    internal Argument Pop()
    {
        if (ReachedEnd)
        {
            throw new InvalidOperationException("No more arguments in the list");
        }

        return this[_position++];
    }
}