using Argx.Subcommands;

namespace Argx.Tests.Subcommands;

public class SubcommandDelegateFactoryTest
{
    [Fact]
    public void Create_ShouldReturnSameInstance_WhenAsyncSubcommandDelegate()
    {
        bool called = false;
        AsyncSubcommandDelegate asyncHandler = args =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var created = SubcommandDelegateFactory.Create(asyncHandler);

        Assert.Same(asyncHandler, created);
        var task = created?.Invoke(null!);
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
        Assert.True(called);
    }

    [Fact]
    public async Task Create_ShouldWrapAndInvokesSyncHandler_WithSubcommandDelegate()
    {
        bool called = false;
        SubcommandDelegate syncHandler = args => called = true;

        var wrapper = SubcommandDelegateFactory.Create(syncHandler);
        var task = wrapper?.Invoke(null!);

        await task!;

        Assert.True(called);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenUnsupportedDelegate()
    {
        Func<int> other = () => 42;
        Assert.Throws<ArgumentException>(() => SubcommandDelegateFactory.Create(other));
    }
}