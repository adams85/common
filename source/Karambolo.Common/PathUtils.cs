﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public static class PathUtils
    {
        private static readonly HashSet<char> s_illegalFileNameChars;
        private static readonly HashSet<string> s_reservedFileNames;
        private static readonly Func<string, bool> s_isReservedFileName;
        private static readonly Func<string, string, bool> s_areEqualPaths;

        static PathUtils()
        {
            s_illegalFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));

            if (Platform.IsWindowsOS == true)
            {
                s_reservedFileNames = new HashSet<string>
                {
                    "con", "prn", "aux", "nul",
                    "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
                    "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
                };
                s_isReservedFileName = value => s_reservedFileNames.Contains(Path.GetFileNameWithoutExtension(value));
                s_areEqualPaths = (x, y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                s_isReservedFileName = False<string>.Func;
                s_areEqualPaths = string.Equals;
            }
        }

        public static bool IsReservedFileName(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return s_isReservedFileName(value);
        }

        /// <remarks>
        /// Maximum length of filenames depends on the underlying filesystem.
        /// <seealso href="https://serverfault.com/questions/9546/filename-length-limits-on-linux" />
        /// </remarks>
        public static string MakeValidFileName(string value, int maxLength = 255)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                throw new ArgumentException(Resources.InvalidValue, nameof(value));

            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            if (value.FindIndex(c => !char.IsWhiteSpace(c)) < 0)
                return new string('_', value.Length);

            char[] chars = null;
            var n = Math.Min(value.Length, maxLength);
            for (var i = 0; i < n; i++)
                if (s_illegalFileNameChars.Contains(value[i]))
                    (chars ?? (chars = value.ToCharArray()))[i] = '_';

            if (chars != null)
                return new string(chars);

            if (s_isReservedFileName(value))
                value = "_" + value;

            return value.Truncate(maxLength);
        }

        internal static string MakeRelativePathCore(string basePath, string path)
        {
            // dealing with trailing backslashes
            var basePathLength = basePath.Length;
            if (basePath[basePathLength - 1] == Path.DirectorySeparatorChar) // index access is safe, Path.GetFullPath doesn't allow empty strings
                basePathLength--;

            var pathLength = path.Length;
            if (path[pathLength - 1] == Path.DirectorySeparatorChar) // index access is safe, Path.GetFullPath doesn't allow empty strings
                pathLength--;

            // determining common part
            char c;
            var length = Math.Min(basePathLength, pathLength);
            int index, lastSeparatorIndex;
            for (index = 0, lastSeparatorIndex = 0; index < length; index++)
                if ((c = basePath[index]) != path[index])
                    break;
                else if (c == Path.DirectorySeparatorChar)
                    lastSeparatorIndex = index;

            // trivial case: basePath is just a prefix of path
            if (index == basePathLength)
                if (pathLength == basePathLength)
                    return lastSeparatorIndex > 0 && path.Length > pathLength ? "." + Path.DirectorySeparatorChar : string.Empty;
                else if (path[index] == Path.DirectorySeparatorChar)
                    return path.Substring(index + 1);

            // no common part
            if (lastSeparatorIndex == 0 && index < pathLength && basePath[0] != Path.DirectorySeparatorChar)
                throw new ArgumentException(Resources.NoCommonBasePath, nameof(path));

            // determining relative part
            lastSeparatorIndex++;
            var subDirCount = 0;
            for (index = lastSeparatorIndex + 1; index < basePathLength; index++)
                if (basePath[index] == Path.DirectorySeparatorChar)
                    subDirCount++;

            length = Math.Max(pathLength - lastSeparatorIndex, 0);
            var sb = new StringBuilder(subDirCount * 3 + length);

            sb.Append("..");
            for (index = 0; index < subDirCount; index++)
                sb.Append(Path.DirectorySeparatorChar).Append("..");

            if (length > 0)
                sb.Append(Path.DirectorySeparatorChar).Append(path.Substring(lastSeparatorIndex));

            return sb.ToString();
        }

        public static string MakeRelativePath(string basePath, string path)
        {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            // normalizing paths
            return MakeRelativePathCore(Path.GetFullPath(basePath), Path.GetFullPath(path));
        }
    }
}

