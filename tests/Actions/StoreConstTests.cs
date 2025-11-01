using Argx.Actions;
using Argx.Parsing;
using Argx.Tests.TestUtils;

using Moq;

namespace Argx.Tests.Actions;

public class StoreConstTests
{
    private readonly Mock<IArgumentStore> _mockStore = new();
    private readonly StoreConstAction _sut = new();

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityNotZero()
    {
        var arg = new Argument("--foo", constValue: "bar", arity: "1");
        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArgumentConstValueIsNull()
    {
        var arg = new Argument("--foo", constValue: null, arity: "0");
        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Fact]
    public void Execute_ShouldStoreConst_WhenValid()
    {
        var arg = new Argument("--foo", constValue: true, dest: "foo", arity: "0");

        _sut.Execute(arg, Create.Token("bar"), [], _mockStore.Object);

        _mockStore.Verify(x => x.Set("foo", true), Times.Once());
    }
}