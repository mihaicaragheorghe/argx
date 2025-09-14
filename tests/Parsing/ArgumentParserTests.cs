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
}