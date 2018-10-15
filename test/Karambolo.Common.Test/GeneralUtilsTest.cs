using Xunit;

namespace Karambolo.Common.Test
{
    public class GeneralUtilsTest
    {
        [Fact]
        public void BitCountTest()
        {
            Assert.Equal(8, GeneralUtils.BitCount(46380));
            Assert.Equal(17, GeneralUtils.BitCount(3039589677u));
            Assert.Equal(34, GeneralUtils.BitCount(0xB52C752CBF2C752CUL));
        }
    }
}
