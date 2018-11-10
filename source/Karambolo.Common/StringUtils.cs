using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Karambolo.Common
{
    public static class StringUtils
    {
        public static string ToHexString(byte[] value, bool upperCase = false)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var digitLookup = !upperCase ? "0123456789abcdef" : "0123456789ABCDEF";

            var byteCount = value.Length;
            var result = new char[byteCount << 1];
            var index = 0;
            for (var i = 0; i < byteCount; i++)
            {
                var @byte = value[i];
                result[index++] = digitLookup[@byte >> 4];
                result[index++] = digitLookup[@byte & 0xF];
            }

            return new string(result);
        }

        public static byte[] FromHexString(string value)
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
                int hi, lo;
                if ((hi = GetDigitValue(value[index++])) < 0 ||
                    (lo = GetDigitValue(value[index++])) < 0)
                    throw new FormatException();

                result[i] = (byte)(hi << 4 | lo);
            }

            return result;

            int GetDigitValue(char digit)
            {
                if ('0' <= digit && digit <= '9')
                    return digit - 0x30;
                else if ('a' <= digit && digit <= 'f')
                    return digit - 0x57;
                else if ('A' <= digit && digit <= 'F')
                    return digit - 0x37;
                else
                    return -1;
            }
        }

        public static int FindIndex(this string @string, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.FindIndex(0, @string.Length, match);
        }

        public static int FindIndex(this string @string, int startIndex, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.FindIndex(startIndex, @string.Length - startIndex, match);
        }

        public static int FindIndex(this string @string, int startIndex, int count, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            if (startIndex < 0 || length < startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            var endIndex = startIndex + count;
            if (count < 0 || length < endIndex)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (; startIndex < endIndex; startIndex++)
                if (match(@string[startIndex]))
                    return startIndex;

            return -1;
        }

        public static int FindLastIndex(this string @string, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            return @string.FindLastIndex(length - 1, length, match);
        }

        public static int FindLastIndex(this string @string, int startIndex, Predicate<char> match)
        {
            return @string.FindLastIndex(startIndex, startIndex + 1, match);
        }

        public static int FindLastIndex(this string @string, int startIndex, int count, Predicate<char> match)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            if (startIndex < -1 || length <= startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            var endIndex = startIndex - count;
            if (count < 0 || endIndex < -1)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (; startIndex > endIndex; startIndex--)
                if (match(@string[startIndex]))
                    return startIndex;

            return -1;
        }

        public static IEnumerable<string> Split(this string @string, Predicate<char> match, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            string section;
            var startIndex = 0;
            for (var index = 0; index < @string.Length; index++)
            {
                if (match(@string[index]))
                {
                    section = @string.Substring(startIndex, index - startIndex);
                    if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                        yield return section;

                    startIndex = index + 1;
                }
            }

            section = @string.Substring(startIndex);
            if (options != StringSplitOptions.RemoveEmptyEntries || section.Length > 0)
                yield return section;
        }

        public static string Truncate(this string @string, int length)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.Length <= length ? @string : @string.Substring(0, length);
        }

        /// <summary>
        /// Escapes special characters specified by array <paramref name="specialChars"/> of a string using character <paramref name="escapeChar"/>.
        /// </summary>
        public static string Escape(this string @string, char escapeChar, params char[] specialChars)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (@string.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            for (var index = 0; index < @string.Length; index++)
            {
                var c = @string[index];
                if (c == escapeChar || specialChars.Contains(c))
                    result.Append(escapeChar);
                result.Append(c);
            }
            return result.ToString();
        }

        /// <summary>
        /// Inverse operation of <see cref="Escape"/>.
        /// </summary>
        public static string Unescape(this string @string, char escapeChar, params char[] specialChars)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (@string.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            for (var index = 0; index < @string.Length; index++)
            {
                var c = @string[index];
                if (c == escapeChar)
                {
                    var cn = index + 1 < @string.Length ? (char?)@string[index + 1] : null;
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

        public static int IndexOfEscaped(this string @string, char escapeChar, char value)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.IndexOfEscaped(escapeChar, value, 0, @string.Length);
        }

        public static int IndexOfEscaped(this string @string, char escapeChar, char value, int startIndex)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            return @string.IndexOfEscaped(escapeChar, value, startIndex, @string.Length - startIndex);
        }

        public static int IndexOfEscaped(this string @string, char escapeChar, char value, int startIndex, int count)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            if (startIndex < 0 || length < startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            var endIndex = startIndex + count;
            if (count < 0 || length < endIndex)
                throw new ArgumentOutOfRangeException(nameof(count));

            Func<string, int, int, char, bool> isEscaped;
            if (escapeChar == value)
                isEscaped = (s, i, endIdx, val) => IsEscaped(s, i, endIdx, val);
            else
                isEscaped = (s, i, endIdx, val) => true;

            for (; startIndex < endIndex; startIndex++)
            {
                var c = @string[startIndex];

                if (c == escapeChar && isEscaped(@string, startIndex, endIndex, value))
                    startIndex++;
                else if (c == value)
                    return startIndex;
            }

            return -1;

            bool IsEscaped(string s, int i, int endIdx, char val)
            {
                return i < endIdx - 1 && s[i + 1] == val;
            }
        }

        public static int LastIndexOfEscaped(this string @string, char escapeChar, char value)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            return @string.LastIndexOfEscaped(escapeChar, value, length - 1, length);
        }

        public static int LastIndexOfEscaped(this string @string, char escapeChar, char value, int startIndex)
        {
            return @string.LastIndexOfEscaped(escapeChar, value, startIndex, startIndex + 1);
        }

        public static int LastIndexOfEscaped(this string @string, char escapeChar, char value, int startIndex, int count)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            var length = @string.Length;
            if (startIndex < -1 || length <= startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            var endIndex = startIndex - count;
            if (count < 0 || endIndex < -1)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (; startIndex > endIndex; startIndex--)
            {
                var c = @string[startIndex];

                if (c == value)
                    if (IsEscaped(@string, startIndex, endIndex, escapeChar))
                        startIndex--;
                    else
                        return startIndex;
            }

            return -1;

            bool IsEscaped(string s, int i, int endIdx, char escChar)
            {
                bool result = false;
                for (i--; i > endIdx; i--)
                    if (s[i] == escChar)
                        result = !result;
                    else
                        break;

                return result;
            }
        }

        public static IEnumerable<string> SplitEscaped(this string @string, char escapeChar, char separatorChar, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (@string.Length == 0)
            {
                if (options != StringSplitOptions.RemoveEmptyEntries)
                    yield return string.Empty;
                yield break;
            }

            var startIndex = 0;
            do
            {
                var index = startIndex < @string.Length ? @string.IndexOfEscaped(escapeChar, separatorChar, startIndex) : -1;
                var section = index >= 0 ? @string.Substring(startIndex, index - startIndex) : @string.Substring(startIndex);
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

#if !NETSTANDARD1_0
        // http://weblogs.asp.net/fmarguerie/archive/2006/10/30/removing-diacritics-accents-from-strings.aspx
        public static string RemoveDiacritics(this string @string)
        {
            if (@string == null)
                throw new ArgumentNullException(nameof(@string));

            if (@string.Length == 0)
                return string.Empty;

            var normalizedString = @string.Normalize(NormalizationForm.FormD);
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
