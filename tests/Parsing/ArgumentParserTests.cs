using Argx.Parsing;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Add_ShouldThrowArgumentException_WhenNullOrEmptyName(string? arg)
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add(arg!));
    }

    [Fact]
    public void Parse_ShouldOnlyParsePositionals_WhenSeparatorEncountered()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("arg1");
        parser.Add("arg2");

        var result = parser.Parse(["--foo", "bar", "--", "--baz", "qux"]);

        Assert.Equal("--baz", result["arg1"]);
        Assert.Equal("qux", result["arg2"]);
    }

    public static TheoryData<string[]> UnorderedArgsData => new()
    {
        {["--foo", "bar", "--bax", "qux", "123", "456"]},
        {["123", "456", "--foo", "bar", "--bax", "qux"]},
        {["--foo", "bar", "123", "456", "--bax", "qux"]},
        {["--bax", "qux", "123", "456", "--foo", "bar"]},
    };

    [Theory]
    [MemberData(nameof(UnorderedArgsData))]
    public void Parse_ShouldCorrectlyParseOptsAndArgs_WhenNotInOrder(string[] args)
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("--bax");
        parser.Add("x");
        parser.Add("y");

        var result = parser.Parse(args);

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("qux", result["bax"]);
        Assert.Equal("123", result["x"]);
        Assert.Equal("456", result["y"]);
    }
}