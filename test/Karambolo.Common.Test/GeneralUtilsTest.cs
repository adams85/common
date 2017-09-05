using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Karambolo.Common.Test
{
    [TestClass()]
    public class GeneralUtilsTest
    {
        [TestMethod]
        public void EscapeTest1()
        {
            Assert.AreEqual(8, GeneralUtils.BitCount((ushort)46380));
            Assert.AreEqual(17, GeneralUtils.BitCount(3039589677u));
            Assert.AreEqual(34, GeneralUtils.BitCount(0xB52C752CBF2C752CUL));
        }
    }
}
