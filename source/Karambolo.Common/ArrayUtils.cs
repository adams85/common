using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    public static class ArrayUtils
    {
        static class EmptyArray<T>
        {
            public static readonly T[] Instance = new T[0];
        }

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Instance;
        }

        public static T[] FromElement<T>(T element)
        {
            return new[] { element };
        }

        public static T[] FromElements<T>(params T[] elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            return elements;
        }

        public static T[] Shuffle<T>(this T[] array, Random random, Action<T[], int, int> swap = null, bool forceSwapFirst = false)
        {
            if (array == null)
                throw new NullReferenceException();

            if (swap == null)
                swap = GeneralUtils.Swap;

            var to = forceSwapFirst ? 0 : 1;
            for (var i = array.Length - 1; i >= to; i--)
                swap(array, random.Next(i + 1), i);

            return array;
        }

        public static int[] GetLowerBounds(this Array array)
        {
            if (array == null)
                throw new NullReferenceException();

            var dimensions = array.Rank;

            var result = new int[dimensions];
            for (var i = 0; i < dimensions; i++)
                result[i] = array.GetLowerBound(i);

            return result;
        }

        public static int[] GetLengths(this Array array)
        {
            if (array == null)
                throw new NullReferenceException();

            var dimensions = array.Rank;

            var result = new int[dimensions];
            for (var i = 0; i < dimensions; i++)
                result[i] = array.GetLength(i);

            return result;
        }

#if !NETSTANDARD1_0
        public static long[] GetLongLengths(this Array array)
        {
            if (array == null)
                throw new NullReferenceException();

            var dimensions = array.Rank;

            var result = new long[dimensions];
            for (var i = 0; i < dimensions; i++)
                result[i] = array.GetLongLength(i);

            return result;
        }
#endif

        public static void Fill<T>(this T[] array, T value)
        {
            if (array == null)
                throw new NullReferenceException();

            for (var i = 0; i < array.Length; i++)
                array[i] = value;
        }

        public static void Fill<T>(this T[] array, Func<int, T> value)
        {
            if (array == null)
                throw new NullReferenceException();

            for (var i = 0; i < array.Length; i++)
                array[i] = value(i);
        }

        public static bool ContentEquals<T>(T[] array, T[] otherArray, IEqualityComparer<T> comparer = null)
        {
            if (array == otherArray)
                return true;

            if (array == null || otherArray == null)
                return false;

            if (array.Length != otherArray.Length)
                return false;

            comparer = comparer ?? EqualityComparer<T>.Default;
            for (var i = 0; i < array.Length; i++)
                if (!comparer.Equals(array[i], otherArray[i]))
                    return false;

            return true;
        }

        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }
    }
}
