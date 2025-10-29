using System.Globalization;

namespace Argx.Binding;

/// <summary>
/// Defines how numeric string tokens are parsed into numeric types.
/// Each property specifies the <see cref="NumberStyles"/> flags used by
/// the corresponding numeric type’s <c>TryParse</c> method.
/// </summary>
public class NumberStylesOptions
{
    /// <summary>
    /// The <see cref="NumberStyles"/> used when parsing <see cref="short"/> and <see cref="ushort"/> values.
    /// Default is <see cref="NumberStyles.Integer"/>.
    /// </summary>
    public NumberStyles Short { get; set; } = NumberStyles.Integer;

    /// <summary>
    /// The <see cref="NumberStyles"/> used when parsing <see cref="int"/> and <see cref="uint"/> values.
    /// Default is <see cref="NumberStyles.Integer"/>.
    /// </summary>
    public NumberStyles Int { get; set; } = NumberStyles.Integer;

    /// <summary>
    /// The <see cref="NumberStyles"/> used when parsing <see cref="long"/> and <see cref="ulong"/> values.
    /// Default is <see cref="NumberStyles.Integer"/>.
    /// </summary>
    public NumberStyles Long { get; set; } = NumberStyles.Integer;

    /// <summary>
    /// The <see cref="NumberStyles"/> used when parsing <see cref="decimal"/> values.
    /// Default is <see cref="NumberStyles.Number"/>, which allows decimal points and thousands separators.
    /// </summary>
    public NumberStyles Decimal { get; set; } = NumberStyles.Number;

    /// <summary>
    /// The <see cref="NumberStyles"/> used when parsing <see cref="float"/> values.
    /// Default is <see cref="NumberStyles.Float"/> combined with <see cref="NumberStyles.AllowThousands"/>,
    /// allowing decimal points, exponents, and thousands separators.
    /// </summary>
    public NumberStyles Float { get; set; } = NumberStyles.Float | NumberStyles.AllowThousands;

    /// <summary>
    /// The <see cref="NumberStyles"/> used when parsing <see cref="double"/> values.
    /// Default is <see cref="NumberStyles.Float"/> combined with <see cref="NumberStyles.AllowThousands"/>,
    /// allowing decimal points, exponents, and thousands separators.
    /// </summary>
    public NumberStyles Double { get; set; } = NumberStyles.Float | NumberStyles.AllowThousands;
}
