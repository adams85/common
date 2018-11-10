using System.Collections.Generic;
using Xunit;

namespace Karambolo.Common
{
    public class GeneralUtilsTest
    {
        [Fact]
        public void SwapTest()
        {
            int int1 = 1, int2 = 2;
            int[] intArray = { 1, 2 };
            var intList = new List<int> { 1, 2 };

            GeneralUtils.Swap(ref int1, ref int2);
            Assert.Equal(2, int1);
            Assert.Equal(1, int2);

            GeneralUtils.Swap(ref intArray[0], ref intArray[1]);
            Assert.Equal(2, intArray[0]);
            Assert.Equal(1, intArray[1]);

            GeneralUtils.Swap(intArray, 0, 1);
            Assert.Equal(1, intArray[0]);
            Assert.Equal(2, intArray[1]);

            GeneralUtils.Swap(intList, 0, 1);
            Assert.Equal(2, intList[0]);
            Assert.Equal(1, intList[1]);
        }
    }
}
