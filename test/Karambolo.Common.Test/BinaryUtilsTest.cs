using System.Collections.Generic;
using Xunit;

namespace Karambolo.Common.Test
{
    public class BitUtilsTest
    {
        [Fact]
        public void BitCountTest()
        {
            Assert.Equal(3, ((byte)0b1011).BitCount()); // 11
            Assert.Equal(8, ((sbyte)-1).BitCount());

            Assert.Equal(8, ((ushort)0b1011_0101_0010_1100U).BitCount()); // 46_380
            Assert.Equal(16, ((short)-1).BitCount());

            Assert.Equal(17, 0b1011_0101_0010_1100_0111_0101_0010_1101U.BitCount()); // 3_039_589_677
            Assert.Equal(32, (-1).BitCount());

            Assert.Equal(34, 0b1011_0101_0010_1100_0111_0101_0010_1100_1011_1111_0010_1100_0111_0101_0010_1100U.BitCount()); // 0xB52C_752C_BF2C_752C
            Assert.Equal(64, (-1L).BitCount());
        }

        [Fact]
        public void ReverseByteOrderTest()
        {
            Assert.Equal(0x23_01U, ((ushort)0x01_23U).ReverseByteOrder());
            Assert.Equal((short)-0x2302, ((short)-0x0124).ReverseByteOrder()); // FEDC

            Assert.Equal(0x67_45_23_01U, 0x01_23_45_67U.ReverseByteOrder());
            Assert.Equal(-0x6745_2302, (-0x0123_4568).ReverseByteOrder()); // FEDC_BA98

            Assert.Equal(0xEF_CD_AB_89_67_45_23_01U, 0x01_23_45_67_89_AB_CD_EFU.ReverseByteOrder());
            Assert.Equal(0x1032_5476_98BA_DCFE, (-0x0123_4567_89AB_CDF0).ReverseByteOrder()); // FEDC_BA98_7654_3210
        }
    }
}
