using System;
using System.IO;
using System.Linq;

namespace Karambolo.Common
{
    public static class PathUtils
    {
        static readonly char[] illegalFileNameChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();

        public static string MakeValidFileName(string value)
        {
            var chars = value.ToCharArray();

            var n = chars.Length;
            for (var i = 0; i < n; i++)
                if (Array.IndexOf(illegalFileNameChars, chars[i]) >= 0)
                    chars[i] = '_';

            return new string(chars);
        }
    }
}

