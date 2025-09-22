using Argx.Actions;
using Argx.Parsing;
using Moq;

namespace Argx.Tests.Actions;

public class StoreConstTests
{
    private readonly Mock<IArgumentRepository> _mockRepository = new();
    private readonly StoreConstAction _sut = new();

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenArityNotZero()
    {
        var arg = new Argument("--foo", constValue: "bar", arity: 1);
        Assert.Throws<InvalidOperationException>(() => _sut.Execute(arg, _mockRepository.Object, []));
    }

    [Fact]
    public void Execute_ShouldThrowArgumentException_WhenArgumentConstValueIsNull()
    {
        var arg = new Argument("--foo", constValue: null, arity: 0);
        Assert.Throws<ArgumentException>(() => _sut.Execute(arg, _mockRepository.Object, []));
    }

    [Fact]
    public void Execute_ShouldStoreConst_WhenValid()
    {
        var arg = new Argument("--foo", constValue: true, dest: "foo", arity: 0);

        _sut.Execute(arg, _mockRepository.Object, [new Token("bar")]);

        _mockRepository.Verify(x => x.Set("foo", true), Times.Once());
    }
}