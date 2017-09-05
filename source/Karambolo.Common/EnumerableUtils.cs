using System;
using System.Collections.Generic;
using System.Linq;
using Karambolo.Common.Collections;

namespace Karambolo.Common
{
    public static class EnumerableUtils
    {
        public static IEnumerable<T> FromElement<T>(T element)
        {
            return ArrayUtils.FromElement(element);
        }

        public static IEnumerable<T> FromElement<T>(T element, bool nullAsEmpty)
        {
            return element != null ? FromElement(element) : ArrayUtils.Empty<T>();
        }

        public static IEnumerable<T> FromElements<T>(params T[] elements)
        {
            return ArrayUtils.FromElements(elements);
        }

        public static IEnumerable<T> WithHead<T>(this IEnumerable<T> source, T element)
        {
            if (source == null)
                throw new NullReferenceException();

            yield return element;
            foreach (var e in source)
                yield return e;
        }

        public static IEnumerable<T> WithTail<T>(this IEnumerable<T> source, T element)
        {
            if (source == null)
                throw new NullReferenceException();

            foreach (var e in source)
                yield return e;
            yield return element;
        }

        public static bool SequenceEqualUnordered<T>(this IEnumerable<T> source, IEnumerable<T> second)
        {
            return SequenceEqualUnordered(source, second, EqualityComparer<T>.Default);
        }

        public static bool SequenceEqualUnordered<T>(this IEnumerable<T> source, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (source == null)
                throw new NullReferenceException();
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var counters = new Dictionary<T, int>(comparer);
            foreach (var item in source)
                if (counters.ContainsKey(item))
                    counters[item]++;
                else
                    counters.Add(item, 1);

            foreach (var item in second)
            {
                if (counters.ContainsKey(item))
                    counters[item]--;
                else
                    return false;
            }

            return counters.Values.All(counter => counter == 0);
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new NullReferenceException();

            using (var enumerator = source.GetEnumerator())
                if (enumerator.MoveNext())
                    for (var item = enumerator.Current; enumerator.MoveNext(); item = enumerator.Current)
                        yield return item;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if (source == null)
                throw new NullReferenceException();

            return new HashSet<T>(source, comparer);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return ToHashSet(source, null);
        }

        public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<T, TKey, TElement>(this IEnumerable<T> source,
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new NullReferenceException();

            var result = new OrderedDictionary<TKey, TElement>(comparer);
            foreach (var item in source)
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

        public static GenericKeyedCollection<TKey, TElement> ToKeyedCollection<T, TKey, TElement>(this IEnumerable<T> source,
            Func<TElement, TKey> keyFromValueSelector, Func<T, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null, int dictionaryCreationThreshold = 0)
        {
            if (source == null)
                throw new NullReferenceException();

            var result = new GenericKeyedCollection<TKey, TElement>(keyFromValueSelector, comparer, dictionaryCreationThreshold);
            foreach (var item in source)
                result.Add(elementSelector(item));

            return result;
        }

        public static GenericKeyedCollection<TKey, T> ToKeyedCollection<TKey, T>(this IEnumerable<T> source, Func<T, TKey> keyFromValueSelector,
            IEqualityComparer<TKey> comparer = null, int dictionaryCreationThreshold = 0)
        {
            if (source == null)
                throw new NullReferenceException();

            var result = new GenericKeyedCollection<TKey, T>(keyFromValueSelector, comparer, dictionaryCreationThreshold);
            foreach (var item in source)
                result.Add(item);

            return result;
        }
    }
}
