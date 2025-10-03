namespace Argx;

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

    public Arity(int n)
    {
        Value = n.ToString();
    }

    public bool IsFixed => int.TryParse(Value, out _);

    public bool IsOptional => Value is Any or Optional;

    public bool AcceptsMultipleValues => Value == Any || Value == AtLeastOne || int.TryParse(Value, out var n) && n > 1;

    public int ToInt() => int.Parse(Value);

    public static implicit operator Arity(string value) => new(value);

    public static implicit operator Arity(int value) => new(value.ToString());

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

    public bool Equals(Arity other) => Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => obj is Arity other && Equals(other);
}