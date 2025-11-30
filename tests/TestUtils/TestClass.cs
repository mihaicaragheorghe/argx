namespace Argx.Tests.TestUtils;

public class TestClass
{
    public bool Boolean { get; set; }
    public string String { get; set; } = null!;
    public int Int { get; set; }
    public long Long { get; set; }
    public short Short { get; set; }
    public uint UnsignedInt { get; set; }
    public ulong UnsignedLong { get; set; }
    public ushort UnsignedShort { get; set; }
    public float Float { get; set; }
    public double Double { get; set; }
    public decimal Decimal { get; set; }
    public Guid Guid { get; set; }
    public DateTime DateTime { get; set; }
    public TimeSpan TimeSpan { get; set; }

    public int[] IntArray { get; set; } = null!;

    public List<string> StringList { get; set; } = [];
}