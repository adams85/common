using System;
using System.Collections.Generic;
using System.Linq;

namespace Karambolo.Common
{
#if NET40
    using Karambolo.Common.Collections;
#endif

    public static class ReadOnlyCollectionUtils
    {
        public static bool IsNullOrEmpty<T>(IReadOnlyCollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static bool Contains<T>(this IReadOnlyCollection<T> collection, T item)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            return collection is ICollection<T> collectionIntf ? collectionIntf.Contains(item) : Enumerable.Contains(collection, item);
        }
    }
}
