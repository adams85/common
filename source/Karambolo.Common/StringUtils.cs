using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Karambolo.Common
{
    public static class StringUtils
    {
        const string hexDigitLookup = "0123456789ABCDEF";

        public static string ByteArrayToHexString(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var byteCount = value.Length;
            var result = new char[byteCount << 1];
            var index = 0;
            for (var i = 0; i < byteCount; i++)
            {
                var @byte = value[i];
                result[index++] = hexDigitLookup[@byte >> 4];
                result[index++] = hexDigitLookup[@byte & 0xF];
            }

            return new string(result);
        }

        public static byte[] HexStringToByteArray(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if ((value.Length & 1) != 0)
                throw new FormatException();

            var byteCount = value.Length >> 1;
            var result = new byte[byteCount];
            var index = 0;
            for (var i = 0; i < byteCount; i++)
            {
                var hi = hexDigitLookup.IndexOf(char.ToUpperInvariant(value[index++]));
                if (hi < 0)
                    throw new FormatException();
                var lo = hexDigitLookup.IndexOf(char.ToUpperInvariant(value[index++]));
                if (lo < 0)
                    throw new FormatException();
                result[i] = (byte)(hi << 4 | lo);
            }

            return result;
        }

        public static IEnumerable<string> Split(this string @this, Func<char, bool> condition, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            string section;
            var startIndex = 0;
            for (var index = 0; index < @this.Length; index++)
            {
                if (condition(@this[index]))
                {
                    section = @this.Substring(startIndex, index - startIndex);
                    if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                        yield return section;

                    startIndex = index + 1;
                }
            }

            section = @this.Substring(startIndex);
            if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                yield return section;
        }

        /// <summary>
        /// Escapes special characters specified by array <paramref name="specialChars"/> of a string using character <paramref name="escapeChar"/>.
        /// </summary>
        public static string Escape(this string @this, char escapeChar, params char[] specialChars)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this == string.Empty)
                return string.Empty;

            var result = new StringBuilder();
            for (var index = 0; index < @this.Length; index++)
            {
                var c = @this[index];
                if (c == escapeChar || specialChars.Contains(c))
                    result.Append(escapeChar);
                result.Append(c);
            }
            return result.ToString();
        }

        /// <summary>
        /// Inverse operation of <see cref="Escape"/>.
        /// </summary>
        public static string Unescape(this string @this, char escapeChar, params char[] specialChars)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this == string.Empty)
                return string.Empty;

            var result = new StringBuilder();
            for (var index = 0; index < @this.Length; index++)
            {
                var c = @this[index];
                if (c == escapeChar)
                {
                    var cn = index + 1 < @this.Length ? (char?)@this[index + 1] : null;
                    if (cn == null || !(cn == escapeChar || specialChars.Contains(cn.Value)))
                        throw new FormatException();
                    result.Append(cn);
                    index++;
                    continue;
                }
                else if (specialChars.Contains(c))
                    throw new FormatException();
                result.Append(c);
            }
            return result.ToString();
        }

        public static int IndexOfEscaped(this string @this, char escapeChar, char value, int startIndex = 0)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (startIndex < 0 || startIndex >= @this.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (@this == string.Empty)
                return -1;

            Func<int, bool> checker;
            if (escapeChar != value)
                checker = j => true;
            else
                checker = j => j < @this.Length - 1 && @this[j + 1] == value;

            for (var i = startIndex; i < @this.Length; i++)
            {
                var c = @this[i];

                if (c == escapeChar && checker(i))
                    i++;
                else if (c == value)
                    return i;
            }

            return -1;
        }

        public static IEnumerable<string> SplitEscaped(this string @this, char escapeChar, char separatorChar, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this == string.Empty)
            {
                if (options != StringSplitOptions.RemoveEmptyEntries)
                    yield return string.Empty;
                yield break;
            }

            var startIndex = 0;
            do
            {
                var index = startIndex < @this.Length ? @this.IndexOfEscaped(escapeChar, separatorChar, startIndex) : -1;
                var section = index >= 0 ? @this.Substring(startIndex, index - startIndex) : @this.Substring(startIndex);
                if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                    yield return section.Unescape(escapeChar, separatorChar);
                startIndex = index + 1;
            }
            while (startIndex > 0);
        }

        public static string JoinEscaped(char escapeChar, char separatorChar, IEnumerable<string> values)
        {
            return string.Join(separatorChar.ToString(), values.Select(v => v.Escape(escapeChar, separatorChar)));
        }

        public static string Truncate(this string @this, int length)
        {
            return @this.Length <= length ? @this : @this.Substring(0, length);
        }

#if !NETSTANDARD1_2
        // http://weblogs.asp.net/fmarguerie/archive/2006/10/30/removing-diacritics-accents-from-strings.aspx
        public static string RemoveDiacritics(this string @this)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this == string.Empty)
                return string.Empty;

            var normalizedString = @this.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
#endif
    }
}
