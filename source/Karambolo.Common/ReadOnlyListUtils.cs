using System;
using System.Collections.Generic;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public static class ReadOnlyListUtils
    {
        #region BinarySearch

        // taken from https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Collections/Generic/ArraySortHelper.cs

        public static int BinarySearch<T>(this IReadOnlyList<T> list, int index, int count, T item, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), Resources.ValueMustBeNonNegative);

            if (list.Count - index < count)
                throw new ArgumentException(nameof(index), Resources.CollectionNotLongEnough);

            if (comparer == null)
                comparer = Comparer<T>.Default;

            var lo = index;
            var hi = index + count - 1;

            while (lo <= hi)
            {
                var i = lo + ((hi - lo) >> 1);
                var order = comparer.Compare(list[i], item);

                if (order < 0)
                    lo = i + 1;
                else if (order > 0)
                    hi = i - 1;
                else
                    return i;
            }

            return ~lo;
        }

        public static int BinarySearch<T>(this IReadOnlyList<T> list, T item, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return BinarySearch(list, 0, list.Count, item, comparer);
        }

        #endregion

        #region ConvertAll

        public static List<TOutput> ConvertAll<T, TOutput>(this IReadOnlyList<T> list, Func<T, TOutput> converter)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            var n = list.Count;
            var result = new List<TOutput>(n);
            for (int i = 0; i < n; i++)
                result.Add(converter(list[i]));

            return result;
        }

        #endregion

        #region Exists

        public static bool Exists<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            return list.FindIndex(match) >= 0;
        }

        #endregion

        #region Find

        public static T Find<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            T item;
            for (int i = 0, n = list.Count; i < n; i++)
                if (match(item = list[i]))
                    return item;

            return default;
        }

        #endregion

        #region FindAll

        public static List<T> FindAll<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            T item;
            List<T> items = new List<T>();
            for (int i = 0, n = list.Count; i < n; i++)
                if (match(item = list[i]))
                    items.Add(item);

            return items;
        }

        #endregion

        #region FindIndex

        public static int FindIndex<T>(this IReadOnlyList<T> list, int startIndex, int count, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;

            if (startIndex > n)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > n - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (var endIndex = startIndex + count; startIndex < endIndex; startIndex++)
                if (match(list[startIndex]))
                    return startIndex;

            return -1;
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, int startIndex, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return list.FindIndex(startIndex, list.Count - startIndex, match);
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return list.FindIndex(0, list.Count, match);
        }

        #endregion

        #region FindLast

        public static T FindLast<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            T item;
            for (var i = list.Count - 1; i >= 0; i--)
                if (match(item = list[i]))
                    return item;

            return default;
        }

        #endregion

        #region FindLastIndex

        public static int FindLastIndex<T>(this IReadOnlyList<T> list, int startIndex, int count, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;

            if (n == 0)
            {
                if (startIndex != -1)
                    throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            else
            {
                if (startIndex >= n)
                    throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (count < 0 || startIndex - count + 1 < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (var endIndex = startIndex - count; startIndex > endIndex; startIndex--)
                if (match(list[startIndex]))
                    return startIndex;

            return -1;
        }

        public static int FindLastIndex<T>(this IReadOnlyList<T> list, int startIndex, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, startIndex + 1, match);
        }

        public static int FindLastIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;
            return list.FindLastIndex(n - 1, n, match);
        }

        #endregion

        #region ForEach

        public static void ForEach<T>(this IReadOnlyList<T> list, Action<T> action)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            for (int i = 0, n = list.Count; i < n; i++)
                action(list[i]);
        }

        #endregion

        #region GetRange

        public static List<T> GetRange<T>(this IReadOnlyList<T> list, int index, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), Resources.ValueMustBeNonNegative);

            if (list.Count - index < count)
                throw new ArgumentException(nameof(index), Resources.CollectionNotLongEnough);

            var result = new List<T>(count);
            var endIndex = index + count;
            for (; index < endIndex; index++)
                result.Add(list[index]);

            return result;
        }

        #endregion

        #region IndexOf

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item, int startIndex, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;

            if (startIndex > n)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > n - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            var comparer = EqualityComparer<T>.Default;
            for (var endIndex = startIndex + count; startIndex < endIndex; startIndex++)
                if (comparer.Equals(list[startIndex], item))
                    return startIndex;

            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item, int startIndex)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return list.IndexOf(item, startIndex, list.Count - startIndex);
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return list.IndexOf(item, 0, list.Count);
        }

        #endregion

        #region LastIndexOf

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, T item, int startIndex, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;

            if (n == 0)
            {
                if (startIndex != -1)
                    throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            else
            {
                if (startIndex >= n)
                    throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (count < 0 || startIndex - count + 1 < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var comparer = EqualityComparer<T>.Default;
            for (var endIndex = startIndex - count; startIndex > endIndex; startIndex--)
                if (comparer.Equals(list[startIndex], item))
                    return startIndex;

            return -1;
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, T item, int startIndex)
        {
            return list.LastIndexOf(item, startIndex, startIndex + 1);
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;
            return list.LastIndexOf(item, n - 1, n);
        }

        #endregion

        #region TrueForAll

        public static bool TrueForAll<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0, n = list.Count; i < n; i++)
                if (!match(list[i]))
                    return false;

            return true;
        }

        #endregion
    }
}
