using Argx.Subcommands;

namespace Argx.Tests.Subcommands;

public class SubcommandStoreTests
{
    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenHandlerIsNull()
    {
        var sut = new SubcommandStore();
        Assert.Throws<ArgumentNullException>(() => sut.Register("foo", null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Register_ShouldThrowArgumentException_WhenNameNullOrEmpty(string? name)
    {
        var sut = new SubcommandStore();
        Assert.Throws<ArgumentException>(() => sut.Register(name!, args => Task.CompletedTask));
    }

    [Fact]
    public void Register_ShouldStoreHandler_WhenValid()
    {
        var sut = new SubcommandStore();
        sut.Register("foo", args => Task.CompletedTask);
        Assert.True(sut.TryGetHandler("foo", out var handler));
    }

    [Fact]
    public void TryGetHandler_ShouldReturnTrueAndOutHandler_WhenRegistered()
    {
        var sut = new SubcommandStore();
        sut.Register("foo", args => Task.CompletedTask);
        Assert.True(sut.TryGetHandler("foo", out var handler));
        Assert.True(handler([]) == Task.CompletedTask);
    }

    [Fact]
    public void TryGetHandler_ShouldReturnFalse_WhenNotRegistered()
    {
        var sut = new SubcommandStore();
        sut.Register("foo", args => Task.CompletedTask);
        Assert.False(sut.TryGetHandler("bar", out var handler));
    }
}