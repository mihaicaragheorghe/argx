using System.Collections;

namespace Argx.Utils;

internal static class CollectionUtils
{
    internal static IList CreateCollection(Type type, Type itemType, int capacity = 0)
    {
        if (type.IsArray)
        {
            return CreateArray(itemType, capacity);
        }

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();

            if (genericType == typeof(IEnumerable<>) ||
                genericType == typeof(IList<>) ||
                genericType == typeof(ICollection<>))
            {
                return CreateArray(itemType, capacity);
            }

            if (genericType == typeof(List<>))
            {
                return CreateList(type);
            }
        }

        throw new ArgumentException($"Type {type} cannot be created.");
    }

    private static Array CreateArray(Type itemType, int capacity) => Array.CreateInstance(itemType, capacity);

    private static IList CreateList(Type listType) => (IList)Activator.CreateInstance(listType)!;
}