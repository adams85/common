using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Karambolo.Common.Test
{
    public class FileStructureFixture : IDisposable
    {
        static void CreateDirStructureHelper(byte[] fileContent, byte[] fileContentModified, string leftDirPath, string rightDirPath, bool sameOnly, int level = 0)
        {
            Directory.CreateDirectory(leftDirPath);
            Directory.CreateDirectory(Path.Combine(leftDirPath, "dirInBoth"));
            File.WriteAllBytes(Path.Combine(leftDirPath, "fileInBothSameContent"), fileContent);
            if (!sameOnly)
            {
                Directory.CreateDirectory(Path.Combine(leftDirPath, "dirInLeft"));
                File.WriteAllText(Path.Combine(leftDirPath, "fileInLeftDirInRight"), "");
                Directory.CreateDirectory(Path.Combine(leftDirPath, "dirInLeftFileInRight"));
                File.WriteAllText(Path.Combine(leftDirPath, "fileInLeft"), "");
                File.WriteAllBytes(Path.Combine(leftDirPath, "fileInBothNotSameContent"), fileContent);
                File.WriteAllText(Path.Combine(leftDirPath, "fileInBothNotSameLength"), "");
            }

            Directory.CreateDirectory(rightDirPath);
            Directory.CreateDirectory(Path.Combine(rightDirPath, "dirinboth")); // for case-insensitive testing
            File.WriteAllBytes(Path.Combine(rightDirPath, "fileInBothSameContent"), fileContent);
            if (!sameOnly)
            {
                Directory.CreateDirectory(Path.Combine(rightDirPath, "dirInRight"));
                Directory.CreateDirectory(Path.Combine(rightDirPath, "fileInLeftDirInRight"));
                File.WriteAllText(Path.Combine(rightDirPath, "dirInLeftFileInRight"), "");
                File.WriteAllText(Path.Combine(rightDirPath, "fileInRight"), "");
                File.WriteAllBytes(Path.Combine(rightDirPath, "fileInBothNotSameContent"), fileContentModified);
                File.WriteAllText(Path.Combine(rightDirPath, "fileInBothNotSameLength"), "1");
            }

            if (level < 1)
            {
                CreateDirStructureHelper(fileContent, fileContentModified, Path.Combine(leftDirPath, "subdirNotSame"), Path.Combine(rightDirPath, "subdirNotSame"), false, level + 1);
                CreateDirStructureHelper(fileContent, fileContentModified, Path.Combine(leftDirPath, "subdirSame"), Path.Combine(rightDirPath, "subdirSame"), true, level + 1);
            }
        }

        public string leftDirPath { get; }
        public string rightDirPath { get; }

        public FileStructureFixture()
        {
            var random = new Random();
            var fileContent = new byte[10000];
            var fileContentModified = new byte[10000];

            for (var i = 0; i < fileContent.Length; i++)
                fileContent[i] = fileContentModified[i] = (byte)random.Next(256);
            fileContentModified[7000] = unchecked((byte)(fileContentModified[7000] + 1));

            var tempPath = Path.Combine(Path.GetTempPath(), "FileUtilsTest");
            leftDirPath = Path.Combine(tempPath, "left");
            rightDirPath = Path.Combine(tempPath, "right");
            if (Directory.Exists(leftDirPath))
                Directory.Delete(leftDirPath, true);
            if (Directory.Exists(rightDirPath))
                Directory.Delete(rightDirPath, true);
            CreateDirStructureHelper(fileContent, fileContentModified, leftDirPath, rightDirPath, false);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path.GetDirectoryName(leftDirPath)))
                Directory.Delete(Path.GetDirectoryName(leftDirPath), true);
        }
    }

    public class FileUtilsTest : IClassFixture<FileStructureFixture>
    {
        readonly FileStructureFixture _fileStructureFixture;

        public FileUtilsTest(FileStructureFixture fileStructureFixture)
        {
            _fileStructureFixture = fileStructureFixture;
        }

        /// <summary>
        ///A test for CompareDirs
        ///</summary>
        [Fact]
        public void CompareDirsTest1()
        {
            try { FileUtils.CompareDirs(null, string.Empty); }
            catch (ArgumentNullException) { }
            try { FileUtils.CompareDirs(string.Empty, null); }
            catch (ArgumentNullException) { }

            ICollection<CompareDirResultItem> result = null;
            var processedLength = 0L;
            var totalLength = 0L;
            var comparedFiles = new HashSet<Tuple<string, string>>();

            CompareProgressCallback progressCallback = (lfp, rfp, pl, tl) =>
            {
                comparedFiles.Add(Tuple.Create(lfp, rfp));
                processedLength = pl;
            };

            #region recursive, include dirs, check file content, using callback
            totalLength = 5 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, progressCallback: progressCallback);
            Assert.Equal(29, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeft", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeft", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subDirSame\dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subDirSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);

            // CompareDirsResultItem.ToString()
            Assert.Equal("[DLR!] ", result.SingleOrDefault(x => string.Equals(x.RelativePath, @"")).ToString());
            Assert.Equal("[FLR=] fileInBothSameContent", result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent")).ToString());
            Assert.Equal("[FL-!] fileInLeft", result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft")).ToString());
            Assert.Equal("[D-R!] dirInRight", result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInRight")).ToString());

            #endregion

            #region recursive, include dirs, ignore file content, using callback
            totalLength = 5 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, checkFilesContent: false, progressCallback: progressCallback);
            Assert.Equal(29, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeft", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeft", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subDirSame\dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subDirSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region recursive, exclude dirs, check file content, using callback
            totalLength = 5 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, excludeDirs: true, progressCallback: progressCallback);
            Assert.Equal(15, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subDirSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region recursive, exclude dirs, check file content, using callback
            totalLength = 5 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, excludeDirs: true, checkFilesContent: false, progressCallback: progressCallback);
            Assert.Equal(15, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subDirSame\fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region non-recursive, include dirs, check file content, using callback
            totalLength = 2 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, recursive: false, progressCallback: progressCallback);
            Assert.Equal(15, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeft", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region non-recursive, include dirs, ignore file content, using callback
            totalLength = 2 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, recursive: false, checkFilesContent: false, progressCallback: progressCallback);
            Assert.Equal(15, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInBoth", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeft", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirNotSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"subdirSame", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region non-recursive, exclude dirs, check file content, using callback
            totalLength = 2 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, recursive: false, excludeDirs: true, progressCallback: progressCallback);
            Assert.Equal(7, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region non-recursive, exclude dirs, ignore file content, using callback
            totalLength = 2 * 10000;
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, recursive: false, excludeDirs: true, checkFilesContent: false, progressCallback: progressCallback);
            Assert.Equal(7, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));

            Assert.Equal(totalLength, processedLength);
            #endregion

            #region non-recursive, exclude dirs, ignore file content, without callback
            result = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, recursive: false, excludeDirs: true, checkFilesContent: false);
            Assert.Equal(7, result.Count);

            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"dirInLeftFileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeft", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && !x.ExistsInLeft && x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInLeftDirInRight", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && !x.ExistsInRight && !x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameLength", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothNotSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            Assert.NotNull(result.SingleOrDefault(x => string.Equals(x.RelativePath, @"fileInBothSameContent", StringComparison.InvariantCultureIgnoreCase) && !x.IsDirectory && x.ExistsInLeft && x.ExistsInRight && x.IsIdentical));
            #endregion
        }

        /// <summary>
        ///A test for CompareDirs
        ///</summary>
        [Fact]
        public void CompareDirsTest2()
        {
            var flat = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath);

            CompareDirResultItem root;
            var tree = FileUtils.CompareDirs(_fileStructureFixture.leftDirPath, _fileStructureFixture.rightDirPath, out root);
            var flatFromTree = new List<CompareDirResultItem>();

            Action<CompareDirResultItem> flatten = null;
            flatten = ri =>
            {
                flatFromTree.Add(ri);
                var children = tree[ri.RelativePath];
                foreach (var child in children)
                    if (child.IsDirectory)
                        flatten(child);
                    else
                        flatFromTree.Add(child);
            };
            flatten(root);

            Assert.Equal(flat.Count, flatFromTree.Count);
            flat.All(ri1 => flatFromTree.SingleOrDefault(ri2 =>
                                ri1.RelativePath == ri2.RelativePath &&
                                ri1.ExistsInLeft == ri2.ExistsInLeft &&
                                ri1.ExistsInRight == ri2.ExistsInRight &&
                                ri1.IsDirectory && ri2.IsDirectory &&
                                ri1.IsIdentical == ri2.IsIdentical) != null);
        }

        /// <summary>
        ///A test for CompareFiles
        ///</summary>
        [Fact]
        public void CompareFilesTest()
        {
            try { FileUtils.CompareFiles(null, string.Empty); }
            catch (ArgumentNullException) { }
            try { FileUtils.CompareFiles(string.Empty, null); }
            catch (ArgumentNullException) { }

            string leftFilePath = null;
            string rightFilePath = null;
            var processedLength = 0L;
            var totalLength = 0L;

            CompareProgressCallback progressCallback = (lfp, rfp, pl, tl) =>
            {
                Assert.Equal(leftFilePath, lfp);
                Assert.Equal(rightFilePath, rfp);
                Assert.Equal(totalLength, tl);
                Assert.True(pl >= 0 && pl <= tl);
                processedLength = pl;
            };

            // sizes mismatch
            leftFilePath = Path.Combine(_fileStructureFixture.leftDirPath, "fileInBothNotSameLength");
            rightFilePath = Path.Combine(_fileStructureFixture.rightDirPath, "fileInBothNotSameLength");
            totalLength = 1;
            Assert.False(FileUtils.CompareFiles(leftFilePath, rightFilePath, progressCallback));
            Assert.Equal(totalLength, processedLength);

            // sizes match, equal to 0
            leftFilePath = Path.Combine(_fileStructureFixture.leftDirPath, "fileInLeftDirInRight");
            rightFilePath = Path.Combine(_fileStructureFixture.rightDirPath, "dirInLeftFileInRight");
            totalLength = 0;
            Assert.True(FileUtils.CompareFiles(leftFilePath, rightFilePath, progressCallback));
            Assert.Equal(totalLength, processedLength);

            // sizes match, greater than 0, contents match
            leftFilePath = Path.Combine(_fileStructureFixture.leftDirPath, "fileInBothSameContent");
            rightFilePath = Path.Combine(_fileStructureFixture.rightDirPath, "fileInBothSameContent");
            totalLength = 10000;
            Assert.True(FileUtils.CompareFiles(leftFilePath, rightFilePath, progressCallback));
            Assert.Equal(totalLength, processedLength);
            // the same without callback
            Assert.True(FileUtils.CompareFiles(leftFilePath, rightFilePath));

            // sizes match, greater than 0, contents mismatch
            leftFilePath = Path.Combine(_fileStructureFixture.leftDirPath, "fileInBothNotSameContent");
            rightFilePath = Path.Combine(_fileStructureFixture.rightDirPath, "fileInBothNotSameContent");
            totalLength = 10000;
            Assert.False(FileUtils.CompareFiles(leftFilePath, rightFilePath, progressCallback));
            Assert.Equal(totalLength, processedLength);
        }
    }
}
