using Argx.Binding;
using Argx.Parsing;

namespace Argx.Tests.Binding;

public class ArgumentConverterTests
{
    [Fact]
    public void ConvertToken_ShouldConvertToString_WhenArgIsValid()
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
    [InlineData("false", false)]
    [InlineData("False", false)]
    public void ConvertToken_ShouldConvertToBool_WhenArgIsValid(string val, bool expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(bool), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123", 123)]
    public void ConvertToken_ShouldConvertToInt_WhenArgIsValid(string val, int expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(int), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("202508301040", 202508301040)]
    public void ConvertToken_ShouldConvertToLong_WhenArgIsValid(string val, long expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(long), token);

        // Assert
        Assert.Equal(expected, actual);
    }


    [Theory]
    [InlineData("28", 28)]
    public void ConvertToken_ShouldConvertToShort_WhenArgIsValid(string val, short expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(short), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123", 123)]
    public void ConvertToken_ShouldConvertToUint_WhenArgIsValid(string val, uint expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(uint), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("202508301040", 202508301040)]
    public void ConvertToken_ShouldConvertToUlong_WhenArgIsValid(string val, ulong expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(ulong), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("28", 28)]
    public void ConvertToken_ShouldConvertToUshort_WhenArgIsValid(string val, ushort expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(ushort), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("3,14", 3.14)]
    public void ConvertToken_ShouldConvertToDecimal_WhenArgIsValid(string val, decimal expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(decimal), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("3,14", 3.14f)]
    public void ConvertToken_ShouldConvertToFloat_WhenArgIsValid(string val, float expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(float), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("3,14", 3.14d)]
    public void ConvertToken_ShouldConvertToDouble_WhenArgIsValid(string val, double expected)
    {
        // Arrange
        var token = new Token(val);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(double), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToGuid_WhenArgIsValid()
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
    [InlineData("2025-08-30 10:50:30", 2025, 08, 30, 10, 50, 30)]
    public void ConvertToken_ShouldConvertToDateTime_WhenArgIsValid(string val, int y, int m, int d, int h, int min, int s)
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
    [InlineData("2025-08-30 10:50:30", 2025, 08, 30, 10, 50, 30)]
    public void ConvertToken_ShouldConvertToDateTimeOffset_WhenArgIsValid(string val, int y, int m, int d, int h, int min, int s)
    {
        // Arrange
        var token = new Token(val);
        var expected = new DateTimeOffset(new DateTime(y, m, d, h, min, s));

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(DateTimeOffset), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("02:30:00", 02, 30, 00)]
    public void ConvertToken_ShouldConvertToTimeSpan_WhenArgIsValid(string val, int h, int m, int s)
    {
        // Arrange
        var token = new Token(val);
        var expected = TimeSpan.FromHours(h, m, s);

        // Act
        var actual = ArgumentConverter.ConvertToken(typeof(TimeSpan), token);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConvertToken_ShouldConvertToIntArray_WhenArgIsValid()
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
    public void ConvertToken_ShouldConvertToIntList_WhenArgIsValid()
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
}