using System;
using System.Collections.Generic;
using Karambolo.Common.Collections;

namespace Karambolo.Common
{
    public static partial class EnumerableUtils
    {
        public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<T, TKey, TElement>(this IEnumerable<T> source,
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new OrderedDictionary<TKey, TElement>(comparer);
            foreach (T item in source)
                result.Add(keySelector(item), elementSelector(item));

            return result;
        }

        public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<T, TKey, TElement>(this IEnumerable<T> source,
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
        {
            return ToOrderedDictionary(source, keySelector, elementSelector, null);
        }

        public static OrderedDictionary<TKey, T> ToOrderedDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return ToOrderedDictionary(source, keySelector, Identity<T>.Func, comparer);
        }

        public static OrderedDictionary<TKey, T> ToOrderedDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return ToOrderedDictionary(source, keySelector, Identity<T>.Func, null);
        }
    }
}
