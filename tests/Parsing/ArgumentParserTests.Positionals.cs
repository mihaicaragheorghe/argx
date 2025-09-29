using Argx.Parsing;
using Moq;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Fact]
    public void Parse_ShouldStoreArg_WhenValidPositional()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");

        var result = parser.Parse(["bar"]);

        Assert.Equal("bar", result["foo"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgs_WhenValidPositionalArguments()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");
        parser.Add("bar");

        var result = parser.Parse(["baz", "qux"]);

        Assert.Equal("baz", result["foo"]);
        Assert.Equal("qux", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgToDest_WhenValidPositional()
    {
        var parser = new ArgumentParser();
        parser.Add("foo", dest: "bar");

        var result = parser.Parse(["baz"]);

        Assert.Equal("baz", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgT_WhenValidPositionalT()
    {
        var repo = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repo.Object);
        parser.Add<int>("x");

        _ = parser.Parse(["21"]);

        repo.Verify(x => x.Set("x", 21));
    }

    [Fact]
    public void Parse_ShouldStoreValues_WhenTypedPositionalArguments()
    {
        var parser = new ArgumentParser();
        string[] args = ["21", "22"];
        parser.Add<int>("x");
        parser.Add<int>("y");

        var result = parser.Parse(args);

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

        var result = parser.Parse(args);

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

        Assert.Throws<InvalidOperationException>(() => parser.Parse(args));
    }
}