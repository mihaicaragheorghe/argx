using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;
using Argx.Tests.TestUtils;

using Moq;

namespace Argx.Tests.Actions;

public class ChoiceActionTests
{
    private readonly Mock<IArgumentRepository> _repositoryMock = new();
    private readonly ChoiceAction _sut = new();

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: "0");
        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityGreaterThanOne()
    {
        var arg = new Argument("--foo", arity: "2");
        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Fact]
    public void Execute_ShouldThrowBadArgumentException_WhenTokensLenLessThanTwo()
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo");

        var ex = Assert.Throws<ArgumentValueException>(() =>
            _sut.Execute(arg, _repositoryMock.Object, Create.Tokens("--foo")));
        Assert.Equal("Error: argument --foo: expected value", ex.Message);
    }

    [Fact]
    public void Execute_ShouldThrowBadArgumentException_WhenInvalidChoice()
    {
        var arg = new Argument("--color", dest: "color", arity: "1", choices: ["white", "gray", "black"]);
        var ex = Assert.Throws<ArgumentValueException>(() =>
            _sut.Execute(arg, _repositoryMock.Object, Create.Tokens("--color", "blue")));
        Assert.Equal("Error: argument --color: invalid choice: blue, chose from white, gray, black", ex.Message);
    }

    [Fact]
    public void Execute_ShouldStoreValue_WhenValidChoice()
    {
        var arg = new Argument("--color", dest: "color", arity: "1", choices: ["white", "gray", "black"]);

        _sut.Execute(arg, _repositoryMock.Object, Create.Tokens("--color", "white"));

        _repositoryMock.Verify(x => x.Set("color", "white"), Times.Once);
    }
}