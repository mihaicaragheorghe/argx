using System.Reflection;

using Argx.Actions;
using Argx.Errors;
using Argx.Parsing;

using Moq;

namespace Argx.Tests.Parsing;

public partial class ArgumentParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Add_ShouldThrowArgumentException_WhenNameNullOrEmpty(string? name)
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add(name!));
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenActionValidationFails()
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("foo", arity: "2", action: ArgumentActions.Store));
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenActionNotFound()
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("--foo", ["-f"], action: "void"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Add_ShouldThrowArgumentException_WhenAliasIsEmptyOrWhitespace(string alias)
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("--foo", [alias]));
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenInvalidAlias()
    {
        var parser = new ArgumentParser();
        Assert.Throws<ArgumentException>(() => parser.Add("--foo", ["f"]));
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenPositionalArgWithAlias()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.Add("foo", ["-f"]));
    }

    [Fact]
    public void Add_ShouldThrowInvalidOperationException_WhenPositionalArgWithArityZero()
    {
        var parser = new ArgumentParser();
        Assert.Throws<InvalidOperationException>(() => parser.Add("foo", arity: "0"));
    }

    [Theory]
    [InlineData(Arity.Any)]
    [InlineData(Arity.AtLeastOne)]
    public void Add_ShouldThrowInvalidOperationException_WhenArgWithInvalidArityForType(string arity)
    {
        var parser = new ArgumentParser();
        var ex = Assert.Throws<InvalidOperationException>(() => parser.Add<int>("foo", arity: arity));
        Assert.Equal($"Argument foo: only arguments with enumerable types can have arity '{arity}'", ex.Message);
    }

    [Fact]
    public void Add_ShouldAddArgument_WhenValid()
    {
        var store = new Mock<IArgumentStore>();
        var parser = new ArgumentParser(store.Object);
        parser.Add("--foo");

        _ = parser.ParseInternal(["--foo", "bar"]);

        store.Verify(x => x.Set("foo", "bar"));
    }

    [Fact]
    public void AddT_ShouldAddArgument_WhenValid()
    {
        var store = new Mock<IArgumentStore>();
        var parser = new ArgumentParser(store.Object);
        parser.Add<int>("--foo");

        _ = parser.ParseInternal(["--foo", "69"]);

        store.Verify(x => x.Set("foo", 69));
    }

    [Fact]
    public void Parse_ShouldOnlyParsePositionals_WhenSeparatorEncountered()
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("arg1");
        parser.Add("arg2");

        var result = parser.ParseInternal(["--foo", "bar", "--", "--baz", "qux"]);

        Assert.Equal("--baz", result["arg1"]);
        Assert.Equal("qux", result["arg2"]);
    }

    public static TheoryData<string[]> UnorderedArgsData =>
    [
        ["--foo", "bar", "--bax", "qux", "123", "456"],
        ["123", "456", "--foo", "bar", "--bax", "qux"],
        ["--foo", "bar", "123", "456", "--bax", "qux"],
        ["--bax", "qux", "123", "456", "--foo", "bar"]
    ];

    [Theory]
    [MemberData(nameof(UnorderedArgsData))]
    public void Parse_ShouldCorrectlyParseOptsAndArgs_WhenNotInOrder(string[] args)
    {
        var parser = new ArgumentParser();
        parser.Add("--foo");
        parser.Add("--bax");
        parser.Add("x");
        parser.Add("y");

        var result = parser.ParseInternal(args);

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("qux", result["bax"]);
        Assert.Equal("123", result["x"]);
        Assert.Equal("456", result["y"]);
    }

    [Fact]
    public void Parse_ShouldThrow_WhenNotExitOnError()
    {
        var parser = new ArgumentParser(configuration: new ArgumentParserConfiguration { ExitOnError = false });
        parser.Add("--foo", action: ArgumentActions.Store, choices: ["bar"]);

        Assert.Throws<ArgumentValueException>(() => parser.Parse("--foo", "baz"));
    }

    [Fact]
    public void Parse_ShouldThrowArgumentValueException_WhenRequiredArgNotProvided()
    {
        var parser = new ArgumentParser();
        parser.Add("foo");
        parser.Add("bar");

        var ex = Assert.Throws<ArgumentValueException>(() => parser.ParseInternal(["foo"]));
        Assert.Equal("Error: argument bar: expected value", ex.Message);
    }

    [Fact]
    public void WriteHelp_ShouldWriteProgramDescriptionAndEpilogue_WhenProvided()
    {
        var parser = new ArgumentParser("prog", "what the program does", epilogue: "check website");
        parser.Add("--foo");
        parser.Add("x");
        parser.Add("y");
        var writer = new StringWriter();
        const string expected = """
                                prog:
                                  what the program does

                                Usage:
                                  prog [--help] [--foo FOO] x y

                                Positional arguments:
                                  x
                                  y

                                Options:
                                  --foo
                                  --help, -h  Print help message

                                check website

                                """;

        parser.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldUseArgvZeroInUsage_WhenProgramNameNullOrEmpty()
    {
        var parser = new ArgumentParser(description: "what the program does");
        parser.Add("--foo");
        parser.Add("x");
        parser.Add("y");
        var program = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        var writer = new StringWriter();
        var expected = $"""
                        what the program does

                        Usage:
                          {program} [--help] [--foo FOO] x y

                        Positional arguments:
                          x
                          y

                        Options:
                          --foo
                          --help, -h  Print help message

                        """;

        parser.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }


    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WriteHelp_ShouldNotWriteProgramName_WhenNullOrEmpty(string? prog)
    {
        var parser = new ArgumentParser(app: prog, description: "what the program does");
        parser.Add("--foo");
        parser.Add("x");
        parser.Add("y");
        var program = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        var writer = new StringWriter();
        var expected = $"""
                        what the program does

                        Usage:
                          {program} [--help] [--foo FOO] x y

                        Positional arguments:
                          x
                          y

                        Options:
                          --foo
                          --help, -h  Print help message

                        """;

        parser.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WriteHelp_ShouldNotWriteDescription_WhenNullOrEmpty(string? description)
    {
        var parser = new ArgumentParser(description: description);
        parser.Add("--foo");
        parser.Add("x");
        parser.Add("y");
        var program = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        var writer = new StringWriter();
        var expected = $"""
                        Usage:
                          {program} [--help] [--foo FOO] x y

                        Positional arguments:
                          x
                          y

                        Options:
                          --foo
                          --help, -h  Print help message

                        """;

        parser.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldNotWriteEpilogue_WhenNullOrEmpty()
    {
        var parser = new ArgumentParser(usage: "prog x y");
        parser.Add("--foo");
        parser.Add("x");
        parser.Add("y");
        var writer = new StringWriter();
        const string expected = $"""
                                 Usage:
                                   prog x y

                                 Positional arguments:
                                   x
                                   y

                                 Options:
                                   --foo
                                   --help, -h  Print help message

                                 """;

        parser.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldWriteUsage_WhenNotNullOrEmpty()
    {
        var parser = new ArgumentParser(usage: "prog x y");
        parser.Add("--foo");
        parser.Add("x");
        parser.Add("y");
        var writer = new StringWriter();
        const string expected = $"""
                                 Usage:
                                   prog x y

                                 Positional arguments:
                                   x
                                   y

                                 Options:
                                   --foo
                                   --help, -h  Print help message

                                 """;

        parser.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }
}