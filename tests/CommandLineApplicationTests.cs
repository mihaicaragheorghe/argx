using Argx.Subcommands;

namespace Argx.Tests;

public class CommandLineApplicationTest
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

        await app.RunAsync(["foo", "bar", "baz"]);

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

        await app.RunAsync(["sync", "x", "y", "z"]);

        Assert.True(called);
        Assert.Equal(["x", "y", "z"], captured);
    }

    [Fact]
    public async Task RunAsync_ShouldThrowInvalidOperationException_WhenUnknownSubcommand()
    {
        var app = new CommandLineApplication(new SubcommandStore());

        await Assert.ThrowsAsync<InvalidOperationException>(() => app.RunAsync(["missing"]));
    }

    [Fact]
    public async Task RunAsync_ShouldThrowArgumentException_WhenNoArguments()
    {
        var app = new CommandLineApplication(new SubcommandStore());

        await Assert.ThrowsAsync<ArgumentException>(() => app.RunAsync([]));
    }
}