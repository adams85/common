using System;
using System.Collections.Generic;
using System.Linq;

namespace Karambolo.Common
{
    public static partial class EnumerableUtils
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
                throw new ArgumentNullException(nameof(source));

            yield return element;

            foreach (var e in source)
                yield return e;
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

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
                throw new ArgumentNullException(nameof(source));

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
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var enumerator = source.GetEnumerator())
                if (enumerator.MoveNext())
                    for (var item = enumerator.Current; enumerator.MoveNext(); item = enumerator.Current)
                        yield return item;
        }

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var enumerator = source.GetEnumerator())
                if (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    while (enumerator.MoveNext())
                        item = enumerator.Current;
                    yield return item;
                }
        }

        static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(IEnumerable<TSource> source,
            Func<IEnumerator<TSource>, KeyValuePair<bool, TAccumulate>> seeder, Func<TAccumulate, TSource, TAccumulate> func)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var seed = seeder(enumerator);

                if (!seed.Key)
                    yield break;

                var accumulator = seed.Value;
                yield return accumulator;

                while (enumerator.MoveNext())
                {
                    accumulator = func(accumulator, enumerator.Current);
                    yield return accumulator;
                }
            }
        }

        public static IEnumerable<TSource> Scan<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return Scan(source,
                enumerator => enumerator.MoveNext() ? new KeyValuePair<bool, TSource>(true, enumerator.Current) : default,
                func);
        }

        public static IEnumerable<TAccumulate> Scan<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return Scan(source,
                enumerator => new KeyValuePair<bool, TAccumulate>(true, seed),
                func);
        }

    }
}
