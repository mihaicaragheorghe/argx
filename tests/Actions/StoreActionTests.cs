using Argx.Actions;
using Argx.Parsing;

namespace Argx.Tests.Actions;

public class StoreActionTests
{
    private readonly ArgumentRepository _repository = new();
    private readonly StoreAction _sut = new();

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: 0);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(arg, _repository, "foo", TokenSpan(["--foo"])));
    }

    [Fact]
    public void Execute_ShouldStoreArgumentValue_WhenConvertedSuccessfully()
    {
        var arg = new Argument("--foo", arity: 1);

        _sut.Execute(arg, _repository, "foo", TokenSpan(["--foo", "bar"]));

        Assert.True(_repository.TryGetValue("foo", out var value));
        Assert.Equal("bar", value);
    }

    [Fact]
    public void Execute_ShouldDefaultArityToOne_WhenNull()
    {
        var arg = new Argument("--foo", type: typeof(int), arity: null);

        _sut.Execute(arg, _repository, "foo", TokenSpan(["--foo", "123"]));

        Assert.True(_repository.TryGetValue<int>("foo", out var value));
        Assert.Equal(123, value);
    }

    [Fact]
    public void Execute_ShouldStoreCollections_WhenTypeIsEnumerable()
    {
        var arg = new Argument("--foo", arity: 3, type: typeof(string[]));
        var span = TokenSpan(["--foo", "bar", "baz", "qux"]);
        var expected = new[] { "bar", "baz", "qux" };

        _sut.Execute(arg, _repository, "foo", span);

        Assert.True(_repository.TryGetValue<string[]>("foo", out var value));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Execute_ShouldThrow_WhenConversionFails()
    {
        Assert.Throws<InvalidCastException>(() =>
        {
            var arg = new Argument("--foo", type: typeof(int));
            var span = TokenSpan(["--foo", "bar"]);
            _sut.Execute(arg, _repository, "foo", span);
        });
    }

    private ReadOnlySpan<Token> TokenSpan(params string[] tokens)
        => tokens.Select(s => new Token(s)).ToArray().AsSpan();
}