using Argx.Parsing;
using Argx.Tests.TestUtils;
using Argx.Extensions;

using Moq;

namespace Argx.Tests.Extensions;

public class ArgumentsExtensionsTests
{
    private readonly Mock<IArgumentStore> _storeMock = new();

    public ArgumentsExtensionsTests()
    {
        _storeMock = new();
    }

    [Fact]
    public void Bind_ShouldThrowArgumentNullException_WhenInstanceIsNull()
    {
        var sut = new Arguments(_storeMock.Object);

        Assert.Throws<ArgumentNullException>(() => sut.Bind<TestClass>(null!));
    }

    [Fact]
    public void Bind_ShouldSetProperties_OnInstance()
    {
        var boolean = true;
        var str = "foo";
        int intVal = 42;
        long longVal = 1234567890L;
        short shortVal = 7;
        uint uintVal = 24;
        ulong ulongVal = 9876543210UL;
        ushort ushortVal = 14;
        float floatVal = 3.14f;
        double doubleVal = 2.71828;
        decimal decimalVal = 1.6180339887m;
        Guid guidVal = Guid.NewGuid();
        DateTime dateTimeVal = DateTime.Now;
        TimeSpan timeSpanVal = TimeSpan.FromHours(1);
        List<string> stringList = ["tag1", "tag2"];
        int[] intArray = [1, 2, 3, 4, 5];

        var store = new ArgumentStore();
        store.Set("boolean", boolean);
        store.Set("string", str);
        store.Set("int", intVal);
        store.Set("long", longVal);
        store.Set("short", shortVal);
        store.Set("unsigned-int", uintVal);
        store.Set("unsigned-long", ulongVal);
        store.Set("unsigned-short", ushortVal);
        store.Set("float", floatVal);
        store.Set("double", doubleVal);
        store.Set("decimal", decimalVal);
        store.Set("guid", guidVal);
        store.Set("date-time", dateTimeVal);
        store.Set("time-span", timeSpanVal);
        store.Set("string-list", stringList);
        store.Set("int-array", intArray);

        var testClass = new TestClass();
        var sut = new Arguments(store);

        sut.Bind(testClass);

        Assert.Equal(boolean, testClass.Boolean);
        Assert.Equal(str, testClass.String);
        Assert.Equal(intVal, testClass.Int);
        Assert.Equal(longVal, testClass.Long);
        Assert.Equal(shortVal, testClass.Short);
        Assert.Equal(uintVal, testClass.UnsignedInt);
        Assert.Equal(ulongVal, testClass.UnsignedLong);
        Assert.Equal(ushortVal, testClass.UnsignedShort);
        Assert.Equal(floatVal, testClass.Float);
        Assert.Equal(doubleVal, testClass.Double);
        Assert.Equal(decimalVal, testClass.Decimal);
        Assert.Equal(guidVal, testClass.Guid);
        Assert.Equal(dateTimeVal, testClass.DateTime);
        Assert.Equal(timeSpanVal, testClass.TimeSpan);
        Assert.Equal(stringList, testClass.StringList);
        Assert.Equal(intArray, testClass.IntArray);
    }

