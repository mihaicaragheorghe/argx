using System.Globalization;

using Argx.Binding;
using Argx.Parsing;

namespace Argx.Tests.Binding;

public class TokenConverterTests
{
    public TokenConverterTests()
    {
        // Reset configuration before each test
        ArgumentConversionDefaults.FormatProvider = CultureInfo.InvariantCulture;
        ArgumentConversionDefaults.NumberStyles = new();
        ArgumentConversionDefaults.DateTimeFormat = null;
        ArgumentConversionDefaults.TimeSpanFormat = null;
    }

    [Fact]
    public void ConvertTokens_ShouldConvertToSingleValueType_WhenNotEnumerable()
    {
        var tokens = new[] { new Token("123", TokenType.Argument, 1) };
        var span = tokens.AsSpan();

        var result = TokenConverter.ConvertTokens(type: typeof(int), tokens: span);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, 123);
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ConvertTokens_ShouldThrowInvalidOperationException_WhenMultipleTokensAndTypeNotEnumerable()
    {
        var tokens = new[] { new Token("123", TokenType.Argument, 1), new Token("456", TokenType.Argument, 2), };

        Assert.Throws<InvalidOperationException>(() =>
            TokenConverter.ConvertTokens(type: typeof(int), tokens: tokens.AsSpan()));
    }

