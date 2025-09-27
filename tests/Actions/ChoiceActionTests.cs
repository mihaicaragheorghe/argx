using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;
using Moq;

namespace Argx.Tests.Actions;

public class ChoiceActionTests
{
    private readonly Mock<IArgumentRepository> _repositoryMock = new();
    private readonly ChoiceAction _sut = new();

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: 0);
        Assert.Throws<InvalidOperationException>(
            () => _sut.Execute(arg, _repositoryMock.Object, [new Token("--foo")]));
    }

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenGreaterThanOne()
    {
        var arg = new Argument("--foo", arity: 2);
        Assert.Throws<InvalidOperationException>(
            () => _sut.Execute(arg, _repositoryMock.Object, [new Token("--foo"), new Token("bar"), new Token("baz")]));
    }

    [Fact]
    public void Execute_ShouldThrowBadArgumentException_WhenTokensLenLessThanTwo()
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo");

        var ex = Assert.Throws<ArgumentValueException>(
            () => _sut.Execute(arg, _repositoryMock.Object, [new Token("--foo")]));
        Assert.Equal("Error: argument --foo: expected one value", ex.Message);
    }

    [Fact]
    public void Execute_ShouldThrowBadArgumentException_WhenInvalidChoice()
    {
        var arg = new Argument("--color", dest: "color", arity: 1, choices: ["white", "gray", "black"]);
        var ex = Assert.Throws<ArgumentValueException>(
            () => _sut.Execute(arg, _repositoryMock.Object, [new Token("--color"), new Token("blue")]));
        Assert.Equal("Error: argument --color: invalid choice: blue, chose from white, gray, black", ex.Message);
    }

    [Fact]
    public void Execute_ShouldStoreValue_WhenValidChoice()
    {
        var arg = new Argument("--color", dest: "color", arity: 1, choices: ["white", "gray", "black"]);

        _sut.Execute(arg, _repositoryMock.Object, [new Token("--color"), new Token("white")]);

        _repositoryMock.Verify(x => x.Set("color", "white"), Times.Once);
    }
}