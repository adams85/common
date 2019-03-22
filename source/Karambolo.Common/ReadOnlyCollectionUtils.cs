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
    }
}
