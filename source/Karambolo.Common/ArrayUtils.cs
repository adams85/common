using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    // Arrays cannot contain more than Int32.MaxValue elements along a single dimension and more than UInt32.MaxValue elements in total currently.
    // https://docs.microsoft.com/en-us/dotnet/api/system.array#remarks
    public static class ArrayUtils
    {
#if NET40 || NET45 || NETSTANDARD1_0
        private static class EmptyArray<T>
        {
            public static readonly T[] Instance = new T[0];
        }

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Instance;
        }
#else
        [Obsolete("This method is redundant (due to the introduction of Array.Empty<T>) and thus will be removed in a future version.")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static T[] Empty<T>()
        {
            return Array.Empty<T>();
        }
#endif

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullOrEmpty(Array array)
        {
            return
                array == null ||
#if !NETSTANDARD1_0
                array.LongLength == 0;
#else
                array.Length == 0;
#endif
        }

        // This overload is necessary as accessing one-dimensional arrays through the Array base class is 5-6x slower.
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static int[] GetLowerBounds(this Array array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var dimensions = array.Rank;

            var result = new int[dimensions];
            for (var i = 0; i < dimensions; i++)
                result[i] = array.GetLowerBound(i);

            return result;
        }

        public static int[] GetUpperBounds(this Array array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var dimensions = array.Rank;

            var result = new int[dimensions];
            for (var i = 0; i < dimensions; i++)
                result[i] = array.GetUpperBound(i);

            return result;
        }

        public static int[] GetLengths(this Array array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var dimensions = array.Rank;

            var result = new int[dimensions];
            for (var i = 0; i < dimensions; i++)
                result[i] = array.GetLength(i);

            return result;
        }

#if NETSTANDARD2_1
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void Fill<T>(this T[] array, T value)
        {
#if NET40 || NET45 || NETSTANDARD1_0 || NETSTANDARD2_0
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            for (int i = 0, n = array.Length; i < n; i++)
                array[i] = value;
#else
            Array.Fill(array, value);
#endif
        }

#if NETSTANDARD2_1
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void Fill<T>(this T[] array, T value, int startIndex, int count)
        {
#if NET40 || NET45 || NETSTANDARD1_0 || NETSTANDARD2_0
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > array.Length - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int endIndex = startIndex + count; startIndex < endIndex; startIndex++)
                array[startIndex] = value;
#else
            Array.Fill(array, value, startIndex, count);
#endif
        }

        public static void Fill<T>(this T[] array, Func<int, T> value)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var n = array.Length;
            for (var i = 0; i < n; i++)
                array[i] = value(i);
        }

        public static void Fill<T>(this T[] array, Func<int, T> value, int startIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || startIndex > array.Length - count)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int endIndex = startIndex + count; startIndex < endIndex; startIndex++)
                array[startIndex] = value(startIndex);
        }

        public static void Shuffle<T>(this T[] array, Func<int, int> randomIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var n = array.Length;
            while (n > 1)
                GeneralUtils.Swap(ref array[randomIndex(n)], ref array[--n]);
        }

        public static void Shuffle<T>(this T[] array, Random random)
        {
            array.Shuffle(random.Next);
        }

        public static bool ContentEquals<T>(T[] array, T[] otherArray, IEqualityComparer<T> comparer = null)
        {
            if (array == otherArray)
                return true;

            if (array == null || otherArray == null)
                return false;

            var n = array.Length;
            if (n != otherArray.Length)
                return false;

            comparer = comparer ?? EqualityComparer<T>.Default;

            for (var i = 0; i < n; i++)
                if (!comparer.Equals(array[i], otherArray[i]))
                    return false;

            return true;
        }
    }
}
