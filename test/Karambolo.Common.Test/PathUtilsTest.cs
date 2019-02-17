using System;
using System.Linq;
using Xunit;

namespace Karambolo.Common
{
    public class PathUtilsTest
    {
#if NET461
        [Fact]
        public void IsReservedFileNameTest()
        {
            Assert.True(PathUtils.IsReservedFileName("aux"));
            Assert.True(PathUtils.IsReservedFileName("aux."));
            Assert.True(PathUtils.IsReservedFileName("aux.xyz"));
            Assert.False(PathUtils.IsReservedFileName("_con"));
        }
#endif

        [Fact]
        public void MakeValidFileNameTest()
        {
            Assert.Equal("__", PathUtils.MakeValidFileName("  "));
            Assert.Equal(" a ", PathUtils.MakeValidFileName(" a "));

#if NET461
            Assert.Equal("_nul", PathUtils.MakeValidFileName("nul"));
            Assert.Equal("_prn.txt", PathUtils.MakeValidFileName("prn.txt"));
#endif

            Assert.Equal("_some_invalid_filename_", PathUtils.MakeValidFileName("*some/invalid|filename?"));
            Assert.Same("This should be a valid file name.", PathUtils.MakeValidFileName("This should be a valid file name."));

            Assert.Equal(
                "0123456789abcdef_123456789abcde",
                PathUtils.MakeValidFileName("0123456789abcdef?123456789abcde", 31));

            Assert.Equal(
                "0123456789abcdef0123456789abcde",
                PathUtils.MakeValidFileName("0123456789abcdef0123456789abcdef", 31));
        }
    }
}
