using Argx.Actions;
using Argx.Binding;
using Argx.Parsing;
using Argx.Utils;

namespace Argx.Tests.Actions;

public class AppendActionCollectionTests
{
    [Theory]
    [InlineData(typeof(DayOfWeek[]))]
    [InlineData(typeof(List<DayOfWeek>))]
    [InlineData(typeof(IEnumerable<DayOfWeek>))]
    [InlineData(typeof(IList<DayOfWeek>))]
    [InlineData(typeof(ICollection<DayOfWeek>))]
    public void Execute_ShouldAppendEnumConstants_WithRegistration(Type type)
    {
        ArgumentCollectionFactories.Register<DayOfWeek>();
        var store = new ArgumentStore();
        var action = new AppendAction();
        var argument = new Argument("--monday", type: type, arity: "?", dest: "days",
            constValue: DayOfWeek.Monday);
        var invocation = new Token("--monday", TokenType.Option, 0);

        action.Validate(argument);
        action.Execute(argument, invocation, [], store);
        action.Execute(argument, invocation, [], store);

        Assert.True(store.TryGetValue<IEnumerable<DayOfWeek>>("days", out var days));
        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Monday], days);
    }

    [Fact]
    public void Register_ShouldCreateAllSupportedCollectionShapes()
    {
        ArgumentCollectionFactories.Register<CustomValue>();
        ArgumentCollectionFactories.Register<CustomValue>();

        Type[] types = [typeof(CustomValue[]), typeof(List<CustomValue>), typeof(IEnumerable<CustomValue>),
            typeof(IList<CustomValue>), typeof(ICollection<CustomValue>)];

        foreach (var type in types)
        {
            Assert.True(ArgumentCollectionFactories.TryCreate(type, 2, out var collection));
            Assert.True(type.IsInstanceOfType(collection));
            if (collection is CustomValue[] array)
            {
                Assert.Equal(2, array.Length);
            }
            else
            {
                Assert.Empty(collection);
            }
        }
    }

    [Fact]
    public void CreateCollection_ShouldRejectUnsupportedCollectionShapes()
    {
        ArgumentCollectionFactories.Register<CustomValue>();

        Assert.Throws<ArgumentException>(() =>
            CollectionUtils.CreateCollection(typeof(HashSet<CustomValue>), typeof(CustomValue)));
    }

    private sealed class CustomValue;
}
