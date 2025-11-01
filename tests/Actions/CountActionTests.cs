using Argx.Actions;
using Argx.Parsing;
using Argx.Tests.TestUtils;

using Moq;

namespace Argx.Tests.Actions;

public class CountActionTests
{
    private readonly Mock<IArgumentStore> _storeMock = new();
    private readonly CountAction _sut = new();

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityNotZero()
    {
        var arg = new Argument("--foo", alias: "-f", action: ArgumentActions.Count, arity: "1");
        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Fact]
    public void Execute_ShouldStoreOne_WhenNotStored()
    {
        var arg = new Argument("--foo", alias: "-f", action: ArgumentActions.Count, arity: "0");
        var value = 0;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out value))
            .Returns(false);

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), _storeMock.Object);

        _storeMock.Verify(x => x.Set("foo", 1), Times.Once);
    }

    [Fact]
    public void Execute_ShouldIncrement_WhenStored()
    {
        var arg = new Argument("--foo", alias: "-f", action: ArgumentActions.Count, arity: "0");
        var value = 3;
        _storeMock
            .Setup(x => x.TryGetValue("foo", out value))
            .Returns(true);

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), _storeMock.Object);

        _storeMock.Verify(x => x.Set("foo", 4), Times.Once);
    }
}