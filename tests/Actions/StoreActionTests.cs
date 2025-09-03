using Argx.Actions;
using Argx.Parsing;

namespace Argx.Tests.Actions;

public class StoreActionTests
{
    private readonly ArgumentStore _store = new();
    private readonly StoreAction _sut = new();

    [Fact]
    public void Execute_ShouldThrowInvalidOperationException_WhenArityIsZero()
    {
        var arg = new Argument("--foo", arity: 0);

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(arg, _store, "foo", TokenSpan(["--foo"])));
    }

    [Fact]
    public void Execute_ShouldStoreArgumentValue_WhenConvertedSuccessfully()
    {
        var arg = new Argument("--foo", arity: 1);

        _sut.Execute(arg, _store, "foo", TokenSpan(["--foo", "bar"]));

        Assert.Equal("bar", _store.Get<string>("foo"));
    }

    [Fact]
    public void Execute_ShouldDefaultArityToOne_WhenNull()
    {
        var arg = new Argument("--foo", type: typeof(int), arity: null);

        _sut.Execute(arg, _store, "foo", TokenSpan(["--foo", "123"]));

        Assert.Equal(123, _store.Get<int>("foo"));
    }

    [Fact]
    public void Execute_ShouldStoreCollections_WhenTypeIsEnumerable()
    {
        var arg = new Argument("--foo", arity: 3, type: typeof(string[]));
        var span = TokenSpan(["--foo", "bar", "baz", "qux"]);
        var expected = new[] { "bar", "baz", "qux" };

        _sut.Execute(arg, _store, "foo", span);

        Assert.Equivalent(expected, _store.Get<string[]>("foo"));
    }

    [Fact]
    public void Execute_ShouldThrow_WhenConversionFails()
    {
        Assert.Throws<InvalidCastException>(() =>
        {
            var arg = new Argument("--foo", type: typeof(int));
            var span = TokenSpan(["--foo", "bar"]);
            _sut.Execute(arg, _store, "foo", span);
        });
    }

    private ReadOnlySpan<Token> TokenSpan(params string[] tokens)
        => tokens.Select(s => new Token(s)).ToArray().AsSpan();
}