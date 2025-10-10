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
    public void AddArgumentT_ShouldAddPositionalArgument_WhenValid()
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
    public void AddArgument_ShouldAddPositionalArgument_WhenValid()
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
    public void Parse_ShouldStoreArg_WhenValidPositional()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");

        var result = parser.ParseInternal(["bar"]);

        Assert.Equal("bar", result["foo"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgs_WhenValidPositionalArguments()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");
        parser.Add("bar");

        var result = parser.ParseInternal(["baz", "qux"]);

        Assert.Equal("baz", result["foo"]);
        Assert.Equal("qux", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgToDest_WhenValidPositional()
    {
        var parser = new ArgumentParser();
        parser.Add("foo", dest: "bar");

        var result = parser.ParseInternal(["baz"]);

        Assert.Equal("baz", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgT_WhenValidPositionalT()
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
    public void Parse_ShouldStoreValues_WhenTypedPositionalArguments()
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
    public void Parse_ShouldStoreCorrectType_WhenTypedPositionalArguments()
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
    public void Parse_ShouldThrowInvalidOperationException_WhenArityNotOne()
    {
        var parser = new ArgumentParser();
        string[] args = ["foo"];
        parser.Add<int[]>("echo", arity: "2");

        Assert.Throws<InvalidOperationException>(() => parser.ParseInternal(args));
    }

    [Fact]
    public void Parse_ShouldAddToExtras_WhenNoMoreArguments()
    {
        var parser = new ArgumentParser();
        parser.Add<int>("x");
        parser.Add<int>("y");

        var result = parser.ParseInternal(["1", "2", "3"]);

        Assert.Equal("1", result["x"]);
        Assert.Equal("2", result["y"]);
        Assert.Contains("3", result.Extras);
    }
}