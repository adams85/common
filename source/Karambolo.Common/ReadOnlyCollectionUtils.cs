using System.Collections.Generic;

namespace Karambolo.Common
{
    public static class ReadOnlyCollectionUtils
    {
        public static bool IsNullOrEmpty<T>(IReadOnlyCollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
