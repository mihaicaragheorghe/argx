using Argx.Errors;
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
}