using Argx.Parsing;

namespace Argx.Tests.Parsing;

public class ArgumentParserTests
{
    [Fact]
    public void Parse_ShouldCorrectlyParseStringArgs_WhenSinglePositionalStringArg()
    {
        var parser = new ArgumentParser();
        string[] args = ["hello"];
        parser.Add("echo");

        var result = parser.Parse(args);

        Assert.Equal("hello", result["echo"]);
    }

    [Fact]
    public void Parse_ShouldCorrectlyParseStringArgs_WhenMultiplePositionalStringArg()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");
        parser.Add("bar");

        var result = parser.Parse(["baz", "qux"]);

        Assert.Equal("baz", result["foo"]);
        Assert.Equal("qux", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldCorrectlyParseArgs_WhenMultiplePositionalIntArgs()
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
    public void Parse_ShouldBeCorrectType_WhenRetrieved()
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
}