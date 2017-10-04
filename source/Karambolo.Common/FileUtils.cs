using System.Linq;
using System.Text;
using System.IO;

namespace Karambolo.Common
{
    public static partial class FileUtils
    {
        static readonly char[] illegalChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();

        public static string MakeValidFileName(string value)
        {
            var result = new StringBuilder(value);
            for (var i = 0; i < result.Length; i++)
                if (illegalChars.Contains(result[i]))
                    result[i] = '_';
            return result.ToString();
        }
    }
}

