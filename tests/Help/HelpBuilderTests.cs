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
}