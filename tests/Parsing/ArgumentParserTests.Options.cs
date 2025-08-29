using Argx.Actions;
using Argx.Parsing;
using Moq;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Fact]
    public void Parse_ShouldStoreArg_WhenValidOption()
    {
        var parser = new ArgumentParser();
        parser.Add("--input");

        var result = parser.Parse(["--input", "foo.txt"]);

        Assert.Equal("foo.txt", result["input"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgs_WhenValidOptions()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("--bar");

        var result = parser.Parse(["--foo", "bar", "--bar", "baz"]);

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("baz", result["bar"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgToDest_WhenArgHasDest()
    {
        var parser = new ArgumentParser();
        parser.Add("--input", dest: "file");

        var result = parser.Parse(["--input", "foo.txt"]);

        Assert.Equal("foo.txt", result["file"]);
    }

    [Fact]
    public void Parse_ShouldStoreArgT_WhenValidOptionT()
    {
        var repo = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repo.Object);
        parser.Add<int>("--foo");

        _ = parser.Parse(["--foo", "21"]);

        repo.Verify(x => x.Set("foo", 21));
    }

    [Fact]
    public void Parse_ShouldThrowInvalidCastException_WhenOptionConversionFails()
    {
        var parser = new ArgumentParser();
        parser.Add<int>("--foo");

        Assert.Throws<InvalidCastException>(() => parser.Parse(["--foo", "bar"]));
    }

    [Fact]
    public void Parse_ShouldAddToExtras_WhenOptionNotRegistered()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo", "-f");

        var result = parser.Parse(["--foo", "bar", "--baz", "qux"]);

        Assert.Null(result["baz"]);
        Assert.Contains("--baz", result.Extras);
    }

    [Fact]
    public void Parse_ShouldThrow_WhenActionNotFound()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo", "-f", action: "void");

        Assert.Throws<InvalidOperationException>(() => parser.Parse(["--foo", "bar"]));
    }

    [Fact]
    public void Parse_ShouldSendCorrectTokensToAction_WhenAritySet()
    {
        var parser = new ArgumentParser();
        string[] expected = ["bar", "baz", "qux"];
        parser.Add<string[]>("--foo", arity: 3, action: ArgumentActions.Store);

        var result = parser.Parse(["--foo", "bar", "baz", "qux", "--extra"]);

        Assert.True(result.TryGetValue<string[]>("foo", out var actual));
        Assert.Equivalent(expected, actual);
        Assert.Contains("--extra", result.Extras);
    }
}