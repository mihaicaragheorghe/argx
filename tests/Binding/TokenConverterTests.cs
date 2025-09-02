using Argx.Binding;
using Argx.Parsing;

namespace Argx.Tests.Binding;

public class TokenConverterTests
{
    [Fact]
    public void TryConvertSpan_ShouldReturnOkResult_WhenConversionDoesntThrow()
    {
        // Arrange
        var expected = 123;
        var tokens = new Token[] { new Token(expected.ToString()) };
        var span = tokens.AsSpan();

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int), tokens: span);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void TryConvertSpan_ShouldReturnErrorResult_WhenConversionThrows()
    {
        // Arrange
        var tokens = new Token[] { new Token("abc") };
        var span = tokens.AsSpan();

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int[]), tokens: span);

        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Error!);
        Assert.Null(result.Value);
    }

    [Fact]
    public void TryConvertObject_ShouldReturnResult_WhenConversionDoesntThrow()
    {
        // Arrange
        var expected = 123;
        var token = new Token(expected.ToString());

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int), obj: token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);
        Assert.Null(result.Error);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void TryConvertTokenObject_ShouldReturnErrorResult_WhenConversionThrows()
    {
        // Arrange
        var token = new Token("str");

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int[]), obj: token);

        // Assert
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Error ?? "");
    }

    [Fact]
    public void ConvertTokens_ShouldConvertToSingleValueType_WhenNotEnumerable()
    {
        // Arrange
        var tokens = new Token[] { new Token("123") };
        var span = tokens.AsSpan();

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int), tokens: span);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, 123);
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ConvertTokens_ShouldThrowInvalidCastException_WhenMultipleTokensAndTypeNotEnumerable()
    {
        Assert.Throws<InvalidCastException>(() =>
        {
            var tokens = new Token[] { new Token("123"), new Token("456"), new Token("789") };
            var span = tokens.AsSpan();
            TokenConverter.ConvertTokens(type: typeof(int), tokens: span);
        });
    }

    [Fact]
    public void ConvertTokens_ShouldConvertCollection_WhenSingleTokenAndTypeEnumerable()
    {
        // Arrange
        var tokens = new Token[] { new Token("123") };
        var span = tokens.AsSpan();

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int[]), tokens: span);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, new int[] { 123 });
        Assert.False(result.IsError);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ConvertTokens_ShouldConvertToCollection_WhenSpanLengthNotOne()
    {
        // Arrange
        var tokens = new Token[] { new Token("123"), new Token("456"), new Token("789") };
        var span = tokens.AsSpan();

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int[]), tokens: span);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, new int[] { 123, 456, 789 });
    }

    [Fact]
    public void ConvertTokens_ShouldConvertToEmptyCollection_WhenSpanLengthIsZero()
    {
        // Arrange
        var tokens = Array.Empty<Token>();
        var span = tokens.AsSpan();

        // Act
        var result = TokenConverter.TryConvert(type: typeof(int[]), tokens: span);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, Array.Empty<int>());
    }

    [Fact]
    public void ConvertObject_ShouldThrow_WhenObjectIsNotTypeToken()
    {
        // Arrange
        var token = new { Value = "foo" };

        // Act & Assert
        Assert.Throws<InvalidCastException>(() => TokenConverter.ConvertObject(typeof(string[]), token));
    }

    [Fact]
    public void ConvertObject_ShouldThrow_WhenTypeIsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => TokenConverter.ConvertObject(typeof(void), new Token("12:45")));
    }

    [Fact]
    public void ConvertObject_ShouldConvertToString_WhenTokenIsValid()
    {
        // Arrange
        var token = new Token("str");

        // Act
        var result = TokenConverter.ConvertObject(typeof(string), token);

        // Assert
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
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(bool), token);

        // Assert
        Assert.Equal(expected, actual.Value);
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
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(int), token);

        // Assert
        Assert.Equal(expected, actual.Value);
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
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(long), token);

        // Assert
        Assert.Equal(expected, actual.Value);
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
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(short), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("0", uint.MinValue)]
    [InlineData("4294967295", uint.MaxValue)]
    public void ConvertObject_ShouldConvertToUint_WhenTokenIsValid(string val, uint expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(uint), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("64", 64)]
    [InlineData("202508301040", 202508301040)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    [InlineData("0", ulong.MinValue)]
    public void ConvertObject_ShouldConvertToUlong_WhenTokenIsValid(string val, ulong expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(ulong), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("28", 28)]
    [InlineData("12345", 12345)]
    [InlineData("0", ushort.MinValue)]
    [InlineData("65535", ushort.MaxValue)]
    public void ConvertObject_ShouldConvertToUshort_WhenTokenIsValid(string val, ushort expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(ushort), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    public static IEnumerable<object[]> DecimalData => new List<object[]>
    {
        new object[] {"0.0", 0.0m},
        new object[] {"1.38", 1.38m},
        new object[] {"420.69", 420.69m},
        new object[] {"1.23456789", 1.23456789m},
        new object[] {"3.14159265358979", 3.14159265358979m},
        new object[] {"-3.14159265358979", -3.14159265358979m},
        new object[] {"79228162514264337593543950335", decimal.MaxValue},
        new object[] {"-79228162514264337593543950335", decimal.MinValue},
        new object[] {"0.0000000000000000000000000001", 0.0000000000000000000000000001m},
        new object[] {"12345678901234567890.1234567890", 12345678901234567890.1234567890m},
        new object[] {"-12345678901234567890.1234567890", -12345678901234567890.1234567890m},
        new object[] {"100000000000000000000.0", 100000000000000000000.0m},
        new object[] {"0.000000000000000000001", 0.000000000000000000001m},
        new object[] {"123.4500", 123.4500m},
    };

    [Theory]
    [MemberData(nameof(DecimalData))]
    public void ConvertObject_ShouldConvertToDecimal_WhenTokenIsValid(string val, decimal expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(decimal), token);

        // Assert
        Assert.Equal(expected, actual.Value);
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
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(float), token);

        // Assert
        Assert.Equal(expected, actual.Value);
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
        // Arrange
        var token = new Token(val);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(double), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToGuid_WhenTokenIsValid()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var token = new Token(expected.ToString());

        // Act
        var actual = TokenConverter.ConvertObject(typeof(Guid), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("2025-08-30", 2025, 08, 30, 0, 0, 0)]
    [InlineData("2025-08-30 10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30T10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30 10:50:30", 2025, 08, 30, 10, 50, 30)]
    [InlineData("2025-08-30T10:50:30", 2025, 08, 30, 10, 50, 30)]
    public void ConvertObject_ShouldConvertToDateTime_WhenTokenIsValid(string val, int y, int m, int d, int h, int min, int s)
    {
        // Arrange
        var token = new Token(val);
        var expected = new DateTime(y, m, d, h, min, s);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(DateTime), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Theory]
    [InlineData("2025-08-30", 2025, 08, 30, 0, 0, 0)]
    [InlineData("2025-08-30 10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30T10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30 10:50:30", 2025, 08, 30, 10, 50, 30)]
    [InlineData("2025-08-30T10:50:30", 2025, 08, 30, 10, 50, 30)]
    public void ConvertObject_ShouldConvertToDateTimeOffset_WhenTokenIsValid(string val, int y, int m, int d, int h, int min, int s)
    {
        // Arrange
        var token = new Token(val);
        var dt = new DateTime(y, m, d, h, min, s);
        var expected = new DateTimeOffset(dt);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(DateTimeOffset), token);

        // Assert
        Assert.Equal(expected, actual.Value);
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
        // Arrange
        var token = new Token(val);
        var expected = new TimeSpan(d, h, m, s);

        // Act
        var actual = TokenConverter.ConvertObject(typeof(TimeSpan), token);

        // Assert
        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntArray_WhenTokenIsIntArray()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(int[]), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToEmptyIntArray_WhenTokenIsEmptyIntArray()
    {
        // Arrange
        int[] expected = [];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(int[]), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntList_WhenTokenIsIntList()
    {
        // Arrange
        List<int> expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(List<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToEmptyIntList_WhenTokenIsEmptyIntList()
    {
        // Arrange
        List<int> expected = [];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(List<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntArray_WhenTokenIsIEnumerableInt()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(IEnumerable<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntArray_WhenTokenIsICollectionInt()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(ICollection<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldConvertToIntArray_WhenTokenIsIListInt()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = TokenConverter.ConvertObject(typeof(IList<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual.Value);
    }

    [Fact]
    public void ConvertObject_ShouldThrow_WhenTypeIsNotEnumerable()
    {
        // Arrange
        IReadOnlyList<Token> tokens = Enumerable
            .Range(1, 5)
            .Select(i => new Token(i.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        Assert.Throws<InvalidCastException>(() => TokenConverter.ConvertObject(typeof(int), tokens));
    }

    [Fact]
    public void ConvertObject_ShouldThrow_WhenItemConversionFails()
    {
        // Arrange
        IReadOnlyList<Token> tokens = Enumerable
            .Range(1, 5)
            .Select(i => new Token(i.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        Assert.Throws<InvalidCastException>(() => TokenConverter.ConvertObject(typeof(Guid), tokens));
    }
}