using System.Collections;
namespace Argx.Utils;
internal static class CollectionUtils
{
    internal static IList CreateCollection(Type type, Type itemType, int capacity = 0)
    {
        if (itemType == typeof(string)) return Create<string>(type, capacity);
        if (itemType == typeof(bool)) return Create<bool>(type, capacity);
        if (itemType == typeof(int)) return Create<int>(type, capacity);
        if (itemType == typeof(long)) return Create<long>(type, capacity);
        if (itemType == typeof(short)) return Create<short>(type, capacity);
        if (itemType == typeof(uint)) return Create<uint>(type, capacity);
        if (itemType == typeof(ulong)) return Create<ulong>(type, capacity);
        if (itemType == typeof(ushort)) return Create<ushort>(type, capacity);
        if (itemType == typeof(decimal)) return Create<decimal>(type, capacity);
        if (itemType == typeof(double)) return Create<double>(type, capacity);
        if (itemType == typeof(float)) return Create<float>(type, capacity);
        if (itemType == typeof(Guid)) return Create<Guid>(type, capacity);
        if (itemType == typeof(DateTime)) return Create<DateTime>(type, capacity);
        if (itemType == typeof(DateTimeOffset)) return Create<DateTimeOffset>(type, capacity);
        if (itemType == typeof(TimeSpan)) return Create<TimeSpan>(type, capacity);
        if (itemType == typeof(bool?)) return Create<bool?>(type, capacity);
        if (itemType == typeof(int?)) return Create<int?>(type, capacity);
        if (itemType == typeof(long?)) return Create<long?>(type, capacity);
        if (itemType == typeof(short?)) return Create<short?>(type, capacity);
        if (itemType == typeof(uint?)) return Create<uint?>(type, capacity);
        if (itemType == typeof(ulong?)) return Create<ulong?>(type, capacity);
        if (itemType == typeof(ushort?)) return Create<ushort?>(type, capacity);
        if (itemType == typeof(decimal?)) return Create<decimal?>(type, capacity);
        if (itemType == typeof(double?)) return Create<double?>(type, capacity);
        if (itemType == typeof(float?)) return Create<float?>(type, capacity);
        if (itemType == typeof(Guid?)) return Create<Guid?>(type, capacity);
        if (itemType == typeof(DateTime?)) return Create<DateTime?>(type, capacity);
        if (itemType == typeof(DateTimeOffset?)) return Create<DateTimeOffset?>(type, capacity);
        if (itemType == typeof(TimeSpan?)) return Create<TimeSpan?>(type, capacity);
        throw new ArgumentException($"Type {type} cannot be created.");
    }
    private static IList Create<T>(Type type, int capacity)
    {
        if (type == typeof(T[]) || type == typeof(IEnumerable<T>) ||
            type == typeof(IList<T>) || type == typeof(ICollection<T>)) return new T[capacity];
        if (type == typeof(List<T>)) return new List<T>(capacity);
        throw new ArgumentException($"Type {type} cannot be created.");
    }
}
