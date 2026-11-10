using Argx.Attributes;
using Argx.Binding;
using Argx.Extensions;
using Argx.Parsing;

namespace Argx.Tests.Extensions;

public class GeneratedBindingTests
{
    [Fact]
    public void BindsSiblingPathsAndPreservesDefaults()
    {
        var store = new ArgumentStore();
        store.Set("first-name", "one");
        store.Set("second-name", "two");
        store.Set("tail", 7);
        store.Set("renamed", 9);
        var date = DateTimeOffset.UtcNow;
        store.Set("date", date);
        var result = new Arguments(store).Get(EdgeBindings.Default.Siblings);
        Assert.Equal("one", result.First.Name);
        Assert.Equal("two", result.Second.Name);
        Assert.Equal(7, result.Tail);
        Assert.Equal(9, result.Renamed);
        Assert.Equal(11, result.DefaultValue);
        Assert.Equal(date, result.Date);
        Assert.Equal(0, result.Ignored);
    }

    [Fact]
    public void ResolvesRegisteredRuntimeSubtypesAndRejectsUnknownTypes()
    {
        var store = new ArgumentStore();
        store.Set("extra", 42);
        var args = new Arguments(store);
        BaseModel model = new DerivedModel();
        args.Bind(model, EdgeBindings.Default.BaseModel);
        Assert.Equal(42, ((DerivedModel)model).Extra);
        Assert.Throws<InvalidOperationException>(() => args.Bind<BaseModel>(new UnknownModel(), EdgeBindings.Default.BaseModel));
    }

    [Fact]
    public void RegistryIsIdempotentAndSupportsGenericHelpers()
    {
        EdgeBindings.Default.Register();
        EdgeBindings.Default.Register();
        var store = new ArgumentStore();
        store.Set("extra", 12);
        Assert.Equal(12, Create<DerivedModel>(new Arguments(store)).Extra);
        var registry = new ArgumentBindingRegistry();
        registry.Register(EdgeBindings.Default.BaseModel);
        Assert.Throws<InvalidOperationException>(() => registry.Register(new ArgumentBinding<BaseModel>((a,b,c,d) => {}, () => new(), registry)));
        Assert.Throws<InvalidOperationException>(() => registry.Get<UnknownModel>());
    }
    private static T Create<T>(IArguments args) where T : class, new() => args.Get<T>();
}

public class Siblings
{
    public NamedModel First { get; set; } = null!;
    public NamedModel Second { get; set; } = null!;
    public int Tail { get; set; }
    [Argument("renamed")] public int Renamed { get; set; }
    public int DefaultValue { get; set; } = 11;
    public DateTimeOffset Date { get; set; }
    [Ignore] public int Ignored { get; set; }
}
public class NamedModel { public string Name { get; set; } = ""; }
public class BaseModel { public int Value { get; set; } }
public class DerivedModel : BaseModel { public int Extra { get; set; } }
public class UnknownModel : BaseModel { }
[ArgxBinding(typeof(Siblings))]
[ArgxBinding(typeof(BaseModel))]
[ArgxBinding(typeof(DerivedModel))]
internal partial class EdgeBindings : ArgumentBindingContext { }
