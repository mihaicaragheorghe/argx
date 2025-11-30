namespace Argx.Tests.TestUtils;

public class RootOptions
{
    public string Name { get; set; } = null!;
    public int Count { get; set; }
    public ChildOptions Child { get; set; } = null!;
}

public class ChildOptions
{
    public string Label { get; set; } = null!;
    public int Value { get; set; }
    public GrandChildOptions GrandChild { get; set; } = null!;
}

public class GrandChildOptions
{
    public bool Flag { get; set; }
}

public class RootOptionsWithoutParameterlessConstructorChild
{
    public string Name { get; set; } = null!;
    public int Count { get; set; }
    public ChildOptionsWithoutParameterlessConstructor Child { get; set; } = null!;
}

public class ChildOptionsWithoutParameterlessConstructor(string label)
{
    public string Label { get; set; } = label;
    public int Value { get; set; }
    public GrandChildOptions GrandChild { get; set; } = null!;
}