using Argx.Actions;
using Argx.Parsing;

namespace Argx.Tests.Actions;

public class ActionRegistryTests
{
    [Theory]
    [InlineData(null, typeof(StoreAction))]
    [InlineData(ArgumentActions.Store, typeof(StoreAction))]
    [InlineData(ArgumentActions.StoreTrue, typeof(StoreTrueAction))]
    [InlineData(ArgumentActions.StoreFalse, typeof(StoreFalseAction))]
    [InlineData(ArgumentActions.StoreConst, typeof(StoreConstAction))]
    [InlineData(ArgumentActions.Choice, typeof(ChoiceAction))]
    [InlineData(ArgumentActions.Count, typeof(CountAction))]
    [InlineData(ArgumentActions.Append, typeof(AppendAction))]
    public void Registry_ShouldContainBuiltinValues_WhenNotExplicitlySet(string? action, Type expectedType)
    {
        var success = ActionRegistry.TryGetHandler(action, out var handler);

        Assert.True(success);
        Assert.IsType(expectedType, handler);
    }

    [Fact]
    public void TryGetHandler_ShouldReturnFalse_WhenActionNotRegistered()
    {
        var success = ActionRegistry.TryGetHandler("bad_name", out var handler);

        Assert.False(success);
        Assert.Null(handler);
    }

    [Fact]
    public void Add_ShouldAddActionToRegistry_WhenActionImplementsArgumentAction()
    {
        const string action = "custom";
        ActionRegistry.Add(action, new CustomAction());

        var success = ActionRegistry.TryGetHandler(action, out var handler);

        Assert.True(success);
        Assert.IsType<CustomAction>(handler);
    }

    private class CustomAction : ArgumentAction
    {
        public override void Execute(Argument argument, ArgumentStore store, string dest, ReadOnlySpan<Token> values)
        {
            throw new NotImplementedException();
        }
    }
}