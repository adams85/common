using System;
using System.IO;
using Xunit;

#if !NETCOREAPP1_0
namespace Karambolo.Common
{
    public class PathUtilsTest
    {
#if NET45 || NET461
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

#if NET45 || NET461
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
#if NET45 || NET461 || NETCOREAPP1_0 || NETCOREAPP2_0
            var s = Path.DirectorySeparatorChar;

            Assert.Equal(".", PathUtils.MakeRelativePathCore($"{s}", $"{s}"));

            Assert.Equal($"aa", PathUtils.MakeRelativePathCore($"{s}", $"{s}aa"));
            Assert.Equal($"aa{s}", PathUtils.MakeRelativePathCore($"{s}", $"{s}aa{s}"));

            Assert.Equal("..", PathUtils.MakeRelativePathCore($"{s}aa", $"{s}"));
            Assert.Equal("..", PathUtils.MakeRelativePathCore($"{s}aa{s}", $"{s}"));

            Assert.Equal($"..{s}bb", PathUtils.MakeRelativePathCore($"{s}aa", $"{s}bb"));
            Assert.Equal($"..{s}bb", PathUtils.MakeRelativePathCore($"{s}aa{s}", $"{s}bb"));
            Assert.Equal($"..{s}bb{s}", PathUtils.MakeRelativePathCore($"{s}aa", $"{s}bb{s}"));
            Assert.Equal($"..{s}bb{s}", PathUtils.MakeRelativePathCore($"{s}aa{s}", $"{s}bb{s}"));
#endif

            if (Platform.IsWindowsOS == true)
            {
                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:\", @"C:\"));
                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:", @"C:"));
                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:", @"C:"));

                Assert.Equal("..", PathUtils.MakeRelativePath(@"\DIR", @"\"));
                Assert.Equal("..", PathUtils.MakeRelativePath(@"\DIR", @"\"));
                Assert.Equal("..", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\"));
                Assert.Equal("..", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\"));

                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\DIR"));
                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\DIR"));
                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\DIR\"));
                Assert.Equal(".", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\DIR\"));

                Assert.Equal(@"SUBDIR", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\DIR\SUBDIR"));
                Assert.Equal(@"SUBDIR", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\DIR\SUBDIR"));
                Assert.Equal(@"SUBDIR\", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\DIR\SUBDIR\"));
                Assert.Equal(@"SUBDIR\", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\DIR\SUBDIR\"));

                Assert.Equal(@"..\DI", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\DI"));
                Assert.Equal(@"..\DI", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\DI"));
                Assert.Equal(@"..\DI\", PathUtils.MakeRelativePath(@"C:\DIR", @"C:\DI\"));
                Assert.Equal(@"..\DI\", PathUtils.MakeRelativePath(@"C:\DIR\", @"C:\DI\"));

                Assert.Equal(@"..\DIR", PathUtils.MakeRelativePath(@"C:\DI", @"C:\DIR"));
                Assert.Equal(@"..\DIR", PathUtils.MakeRelativePath(@"C:\DI\", @"C:\DIR"));
                Assert.Equal(@"..\DIR\", PathUtils.MakeRelativePath(@"C:\DI", @"C:\DIR\"));
                Assert.Equal(@"..\DIR\", PathUtils.MakeRelativePath(@"C:\DI\", @"C:\DIR\"));

                Assert.Equal(@"..\DIR2", PathUtils.MakeRelativePath(@"C:\DIR1", @"C:\DIR2"));
                Assert.Equal(@"..\DIR2", PathUtils.MakeRelativePath(@"C:\DIR1\", @"C:\DIR2"));
                Assert.Equal(@"..\DIR2\", PathUtils.MakeRelativePath(@"C:\DIR1", @"C:\DIR2\"));
                Assert.Equal(@"..\DIR2\", PathUtils.MakeRelativePath(@"C:\DIR1\", @"C:\DIR2\"));

                Assert.Equal(@"..\..\DIR2", PathUtils.MakeRelativePath(@"C:\DIR1\SUBDIR", @"C:\DIR2"));
                Assert.Equal(@"..\..\DIR2", PathUtils.MakeRelativePath(@"C:\DIR1\SUBDIR\", @"C:\DIR2"));
                Assert.Equal(@"..\..\DIR2\", PathUtils.MakeRelativePath(@"C:\DIR1\SUBDIR", @"C:\DIR2\"));
                Assert.Equal(@"..\..\DIR2\", PathUtils.MakeRelativePath(@"C:\DIR1\SUBDIR\", @"C:\DIR2\"));

                Assert.Equal(@"..\x", PathUtils.MakeRelativePath(@"c:\dir\.\SUBDIR\\", @"C:\DIR\\SuBDiR\..\x"));

                Assert.Equal(Path.GetFullPath(@"D:\"), PathUtils.MakeRelativePath(@"C:\", @"D:\"));
                Assert.Equal(Path.GetFullPath(@"D:\DIR"), PathUtils.MakeRelativePath(@"C:\DIR", @"D:\DIR"));

#if NET45 || NET461
                Assert.Throws<NotSupportedException>(() => PathUtils.MakeRelativePath(@"A:\", @"AA:\"));
#else
                // Path.GetFullPath has a weird behavior in .NET Core when called with an argument like @"AA:\"...
                Assert.Equal(Path.GetFullPath(@"AA:\"), PathUtils.MakeRelativePath(@"A:\", @"AA:\"));
#endif
            }
        }
    }
}
#endif
