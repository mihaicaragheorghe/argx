using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;
using Argx.Store;

using Moq;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Fact]
    public void AddArgument_ShouldThrowInvalidOperationException_WhenArgIsOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddArgument("--foo"));
    }

    [Fact]
    public void AddArgumentT_ShouldThrowInvalidOperationException_WhenArgIsOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddArgument<int>("--foo"));
    }

    [Fact]
    public void AddArgumentT_ShouldAddPositional_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddArgument<int>("foo");
        repository
            .Setup(x => x.Contains("foo"))
            .Returns(true);

        _ = parser.ParseInternal(["69"]);

        repository.Verify(x => x.Set("foo", 69));
    }

    [Fact]
    public void AddArgument_ShouldAddPositional_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddArgument("foo");
        repository
            .Setup(x => x.Contains("foo"))
            .Returns(true);

        _ = parser.ParseInternal(["bar"]);

        repository.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void Parse_ShouldStoreValue_WhenValidPositional()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");

        var result = parser.ParseInternal(["bar"]);

        Assert.Equal("bar", result["foo"]);
    }

    [Fact]
    public void Parse_ShouldStoreValues_WhenValidPositionals()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");
        parser.Add("bar");

        var result = parser.ParseInternal(["baz", "qux"]);

        Assert.Equal("baz", result["foo"]);
        Assert.Equal("qux", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreValueToDest_WhenHasDest()
    {
        var parser = new ArgumentParser();
        parser.Add("foo", dest: "bar");

        var result = parser.ParseInternal(["baz"]);

        Assert.Equal("baz", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreTValue_WhenValidPositionalT()
    {
        var repo = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repo.Object);
        parser.Add<int>("x");
        repo.Setup(x => x.Contains("x"))
            .Returns(true);

        _ = parser.ParseInternal(["21"]);

        repo.Verify(x => x.Set("x", 21));
    }

    [Fact]
    public void Parse_ShouldStoreValues_WhenTypedPositionals()
    {
        var parser = new ArgumentParser();
        string[] args = ["21", "22"];
        parser.Add<int>("x");
        parser.Add<int>("y");

        var result = parser.ParseInternal(args);

        Assert.Equal("21", result["x"]);
        Assert.Equal("22", result["y"]);
    }

    [Fact]
    public void Parse_ShouldStoreCorrectType_WhenTypedPositionals()
    {
        var parser = new ArgumentParser();
        string[] args = ["21", "22"];
        parser.Add<int>("x");
        parser.Add<int>("y");

        var result = parser.ParseInternal(args);

        Assert.True(result.TryGetValue<int>("x", out var x));
        Assert.True(result.TryGetValue<int>("y", out var y));
        Assert.Equal(21, x);
        Assert.Equal(22, y);
    }

    [Fact]
    public void Parse_ShouldAddToExtras_WhenNoPositionalsLeft()
    {
        var parser = new ArgumentParser();
        parser.Add<int>("x");
        parser.Add<int>("y");

        var result = parser.ParseInternal(["1", "2", "3"]);

        Assert.Equal("1", result["x"]);
        Assert.Equal("2", result["y"]);
        Assert.Contains("3", result.Extras);
    }

    [Fact]
    public void Parse_ShouldParseFixedTokens_WhenArityIsFixed()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz", "qux"];
        parser.Add<string[]>("foo", arity: "3", action: ArgumentActions.Store);

        var result = parser.ParseInternal(["bar", "baz", "qux", "--extra"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
        Assert.Contains("--extra", result.Extras);
    }

    [Fact]
    public void Parse_ShouldParseToken_WhenPositionalAndArityOptional()
    {
        var parser = new ArgumentParser();
        parser.Add<string>("foo", arity: Arity.Optional, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["bar", "baz", "qux"]);
        Assert.True(result.TryGetValue<string>("foo", out var actual));
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void Parse_ShouldParseArgumentTokens_WhenPositionalAndArityAny()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("foo", arity: Arity.Any, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["bar", "baz", "--qux", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(["bar", "baz"], actual);
    }

    [Fact]
    public void Parse_ShouldParseArgumentTokens_WhenArityAtLeastOne()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz"];
        parser.Add<string[]>("foo", arity: Arity.AtLeastOne, action: ArgumentActions.Store);

        var result = parser.ParseInternal(["bar", "baz", "--qux", "quux"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Parse_ShouldThrowInvalidOperationException_WhenArityAtLeastOneAndNoToken()
    {
        var parser = new ArgumentParser();
        parser.Add<string[]>("foo", arity: Arity.AtLeastOne, action: ArgumentActions.Store);
        parser.Add<string[]>("--bar", arity: Arity.AtLeastOne, action: ArgumentActions.Store);

        Assert.Throws<ArgumentValueException>(() => parser.ParseInternal(["--bar", "baz", "qux", "quux"]));
    }
}