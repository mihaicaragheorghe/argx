using Argx.Actions;
using Argx.Errors;
using Argx.Store;
using Argx.Tests.TestUtils;

using Moq;

namespace Argx.Tests.Actions;

public class StoreActionTests
{
    private readonly Mock<IArgumentRepository> _mockRepository = new();
    private readonly StoreAction _sut = new();

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: "0", dest: "foo");

        Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
    }

    [Theory]
    [InlineData("2")]
    [InlineData(Arity.AtLeastOne)]
    public void Validate_ShouldThrowArgumentException_WhenNoConstValueAndArityNotOptional(string arity)
    {
        var arg = new Argument("--foo", arity: arity, dest: "foo", constValue: "bar");

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal($"Argument --foo: arity must be {Arity.Optional} or {Arity.Any} to supply a const value",
            ex.Message);
    }

    [Theory]
    [InlineData("2")]
    [InlineData(Arity.Any)]
    [InlineData(Arity.AtLeastOne)]
    public void Validate_ShouldThrowArgumentException_WhenArityAcceptsMultipleAndTypeNotEnumerable(string arity)
    {
        var arg = new Argument("--foo", arity: arity, dest: "foo", type: typeof(string));

        var ex = Assert.Throws<ArgumentException>(() => _sut.Validate(arg));
        Assert.Equal(
            $"Argument --foo: type must be enumerable for arity > 1, {Arity.Any} or {Arity.AtLeastOne}, consider {Arity.Optional}",
            ex.Message);
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
    public void Execute_ShouldStoreValue_WhenConvertedSuccessfully()
    {
        const string key = "foo";
        const string value = "bar";
        var arg = new Argument($"--{key}", arity: "1", dest: key);

        _sut.Execute(arg, Create.Token(arg.Name), Create.Tokens(value), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set(key, value));
    }

    [Fact]
    public void Execute_ShouldDefaultArityToOne_WhenNull()
    {
        const string key = "foo";
        const string value = "bar";
        var arg = new Argument($"--{key}", arity: null, dest: key);

        _sut.Execute(arg, Create.Token(arg.Name), Create.Tokens(value), _mockRepository.Object);

        _mockRepository.Verify(x => x.Set(key, value));
    }

    [Fact]
    public void Execute_ShouldStoreConstValue_WhenArityIsOptionalAndNoTokens()
    {
        var arg = new Argument($"--foo", arity: Arity.Optional, dest: "foo", constValue: "bar");

        _sut.Execute(arg, Create.Token("--foo"), [], _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void Execute_ShouldStoreCollections_WhenTypeIsArray()
    {
        var arg = new Argument("--foo", arity: "3", type: typeof(string[]), dest: "foo");
        var tokens = Create.Tokens("--foo", "bar", "baz", "qux");
        var value = new[] { "bar", "baz", "qux" };

        _sut.Execute(arg, tokens[0], tokens[1..], _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", value));
    }

    [Fact]
    public void Execute_ShouldStoreCollections_WhenTypeIsList()
    {
        var arg = new Argument("--foo", arity: "3", type: typeof(List<string>), dest: "foo");
        var tokens = Create.Tokens("--foo", "bar", "baz", "qux");
        var value = new[] { "bar", "baz", "qux" };

        _sut.Execute(arg, tokens[0], tokens[1..], _mockRepository.Object);

        _mockRepository.Verify(x => x.Set("foo", value));
    }

    [Theory]
    [InlineData(typeof(bool), "bool")]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(uint), "uint")]
    [InlineData(typeof(short), "short")]
    [InlineData(typeof(ushort), "ushort")]
    [InlineData(typeof(long), "long")]
    [InlineData(typeof(ulong), "ulong")]
    [InlineData(typeof(float), "float")]
    [InlineData(typeof(decimal), "decimal")]
    [InlineData(typeof(double), "double")]
    [InlineData(typeof(Guid), "guid")]
    [InlineData(typeof(DateTime), "DateTime")]
    [InlineData(typeof(TimeSpan), "TimeSpan")]
    [InlineData(typeof(int[]), "int[]")]
    [InlineData(typeof(List<int>), "int[]")]
    [InlineData(typeof(IList<int>), "int[]")]
    [InlineData(typeof(ICollection<int>), "int[]")]
    public void Execute_ShouldThrowBadArgumentException_WhenConversionFails(Type type, string typeStr)
    {
        var ex = Assert.Throws<ArgumentValueException>(() =>
        {
            var arg = new Argument("--foo", type: type, dest: "foo");
            var span = Create.Tokens("--foo", "bar");
            _sut.Execute(arg, span[0], span[1..], _mockRepository.Object);
        });
        Assert.StartsWith($"Error: argument --foo: expected type {typeStr}", ex.Message);
    }
}