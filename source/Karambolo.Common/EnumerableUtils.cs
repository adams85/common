﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Karambolo.Common
{
    public static class EnumerableUtils
    {
        public static IEnumerable<TResult> Return<TResult>(TResult element)
        {
            yield return element;
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element)
        {
            while (true)
                yield return element;
        }

#if !NETSTANDARD2_0
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new NullReferenceException();

            yield return element;

            foreach (var e in source)
                yield return e;
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new NullReferenceException();

            foreach (var e in source)
                yield return e;

            yield return element;
        }
#endif

        public static bool SequenceEqualUnordered<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> second)
        {
            return SequenceEqualUnordered(source, second, EqualityComparer<TSource>.Default);
        }

        public static bool SequenceEqualUnordered<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
                throw new NullReferenceException();
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var counters = new Dictionary<TSource, int>(comparer);
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

        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source)
        {
            using (var enumerator = source.GetEnumerator())
                if (enumerator.MoveNext())
                    for (var item = enumerator.Current; enumerator.MoveNext(); item = enumerator.Current)
                        yield return item;
        }

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source)
        {
            using (var enumerator = source.GetEnumerator())
                if (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    while (enumerator.MoveNext())
                        item = enumerator.Current;
                    yield return item;
                }
        }
    }
}
