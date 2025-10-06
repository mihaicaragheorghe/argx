using Argx.Help;

namespace Argx.Tests.Help;

public class HelpBuilderTests
{
    [Fact]
    public void AddSection_ShouldAddSection()
    {
        var section = new HelpSection("foo", "bar");
        var result = new HelpBuilder(HelpConfiguration.Default())
            .AddSection(section.Title, section.Content)
            .Build();

        Assert.Equal(section.Render(), result);
    }

    [Fact]
    public void AddText_ShouldAddText()
    {
        var result = new HelpBuilder(HelpConfiguration.Default())
            .AddText("foo")
            .Build();

        Assert.Equal("foo", result);
    }

    [Fact]
    public void AddArguments_ShouldAddSingleArgument()
    {
        var arg = new Argument("--foo", usage: "foo argument", alias: "-f");
        const string expected = """
                                Arguments:
                                  --foo, -f  foo argument
                                """;

        var result = new HelpBuilder(HelpConfiguration.Default())
            .AddArguments([arg])
            .Build();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddArguments_ShouldAddMultipleArguments()
    {
        List<Argument> args =
        [
            new("--foo", usage: "foo argument"),
            new("--bar", usage: "bar argument", alias: "-b"),
            new("--baz", usage: "baz argument", alias: "-baz"),
            new("qux", usage: "qux argument"),
            new("--quux")
        ];
        const string expected = """
                                Arguments:
                                  --bar, -b    bar argument
                                  --baz, -baz  baz argument
                                  --foo        foo argument
                                  --quux
                                  qux          qux argument
                                """;

        var result = new HelpBuilder(HelpConfiguration.Default())
            .AddArguments(args)
            .Build();


        Assert.Equal(expected, result);
    }

    [Fact]
    public void Build_ShouldRenderAllSections()
    {
        var program = new HelpSection("Program", "What the program does");
        var usage = new HelpSection("", "Usage: Program [--help, -h] [--foo, -f FOO] bar");
        var option = new Argument("--foo", usage: "foo option", alias: "-f");
        var positional = new Argument("bar", usage: "bar positional");
        const string expected = """
                                Program:
                                  What the program does

                                Usage: Program [--help, -h] [--foo, -f FOO] bar

                                Positional arguments:
                                  bar  bar positional

                                Options:
                                  --foo, -f  foo option
                                """;

        var result = new HelpBuilder(HelpConfiguration.Default())
            .AddSection(program.Title, program.Content)
            .AddSection(usage.Title, usage.Content)
            .AddArguments([positional], "Positional arguments")
            .AddArguments([option], "Options")
            .Build();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenPositional()
    {
        var arg = new Argument("foo", isPositional: true);
        const string expected = """
                                Usage:
                                  prog foo
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArguments_WhenPositional()
    {
        List<Argument> args =
        [
            new("x", isPositional: true),
            new("y", isPositional: true),
            new("z", isPositional: true)
        ];
        const string expected = """
                                Usage:
                                  prog x y z
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage(args, prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenOptionalWithZeroArity()
    {
        var arg = new Argument("--foo", arity: "0");
        const string expected = """
                                Usage:
                                  prog [--foo]
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenOptionalWithArityOne()
    {
        var arg = new Argument("--foo", arity: "1");
        const string expected = """
                                Usage:
                                  prog [--foo FOO]
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenOptionalWithArityFixed()
    {
        var arg = new Argument("--foo", arity: "3");
        const string expected = """
                                Usage:
                                  prog [--foo FOO FOO FOO]
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenOptionalWithArityOptional()
    {
        var arg = new Argument("--foo", arity: Arity.Optional);
        const string expected = """
                                Usage:
                                  prog [--foo [FOO]]
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenOptionalWithArityAny()
    {
        var arg = new Argument("--foo", arity: Arity.Any);
        const string expected = """
                                Usage:
                                  prog [--foo [FOO ...]]
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArgument_WhenOptionalWithArityAtLeastOne()
    {
        var arg = new Argument("--foo", arity: Arity.AtLeastOne);
        const string expected = """
                                Usage:
                                  prog [--foo [FOO ...]]
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage([arg], prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldUseAlias_WhenPrintAliasInUsageTrue()
    {
        List<Argument> args =
        [
            new("--foo", arity: "1", alias: "-f"),
            new("--bar", arity: "1", alias: "-b")
        ];
        const string expected = """
                                Usage:
                                  prog [-f FOO] [-b BAR]
                                """;
        var actual = new HelpBuilder(new HelpConfiguration { PrintAliasInUsage = true })
            .AddUsage(args, prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddUsage_ShouldCorrectlyAddArguments_WhenBothPositionalAndOptions()
    {
        List<Argument> args =
        [
            new("--help", arity: "0", alias: "-h"),
            new("-v", arity: "0"),
            new("--foo", arity: "1", alias: "-f"),
            new("filename", isPositional: true),
            new("action", isPositional: true),
        ];

        const string expected = """
                                Usage:
                                  prog [--help] [-v] [--foo FOO] filename action
                                """;

        var actual = new HelpBuilder(HelpConfiguration.Default())
            .AddUsage(args, prefix: "prog")
            .Build();

        Assert.Equal(expected, actual);
    }
}