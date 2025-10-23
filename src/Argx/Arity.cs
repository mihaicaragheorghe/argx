namespace Argx;

/// <summary>
/// Defines the arity (number of values) an argument can accept.
/// </summary>
public class Arity
{
    /// <summary>
    /// The string representation of the arity.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Optional value. The argument can be provided with or without a value.
    /// </summary>
    /// <remarks>
    /// One argument will be consumed from the command line if possible, and produced as a single item.
    /// </remarks>
    public const string Optional = "?";

    /// <summary>
    /// Any number of values. The argument can be provided with zero or more values.
    /// </summary>
    /// <remarks>
    /// Zero or more arguments will be consumed from the command line, and produced as a collection.<br/>
    /// Arguments with this arity will consume all following values until an option is encountered
    /// or the end of the command line is reached.
    /// </remarks>
    public const string Any = "*";

    /// <summary>
    /// At least one value. The argument must be provided with one or more values.
    /// </summary>
    /// <remarks>
    /// One or more arguments will be consumed from the command line, and produced as a collection.<br/>
    /// Arguments with this arity will consume all following values until an option is encountered
    /// or the end of the command line is reached.
    /// </remarks>
    public const string AtLeastOne = "+";

    /// <summary>
    /// Initializes a new instance of the <see cref="Arity"/> class.
    /// </summary>
    /// <param name="value">The string representation of the arity.</param>
    public Arity(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Arity"/> class.
    /// </summary>
    /// <param name="n">The fixed number of values the argument can accept.</param>
    public Arity(int n)
    {
        Value = n.ToString();
    }

    /// <summary>
    /// Indicates whether the arity is a fixed number.
    /// </summary>
    public bool IsFixed => int.TryParse(Value, out _);

    /// <summary>
    /// Indicates whether the arity is <see cref="Optional"/> or <see cref="Any"/>.
    /// </summary>
    public bool IsOptional => Value is Any or Optional;

    /// <summary>
    /// Indicates whether the arity allows multiple values.
    /// </summary>
    public bool AcceptsMultipleValues => Value == Any || Value == AtLeastOne || int.TryParse(Value, out var n) && n > 1;

    /// <summary>
    /// Implicitly converts a string to an <see cref="Arity"/> instance.
    /// </summary>
    /// <param name="value">The string representation of the arity.</param>
    /// <returns>An <see cref="Arity"/> instance.</returns>
    public static implicit operator Arity(string value) => new(value);

    /// <summary>
    /// Implicitly converts an integer to an <see cref="Arity"/> instance.
    /// </summary>
    /// <param name="value">The fixed number of values the argument can accept.</param>
    /// <returns>An <see cref="Arity"/> instance.</returns>
    public static implicit operator Arity(int value) => new(value.ToString());

    /// <summary>
    /// Implicitly converts a char to an <see cref="Arity"/> instance.
    /// </summary>
    /// <param name="value">The char representation of the arity.</param>
    /// <returns>An <see cref="Arity"/> instance.</returns>
    public static implicit operator Arity(char value) => new(value.ToString());

    public static bool operator ==(Arity left, string right) => left.Value == right;
    public static bool operator !=(Arity left, string right) => left.Value != right;

    public static bool operator ==(Arity left, char right) => left.Value == right.ToString();
    public static bool operator !=(Arity left, char right) => left.Value != right.ToString();

    public static bool operator ==(Arity left, int right) => int.TryParse(left.Value, out var num) && num == right;
    public static bool operator !=(Arity left, int right) => !int.TryParse(left.Value, out var num) || num != right;

    public static bool operator >=(Arity left, int right) => int.TryParse(left.Value, out var num) && num >= right;
    public static bool operator <=(Arity left, int right) => int.TryParse(left.Value, out var num) && num <= right;

    public static bool operator >(Arity left, int right) => int.TryParse(left.Value, out var num) && num > right;
    public static bool operator <(Arity left, int right) => int.TryParse(left.Value, out var num) && num < right;

    /// <summary>
    /// Determines whether the specified <see cref="Arity"/> is equal to the current <see cref="Arity"/>.
    /// </summary>
    /// <param name="other">The <see cref="Arity"/> to compare with the current <see cref="Arity"/>.</param>
    /// <returns>
    /// true if the specified <see cref="Arity"/>'s value is equal to the current <see cref="Arity"/>'s value; otherwise false.
    /// </returns>
    public bool Equals(Arity other) => Value == other.Value;

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Arity other && Equals(other);
}