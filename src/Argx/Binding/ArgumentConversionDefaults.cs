using System.Globalization;

namespace Argx.Binding;

/// <summary>
/// Default formats for argument type conversion.
/// </summary>
public static class ArgumentConversionDefaults
{
    /// <summary>
    /// Number styles used when converting string tokens to numeric types.
    /// </summary>
    public static NumberStylesOptions NumberStyles { get; set; } = new();

    /// <summary>
    /// The format provider used when converting string tokens.<br/>
    /// Default is <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// The default date-time format string used when converting string tokens to <see cref="DateTime"/> types.<br/>
    /// Default is null, which indicates that the default parsing behavior should be used
    /// (<c>TryParse</c> instead of <c>TryParseExact</c>).
    /// </summary>
    public static string? DateTimeFormat { get; set; }

    /// <summary>
    /// The time-span format string used when converting string tokens to <see cref="TimeSpan"/> types.
    /// Default is null, which indicates that the default parsing behavior should be used.
    /// (<c>TryParse</c> instead of <c>TryParseExact</c>).
    /// </summary>
    public static string? TimeSpanFormat { get; set; }
}
