using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
