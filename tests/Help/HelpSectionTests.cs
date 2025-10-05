using Argx.Help;

namespace Argx.Tests.Help;

public class HelpSectionTests
{
    [Fact]
    public void AddChild_ShouldAddChild()
    {
        var root = new HelpSection("root");
        var child = new HelpSection("child");
        root.AddChild(child);
        Assert.Contains(child, root.GetChildren());
    }

    [Fact]
    public void AppendText_ShouldAppendText_WhenContentIsEmpty()
    {
        var section = new HelpSection("test");
        section.AppendText("foo");
        Assert.Equal("foo", section.Content);
    }

    [Fact]
    public void AppendText_ShouldAppendText_WhenContentIsNotEmpty()
    {
        var section = new HelpSection("test", "foo");
        section.AppendText(" bar");
        Assert.Equal("foo bar", section.Content);
    }

    [Fact]
    public void AppendLine_ShouldAppendLine_WhenContentIsNotEmpty()
    {
        var section = new HelpSection("test", "foo");
        section.AppendLine("bar");
        Assert.Equal($"foo{Environment.NewLine}bar", section.Content);
    }

    [Fact]
    public void AppendColumns_ShouldAppendColumns()
    {
        var section = new HelpSection("test");
        section.AppendColumns([new TwoColumnRow("foo", "bar")]);
        Assert.Equal("foo  bar", section.Content);
    }

    [Fact]
    public void AppendColumns_ShouldWrapRightColumns_WhenOverMaxWidth()
    {
        var section = new HelpSection("test", maxLineWidth: 20);
        const string expected = """
                                foo  Lorem ipsum
                                     dolor sit amet
                                """;

        section.AppendColumns([new TwoColumnRow("foo", "Lorem ipsum dolor sit amet")]);

        Assert.Equal(expected, section.Content);
    }

    [Fact]
    public void AppendColumns_ShouldAppendColumns_WhenLeftOverMaxWidth()
    {
        var section = new HelpSection("test", maxLineWidth: 5);
        const string expected = """
                                --foo, -f  Lorem
                                           ipsum
                                           dolor
                                           sit
                                           amet
                                """;

        section.AppendColumns([new TwoColumnRow("--foo, -f", "Lorem ipsum dolor sit amet")]);

        Assert.Equal(expected, section.Content);
    }

    [Fact]
    public void Render_ShouldNotWriteTitle_WhenTitleIsEmpty()
    {
        var section = new HelpSection(string.Empty, "foo");
        var result = section.Render();
        Assert.Equal("foo", result);
    }

    [Fact]
    public void Render_ShouldWriteTitle_WhenTitleIsNotEmpty()
    {
        var section = new HelpSection("foo");
        var result = section.Render();
        Assert.Equal("foo:", result);
    }

    [Fact]
    public void Render_ShouldNotWriteContent_WhenContentIsEmpty()
    {
        var section = new HelpSection("");
        var result = section.Render();
        Assert.Empty(result);
    }

    [Fact]
    public void Render_ShouldWriteContent_WhenContentIsNotEmpty()
    {
        var section = new HelpSection("foo", "bar");
        var result = section.Render();
        const string expected = """
                                foo:
                                  bar
                                """;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Render_ShouldRenderChildren_WhenHasChildren()
    {
        var root = new HelpSection("root", "root content");
        root.AddChild(new HelpSection("foo", "foo content"));
        root.AddChild(new HelpSection("bar", "bar content"));
        const string expected =
            """
            root:
              root content

              foo:
                foo content
              bar:
                bar content
            """;

        var result = root.Render();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Render_ShouldRenderNestedChildren_WhenChildrenHaveChildren()
    {
        var root = new HelpSection("root", "root content");
        var foo = new HelpSection("foo", "foo content");
        var bar = new HelpSection("bar", "bar content");
        root.AddChild(foo);
        foo.AddChild(bar);
        const string expected =
            """
            root:
              root content

              foo:
                foo content

                bar:
                  bar content
            """;

        var result = root.Render();

        Assert.Equal(expected, result);
    }
}