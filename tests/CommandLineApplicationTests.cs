using System.Reflection;

using Argx.Abstractions;
using Argx.Errors;
using Argx.Subcommands;

using Moq;

namespace Argx.Tests;

[Collection("HelpTests")]
public class CommandLineApplicationTests
{
    private readonly Mock<IEnvironment> _mockEnvironment = new();

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
        Assert.Contains("Available commands", output);
        Assert.Contains("help", output);
    }

    [Fact]
    public async Task RunAsync_ShouldExitWithCode1_WhenUnknownSubcommandAndExitOnError()
    {
        var app = new CommandLineApplication(
            new SubcommandStore(),
            _mockEnvironment.Object,
            name: "testapp",
            exitOnError: true);

        using var sw = new StringWriter();
        Console.SetOut(sw);

        await app.RunAsync(["foo"]);

        var output = sw.ToString();
        Assert.Contains("testapp: Unknown subcommand: foo. See 'help' for a list of available commands.", output);
        _mockEnvironment.Verify(env => env.Exit(1), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldThrowUnknownSubcommandException_WhenUnknownSubcommandAndNotExitOnError()
    {
        var app = new CommandLineApplication(
            new SubcommandStore(),
            name: "testapp",
            exitOnError: false);

        await Assert.ThrowsAsync<UnknownSubcommandException>(() => app.RunAsync(["unknown"]));
        _mockEnvironment.Verify(env => env.Exit(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ShouldExitWithCode1_WhenNoSubcommandAndExitOnError()
    {
        var app = new CommandLineApplication(
            new SubcommandStore(),
            _mockEnvironment.Object,
            name: "testapp",
            exitOnError: true);

        using var helpWriter = new StringWriter();
        app.WriteHelp(helpWriter);
        using var sw = new StringWriter();
        Console.SetOut(sw);

        await app.RunAsync([]);

        Assert.Equal(helpWriter.ToString(), sw.ToString());
        _mockEnvironment.Verify(env => env.Exit(1), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ShouldThrowNoSubcommandException_WhenNoSubcommandAndNotExitOnError()
    {
        var app = new CommandLineApplication(
            new SubcommandStore(),
            name: "testapp",
            exitOnError: false);

        await Assert.ThrowsAsync<NoSubcommandException>(() => app.RunAsync([]));
        _mockEnvironment.Verify(env => env.Exit(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void WriteHelp_ShouldIncludeNameSection_WhenNameNotNullOrEmpty()
    {
        var app = new CommandLineApplication(name: "myapp");
        using var writer = new StringWriter();
        var expected =
        """
        myapp

        Usage
          myapp <command> [<arguments>]

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.

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
        using var writer = new StringWriter();
        var expected =
        $"""
        Usage
          {prog} <command> [<arguments>]

        Available commands
          help  Display help information about this application
        
        Use '<command> --help' to get more information about a specific command.

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldUseArgvZeroInUsage_WhenNameNullOrEmpty()
    {
        var app = new CommandLineApplication(name: null);
        var prog = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        using var writer = new StringWriter();
        var expected =
        $"""
        Usage
          {prog} <command> [<arguments>]

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.
        
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

        using var writer = new StringWriter();
        var expected =
        """
        myapp
          This is my application.

        Usage
          myapp <command> [<arguments>]

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldIncludeOnlyDescription_WhenNameNullOrEmpty()
    {
        var app = new CommandLineApplication(
            name: null,
            description: "This is my application.");

        var prog = Path.GetFileName(Assembly.GetEntryAssembly()?.Location);
        using var writer = new StringWriter();
        var expected =
        $"""
        This is my application.

        Usage
          {prog} <command> [<arguments>]

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldIncludeUsage_WhenNotNullOrEmpty()
    {
        var app = new CommandLineApplication(
            name: "customapp",
            usage: "customapp [options] <command> <args>");

        using var writer = new StringWriter();
        var expected =
        """
        customapp

        Usage
          customapp [options] <command> <args>

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void WriteHelp_ShouldBuildUsage_WhenNullOrEmpty()
    {
        var app = new CommandLineApplication(name: "defaultapp");

        using var writer = new StringWriter();
        var expected =
        """
        defaultapp

        Usage
          defaultapp <command> [<arguments>]

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.

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

        using var writer = new StringWriter();
        var expected =
        """
        epilogueapp

        Usage
          epilogueapp <command> [<arguments>]

        Available commands
          help  Display help information about this application

        Use '<command> --help' to get more information about a specific command.

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

        using var writer = new StringWriter();
        var expected =
        """
        multiapp

        Usage
          multiapp <command> [<arguments>]

        Available commands
          help   Display help information about this application
          start  Start the application
          stop   Stop the application

        Use '<command> --help' to get more information about a specific command.

        """;

        app.WriteHelp(writer);

        Assert.Equal(expected, writer.ToString());
    }
}