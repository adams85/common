using System;
using System.Collections.Generic;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public static class ListUtils
    {
        #region BinarySearch

        // taken from https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Collections/Generic/ArraySortHelper.cs

        public static int BinarySearch<T>(this IList<T> list, int index, int count, T item, IComparer<T> comparer = null)
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

        public static int BinarySearch<T>(this IList<T> list, T item, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return BinarySearch(list, 0, list.Count, item, comparer);
        }

        #endregion

        #region ConvertAll

        public static List<TOutput> ConvertAll<T, TOutput>(this IList<T> list, Func<T, TOutput> converter)
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

        public static bool Exists<T>(this IList<T> list, Predicate<T> match)
        {
            return list.FindIndex(match) >= 0;
        }

        #endregion

        #region Find

        public static T Find<T>(this IList<T> list, Predicate<T> match)
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

        public static List<T> FindAll<T>(this IList<T> list, Predicate<T> match)
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

        public static int FindIndex<T>(this IList<T> list, int startIndex, int count, Predicate<T> match)
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

        public static int FindIndex<T>(this IList<T> list, int startIndex, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return list.FindIndex(startIndex, list.Count - startIndex, match);
        }

        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            return list.FindIndex(0, list.Count, match);
        }

        #endregion

        #region FindLast

        public static T FindLast<T>(this IList<T> list, Predicate<T> match)
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

        public static int FindLastIndex<T>(this IList<T> list, int startIndex, int count, Predicate<T> match)
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

        public static int FindLastIndex<T>(this IList<T> list, int startIndex, Predicate<T> match)
        {
            return list.FindLastIndex(startIndex, startIndex + 1, match);
        }

        public static int FindLastIndex<T>(this IList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;
            return list.FindLastIndex(n - 1, n, match);
        }

        #endregion

        #region ForEach

        public static void ForEach<T>(this IList<T> list, Action<T> action)
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

        public static List<T> GetRange<T>(this IList<T> list, int index, int count)
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

        #region LastIndexOf

        public static int LastIndexOf<T>(this IList<T> list, T item, int startIndex, int count)
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

        public static int LastIndexOf<T>(this IList<T> list, T item, int startIndex)
        {
            return list.LastIndexOf(item, startIndex, startIndex + 1);
        }

        public static int LastIndexOf<T>(this IList<T> list, T item)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            var n = list.Count;
            return list.LastIndexOf(item, n - 1, n);
        }

        #endregion

        #region RemoveAll

        public static void RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.IsReadOnly)
                throw new ArgumentException(nameof(list), Resources.CollectionReadOnly);

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (list is List<T> listClass)
            {
                listClass.RemoveAll(match);
                return;
            }

            for (var i = list.Count - 1; i >= 0; i--)
                if (match(list[i]))
                    list.RemoveAt(i);
        }

        #endregion

        #region Reverse

        public static void Reverse<T>(this IList<T> list, int index, int count)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.IsReadOnly)
                throw new ArgumentException(nameof(list), Resources.CollectionReadOnly);

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), Resources.ValueMustBeNonNegative);

            if (list.Count - index < count)
                throw new ArgumentException(nameof(index), Resources.CollectionNotLongEnough);

            var endIndex = index + count - 1;
            for (; index < endIndex; index++, endIndex--)
                GeneralUtils.Swap(list, index, endIndex);
        }

        public static void Reverse<T>(this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            list.Reverse(0, list.Count);
        }

        #endregion

        #region Sort

        // taken from https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Collections/Generic/ArraySortHelper.cs

        const int introsortSizeThreshold = 16;

        static int FloorLog2PlusOne(int n)
        {
            int result = 0;
            while (n >= 1)
            {
                result++;
                n = n / 2;
            }
            return result;
        }

        static class SortHelper<T>
        {
            // comparing via interface is faster than comparing via delegate
            // https://stackoverflow.com/questions/52459054/is-performance-of-list-sort-by-comparison-is-better-than-custom-icomparer/52460370#52460370
            public static void Sort(IList<T> list, int index, int count, IComparer<T> comparer)
            {
                if (comparer == null)
                    comparer = Comparer<T>.Default;

                IntrospectiveSort(list, index, count, comparer);
            }

            static void SwapIfGreater(IList<T> list, IComparer<T> comparer, int i, int j)
            {
                if (i != j && comparer.Compare(list[i], list[j]) > 0)
                    GeneralUtils.Swap(list, i, j);
            }

            static void Swap(IList<T> list, int i, int j)
            {
                if (i != j)
                    GeneralUtils.Swap(list, i, j);
            }

            static void IntrospectiveSort(IList<T> list, int left, int count, IComparer<T> comparer)
            {
                if (count < 2)
                    return;

                IntroSort(list, left, count + left - 1, 2 * FloorLog2PlusOne(count), comparer);
            }

            static void IntroSort(IList<T> list, int lo, int hi, int depthLimit, IComparer<T> comparer)
            {
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= introsortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            SwapIfGreater(list, comparer, lo, hi);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            SwapIfGreater(list, comparer, lo, hi - 1);
                            SwapIfGreater(list, comparer, lo, hi);
                            SwapIfGreater(list, comparer, hi - 1, hi);
                            return;
                        }

                        InsertionSort(list, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        Heapsort(list, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;

                    int p = PickPivotAndPartition(list, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(list, p + 1, hi, depthLimit, comparer);
                    hi = p - 1;
                }
            }

            static int PickPivotAndPartition(IList<T> list, int lo, int hi, IComparer<T> comparer)
            {
                // Compute median-of-three.  But also partition them, since we've done the comparison.
                int middle = lo + ((hi - lo) / 2);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                SwapIfGreater(list, comparer, lo, middle);  // swap the low with the mid point
                SwapIfGreater(list, comparer, lo, hi);   // swap the low with the high
                SwapIfGreater(list, comparer, middle, hi); // swap the middle with the high

                T pivot = list[middle];
                Swap(list, middle, hi - 1);
                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    while (comparer.Compare(list[++left], pivot) < 0) ;
                    while (comparer.Compare(pivot, list[--right]) < 0) ;

                    if (left >= right)
                        break;

                    Swap(list, left, right);
                }

                // Put pivot in the right location.
                Swap(list, left, (hi - 1));
                return left;
            }

            static void Heapsort(IList<T> list, int lo, int hi, IComparer<T> comparer)
            {
                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; i = i - 1)
                {
                    DownHeap(list, i, n, lo, comparer);
                }
                for (int i = n; i > 1; i = i - 1)
                {
                    Swap(list, lo, lo + i - 1);
                    DownHeap(list, 1, i - 1, lo, comparer);
                }
            }

            static void DownHeap(IList<T> list, int i, int n, int lo, IComparer<T> comparer)
            {
                T d = list[lo + i - 1];
                int child;
                while (i <= n / 2)
                {
                    child = 2 * i;
                    if (child < n && comparer.Compare(list[lo + child - 1], list[lo + child]) < 0)
                    {
                        child++;
                    }
                    if (!(comparer.Compare(d, list[lo + child - 1]) < 0))
                        break;
                    list[lo + i - 1] = list[lo + child - 1];
                    i = child;
                }
                list[lo + i - 1] = d;
            }

            static void InsertionSort(IList<T> list, int lo, int hi, IComparer<T> comparer)
            {
                int i, j;
                T t;
                for (i = lo; i < hi; i++)
                {
                    j = i;
                    t = list[i + 1];
                    while (j >= lo && comparer.Compare(t, list[j]) < 0)
                    {
                        list[j + 1] = list[j];
                        j--;
                    }
                    list[j + 1] = t;
                }
            }
        }

        public static void Sort<T>(this IList<T> list, int index, int count, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.IsReadOnly)
                throw new ArgumentException(nameof(list), Resources.CollectionReadOnly);

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), Resources.ValueMustBeNonNegative);

            var n = list.Count;

            if (n - index < count)
                throw new ArgumentException(nameof(index), Resources.CollectionNotLongEnough);

            if (n > 1)
                SortHelper<T>.Sort(list, index, count, comparer);
        }

        public static void Sort<T>(this IList<T> list, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.IsReadOnly)
                throw new ArgumentException(nameof(list), Resources.CollectionReadOnly);

            var n = list.Count;

            if (n > 1)
                SortHelper<T>.Sort(list, 0, n, comparer);
        }

        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (list.IsReadOnly)
                throw new ArgumentException(nameof(list), Resources.CollectionReadOnly);

            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            var n = list.Count;

            if (n > 1)
                SortHelper<T>.Sort(list, 0, n, DelegatedComparer.Create(new Func<T, T, int>(comparison)));
        }

        #endregion

        #region TrueForAll

        public static bool TrueForAll<T>(this IList<T> list, Predicate<T> match)
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
