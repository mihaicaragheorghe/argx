using System.Diagnostics.CodeAnalysis;

using Argx.Actions;
using Argx.Errors;
using Argx.Store;
using Argx.Tests.TestUtils;

using Moq;

namespace Argx.Tests.Actions;

// ReSharper disable ConvertTypeCheckToNullCheck
[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
public class AppendActionTests
{
    private readonly Mock<IArgumentRepository> _mockRepository = new();
    private readonly AppendAction _sut = new();

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: "0", dest: "foo", type: typeof(string[]));

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal("Argument --foo: arity for 'append' must be != 0", ex.Message);
    }

    [Theory]
    [InlineData("2")]
    [InlineData(Arity.AtLeastOne)]
    public void Validate_ShouldThrowArgumentException_WhenNoConstValueAndArityNotOptional(string arity)
    {
        var arg = new Argument("--foo", arity: arity, dest: "foo", type: typeof(string[]), constValue: new[] { "bar" });

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal($"Argument --foo: arity must be {Arity.Optional} or {Arity.Any} to supply a const value",
            ex.Message);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Guid))]
    public void Validate_ShouldThrowArgumentException_WhenTypeNotEnumerable(Type type)
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo", type: type);

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal("Argument --foo: type for 'append' must be an enumerable", ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenInvalidConstValueType()
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]), constValue: 3);

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal(
            "Argument --foo: const value must be either an enumerable of type string or an instance of type string",
            ex.Message);
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenInvalidConstValueItemType()
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]),
            constValue: new[] { 3 });

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal(
            "Argument --foo: const value must be either an enumerable of type string or an instance of type string",
            ex.Message);
    }

    [Theory]
    [InlineData(typeof(int[]), 3)]
    [InlineData(typeof(int[]), new[] { 3 })]
    [InlineData(typeof(string[]), "bar")]
    [InlineData(typeof(string[]), new[] { "bar" })]
    public void Validate_ShouldNotThrow_WhenValid(Type type, object value)
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: type, constValue: value);

        _sut.Validate(arg);
    }

    [Fact]
    public void Execute_ShouldThrowBadArgumentException_WhenTokensLengthIsZero()
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo");

        var ex = Assert.Throws<ArgumentValueException>(() =>
            _sut.Execute(arg, Create.Token("--foo"), [], _mockRepository.Object));
        Assert.Equal("Error: argument --foo: expected value", ex.Message);
    }

    [Fact]
    public void Execute_ShouldCreateArray_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo", type: typeof(string[]));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldCreateList_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo", type: typeof(List<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new List<string> { "bar" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldCreateIEnumerable_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo", type: typeof(IEnumerable<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldCreateICollection_WhenNotAlreadyStored()
    {
        var arg = new Argument("--foo", arity: "1", dest: "foo", type: typeof(ICollection<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldStoreMultipleValues_WhenArityGreaterThanOne()
    {
        var arg = new Argument("--foo", arity: "3", dest: "foo", type: typeof(List<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar", "baz", "qux"), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new List<string> { "bar", "baz", "qux" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldAppendToArray_WhenExists()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: "2", dest: "foo", type: typeof(string[]));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar", "baz"), repository);
        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("qux", "quux"), repository);

        Assert.True(repository.TryGetValue<string[]>(arg.Dest, out var actual));
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToIList_WhenStored
        ()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: "2", dest: "foo", type: typeof(IList<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar", "baz"), repository);
        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("qux", "quux"), repository);

        Assert.True(repository.TryGetValue<IList<string>>(arg.Dest, out var actual));
        Assert.True(actual is string[]);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToList_WhenStored
        ()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: "2", dest: "foo", type: typeof(List<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar", "baz"), repository);
        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("qux", "quux"), repository);

        Assert.True(repository.TryGetValue<List<string>>(arg.Dest, out var actual));
        Assert.True(actual is List<string>);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToIEnumerable_WhenStored
        ()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: "2", dest: "foo", type: typeof(IEnumerable<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar", "baz"), repository);
        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("qux", "quux"), repository);

        Assert.True(repository.TryGetValue<IEnumerable<string>>(arg.Dest, out var actual));
        Assert.True(actual is IEnumerable<string>);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldAppendToICollection_WhenStored
        ()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: "2", dest: "foo", type: typeof(ICollection<string>));

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar", "baz"), repository);
        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("qux", "quux"), repository);

        Assert.True(repository.TryGetValue<ICollection<string>>(arg.Dest, out var actual));
        Assert.True(actual is ICollection<string>);
        Assert.Equal(["bar", "baz", "qux", "quux"], actual);
    }

    [Fact]
    public void Execute_ShouldStoreConstSingleValue_WhenArityIsOptionalAndNoValueProvided()
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]), constValue: "bar");

        _sut.Execute(arg, Create.Token("--foo"), [], _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldAppendConstSingleValue_WhenArityIsOptionalAndKeyAlreadyStored()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]), constValue: "baz");

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), repository);
        _sut.Execute(arg, Create.Token("--foo"), [], repository);

        Assert.True(repository.TryGetValue<string[]>(arg.Dest, out var actual));
        Assert.Equal(["bar", "baz"], actual);
    }

    [Fact]
    public void Execute_ShouldStoreEnumerableConstValue_WhenArityIsOptionalAndNoValueProvided()
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]),
            constValue: new[] { "bar" });

        _sut.Execute(arg, Create.Token("--foo"), [], _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", new[] { "bar" }), Times.Once);
    }

    [Fact]
    public void Execute_ShouldAppendEnumerableConstValue_WhenArityIsOptionalAndKeyAlreadyStored()
    {
        var repository = new ArgumentRepository();
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]),
            constValue: new[] { "baz", "qux" });

        _sut.Execute(arg, Create.Token("--foo"), Create.Tokens("bar"), repository);
        _sut.Execute(arg, Create.Token("--foo"), [], repository);

        Assert.True(repository.TryGetValue<string[]>(arg.Dest, out var actual));
        Assert.Equal(["bar", "baz", "qux"], actual);
    }

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenDifferentSingleConstValueType()
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]), constValue: 123);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(arg, Create.Token("--foo"), [], _mockRepository.Object));
    }

    [Fact]
    public void Execute_ShouldThrowInvalidCastException_WhenDifferentEnumerableConstValueType()
    {
        var arg = new Argument("--foo", arity: Arity.Optional, dest: "foo", type: typeof(string[]),
            constValue: new[] { 123 });

        Assert.Throws<InvalidCastException>(() => _sut.Execute(arg, Create.Token("--foo"), [], _mockRepository.Object));
    }
}