using Argx.Attributes;

namespace Argx.Tests.TestUtils;

public class TestClassWithArgumentAttribute
{
    [Argument("foo")]
    public string CustomNamedProperty { get; set; } = null!;

    public int Bar { get; set; }
}