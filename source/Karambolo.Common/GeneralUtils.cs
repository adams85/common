using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    public static class GeneralUtils
    {
        #region Fast BitCounter
        /// <summary>
        /// Bit count table from snippets.org
        /// </summary>
        private static readonly byte[] bits =
		{
			0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,  /* 0   - 15  */
			1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 16  - 31  */
			1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 32  - 47  */
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 48  - 63  */
			1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 64  - 79  */
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 80  - 95  */
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 96  - 111 */
			3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 112 - 127 */
			1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 128 - 143 */
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 144 - 159 */
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 160 - 175 */
			3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 176 - 191 */
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 192 - 207 */
			3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 208 - 223 */
			3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 224 - 239 */
			4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8   /* 240 - 255 */
		};

        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">ushort to count</param>
        /// <returns>number of set bits in ushort</returns>
        public static int BitCount(this ushort bitField)
        {
            const ushort mask = 0xFF;
            return
                bits[(int)(bitField & mask)] +
                bits[(int)(bitField >> 8 & mask)];
        }

        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">uint to count</param>
        /// <returns>number of set bits in uint</returns>
        public static int BitCount(this uint bitField)
        {
            const uint mask = 0xFFu;
            return
                bits[(int)(bitField & mask)] +
                bits[(int)(bitField >> 8 & mask)] +
                bits[(int)(bitField >> 16 & mask)] +
                bits[(int)(bitField >> 24 & mask)];
        }
        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">ulong to count</param>
        /// <returns>number of set bits in ulong</returns>
        public static int BitCount(this ulong bitField)
        {
            const ulong mask = 0xFFUL;
            return
                bits[(int)(bitField & mask)] +
                bits[(int)(bitField >> 8 & mask)] +
                bits[(int)(bitField >> 16 & mask)] +
                bits[(int)(bitField >> 24 & mask)] +
                bits[(int)(bitField >> 32 & mask)] +
                bits[(int)(bitField >> 40 & mask)] +
                bits[(int)(bitField >> 48 & mask)] +
                bits[(int)(bitField >> 56 & mask)];
        }
        #endregion

        public static short ReverseByteOrder(this short value)
        {
            return (short)ReverseByteOrder((ushort)value);
        }

        public static ushort ReverseByteOrder(this ushort value)
        {
            return (ushort)((value & 0xFFu) << 8 | (value & 0xFF00u) >> 8);
        }

        public static int ReverseByteOrder(this int value)
        {
            return (int)ReverseByteOrder((uint)value);
        }

        public static uint ReverseByteOrder(this uint value)
        {
            return (value & 0x000000FFu) << 24 | (value & 0x0000FF00u) << 8 |
                (value & 0x00FF0000u) >> 8 | (value & 0xFF000000u) >> 24;
        }

        public static long ReverseByteOrder(this long value)
        {
            return (long)ReverseByteOrder((ulong)value);
        }

        public static ulong ReverseByteOrder(this ulong value)
        {
            return (value & 0x00000000000000FFul) << 56 | (value & 0x000000000000FF00ul) << 40 |
                (value & 0x0000000000FF0000ul) << 24 | (value & 0x00000000FF000000ul) << 8 |
                (value & 0x000000FF00000000ul) >> 8 | (value & 0x0000FF0000000000ul) >> 24 |
                (value & 0x00FF000000000000ul) >> 40 | (value & 0xFF00000000000000ul) >> 56;
        }

        public static void Swap(ref byte value1, ref byte value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref sbyte value1, ref sbyte value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref short value1, ref short value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref ushort value1, ref ushort value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref int value1, ref int value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref uint value1, ref uint value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref long value1, ref long value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap(ref ulong value1, ref ulong value2)
        {
            value2 ^= value1;
            value1 ^= value2;
            value2 ^= value1;
        }

        public static void Swap<T>(ref T value1, ref T value2)
        {
            var temp = value1;
            value1 = value2;
            value2 = temp;
        }

        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            var temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }

        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static IEnumerable<int> IterateWhileTrue(Func<bool> condition)
        {
            var counter = 0;
            while (condition())
                yield return counter++;
        }

        public static IEnumerable<long> IterateWhileTrueInt64(Func<bool> condition)
        {
            var counter = 0L;
            while (condition())
                yield return counter++;
        }
    }
}
