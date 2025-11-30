using Argx.Parsing;

using Moq;

namespace Argx.Tests;

public class ArgumentsTests
{
    private readonly Mock<IArgumentStore> _storeMock;

    public ArgumentsTests()
    {
        _storeMock = new();
    }

    [Fact]
    public void GetValue_ShouldReturnValue_WhenStored()
    {
        string? expected = "bar";
        _storeMock
            .Setup(x => x.TryGetValue("foo", out expected))
            .Returns(true);
        var sut = new Arguments(_storeMock.Object);

        var actual = sut.GetValue("foo");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValue_ShouldReturnNull_WhenNotStored()
    {
        string? value;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out value))
            .Returns(false);
        var sut = new Arguments(_storeMock.Object);

        var result = sut.GetValue("foo");

        Assert.Null(result);
    }

    [Fact]
    public void GetValueT_ShouldReturnValue_WhenStored()
    {
        int expected = 6;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out expected))
            .Returns(true);
        var sut = new Arguments(_storeMock.Object);

        var actual = sut.GetValue<int>("foo");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValueT_ShouldReturnNull_WhenNotStored()
    {
        int value;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out value))
            .Returns(false);
        var sut = new Arguments(_storeMock.Object);

        var result = sut.GetValue<int>("foo");

        Assert.Equal(0, result);
    }

    [Fact]
    public void TryGetValueT_ShouldReturnTrue_WhenStored()
    {
        int expected = 6;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out expected))
            .Returns(true);
        var sut = new Arguments(_storeMock.Object);

        var result = sut.TryGetValue<int>("foo", out var actual);

        Assert.True(result);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TryGetValueT_ShouldReturnFalse_WhenNotStored()
    {
        int placeholder;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out placeholder))
            .Returns(false);
        var sut = new Arguments(_storeMock.Object);

        var result = sut.TryGetValue<int>("foo", out var value);

        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void GetRequiredT_ShouldReturnValue_WhenStored()
    {
        int expected = 6;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out expected))
            .Returns(true);
        var sut = new Arguments(_storeMock.Object);

        var actual = sut.GetRequired<int>("foo");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetRequiredT_ShouldThrowInvalidOperationException_WhenNotStored()
    {
        int value;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out value))
            .Returns(false);
        var sut = new Arguments(_storeMock.Object);

        Assert.Throws<InvalidOperationException>(() => sut.GetRequired<int>("foo"));
    }
}