using Argx.Binding;
using Argx.Parsing;

namespace Argx.Tests.Binding;

public class ArgumentConverterTests
{
    [Fact]
    public void ConvertToken_ShouldConvertToString_WhenTokenIsValid()
    {
        // Arrange
        var token = new Token("str");

        // Act
        var result = ArgumentConverter.ConvertToken(typeof(string), token);

        // Assert
        Assert.Equal(token.Value, result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    public void ConvertToken_ShouldConvertToBool_WhenTokenIsValid(string val, bool expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(bool), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("123", 123)]
    [InlineData("-123", -123)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void ConvertToken_ShouldConvertToInt_WhenTokenIsValid(string val, int expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(int), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("64", 64)]
    [InlineData("-64", -64)]
    [InlineData("202508301040", 202508301040)]
    [InlineData("-202508301040", -202508301040)]
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    public void ConvertToken_ShouldConvertToLong_WhenTokenIsValid(string val, long expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(long), token);

        // Assert
        Assert.Equal(expected, actual);
    }


    [Theory]
    [InlineData("0", 0)]
    [InlineData("28", 28)]
    [InlineData("-28", -28)]
    [InlineData("12345", 12345)]
    [InlineData("-12345", -12345)]
    [InlineData("32767", short.MaxValue)]
    [InlineData("-32768", short.MinValue)]
    public void ConvertToken_ShouldConvertToShort_WhenTokenIsValid(string val, short expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(short), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("0", uint.MinValue)]
    [InlineData("4294967295", uint.MaxValue)]
    public void ConvertToken_ShouldConvertToUint_WhenTokenIsValid(string val, uint expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(uint), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("64", 64)]
    [InlineData("202508301040", 202508301040)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    [InlineData("0", ulong.MinValue)]
    public void ConvertToken_ShouldConvertToUlong_WhenTokenIsValid(string val, ulong expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(ulong), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("28", 28)]
    [InlineData("12345", 12345)]
    [InlineData("0", ushort.MinValue)]
    [InlineData("65535", ushort.MaxValue)]
    public void ConvertToken_ShouldConvertToUshort_WhenTokenIsValid(string val, ushort expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(ushort), token);

        // Assert
        Assert.Equal(expected, actual);
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
    public void ConvertToken_ShouldConvertToDecimal_WhenTokenIsValid(string val, decimal expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(decimal), token);

        // Assert
        Assert.Equal(expected, actual);
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
    public void ConvertToken_ShouldConvertToFloat_WhenTokenIsValid(string val, float expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(float), token);

        // Assert
        Assert.Equal(expected, actual);
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
    public void ConvertToken_ShouldConvertToDouble_WhenTokenIsValid(string val, double expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(double), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToGuid_WhenTokenIsValid()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var token = new Token(expected.ToString());

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(Guid), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2025-08-30", 2025, 08, 30, 0, 0, 0)]
    [InlineData("2025-08-30 10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30T10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30 10:50:30", 2025, 08, 30, 10, 50, 30)]
    [InlineData("2025-08-30T10:50:30", 2025, 08, 30, 10, 50, 30)]
    public void ConvertToken_ShouldConvertToDateTime_WhenTokenIsValid(string val, int y, int m, int d, int h, int min, int s)
    {
        // Arrange
        var token = new Token(val);
        var expected = new DateTime(y, m, d, h, min, s);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(DateTime), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2025-08-30", 2025, 08, 30, 0, 0, 0)]
    [InlineData("2025-08-30 10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30T10:50", 2025, 08, 30, 10, 50, 0)]
    [InlineData("2025-08-30 10:50:30", 2025, 08, 30, 10, 50, 30)]
    [InlineData("2025-08-30T10:50:30", 2025, 08, 30, 10, 50, 30)]
    public void ConvertToken_ShouldConvertToDateTimeOffset_WhenTokenIsValid(string val, int y, int m, int d, int h, int min, int s)
    {
        // Arrange
        var token = new Token(val);
        var dt = new DateTime(y, m, d, h, min, s);
        var expected = new DateTimeOffset(dt);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(DateTimeOffset), token);

        // Assert
        Assert.Equal(expected, actual);
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
    public void ConvertToken_ShouldConvertToTimeSpan_WhenTokenIsValid(string val, int d, int h, int m, int s)
    {
        // Arrange
        var token = new Token(val);
        var expected = new TimeSpan(d, h, m, s);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(TimeSpan), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToIntArray_WhenTokenIsIntArray()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(int[]), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToEmptyIntArray_WhenTokenIsEmptyIntArray()
    {
        // Arrange
        int[] expected = [];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(int[]), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToIntList_WhenTokenIsIntList()
    {
        // Arrange
        List<int> expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(List<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToEmptyIntList_WhenTokenIsEmptyIntList()
    {
        // Arrange
        List<int> expected = [];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(List<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToIntArray_WhenTokenIsIEnumerableInt()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(IEnumerable<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToIntArray_WhenTokenIsICollectionInt()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(ICollection<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToIntArray_WhenTokenIsIListInt()
    {
        // Arrange
        int[] expected = [1, 2, 3, 4, 5];
        IReadOnlyList<Token> tokens = expected
            .Select(x => new Token(x.ToString()))
            .ToList()
            .AsReadOnly<Token>();

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(IList<int>), tokens);

        // Assert
        Assert.Equivalent(expected, actual);
    }
}