    [Fact]
    public void Bind_ShouldSetNullableProperties_OnInstance()
    {
        var boolean = true;
        var str = "foo";
        int intVal = 42;
        long longVal = 1234567890L;
        short shortVal = 7;
        uint uintVal = 24;
        ulong ulongVal = 9876543210UL;
        ushort ushortVal = 14;
        float floatVal = 3.14f;
        double doubleVal = 2.71828;
        decimal decimalVal = 1.6180339887m;
        Guid guidVal = Guid.NewGuid();
        DateTime dateTimeVal = DateTime.Now;
        TimeSpan timeSpanVal = TimeSpan.FromHours(1);

        var store = new ArgumentStore();
        store.Set("boolean", boolean);
        store.Set("string", str);
        store.Set("int", intVal);
        store.Set("long", longVal);
        store.Set("short", shortVal);
        store.Set("unsigned-int", uintVal);
        store.Set("unsigned-long", ulongVal);
        store.Set("unsigned-short", ushortVal);
        store.Set("float", floatVal);
        store.Set("double", doubleVal);
        store.Set("decimal", decimalVal);
        store.Set("guid", guidVal);
        store.Set("date-time", dateTimeVal);
        store.Set("time-span", timeSpanVal);

        var testClass = new TestClassWithNullables();
        var sut = new Arguments(store);

        sut.Bind(testClass);

        Assert.Equal(boolean, testClass.Boolean);
        Assert.Equal(str, testClass.String);
        Assert.Equal(intVal, testClass.Int);
        Assert.Equal(longVal, testClass.Long);
        Assert.Equal(shortVal, testClass.Short);
        Assert.Equal(uintVal, testClass.UnsignedInt);
        Assert.Equal(ulongVal, testClass.UnsignedLong);
        Assert.Equal(ushortVal, testClass.UnsignedShort);
        Assert.Equal(floatVal, testClass.Float);
        Assert.Equal(doubleVal, testClass.Double);
        Assert.Equal(decimalVal, testClass.Decimal);
        Assert.Equal(guidVal, testClass.Guid);
        Assert.Equal(dateTimeVal, testClass.DateTime);
        Assert.Equal(timeSpanVal, testClass.TimeSpan);
    }

    [Fact]
    public void Bind_ShouldIgnoreMissingProperties_OnInstance()
    {
        var store = new ArgumentStore();

        var testClass = new TestClass();
        var sut = new Arguments(store);

        sut.Bind(testClass);

        Assert.False(testClass.Boolean);
        Assert.Null(testClass.String);
        Assert.Equal(0, testClass.Int);
        Assert.Equal(0L, testClass.Long);
        Assert.Equal((short)0, testClass.Short);
        Assert.Equal(0u, testClass.UnsignedInt);
        Assert.Equal(0UL, testClass.UnsignedLong);
        Assert.Equal((ushort)0, testClass.UnsignedShort);
        Assert.Equal(0f, testClass.Float);
        Assert.Equal(0.0, testClass.Double);
        Assert.Equal(0m, testClass.Decimal);
        Assert.Equal(Guid.Empty, testClass.Guid);
        Assert.Equal(DateTime.MinValue, testClass.DateTime);
        Assert.Equal(TimeSpan.Zero, testClass.TimeSpan);
        Assert.Empty(testClass.StringList);
        Assert.Null(testClass.IntArray);
    }

    [Fact]
    public void Bind_ShouldIgnorePropertiesWithIgnoreAttribute_OnInstance()
    {
        var str = "included";
        var ignoredStr = "ignored";

        var store = new ArgumentStore();
        store.Set("included", str);
        store.Set("ignored", ignoredStr);

        var testClass = new TestClassWithIgnore();
        var sut = new Arguments(store);

        sut.Bind(testClass);

        Assert.Equal(str, testClass.Included);
        Assert.NotEqual(ignoredStr, testClass.Ignored);
        Assert.Null(testClass.Ignored);
    }

    [Fact]
    public void Bind_ShouldIgnoreNonWriteableProperties_OnInstance()
    {
        var store = new ArgumentStore();
        store.Set("non-writable", "some value");

        var testClass = new TestClassWithIgnore();
        var sut = new Arguments(store);

        sut.Bind(testClass);

        Assert.Equal("cannot be set", testClass.NonWriteable);
    }

    [Fact]
    public void Bind_ShouldUseArgumentAttributeName_WhenPresent()
    {
        var testClass = new TestClassWithArgumentAttribute();
        var store = new ArgumentStore();
        store.Set("foo", "bar");

        var sut = new Arguments(store);

        sut.Bind(testClass);

        Assert.Equal("bar", testClass.CustomNamedProperty);
    }

