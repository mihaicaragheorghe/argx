using System.Collections;

using Argx.Binding;
using Argx.Parsing;

namespace Argx.Tests.Binding;

public class CollectionFactoryRegistryTests
{
    [Theory]
    [InlineData(typeof(DayOfWeek[]))]
    [InlineData(typeof(IEnumerable<DayOfWeek>))]
    [InlineData(typeof(IList<DayOfWeek>))]
    [InlineData(typeof(ICollection<DayOfWeek>))]
    [InlineData(typeof(List<DayOfWeek>))]
    public void Register_ShouldCreateSupportedCollectionShapes(Type type)
    {
        var registry = new CollectionFactoryRegistry();
        registry.Register<DayOfWeek>();

        var collection = registry.GetFactory(type)(2);

        Assert.True(type.IsInstanceOfType(collection));
        if (collection is Array)
        {
            Assert.Equal(2, collection.Count);
            collection[0] = DayOfWeek.Monday;
        }
        else
        {
            Assert.Empty(collection);
            collection.Add(DayOfWeek.Monday);
        }

        Assert.Equal(DayOfWeek.Monday, collection[0]);
    }

    [Fact]
    public void Register_ShouldAllowRepeatedRegistration()
    {
        var registry = new CollectionFactoryRegistry();
        registry.Register<DayOfWeek>();
        registry.Register<DayOfWeek>();

        Assert.IsType<DayOfWeek[]>(registry.GetFactory(typeof(DayOfWeek[]))(1));
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(int))]
    [InlineData(typeof(long))]
    [InlineData(typeof(short))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(double))]
    [InlineData(typeof(float))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(TimeSpan))]
    [InlineData(typeof(bool?))]
    [InlineData(typeof(int?))]
    [InlineData(typeof(long?))]
    [InlineData(typeof(short?))]
    [InlineData(typeof(uint?))]
    [InlineData(typeof(ulong?))]
    [InlineData(typeof(ushort?))]
    [InlineData(typeof(decimal?))]
    [InlineData(typeof(double?))]
    [InlineData(typeof(float?))]
    [InlineData(typeof(Guid?))]
    [InlineData(typeof(DateTime?))]
    [InlineData(typeof(DateTimeOffset?))]
    [InlineData(typeof(TimeSpan?))]
    public void Constructor_ShouldRegisterBuiltInElementTypes(Type elementType)
    {
        var registry = new CollectionFactoryRegistry();
        var type = elementType.MakeArrayType();

        IList collection = registry.GetFactory(type)(2);

        Assert.Equal(type, collection.GetType());
        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void ConvertTokens_ShouldUseRegisteredFactory_ForCustomCollections()
    {
        var registry = new CollectionFactoryRegistry();
        registry.Register<DayOfWeek>();

        var result = TokenConverter.ConvertTokens(typeof(List<DayOfWeek>), ReadOnlySpan<Token>.Empty,
            registry.GetFactory(typeof(List<DayOfWeek>)));

        Assert.False(result.IsError);
        Assert.Empty(Assert.IsType<List<DayOfWeek>>(result.Value));
    }

    [Fact]
    public void GetFactory_ShouldRejectUnsupportedCollectionShapes()
    {
        var registry = new CollectionFactoryRegistry();
        registry.Register<DayOfWeek>();

        Assert.Throws<ArgumentException>(() => registry.GetFactory(typeof(HashSet<DayOfWeek>)));
    }
}
