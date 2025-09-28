using Argx.Parsing;
using Moq;

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
    public void Add_ShouldThrowArgumentException_WhenActionValidationFails()
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("foo", arity: 0));
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenActionNotFound()
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("--foo", "-f", action: "void"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Add_ShouldThrowArgumentException_WhenAliasIsEmptyOrWhitespace(string alias)
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("--foo", alias));
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenInvalidAlias()
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("--foo", "f"));
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenPositionalArgWithAlias()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.Add("foo", "-f"));
    }

    [Fact]
    public void Add_ShouldAddArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddOption("--foo");
        _ = parser.Parse(["--foo", "bar"]);

        repository.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void AddT_ShouldAddArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddOption<int>("--foo");
        _ = parser.Parse(["--foo", "69"]);

        repository.Verify(x => x.Set("foo", 69));
    }

    [Fact]
    public void AddArgument_ShouldAddPositionalArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddArgument("foo");
        _ = parser.Parse(["bar"]);

        repository.Verify(x => x.Set("foo", "bar"));
    }

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
        _ = parser.Parse(["69"]);

        repository.Verify(x => x.Set("foo", 69));
    }

    [Fact]
    public void AddFlag_ShouldAddFlagOption_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddFlag("--foo", "-f");
        _ = parser.Parse(["--foo"]);

        repository.Verify(x => x.Set("foo", true));
    }

    [Fact]
    public void AddFlag_ShouldAddFalseFlagOption_WhenValueIsFalse()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddFlag("--foo", "-f", value: false);
        _ = parser.Parse(["--foo"]);

        repository.Verify(x => x.Set("foo", false));
    }

    [Fact]
    public void AddFlag_ShouldThrowInvalidOperationException_WhenArgIsNotOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddArgument("--foo"));
    }

    [Fact]
    public void AddOption_ShouldAddOptionalArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddOption("--foo");
        _ = parser.Parse(["--foo", "bar"]);

        repository.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void AddOption_ShouldThrowInvalidOperationException_WhenArgIsNotOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddOption("foo"));
    }

    [Fact]
    public void AddOptionT_ShouldThrowInvalidOperationException_WhenArgIsOption()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.AddOption<int>("foo"));
    }

    [Fact]
    public void AddOptionT_ShouldAddOptionalArgument_WhenValid()
    {
        var repository = new Mock<IArgumentRepository>();
        var parser = new ArgumentParser(repository.Object);
        parser.AddOption<int>("--foo");
        _ = parser.Parse(["--foo", "69"]);

        repository.Verify(x => x.Set("foo", 69));
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