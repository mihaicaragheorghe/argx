using Argx.Attributes;
using Argx.Binding;
namespace Argx.Tests.TestUtils;
[ArgxBinding(typeof(TestClass))]
[ArgxBinding(typeof(TestClassWithNullables))]
[ArgxBinding(typeof(TestClassWithIgnore))]
[ArgxBinding(typeof(TestClassWithArgumentAttribute))]
[ArgxBinding(typeof(RootOptions))]
[ArgxBinding(typeof(RootOptionsWithoutParameterlessConstructorChild))]
internal partial class TestBindings : ArgumentBindingContext { }
