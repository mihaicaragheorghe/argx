namespace Argx.Parsing;

public class Arity
{
    public string Value { get; }

    public const string Any = "*";

    public const string Optional = "?";

    public const string AtLeastOne = "+";

    public Arity(string value)
    {
        Value = value;
    }

    public bool IsFixed => int.TryParse(Value, out _);

    public int ToInt() => int.Parse(Value);

    public static implicit operator Arity(string value) => new(value);

    public static implicit operator Arity(int value) => new(value.ToString());

    public static implicit operator Arity(char value) => new(value.ToString());

    public static bool operator ==(Arity left, char right) => left.Value == right.ToString();
    public static bool operator !=(Arity left, char right) => left.Value != right.ToString();

    public static bool operator ==(Arity left, int right) => int.TryParse(left.Value, out var num) && num == right;
    public static bool operator !=(Arity left, int right) => !int.TryParse(left.Value, out var num) || num != right;

    public static bool operator >=(Arity left, int right) => int.TryParse(left.Value, out var num) && num >= right;
    public static bool operator <=(Arity left, int right) => int.TryParse(left.Value, out var num) && num <= right;

    public static bool operator >(Arity left, int right) => int.TryParse(left.Value, out var num) && num > right;
    public static bool operator <(Arity left, int right) => int.TryParse(left.Value, out var num) && num < right;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Arity arity && Equals(arity);

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();
}