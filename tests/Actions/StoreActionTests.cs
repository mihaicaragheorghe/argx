using Argx.Actions;
using Argx.Parsing;

using Moq;

namespace Argx.Tests.Actions;

public class StoreActionTests
{
    private readonly Mock<IArgumentRepository> _mockRepository = new();
    private readonly StoreAction _sut = new();

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: 0);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(arg, _mockRepository.Object, "foo", TokenSpan(["--foo"])));
    }

    [Fact]
    public void Execute_ShouldStoreArgumentValue_WhenConvertedSuccessfully()
    {
        const string key = "foo";
        const string value = "bar";
        var arg = new Argument($"--{key}", arity: 1);

        _sut.Execute(arg, _mockRepository.Object, key, TokenSpan(arg.Name, value));

        _mockRepository.Verify(x => x.Set(key, value));
    }

    [Fact]
    public void Execute_ShouldDefaultArityToOne_WhenNull()
    {
        const string key = "foo";
        const string value = "bar";
        var arg = new Argument($"--{key}", arity: null);

        _sut.Execute(arg, _mockRepository.Object, key, TokenSpan(arg.Name, value));

        _mockRepository.Verify(x => x.Set(key, value));
    }

    [Fact]
    public void Execute_ShouldStoreCollections_WhenTypeIsEnumerable()
    {
        var arg = new Argument("--foo", arity: 3, type: typeof(string[]));
        var tokens = TokenSpan("--foo", "bar", "baz", "qux");
        var value = new[] { "bar", "baz", "qux" };

        _sut.Execute(arg, _mockRepository.Object, "foo", tokens);

        _mockRepository.Verify(x => x.Set("foo", value));
    }

    [Fact]
    public void Execute_ShouldThrow_WhenConversionFails()
    {
        Assert.Throws<InvalidCastException>(() =>
        {
            var arg = new Argument("--foo", type: typeof(int));
            var span = TokenSpan(["--foo", "bar"]);
            _sut.Execute(arg, _mockRepository.Object, "foo", span);
        });
    }

    private static ReadOnlySpan<Token> TokenSpan(params string[] tokens)
        => tokens.Select(s => new Token(s)).ToArray().AsSpan();
}