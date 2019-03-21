using System;
using Xunit;

#if !NETCOREAPP1_0
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

        [Fact]
        public void MakeRelativePathTest()
        {
            Assert.Equal(string.Empty, PathUtils.MakeRelativePathCore(@"C:\", @"C:\"));
            Assert.Equal(string.Empty, PathUtils.MakeRelativePathCore(@"C:", @"C:"));
            Assert.Equal(string.Empty, PathUtils.MakeRelativePath(@"C:", @"C:"));
            Assert.Equal(string.Empty, PathUtils.MakeRelativePathCore(@"C:", @"C:"));
            Assert.Equal(string.Empty, PathUtils.MakeRelativePath(@"\", @"\"));

            Assert.Equal(string.Empty, PathUtils.MakeRelativePathCore(@"C:\DIR", @"C:\DIR"));
            Assert.Equal(string.Empty, PathUtils.MakeRelativePathCore(@"C:\DIR\", @"C:\DIR"));
            Assert.Equal(@".\", PathUtils.MakeRelativePathCore(@"C:\DIR", @"C:\DIR\"));
            Assert.Equal(@".\", PathUtils.MakeRelativePathCore(@"C:\DIR\", @"C:\DIR\"));

            Assert.Equal(@"SUBDIR", PathUtils.MakeRelativePathCore(@"C:\DIR", @"C:\DIR\SUBDIR"));
            Assert.Equal(@"SUBDIR", PathUtils.MakeRelativePathCore(@"C:\DIR\", @"C:\DIR\SUBDIR"));
            Assert.Equal(@"SUBDIR\", PathUtils.MakeRelativePathCore(@"C:\DIR", @"C:\DIR\SUBDIR\"));
            Assert.Equal(@"SUBDIR\", PathUtils.MakeRelativePathCore(@"C:\DIR\", @"C:\DIR\SUBDIR\"));

            Assert.Equal(@"..\DI", PathUtils.MakeRelativePathCore(@"C:\DIR", @"C:\DI"));
            Assert.Equal(@"..\DI", PathUtils.MakeRelativePathCore(@"C:\DIR\", @"C:\DI"));
            Assert.Equal(@"..\DI\", PathUtils.MakeRelativePathCore(@"C:\DIR", @"C:\DI\"));
            Assert.Equal(@"..\DI\", PathUtils.MakeRelativePathCore(@"C:\DIR\", @"C:\DI\"));

            Assert.Equal(@"..\DIR", PathUtils.MakeRelativePathCore(@"C:\DI", @"C:\DIR"));
            Assert.Equal(@"..\DIR", PathUtils.MakeRelativePathCore(@"C:\DI\", @"C:\DIR"));
            Assert.Equal(@"..\DIR\", PathUtils.MakeRelativePathCore(@"C:\DI", @"C:\DIR\"));
            Assert.Equal(@"..\DIR\", PathUtils.MakeRelativePathCore(@"C:\DI\", @"C:\DIR\"));

            Assert.Equal(@"..\DIR2", PathUtils.MakeRelativePathCore(@"C:\DIR1", @"C:\DIR2"));
            Assert.Equal(@"..\DIR2", PathUtils.MakeRelativePathCore(@"C:\DIR1\", @"C:\DIR2"));
            Assert.Equal(@"..\DIR2\", PathUtils.MakeRelativePathCore(@"C:\DIR1", @"C:\DIR2\"));
            Assert.Equal(@"..\DIR2\", PathUtils.MakeRelativePathCore(@"C:\DIR1\", @"C:\DIR2\"));

            Assert.Equal(@"..\..\DIR2", PathUtils.MakeRelativePathCore(@"C:\DIR1\SUBDIR", @"C:\DIR2"));
            Assert.Equal(@"..\..\DIR2", PathUtils.MakeRelativePathCore(@"C:\DIR1\SUBDIR\", @"C:\DIR2"));
            Assert.Equal(@"..\..\DIR2\", PathUtils.MakeRelativePathCore(@"C:\DIR1\SUBDIR", @"C:\DIR2\"));
            Assert.Equal(@"..\..\DIR2\", PathUtils.MakeRelativePathCore(@"C:\DIR1\SUBDIR\", @"C:\DIR2\"));

            Assert.Throws<ArgumentException>(() => PathUtils.MakeRelativePathCore(@"C:\DIR", @"D:\DIR"));
        }
    }
}
#endif
