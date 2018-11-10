using System.Collections.Generic;

namespace Karambolo.Common
{
    public static class CollectionUtils
    {
        public static bool IsNullOrEmpty<T>(IReadOnlyCollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