    [Fact]
    public void Bind_ShouldBindNestedObjects_OnInstance()
    {
        var rootOptions = new RootOptions
        {
            Child = new ChildOptions
            {
                GrandChild = new GrandChildOptions()
            }
        };
        var store = new ArgumentStore();
        var sut = new Arguments(store);
        store.Set("name", "root");
        store.Set("count", 10);
        store.Set("child-label", "child");
        store.Set("child-value", 20);
        store.Set("child-grand-child-flag", true);

        sut.Bind(rootOptions);

        Assert.Equal("root", rootOptions.Name);
        Assert.Equal(10, rootOptions.Count);
        Assert.NotNull(rootOptions.Child);
        Assert.Equal("child", rootOptions.Child.Label);
        Assert.Equal(20, rootOptions.Child.Value);
        Assert.NotNull(rootOptions.Child.GrandChild);
        Assert.True(rootOptions.Child.GrandChild.Flag);
    }

    [Fact]
    public void Bind_ShouldCreateNestedObjects_WhenNull()
    {
        var rootOptions = new RootOptions
        {
            Child = null!
        };
        var store = new ArgumentStore();
        var sut = new Arguments(store);
        store.Set("name", "root");
        store.Set("count", 10);
        store.Set("child-label", "child");
        store.Set("child-value", 20);
        store.Set("child-grand-child-flag", true);

        sut.Bind(rootOptions);

        Assert.Equal("root", rootOptions.Name);
        Assert.Equal(10, rootOptions.Count);
        Assert.NotNull(rootOptions.Child);
        Assert.Equal("child", rootOptions.Child.Label);
        Assert.Equal(20, rootOptions.Child.Value);
        Assert.NotNull(rootOptions.Child.GrandChild);
        Assert.True(rootOptions.Child.GrandChild.Flag);
    }

    [Fact]
    public void Bind_ShouldThrow_WhenNestedObjectNullAndNoParameterlessConstructor()
    {
        var rootOptions = new RootOptionsWithoutParameterlessConstructorChild
        {
            Child = null!
        };
        var store = new ArgumentStore();
        var sut = new Arguments(store);
        store.Set("name", "root");
        store.Set("count", 10);

        Assert.Throws<MissingMethodException>(() => sut.Bind(rootOptions));
    }

    [Fact]
    public void Get_ShouldCreateAndBindInstance()
    {
        var boolean = true;
        var str = "foo";
        int intVal = 42;
        long longVal = 1234567890L;
        short shortVal = 7;
        uint uintVal = 24;
        ulong ulongVal = 9876543210UL;
        ushort ushortVal = 14;
        float floatVal = 3.14f;
        double doubleVal = 2.71828;
        decimal decimalVal = 1.6180339887m;
        Guid guidVal = Guid.NewGuid();
        DateTime dateTimeVal = DateTime.Now;
        TimeSpan timeSpanVal = TimeSpan.FromHours(1);
        List<string> stringList = ["tag1", "tag2"];
        int[] intArray = [1, 2, 3, 4, 5];

        var store = new ArgumentStore();
        store.Set("boolean", boolean);
        store.Set("string", str);
        store.Set("int", intVal);
        store.Set("long", longVal);
        store.Set("short", shortVal);
        store.Set("unsigned-int", uintVal);
        store.Set("unsigned-long", ulongVal);
        store.Set("unsigned-short", ushortVal);
        store.Set("float", floatVal);
        store.Set("double", doubleVal);
        store.Set("decimal", decimalVal);
        store.Set("guid", guidVal);
        store.Set("date-time", dateTimeVal);
        store.Set("time-span", timeSpanVal);
        store.Set("string-list", stringList);
        store.Set("int-array", intArray);

        var sut = new Arguments(store);

        var created = sut.Get<TestClass>();

        Assert.Equal(boolean, created.Boolean);
        Assert.Equal(str, created.String);
        Assert.Equal(intVal, created.Int);
        Assert.Equal(longVal, created.Long);
        Assert.Equal(shortVal, created.Short);
        Assert.Equal(uintVal, created.UnsignedInt);
        Assert.Equal(ulongVal, created.UnsignedLong);
        Assert.Equal(ushortVal, created.UnsignedShort);
        Assert.Equal(floatVal, created.Float);
        Assert.Equal(doubleVal, created.Double);
        Assert.Equal(decimalVal, created.Decimal);
        Assert.Equal(guidVal, created.Guid);
        Assert.Equal(dateTimeVal, created.DateTime);
        Assert.Equal(timeSpanVal, created.TimeSpan);
        Assert.Equal(stringList, created.StringList);
        Assert.Equal(intArray, created.IntArray);
    }

