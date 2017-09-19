using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Karambolo.Common
{
    public delegate void CompareProgressCallback(string leftFilePath, string rightFilePath, long processedLength, long totalLength);

    public sealed class CompareDirResultItem
    {
        internal CompareDirResultItem(string relativePath, bool isDirectory, bool existsInLeft, bool existsInRight, bool isIdentical)
        {
            RelativePath = relativePath;
            IsDirectory = isDirectory;
            ExistsInLeft = existsInLeft;
            ExistsInRight = existsInRight;
            IsIdentical = isIdentical;
        }

        public string RelativePath { get; internal set; }
        public bool IsDirectory { get; internal set; }
        public bool ExistsInLeft { get; internal set; }
        public bool ExistsInRight { get; internal set; }
        public bool IsIdentical { get; internal set; }
        public object Data { get; set; }

        public override string ToString()
        {
            return $"[{(IsDirectory ? 'D' : 'F')}{(ExistsInLeft ? 'L' : '-')}{(ExistsInRight ? 'R' : '-')}{(IsIdentical ? '=' : '!')}] {RelativePath}";
        }
    }

    public static partial class FileUtils
    {
        const int compareFilesBufferSize = 4096;

        static void CompareDirsMergeEntries(Dictionary<string, FileSystemInfo> leftEntries, Dictionary<string, FileSystemInfo> rightEntries,
            bool checkFilesContent, Dictionary<string, Tuple<FileInfo, FileInfo>> filesToBeChecked, List<CompareDirResultItem> result)
        {
            foreach (var leftEntry in leftEntries)
            {
                var leftIsFile = leftEntry.Value is FileInfo;

                if (rightEntries.TryGetValue(leftEntry.Key, out FileSystemInfo rightEntryValue))
                {
                    var rightIsFile = rightEntryValue is FileInfo;

                    if (leftIsFile ^ !rightIsFile)
                        if (!leftIsFile || ((FileInfo)leftEntry.Value).Length != ((FileInfo)rightEntryValue).Length)
                            result.Add(new CompareDirResultItem(leftEntry.Key, !leftIsFile, true, true, leftIsFile && !checkFilesContent));
                        else
                            filesToBeChecked.Add(leftEntry.Key, Tuple.Create((FileInfo)leftEntry.Value, (FileInfo)rightEntryValue));
                    else
                    {
                        result.Add(new CompareDirResultItem(leftEntry.Key, !leftIsFile, true, false, false));
                        result.Add(new CompareDirResultItem(leftEntry.Key, !rightIsFile, false, true, false));
                    }
                }
                else
                    result.Add(new CompareDirResultItem(leftEntry.Key, !leftIsFile, true, false, false));
            }

            foreach (var rightEntry in rightEntries)
                if (!leftEntries.ContainsKey(rightEntry.Key))
                    result.Add(new CompareDirResultItem(rightEntry.Key, !(rightEntry.Value is FileInfo), false, true, false));
        }

        static void CompareDirsCheckFileContent(bool checkFilesContent, CompareProgressCallback progressCallback, Dictionary<string, Tuple<FileInfo, FileInfo>> filesToBeChecked, List<CompareDirResultItem> result)
        {
            var totalLength = filesToBeChecked.Sum(x => x.Value.Item1.Length);
            var processedLength = 0L;

            var progressCallbackInvoker = progressCallback ?? ((lfp, rfp, pl, tl) => { });

            foreach (var fileToBeChecked in filesToBeChecked)
            {
                bool isIdentical;
                if (checkFilesContent)
                {
                    var previousFileProcessedLength = 0L;
                    isIdentical = CompareFiles(fileToBeChecked.Value.Item1.FullName, fileToBeChecked.Value.Item2.FullName,
                        (lfp, rfp, fpl, ftl) =>
                        {
                            processedLength += fpl - previousFileProcessedLength;
                            previousFileProcessedLength = fpl;
                            progressCallbackInvoker(lfp, rfp, processedLength, totalLength);
                        });
                }
                else
                    isIdentical = true;
                result.Add(new CompareDirResultItem(fileToBeChecked.Key, false, true, true, isIdentical));
            }
        }

        static IDictionary<string, ICollection<CompareDirResultItem>> CompareDirsSetDirsIsIdentical(List<CompareDirResultItem> result,
            out CompareDirResultItem root)
        {
            var tree = new Dictionary<string, ICollection<CompareDirResultItem>>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in result)
            {
                var parent = Path.GetDirectoryName(item.RelativePath);
                if (!tree.TryGetValue(parent, out ICollection<CompareDirResultItem> children))
                    tree.Add(parent, children = new List<CompareDirResultItem>());
                children.Add(item);
                if (item.IsDirectory && !tree.TryGetValue(item.RelativePath, out children))
                    tree.Add(item.RelativePath, new List<CompareDirResultItem>());
            }

            var rootLocal = new CompareDirResultItem(string.Empty, true, true, true, false);
            SetDirsIsIdentical(rootLocal);
            result.Add(rootLocal);

            root = rootLocal;
            return tree;

            bool SetDirsIsIdentical(CompareDirResultItem ri)
            {
                var children = tree[ri.RelativePath];
                var isIdentical = true;
                foreach (var child in children)
                    if (!child.IsDirectory && !child.IsIdentical || child.IsDirectory && (!child.ExistsInLeft || !child.ExistsInRight || !SetDirsIsIdentical(child)))
                        isIdentical = false;
                ri.IsIdentical = isIdentical;
                return isIdentical;
            }
        }

        static void CompareDirsCore(string leftDirPath, string rightDirPath, bool recursive,
            bool excludeDirs, bool checkFilesContent, CompareProgressCallback progressCallback,
            out ICollection<CompareDirResultItem> resultAsCollection,
            out IDictionary<string, ICollection<CompareDirResultItem>> resultAsTree,
            out CompareDirResultItem root)
        {
            // TODO: supporting hard and symbolic links?

            if (leftDirPath == null)
                throw new ArgumentNullException(nameof(leftDirPath));
            if (rightDirPath == null)
                throw new ArgumentNullException(nameof(rightDirPath));

            leftDirPath = Path.GetFullPath(leftDirPath);
            rightDirPath = Path.GetFullPath(rightDirPath);

            var leftPrefixLength = leftDirPath.Length + 1;
            var rightPrefixLength = rightDirPath.Length + 1;
            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var leftEntries =
                (!excludeDirs ?
                    new DirectoryInfo(leftDirPath).GetFileSystemInfos("*", searchOption) :
                    new DirectoryInfo(leftDirPath).GetFiles("*", searchOption))
                .ToDictionary(x => x.FullName.Substring(leftPrefixLength), Identity<FileSystemInfo>.Func, StringComparer.OrdinalIgnoreCase);
            var rightEntries =
                (!excludeDirs ?
                    new DirectoryInfo(rightDirPath).GetFileSystemInfos("*", searchOption) :
                    new DirectoryInfo(rightDirPath).GetFiles("*", searchOption))
                .ToDictionary(x => x.FullName.Substring(rightPrefixLength), Identity<FileSystemInfo>.Func, StringComparer.OrdinalIgnoreCase);

            var result = new List<CompareDirResultItem>();
            var filesToBeChecked = new Dictionary<string, Tuple<FileInfo, FileInfo>>();
            CompareDirsMergeEntries(leftEntries, rightEntries, checkFilesContent, filesToBeChecked, result);

            CompareDirsCheckFileContent(checkFilesContent, progressCallback, filesToBeChecked, result);

            root = null;
            resultAsTree = !excludeDirs ? CompareDirsSetDirsIsIdentical(result, out root) : null;
            resultAsCollection = result;
        }

        public static ICollection<CompareDirResultItem> CompareDirs(string leftDirPath, string rightDirPath, bool recursive = true,
            bool excludeDirs = false, bool checkFilesContent = true, CompareProgressCallback progressCallback = null)
        {
            CompareDirsCore(leftDirPath, rightDirPath, recursive, excludeDirs, checkFilesContent, progressCallback,
                out ICollection<CompareDirResultItem> resultAsCollection, out IDictionary<string, ICollection<CompareDirResultItem>> resultAsTree, out CompareDirResultItem root);

            return resultAsCollection;
        }

        public static IDictionary<string, ICollection<CompareDirResultItem>> CompareDirs(string leftDirPath, string rightDirPath, out CompareDirResultItem root,
            bool recursive = true, bool checkFilesContent = true, CompareProgressCallback progressCallback = null)
        {
            CompareDirsCore(leftDirPath, rightDirPath, recursive, false, checkFilesContent, progressCallback,
                out ICollection<CompareDirResultItem> resultAsCollection, out IDictionary<string, ICollection<CompareDirResultItem>> resultAsTree, out root);

            return resultAsTree;
        }

        public static bool CompareFiles(string leftFilePath, string rightFilePath, CompareProgressCallback progressCallback = null)
        {
            if (leftFilePath == null)
                throw new ArgumentNullException(nameof(leftFilePath));
            if (rightFilePath == null)
                throw new ArgumentNullException(nameof(rightFilePath));

            using (var leftFile = new FileStream(leftFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var rightFile = new FileStream(rightFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var leftFileLength = leftFile.Length;
                var rightFileLength = rightFile.Length;

                var progressCallbackInvoker = progressCallback ?? ((lfp, rfp, pl, tl) => { });

                try
                {
                    if (leftFileLength != rightFileLength)
                        return false;
                    if (leftFileLength == 0)
                        return true;

                    var leftBuffer = new byte[compareFilesBufferSize];
                    var rightBuffer = new byte[compareFilesBufferSize];
                    long position;

                    while ((position = leftFile.Position) < leftFileLength || rightFile.Position < rightFileLength)
                    {
                        progressCallbackInvoker(leftFilePath, rightFilePath, position, leftFileLength);

                        var leftBytesRead = leftFile.Read(leftBuffer, 0, compareFilesBufferSize);
                        var rightBytesRead = rightFile.Read(rightBuffer, 0, compareFilesBufferSize);

                        if (leftBytesRead != rightBytesRead)
                            throw new IOException();

                        if (!ArrayUtils.ContentEquals(leftBuffer, rightBuffer))
                            return false;
                    }

                    return true;
                }
                catch { throw; }
                finally { progressCallbackInvoker(leftFilePath, rightFilePath, Math.Max(leftFileLength, rightFileLength), Math.Max(leftFileLength, rightFileLength)); }
            }
        }
    }
}

