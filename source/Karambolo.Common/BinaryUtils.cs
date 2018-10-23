using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    public static class BinaryUtils
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
        /// <param name="bitField">byte to count</param>
        /// <returns>number of set bits in byte</returns>
        public static int BitCount(this byte bitField)
        {
            return bits[bitField];
        }

        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">sbyte to count</param>
        /// <returns>number of set bits in sbyte</returns>
        public static int BitCount(this sbyte bitField)
        {
            return ((byte)bitField).BitCount();
        }

        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">ushort to count</param>
        /// <returns>number of set bits in ushort</returns>
        public static int BitCount(this ushort bitField)
        {
            const ushort mask = 0xFF;
            return
                bits[bitField & mask] +
                bits[bitField >> 8 & mask];
        }

        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">short to count</param>
        /// <returns>number of set bits in short</returns>
        public static int BitCount(this short bitField)
        {
            return ((ushort)bitField).BitCount();
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
        /// <param name="bitField">int to count</param>
        /// <returns>number of set bits in int</returns>
        public static int BitCount(this int bitField)
        {
            return ((uint)bitField).BitCount();
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


        /// <summary>
        /// Fast Bitcounting method (adapted from snippets.org)
        /// </summary>
        /// <param name="bitField">long to count</param>
        /// <returns>number of set bits in long</returns>
        public static int BitCount(this long bitField)
        {
            return ((ulong)bitField).BitCount();
        }
        #endregion

        public static ushort ReverseByteOrder(this ushort value)
        {
            return (ushort)((value & 0xFFu) << 8 | (value & 0xFF00u) >> 8);
        }

        public static short ReverseByteOrder(this short value)
        {
            return (short)((ushort)value).ReverseByteOrder();
        }

        public static uint ReverseByteOrder(this uint value)
        {
            return (value & 0x000000FFu) << 24 | (value & 0x0000FF00u) << 8 |
                (value & 0x00FF0000u) >> 8 | (value & 0xFF000000u) >> 24;
        }

        public static int ReverseByteOrder(this int value)
        {
            return (int)((uint)value).ReverseByteOrder();
        }

        public static ulong ReverseByteOrder(this ulong value)
        {
            return (value & 0x00000000000000FFul) << 56 | (value & 0x000000000000FF00ul) << 40 |
                (value & 0x0000000000FF0000ul) << 24 | (value & 0x00000000FF000000ul) << 8 |
                (value & 0x000000FF00000000ul) >> 8 | (value & 0x0000FF0000000000ul) >> 24 |
                (value & 0x00FF000000000000ul) >> 40 | (value & 0xFF00000000000000ul) >> 56;
        }

        public static long ReverseByteOrder(this long value)
        {
            return (long)((ulong)value).ReverseByteOrder();
        }
    }
}
