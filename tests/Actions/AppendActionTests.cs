using System.Diagnostics.CodeAnalysis;

using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;

using Moq;

namespace Argx.Tests.Actions;

// ReSharper disable ConvertTypeCheckToNullCheck
[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
public class AppendActionTests
{
    private readonly Mock<IArgumentRepository> _mockRepository = new();
    private readonly AppendAction _sut = new();

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: 0, dest: "foo", type: typeof(string[]));

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(arg, _mockRepository.Object, Tokens("--foo")));
    }

    [Fact]
    public void Execute_ShouldThrowBadArgumentException_WhenTokensLenLessThanTwo()
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo");

        var ex = Assert.Throws<ArgumentValueException>(() => _sut.Execute(arg, _mockRepository.Object, Tokens("--foo")));
        Assert.Equal("Error: argument --foo: expected one value", ex.Message);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Guid))]
    public void Execute_ShouldThrowBadArgumentException_WhenTypeNotEnumerable(Type type)
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo", type: type);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(arg, _mockRepository.Object, Tokens("--foo", "bar")));
    }

    [Fact]
    public void Execute_ShouldCreateArray_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo", type: typeof(string[]));

        _sut.Execute(arg, _mockRepository.Object, Tokens("--foo", "bar"));

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }));
    }

    [Fact]
    public void Execute_ShouldCreateList_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo", type: typeof(List<string>));

        _sut.Execute(arg, _mockRepository.Object, Tokens("--foo", "bar"));

        _mockRepository.Verify(x => x.Set("foo", new List<string> { "bar" }));
    }

    [Fact]
    public void Execute_ShouldCreateIEnumerable_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo", type: typeof(IEnumerable<string>));

        _sut.Execute(arg, _mockRepository.Object, Tokens("--foo", "bar"));

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }));
    }

    [Fact]
    public void Execute_ShouldCreateICollection_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: 1, dest: "foo", type: typeof(ICollection<string>));

        _sut.Execute(arg, _mockRepository.Object, Tokens("--foo", "bar"));

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }));
    }

    [Fact]
    public void Execute_ShouldStoreMultipleValues_WhenArityGreaterThanOne()
    {
        var arg = new Argument("--foo", arity: 3, dest: "foo", type: typeof(List<string>));

        _sut.Execute(arg, _mockRepository.Object, Tokens("--foo", "bar", "baz", "qux"));

        _mockRepository.Verify(x => x.Set("foo", new List<string> { "bar", "baz", "qux" }));
    }

    [Fact]
    public void Execute_ShouldAppendToArray_WhenExists()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: 2, dest: "foo", type: typeof(string[]));

        _sut.Execute(arg, repository, Tokens("--foo", "bar", "baz"));
        _sut.Execute(arg, repository, Tokens("--foo", "qux", "quux"));

        Assert.True(repository.TryGetValue<string[]>(arg.Dest, out var actual));
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToIList_WhenExists()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: 2, dest: "foo", type: typeof(IList<string>));

        _sut.Execute(arg, repository, Tokens("--foo", "bar", "baz"));
        _sut.Execute(arg, repository, Tokens("--foo", "qux", "quux"));

        Assert.True(repository.TryGetValue<IList<string>>(arg.Dest, out var actual));
        Assert.True(actual is string[]);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToList_WhenExists()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: 2, dest: "foo", type: typeof(List<string>));

        _sut.Execute(arg, repository, Tokens("--foo", "bar", "baz"));
        _sut.Execute(arg, repository, Tokens("--foo", "qux", "quux"));

        Assert.True(repository.TryGetValue<List<string>>(arg.Dest, out var actual));
        Assert.True(actual is List<string>);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToIEnumerable_WhenExists()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: 2, dest: "foo", type: typeof(IEnumerable<string>));

        _sut.Execute(arg, repository, Tokens("--foo", "bar", "baz"));
        _sut.Execute(arg, repository, Tokens("--foo", "qux", "quux"));

        Assert.True(repository.TryGetValue<IEnumerable<string>>(arg.Dest, out var actual));
        Assert.True(actual is IEnumerable<string>);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToICollection_WhenExists()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: 2, dest: "foo", type: typeof(ICollection<string>));

        _sut.Execute(arg, repository, Tokens("--foo", "bar", "baz"));
        _sut.Execute(arg, repository, Tokens("--foo", "qux", "quux"));

        Assert.True(repository.TryGetValue<ICollection<string>>(arg.Dest, out var actual));
        Assert.True(actual is ICollection<string>);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    private static ReadOnlySpan<Token> Tokens(params string[] tokens)
        => tokens.Select(x => new Token(x)).ToArray();
}