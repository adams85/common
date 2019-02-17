using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public static class PathUtils
    {
        static readonly HashSet<char> illegalFileNameChars;
        static readonly HashSet<string> reservedFileNames;
        static readonly Func<string, bool> isReservedFileName;

        static PathUtils()
        {
            illegalFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));

            bool isWindowsOS =
#if NETSTANDARD2_0
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
                true;
#endif

            if (isWindowsOS)
            {
                reservedFileNames = new HashSet<string>
                {
                    "con", "prn", "aux", "nul",
                    "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9",
                    "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9",
                };
                isReservedFileName = IsReservedFileNameWindows;
            }
            else
                isReservedFileName = IsReservedFileNameOther;

        }

        static bool IsReservedFileNameWindows(string value)
        {
            return reservedFileNames.Contains(Path.GetFileNameWithoutExtension(value));
        }

        static bool IsReservedFileNameOther(string value)
        {
            return false;
        }

        public static bool IsReservedFileName(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return isReservedFileName(value);
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
                if (illegalFileNameChars.Contains(value[i]))
                    (chars ?? (chars = value.ToCharArray()))[i] = '_';

            if (chars != null)
                return new string(chars);

            if (isReservedFileName(value))
                return "_" + value;

            return value.Truncate(maxLength);
        }
    }
}

