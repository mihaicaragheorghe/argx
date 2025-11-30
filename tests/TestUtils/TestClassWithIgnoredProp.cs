using Argx.Attributes;

namespace Argx.Tests.TestUtils;

public class TestClassWithIgnore
{
    public string Included { get; set; } = null!;

    [Ignore]
    public string Ignored { get; set; } = null!;

    public string NonWriteable { get; } = "cannot be set";
}