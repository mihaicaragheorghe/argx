using System.Reflection;

using Argx.Errors;
using Argx.Subcommands;

namespace Argx.Tests;

public class CommandLineApplicationTests
{
    [Fact]
    public async Task RunAsync_ShouldInvokeHandlerWithRemainingArgs_WhenRegisteredAsyncHandler()
    {
        var app = new CommandLineApplication(new SubcommandStore());

        bool called = false;
        string[] captured = null!;
        AsyncSubcommandDelegate handler = args =>
        {
            called = true;
            captured = args;
            return Task.CompletedTask;
        };

        app.AddSubcommand("foo", handler);

        await app.RunAsyncImpl(["foo", "bar", "baz"]);

        Assert.True(called);
        Assert.Equal(["bar", "baz"], captured);
    }

    [Fact]
    public async Task RunAsync_ShouldWrapAndInvokeHandler_WhenRegisteredSyncHandler()
    {
        var app = new CommandLineApplication(new SubcommandStore());

        bool called = false;
        string[] captured = null!;
        SubcommandDelegate syncHandler = args =>
        {
            called = true;
            captured = args;
        };

        app.AddSubcommand("sync", syncHandler);

        await app.RunAsyncImpl(["sync", "x", "y", "z"]);

        Assert.True(called);
        Assert.Equal(["x", "y", "z"], captured);
    }

    [Fact]
    public async Task RunAsync_ShouldThrowUnknownSubcommandException_WhenUnknownSubcommand()
    {
        var app = new CommandLineApplication(new SubcommandStore());

        await Assert.ThrowsAsync<UnknownSubcommandException>(() => app.RunAsyncImpl(["missing"]));
    }

    [Fact]
    public async Task RunAsync_ShouldThrowNoSubcommandException_WhenNoArguments()
    {
        var app = new CommandLineApplication(new SubcommandStore());

        await Assert.ThrowsAsync<NoSubcommandException>(() => app.RunAsyncImpl([]));
    }

    [Fact]
    public async Task RunAsync_ShouldInvokeHelpSubcommand_WhenHelpRequested()
    {
        var app = new CommandLineApplication(name: "testapp");

        using var sw = new StringWriter();
        Console.SetOut(sw);

        await app.RunAsyncImpl(["help"]);

        var output = sw.ToString();
        Assert.Contains("testapp", output);
        Assert.Contains("Available subcommands", output);
        Assert.Contains("help", output);
    }

    [Fact]
    public void WriteHelp_ShouldIncludeNameSection_WhenNameNotNullOrEmpty()
    {
        var app = new CommandLineApplication(name: "myapp");
        var writer = new StringWriter();
        var expected =
        """
        myapp

        Usage
          myapp <subcommand> [<args>]

        Available subcommands
          help  Display help information about this application

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WriteHelp_ShouldNotIncludeNameSection_WhenNullOrEmpty(string? name)
    {
        var app = new CommandLineApplication(name: name);
        var prog = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        var writer = new StringWriter();
        var expected =
        $"""
        Usage
          {prog} <subcommand> [<args>]

        Available subcommands
          help  Display help information about this application

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldUseArgvZeroInUsage_WhenNameNullOrEmpty()
    {
        var app = new CommandLineApplication(name: null);
        var prog = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        var writer = new StringWriter();
        var expected =
        $"""
        Usage
          {prog} <subcommand> [<args>]

        Available subcommands
          help  Display help information about this application
        
        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldIncludeDescription_WhenNotNullOrEmpty()
    {
        var app = new CommandLineApplication(
            name: "myapp",
            description: "This is my application.");

        var writer = new StringWriter();
        var expected =
        """
        myapp
          This is my application.

        Usage
          myapp <subcommand> [<args>]

        Available subcommands
          help  Display help information about this application

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldIncludeUsage_WhenNotNullOrEmpty()
    {
        var app = new CommandLineApplication(
            name: "customapp",
            usage: "customapp [options] <subcommand> <args>");

        var writer = new StringWriter();
        var expected =
        """
        customapp

        Usage
          customapp [options] <subcommand> <args>

        Available subcommands
          help  Display help information about this application

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldBuildUsage_WhenNullOrEmpty()
    {
        var app = new CommandLineApplication(name: "defaultapp");

        var writer = new StringWriter();
        var expected =
        """
        defaultapp

        Usage
          defaultapp <subcommand> [<args>]

        Available subcommands
          help  Display help information about this application

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldIncludeEpilogue_WhenNotNullOrEmpty()
    {
        var app = new CommandLineApplication(
            name: "epilogueapp",
            epilogue: "For more information, visit our website.");

        var writer = new StringWriter();
        var expected =
        """
        epilogueapp

        Usage
          epilogueapp <subcommand> [<args>]

        Available subcommands
          help  Display help information about this application

        For more information, visit our website.

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldIncludeSubcommands_WhenRegistered()
    {
        var app = new CommandLineApplication(name: "multiapp");
        app.AddSubcommand("start", args => { })
            .WithUsage("Start the application");
        app.AddSubcommand("stop", args => { })
            .WithUsage("Stop the application");

        var writer = new StringWriter();
        var expected =
        """
        multiapp

        Usage
          multiapp <subcommand> [<args>]

        Available subcommands
          help   Display help information about this application
          start  Start the application
          stop   Stop the application

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }
}