using System.Linq;
using System.Text;
using System.IO;

namespace Karambolo.Common
{
    public static partial class FileUtils
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