    [Fact]
    public void Get_ShouldCreateInstanceWithDefaultValues_WhenNoValuesStored()
    {
        var store = new ArgumentStore();
        var sut = new Arguments(store);

        var created = sut.Get<TestClass>();

        Assert.False(created.Boolean);
        Assert.Null(created.String);
        Assert.Equal(0, created.Int);
        Assert.Equal(0L, created.Long);
        Assert.Equal((short)0, created.Short);
        Assert.Equal(0u, created.UnsignedInt);
        Assert.Equal(0UL, created.UnsignedLong);
        Assert.Equal((ushort)0, created.UnsignedShort);
        Assert.Equal(0f, created.Float);
        Assert.Equal(0.0, created.Double);
        Assert.Equal(0m, created.Decimal);
        Assert.Equal(Guid.Empty, created.Guid);
        Assert.Equal(DateTime.MinValue, created.DateTime);
        Assert.Equal(TimeSpan.Zero, created.TimeSpan);
        Assert.Empty(created.StringList);
        Assert.Null(created.IntArray);
    }

    [Fact]
    public void Get_ShouldIgnorePropertiesWithIgnoreAttribute_OnCreatedInstance()
    {
        var str = "included";
        var ignoredStr = "ignored";

        var store = new ArgumentStore();
        store.Set("included", str);
        store.Set("ignored", ignoredStr);

        var sut = new Arguments(store);

        var created = sut.Get<TestClassWithIgnore>();

        Assert.Equal(str, created.Included);
        Assert.NotEqual(ignoredStr, created.Ignored);
        Assert.Null(created.Ignored);
    }

    [Fact]
    public void Get_ShouldUseArgumentAttributeName_WhenPresent_OnCreatedInstance()
    {
        var store = new ArgumentStore();
        store.Set("foo", "bar");

        var sut = new Arguments(store);

        var created = sut.Get<TestClassWithArgumentAttribute>();

        Assert.Equal("bar", created.CustomNamedProperty);
    }

    [Fact]
    public void Get_ShouldBindNestedObjects_OnInstance()
    {
        var store = new ArgumentStore();
        var sut = new Arguments(store);
        store.Set("name", "root");
        store.Set("count", 10);
        store.Set("child-label", "child");
        store.Set("child-value", 20);
        store.Set("child-grand-child-flag", true);

        var result = sut.Get<RootOptions>();

        Assert.Equal("root", result.Name);
        Assert.Equal(10, result.Count);
        Assert.NotNull(result.Child);
        Assert.Equal("child", result.Child.Label);
        Assert.Equal(20, result.Child.Value);
        Assert.NotNull(result.Child.GrandChild);
        Assert.True(result.Child.GrandChild.Flag);
    }

    [Fact]
    public void Get_ShouldThrow_WhenNestedObjectNullAndNoParameterlessConstructor()
    {
        var store = new ArgumentStore();
        var sut = new Arguments(store);
        store.Set("name", "root");
        store.Set("count", 10);
        store.Set("child-label", "child");

        Assert.Throws<MissingMethodException>(() => sut.Get<RootOptionsWithoutParameterlessConstructorChild>());
    }
}