    [Fact]
    public void ConvertTokens_ShouldConvertCollection_WhenSingleTokenAndTypeEnumerable()
    {
        var expected = new[] { 123 };
        var tokens = new[] { new Token("123", TokenType.Argument, 1) };
        var span = tokens.AsSpan();

        var result = TokenConverter.ConvertTokens(type: typeof(int[]), tokens: span);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, expected);
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ConvertTokens_ShouldConvertToCollection_WhenSpanLengthNotOne()
    {
        var expected = new[] { 123, 456 };
        var tokens = new[] { new Token("123", TokenType.Argument, 1), new Token("456", TokenType.Argument, 2), };
        var span = tokens.AsSpan();

        var result = TokenConverter.ConvertTokens(type: typeof(int[]), tokens: span);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void ConvertTokens_ShouldConvertToEmptyCollection_WhenSpanLengthIsZero()
    {
        var tokens = Array.Empty<Token>();
        var span = tokens.AsSpan();

        var result = TokenConverter.ConvertTokens(type: typeof(int[]), tokens: span);

        Assert.True(result.IsSuccess);
        Assert.Equal(Array.Empty<int>(), result.Value);
    }

    [Fact]
    public void ConvertObject_ShouldThrow_WhenObjectIsNotTypeToken()
    {
        var token = new { Value = "foo" };

        Assert.Throws<InvalidCastException>(() => TokenConverter.ConvertObject(typeof(string[]), token));
    }

    [Fact]
    public void ConvertObject_ShouldThrow_WhenTypeIsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            TokenConverter.ConvertObject(typeof(void), new Token("", TokenType.Argument, 1)));
    }

    [Fact]
    public void ConvertObject_ShouldConvertToString_WhenTokenIsValid()
    {
        var token = new Token("str", TokenType.Argument, 1);

        var result = TokenConverter.ConvertObject(typeof(string), token);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.Equal(token.Value, result.Value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    public void ConvertObject_ShouldConvertToBool_WhenTokenIsValid(string val, bool expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(bool), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToBool_WhenTokenIsInvalid()
    {
        var token = new Token("notabool", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(bool), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("123", 123)]
    [InlineData("-123", -123)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void ConvertObject_ShouldConvertToInt_WhenTokenIsValid(string val, int expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(int), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("$1,234", NumberStyles.Currency, "en-US", 1234)]
    [InlineData("€1.234", NumberStyles.Currency, "de-DE", 1234)]
    [InlineData("1,234", NumberStyles.Number, "", 1234)]
    [InlineData("1.234", NumberStyles.Number, "de-DE", 1234)]
    public void ConvertObject_ShouldConvertToInt_WhenFormatConfigured(string val, NumberStyles styles, string provider, int expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Int = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(int), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToInt_WhenTokenIsInvalid()
    {
        var token = new Token("notanint", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(int), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("64", 64)]
    [InlineData("-64", -64)]
    [InlineData("202508301040", 202508301040)]
    [InlineData("-202508301040", -202508301040)]
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    public void ConvertObject_ShouldConvertToLong_WhenTokenIsValid(string val, long expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(long), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("$1,234,567,890", NumberStyles.Currency, "en-US", 1234567890L)]
    [InlineData("€1.234.567.890", NumberStyles.Currency, "de-DE", 1234567890L)]
    [InlineData("1,234,567,890", NumberStyles.Number, "", 1234567890L)]
    [InlineData("1.234.567.890", NumberStyles.Number, "de-DE", 1234567890L)]
    public void ConvertObject_ShouldConvertToLong_WhenFormatConfigured(string val, NumberStyles styles, string provider, long expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Long = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(long), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToLong_WhenTokenIsInvalid()
    {
        var token = new Token("notalong", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(long), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("28", 28)]
    [InlineData("-28", -28)]
    [InlineData("12345", 12345)]
    [InlineData("-12345", -12345)]
    [InlineData("32767", short.MaxValue)]
    [InlineData("-32768", short.MinValue)]
    public void ConvertObject_ShouldConvertToShort_WhenTokenIsValid(string val, short expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(short), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("$12,345", NumberStyles.Currency, "en-US", 12345)]
    [InlineData("€12.345", NumberStyles.Currency, "de-DE", 12345)]
    [InlineData("12,345", NumberStyles.Number, "", 12345)]
    [InlineData("12.345", NumberStyles.Number, "de-DE", 12345)]
    public void ConvertObject_ShouldConvertToShort_WhenFormatConfigured(string val, NumberStyles styles, string provider, short expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Short = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(short), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToShort_WhenTokenIsInvalid()
    {
        var token = new Token("notashort", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(short), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("0", uint.MinValue)]
    [InlineData("4294967295", uint.MaxValue)]
    public void ConvertObject_ShouldConvertToUint_WhenTokenIsValid(string val, uint expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(uint), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("$1,234,567", NumberStyles.Currency, "en-US", 1234567U)]
    [InlineData("€1.234.567", NumberStyles.Currency, "de-DE", 1234567U)]
    [InlineData("1,234,567", NumberStyles.Number, "", 1234567U)]
    [InlineData("1.234.567", NumberStyles.Number, "de-DE", 1234567U)]
    public void ConvertObject_ShouldConvertToUint_WhenFormatConfigured(string val, NumberStyles styles, string provider, uint expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Int = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(uint), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToUint_WhenSigned()
    {
        var token = new Token("-123", TokenType.Argument, 1);

        var result = TokenConverter.ConvertObject(typeof(uint), token);

        Assert.True(result.IsError);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToUint_WhenTokenIsInvalid()
    {
        var token = new Token("notanuint", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(uint), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("64", 64)]
    [InlineData("202508301040", 202508301040)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    [InlineData("0", ulong.MinValue)]
    public void ConvertObject_ShouldConvertToUlong_WhenTokenIsValid(string val, ulong expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(ulong), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("$1,234,567,890", NumberStyles.Currency, "en-US", 1234567890UL)]
    [InlineData("€1.234.567.890", NumberStyles.Currency, "de-DE", 1234567890UL)]
    [InlineData("1,234,567,890", NumberStyles.Number, "", 1234567890UL)]
    [InlineData("1.234.567.890", NumberStyles.Number, "de-DE", 1234567890UL)]
    public void ConvertObject_ShouldConvertToUlong_WhenFormatConfigured(string val, NumberStyles styles, string provider, ulong expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Long = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(ulong), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToUlong_WhenSigned()
    {
        var token = new Token("-123", TokenType.Argument, 1);

        var result = TokenConverter.ConvertObject(typeof(ulong), token);

        Assert.True(result.IsError);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToUlong_WhenTokenIsInvalid()
    {
        var token = new Token("notanulong", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(ulong), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("28", 28)]
    [InlineData("12345", 12345)]
    [InlineData("0", ushort.MinValue)]
    [InlineData("65535", ushort.MaxValue)]
    public void ConvertObject_ShouldConvertToUshort_WhenTokenIsValid(string val, ushort expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(ushort), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("$12,345", NumberStyles.Currency, "en-US", 12345)]
    [InlineData("€12.345", NumberStyles.Currency, "de-DE", 12345)]
    [InlineData("12,345", NumberStyles.Number, "", 12345)]
    [InlineData("12.345", NumberStyles.Number, "de-DE", 12345)]
    public void ConvertObject_ShouldConvertToUshort_WhenFormatConfigured(string val, NumberStyles styles, string provider, ushort expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Short = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(ushort), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToUshort_WhenSigned()
    {
        var token = new Token("-123", TokenType.Argument, 1);

        var result = TokenConverter.ConvertObject(typeof(ushort), token);

        Assert.True(result.IsError);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToUshort_WhenTokenIsInvalid()
    {
        var token = new Token("notanushort", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(ushort), token);

        Assert.True(actual.IsError);
    }

    public static TheoryData<string, decimal> DecimalTheory => new()
    {
        { "0.0", 0.0m },
        { "1.38", 1.38m },
        { "420.69", 420.69m },
        { "1.23456789", 1.23456789m },
        { "3.14159265358979", 3.14159265358979m },
        { "-3.14159265358979", -3.14159265358979m },
        { "79228162514264337593543950335", decimal.MaxValue },
        { "-79228162514264337593543950335", decimal.MinValue },
        { "0.0000000000000000000000000001", 0.0000000000000000000000000001m },
        { "12345678901234567890.1234567890", 12345678901234567890.1234567890m },
        { "-12345678901234567890.1234567890", -12345678901234567890.1234567890m },
        { "100000000000000000000.0", 100000000000000000000.0m },
        { "0.000000000000000000001", 0.000000000000000000001m },
        { "123.4500", 123.4500m },
    };

    [Theory]
    [MemberData(nameof(DecimalTheory))]
    public void ConvertObject_ShouldConvertToDecimal_WhenTokenIsValid(string val, decimal expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(decimal), token);

        Assert.Equal(expected, actual.Value);
    }

    public static TheoryData<string, NumberStyles, string, decimal> FormattedDecimalTheory => new()
    {
        { "1,234.56", NumberStyles.Number, "", 1234.56m },
        { "1.234,56", NumberStyles.Number, "de-DE", 1234.56m },
        { "$1234.56", NumberStyles.Currency, "en-US", 1234.56m },
        { "€1.234,56", NumberStyles.Currency, "de-DE", 1234.56m },
        { "  1234.56  ", NumberStyles.Number, "", 1234.56m },
        { "(1234.56)", NumberStyles.Number | NumberStyles.AllowParentheses, "", -1234.56m },
        { "-1,234.56", NumberStyles.Number | NumberStyles.AllowLeadingSign, "", -1234.56m },
    };

    [Theory]
    [MemberData(nameof(FormattedDecimalTheory))]
    public void ConvertObject_ShouldConvertToDecimal_WhenFormatConfigured(
        string val, NumberStyles styles, string provider, decimal expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Decimal = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(provider)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(provider);

        var actual = TokenConverter.ConvertObject(typeof(decimal), token);

        Assert.Equal(expected, actual.Value);
    }


    [Fact]
    public void ConvertObject_ShouldFailToConvertToDecimal_WhenTokenIsInvalid()
    {
        var token = new Token("notadecimal", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(decimal), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0.0", 0.0f)]
    [InlineData("1.5", 1.5f)]
    [InlineData("-1.5", -1.5f)]
    [InlineData("3.14", 3.14f)]
    [InlineData("-3.14", -3.14f)]
    [InlineData("420.69", 420.69f)]
    [InlineData("-420.69", -420.69f)]
    [InlineData("1234567.89", 1234567.89f)]
    [InlineData("0.0000001", 0.0000001f)]
    [InlineData("1000000.0", 1000000.0f)]
    [InlineData("1.5e-10", 1.5e-10f)]
    [InlineData("2.5e+10", 2.5e+10f)]
    [InlineData("3.4028235E+38", float.MaxValue)]
    [InlineData("-3.4028235E+38", float.MinValue)]
    [InlineData("1.401298E-45", float.Epsilon)]
    [InlineData("NaN", float.NaN)]
    [InlineData("Infinity", float.PositiveInfinity)]
    [InlineData("-Infinity", float.NegativeInfinity)]
    public void ConvertObject_ShouldConvertToFloat_WhenTokenIsValid(string val, float expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(float), token);

        Assert.Equal(expected, actual.Value);
    }

    public static TheoryData<string, NumberStyles, string, float> FormattedFloatTheory => new()
    {
        { "1,234.56", NumberStyles.Number, "", 1234.56f },
        { "1.234,56", NumberStyles.Number, "de-DE", 1234.56f },
        { "$1234.56", NumberStyles.Currency, "en-US", 1234.56f },
        { "€1.234,56", NumberStyles.Currency, "de-DE", 1234.56f },
        { "  1234.56  ", NumberStyles.Number, "", 1234.56f },
        { "(1234.56)", NumberStyles.Number | NumberStyles.AllowParentheses, "", -1234.56f },
        { "-1,234.56", NumberStyles.Number | NumberStyles.AllowLeadingSign, "", -1234.56f },
    };

    [Theory]
    [MemberData(nameof(FormattedFloatTheory))]
    public void ConvertObject_ShouldConvertToFloat_WhenFormatConfigured(
        string val, NumberStyles styles, string providerName, float expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Float = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(providerName)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(providerName);

        var actual = TokenConverter.ConvertObject(typeof(float), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToFloat_WhenTokenIsInvalid()
    {
        var token = new Token("notafloat", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(float), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("0.0", 0.0)]
    [InlineData("1.5", 1.5d)]
    [InlineData("-1.5", -1.5)]
    [InlineData("3.14", 3.14)]
    [InlineData("-3.14", -3.14)]
    [InlineData("3.141592653589793", 3.141592653589793)]
    [InlineData("-3.141592653589793", -3.141592653589793)]
    [InlineData("1.7976931348623157E+308", double.MaxValue)]
    [InlineData("-1.7976931348623157E+308", double.MinValue)]
    [InlineData("4.94065645841247E-324", double.Epsilon)]
    [InlineData("1.5e-300", 1.5e-300)]
    [InlineData("1.5e+300", 1.5e+300)]
    [InlineData("123456789012345.67", 123456789012345.67)]
    [InlineData("0.1234567890123456", 0.1234567890123456)]
    [InlineData("NaN", double.NaN)]
    [InlineData("Infinity", double.PositiveInfinity)]
    [InlineData("-Infinity", double.NegativeInfinity)]
    [InlineData("1.234E+5", 123400.0)]
    [InlineData("1.234E-5", 0.00001234)]
    public void ConvertObject_ShouldConvertToDouble_WhenTokenIsValid(string val, double expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(double), token);

        Assert.Equal(expected, actual.Value);
    }

    public static TheoryData<string, NumberStyles, string, double> FormattedDoubleTheory => new()
    {
        { "1,234.56", NumberStyles.Number, "", 1234.56 },
        { "1.234,56", NumberStyles.Number, "de-DE", 1234.56 },
        { "$1234.56", NumberStyles.Currency, "en-US", 1234.56 },
        { "€1.234,56", NumberStyles.Currency, "de-DE", 1234.56 },
        { "  1234.56  ", NumberStyles.Number, "", 1234.56 },
        { "(1234.56)", NumberStyles.Number | NumberStyles.AllowParentheses, "", -1234.56 },
        { "-1,234.56", NumberStyles.Number | NumberStyles.AllowLeadingSign, "", -1234.56 },
    };

    [Theory]
    [MemberData(nameof(FormattedDoubleTheory))]
    public void ConvertObject_ShouldConvertToDouble_WhenFormatConfigured(
        string val, NumberStyles styles, string providerName, double expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.NumberStyles.Double = styles;
        ArgumentConversionDefaults.FormatProvider = string.IsNullOrEmpty(providerName)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(providerName);

        var actual = TokenConverter.ConvertObject(typeof(double), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToDouble_WhenTokenIsInvalid()
    {
        var token = new Token("notadouble", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(double), token);

        Assert.True(actual.IsError);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToGuid_WhenTokenIsValid()
    {
        var expected = Guid.NewGuid();
        var token = new Token(expected.ToString(), TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(Guid), token);

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToGuid_WhenTokenIsInvalid()
    {
        var token = new Token("notaguid", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(Guid), token);

        Assert.True(actual.IsError);
    }

    public static TheoryData<string, DateTime> DateTimeTheory => new()
    {
        {"2025-08-30", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"2025-08-30 10:50", new DateTime(2025, 08, 30, 10, 50, 0)},
        {"2025-08-30T10:50", new DateTime(2025, 08, 30, 10, 50, 0)},
        {"2025-08-30 10:50:30", new DateTime(2025, 08, 30, 10, 50, 30)},
        {"2025-08-30T10:50:30", new DateTime(2025, 08, 30, 10, 50, 30)},
    };

    [Theory]
    [MemberData(nameof(DateTimeTheory))]
    public void ConvertObject_ShouldConvertToDateTime_WhenTokenIsValid(string val, DateTime expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(DateTime), token);

        Assert.Equal(expected, actual.Value);
    }

    public static TheoryData<string, string, DateTime> FormattedDateTimeTheory => new()
    {
        {"dd.MM.yyyy", "30.08.2025", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"dd/MM/yyyy", "30/08/2025", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"dd.MM.yyyy hh:mm", "30.08.2025 10:50", new DateTime(2025, 08, 30, 10, 50, 0)},
        {"dd.MM.yyyy HH:mm:ss", "30.08.2025 07:05:09", new DateTime(2025, 08, 30, 7, 5, 9)},
        {"dd/MM/yyyy hh:mm", "30/08/2025 10:50", new DateTime(2025, 08, 30, 10, 50, 0)},
        {"MM/dd/yyyy", "08/30/2025", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"MM-dd-yyyy", "08-30-2025", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"dd.MM.yy", "30.08.25", new DateTime(2025, 08, 30, 0, 0, 0)},

        // ISO-like formats
        {"yyyy-MM-dd", "2025-08-30", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"yyyy-MM-dd HH:mm:ss", "2025-08-30 14:35:22", new DateTime(2025, 08, 30, 14, 35, 22)},

        // 12-hour clock with AM/PM
        {"MM/dd/yyyy hh:mm tt", "08/30/2025 02:15 PM", new DateTime(2025, 08, 30, 14, 15, 0)},
        {"MM/dd/yyyy hh:mm:ss tt", "08/30/2025 12:00:00 AM", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"MM/dd/yyyy hh:mm:ss tt", "08/30/2025 12:00:00 PM", new DateTime(2025, 08, 30, 12, 0, 0)},

        // Compact numeric formats
        {"yyyyMMdd", "20250830", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"yyyyMMddHHmmss", "20250830123059", new DateTime(2025, 08, 30, 12, 30, 59)},

        // Month names
        {"dd MMM yyyy", "30 Aug 2025", new DateTime(2025, 08, 30, 0, 0, 0)},
        {"dddd, dd MMMM yyyy", "Saturday, 30 August 2025", new DateTime(2025, 08, 30, 0, 0, 0)},
    };

    [Theory]
    [MemberData(nameof(FormattedDateTimeTheory))]
    public void ConvertObject_ShouldConvertToDateTime_WhenFormatConfigured(string fmt, string val, DateTime expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.DateTimeFormat = fmt;

        var actual = TokenConverter.ConvertObject(typeof(DateTime), token);

        Assert.Equal(expected, actual.Value);
        ArgumentConversionDefaults.DateTimeFormat = null;
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToDateTime_WhenTokenIsInvalid()
    {
        var token = new Token("notadatetime", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(DateTime), token);

        Assert.True(actual.IsError);
    }

    public static TheoryData<string, DateTimeOffset> DateTimeOffsetTheory => new()
    {
        {"2025-08-30+00:00", new DateTimeOffset(2025, 08, 30, 0, 0, 0, TimeSpan.Zero)},
        {"2025-08-30T10:50+00:00", new DateTimeOffset(2025, 08, 30, 10, 50, 0, TimeSpan.Zero)},
        {"2025-08-30 10:50+00:00", new DateTimeOffset(2025, 08, 30, 10, 50, 0, TimeSpan.Zero)},
        {"2025-08-30T10:50:30+00:00", new DateTimeOffset(2025, 08, 30, 10, 50, 30, TimeSpan.Zero)},
        {"2025-08-30T10:50:30Z", new DateTimeOffset(2025, 08, 30, 10, 50, 30, TimeSpan.Zero)},
        {"2025-08-30T10:50:30+02:00", new DateTimeOffset(2025, 08, 30, 10, 50, 30, TimeSpan.FromHours(2))},
    };

    [Theory]
    [MemberData(nameof(DateTimeOffsetTheory))]
    public void ConvertObject_ShouldConvertToDateTimeOffset_WhenTokenIsValid(string val, DateTimeOffset expected)
    {
        var token = new Token(val, TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(DateTimeOffset), token);

        Assert.Equal(expected, actual.Value);
    }

    public static TheoryData<string, string, DateTimeOffset> FormattedDateTimeOffsetTheory => new()
    {
        {"dd.MM.yyyy HH:mm zzz", "30.08.2025 10:50 +02:00", new DateTimeOffset(2025, 08, 30, 10, 50, 0, TimeSpan.FromHours(2))},
        {"dd.MM.yyyy HH:mm:ss zzz", "30.08.2025 07:05:09 -03:00", new DateTimeOffset(2025, 08, 30, 7, 5, 9, TimeSpan.FromHours(-3))},
        {"yyyy-MM-ddTHH:mmzzz", "2025-08-30T10:50+00:00", new DateTimeOffset(2025, 08, 30, 10, 50, 0, TimeSpan.Zero)},
        {"yyyy-MM-dd HH:mm:sszzz", "2025-08-30 14:35:22+05:30", new DateTimeOffset(2025, 08, 30, 14, 35, 22, TimeSpan.FromHours(5.5))},
        {"yyyy-MM-dd HH:mm:sszzz", "2025-08-30 14:35:22-04:00", new DateTimeOffset(2025, 08, 30, 14, 35, 22, TimeSpan.FromHours(-4))},
        {"yyyy-MM-ddTHH:mm:ssK", "2025-08-30T14:35:22Z", new DateTimeOffset(2025, 08, 30, 14, 35, 22, TimeSpan.Zero)},
        {"yyyy-MM-ddTHH:mm:ssK", "2025-08-30T10:35:22+03:00", new DateTimeOffset(2025, 08, 30, 10, 35, 22, TimeSpan.FromHours(3))},
        {"MM/dd/yyyy hh:mm:ss tt zzz", "08/30/2025 02:15:00 PM +02:00", new DateTimeOffset(2025, 08, 30, 14, 15, 0, TimeSpan.FromHours(2))},
        {"MM/dd/yyyy hh:mm:ss tt zzz", "08/30/2025 12:00:00 AM -05:00", new DateTimeOffset(2025, 08, 30, 0, 0, 0, TimeSpan.FromHours(-5))},
        {"yyyyMMddHHmmsszzz", "20250830123059+00:00", new DateTimeOffset(2025, 08, 30, 12, 30, 59, TimeSpan.Zero)},
    };

    [Theory]
    [MemberData(nameof(FormattedDateTimeOffsetTheory))]
    public void ConvertObject_ShouldConvertToDateTimeOffset_WhenFormatConfigured(string fmt, string val, DateTimeOffset expected)
    {
        var token = new Token(val, TokenType.Argument, 1);
        ArgumentConversionDefaults.DateTimeFormat = fmt;

        var actual = TokenConverter.ConvertObject(typeof(DateTimeOffset), token);

        Assert.Equal(expected, actual.Value);
        ArgumentConversionDefaults.DateTimeFormat = null;
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToDateTimeOffset_WhenTokenIsInvalid()
    {
        var token = new Token("notadatetimeoffset", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(DateTimeOffset), token);

        Assert.True(actual.IsError);
    }

    [Theory]
    [InlineData("1:30:45", 0, 1, 30, 45)] // Short format (1 hour, 30 minutes, 45 seconds)
    [InlineData("14:30:45", 0, 14, 30, 45)] // Hours:Minutes:Seconds
    [InlineData("1.14:30:45", 1, 14, 30, 45)] // Days.Hours:Minutes:Seconds
    [InlineData("00:00:00", 0, 0, 0, 0)] // Zero
    [InlineData("00:00:01", 0, 0, 0, 1)] // Min time
    [InlineData("23:59:59", 0, 23, 59, 59)] // Max time
    [InlineData("10675199.02:48:05", 10675199, 2, 48, 5)] // TimeSpan.MaxValue
    [InlineData("-10675199.02:48:05", -10675199, -2, -48, -5)] // TimeSpan.MinValue
    [InlineData("12.23:59:59", 12, 23, 59, 59)] // 12 days, etc.
    public void ConvertObject_ShouldConvertToTimeSpan_WhenTokenIsValid(string val, int d, int h, int m, int s)
    {
        var token = new Token(val, TokenType.Argument, 1);
        var expected = new TimeSpan(d, h, m, s);

        var actual = TokenConverter.ConvertObject(typeof(TimeSpan), token);

        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData(@"hh\:mm", "10:30", 0, 10, 30, 0, 0)]
    [InlineData(@"hh\:mm\:ss", "01:02:03", 0, 1, 2, 3, 0)]
    [InlineData(@"d\.hh\:mm\:ss", "1.02:30:00", 1, 2, 30, 0, 0)]
    [InlineData(@"hh\:mm\:ss\.fff", "10:30:15.250", 0, 10, 30, 15, 250)]
    [InlineData(@"hh\:mm", "00:00", 0, 0, 0, 0, 0)]
    public void ConvertObject_ShouldConvertToTimeSpan_WhenFormatConfigured(
        string fmt, string val, int d, int h, int m, int s, int ms)
    {
        var token = new Token(val, TokenType.Argument, 1);
        var expected = new TimeSpan(d, h, m, s, ms);
        ArgumentConversionDefaults.TimeSpanFormat = fmt;

        var actual = TokenConverter.ConvertObject(typeof(TimeSpan), token);

        Assert.Equal(expected, actual.Value);
        ArgumentConversionDefaults.TimeSpanFormat = null;
    }

    [Fact]
    public void ConvertObject_ShouldFailToConvertToTimeSpan_WhenTokenIsInvalid()
    {
        var token = new Token("notatimespan", TokenType.Argument, 1);

        var actual = TokenConverter.ConvertObject(typeof(TimeSpan), token);

        Assert.True(actual.IsError);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntArray_WhenTypeIsIntArrayAndObjectIsValidTokenList()
    {
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(int[]), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToEmptyIntArray_WhenTypeIsIntArrayAndObjectIsEmptyTokenList()
    {
        int[] expected = [];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(int[]), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntList_WhenTypeIsIntListAndObjectIsValidTokenList()
    {
        List<int> expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(List<int>), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToEmptyIntList_WhenTypeIsIntListAndObjectIsEmptyTokenList()
    {
        List<int> expected = [];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(List<int>), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIEnumerableInt_WhenTypeIsIEnumerableIntAndObjectIsValidTokenList()
    {
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(IEnumerable<int>), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToICollectionInt_WhenTypeIsICollectionIntAndObjectIsValidTokenList()
    {
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(ICollection<int>), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIListInt_WhenTypeIsIListIntAndObjectIsValidTokenList()
    {
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var actual = TokenConverter.ConvertObject(typeof(IList<int>), tokens);

        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldFail_WhenTypeIsNotEnumerableAndObjectIsTokenList()
    {
        IReadOnlyList<Token> tokens = Enumerable
            .Range(1, 5)
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var result = TokenConverter.ConvertObject(typeof(List<Guid>), tokens);

        Assert.True(result.IsError);
    }

    [Fact]
    public void ConvertObject_ShouldFail_WhenItemConversionFails()
    {
        IReadOnlyList<Token> tokens = Enumerable
            .Range(1, 5)
            .Select((v, i) => new Token(v.ToString(), TokenType.Argument, i))
            .ToList()
            .AsReadOnly<Token>();

        var result = TokenConverter.ConvertObject(typeof(List<Guid>), tokens);

        Assert.True(result.IsError);
    }
}