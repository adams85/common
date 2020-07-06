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
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
