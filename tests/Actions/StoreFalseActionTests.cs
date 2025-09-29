using Argx.Actions;
using Argx.Parsing;
using Moq;

namespace Argx.Tests.Actions;

public class StoreFalseActionTests
{
    private readonly Mock<IArgumentRepository> _mockRepository = new();
    private readonly StoreFalseAction _sut = new();

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityNotZero()
    {
        var arg = new Argument("--foo", arity: "1");
        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Fact]
    public void Execute_ShouldStoreConst_WhenValid()
    {
        var arg = new Argument("--foo", dest: "foo", arity: "0");

        _sut.Execute(arg, _mockRepository.Object, [new Token("bar")]);

        _mockRepository.Verify(x => x.Set("foo", false), Times.Once());
    }
}