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

        public static int FindIndex(this string @this, Func<char, bool> match)
        {
            return @this.FindIndex(match, 0, @this.Length);
        }

        public static int FindIndex(this string @this, Func<char, bool> match, int startIndex)
        {
            return @this.FindIndex(match, startIndex, @this.Length - startIndex);
        }

        public static int FindIndex(this string @this, Func<char, bool> match, int startIndex, int count)
        {
            var length = @this.Length;
            var endIndex = startIndex + count;

            if (startIndex < 0 || length < startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || length < endIndex)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (; startIndex < endIndex; startIndex++)
                if (match(@this[startIndex]))
                    return startIndex;

            return -1;
        }

        public static int FindLastIndex(this string @this, Func<char, bool> match)
        {
            var length = @this.Length;
            return @this.FindLastIndex(match, length - 1, length);
        }

        public static int FindLastIndex(this string @this, Func<char, bool> match, int startIndex)
        {
            return @this.FindLastIndex(match, startIndex, startIndex + 1);
        }

        public static int FindLastIndex(this string @this, Func<char, bool> match, int startIndex, int count)
        {
            var length = @this.Length;
            var endIndex = startIndex - count;

            if (startIndex < -1 || length <= startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || endIndex < -1)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (; startIndex > endIndex; startIndex--)
                if (match(@this[startIndex]))
                    return startIndex;

            return -1;
        }

        public static IEnumerable<string> Split(this string @this, Func<char, bool> match, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            string section;
            var startIndex = 0;
            for (var index = 0; index < @this.Length; index++)
            {
                if (match(@this[index]))
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

        public static string Truncate(this string @this, int length)
        {
            return @this.Length <= length ? @this : @this.Substring(0, length);
        }

        /// <summary>
        /// Escapes special characters specified by array <paramref name="specialChars"/> of a string using character <paramref name="escapeChar"/>.
        /// </summary>
        public static string Escape(this string @this, char escapeChar, params char[] specialChars)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this.Length == 0)
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

            if (@this.Length == 0)
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

        public static int IndexOfEscaped(this string @this, char escapeChar, char value)
        {
            return @this.IndexOfEscaped(escapeChar, value, 0, @this.Length);
        }

        public static int IndexOfEscaped(this string @this, char escapeChar, char value, int startIndex)
        {
            return @this.IndexOfEscaped(escapeChar, value, startIndex, @this.Length - startIndex);
        }

        public static int IndexOfEscaped(this string @this, char escapeChar, char value, int startIndex, int count)
        {
            var length = @this.Length;
            var endIndex = startIndex + count;

            if (startIndex < 0 || length < startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || length < endIndex)
                throw new ArgumentOutOfRangeException(nameof(count));

            Func<string, int, int, char, bool> isEscaped;
            if (escapeChar == value)
                isEscaped = (s, i, endIdx, val) => IsEscaped(s, i, endIdx, val);
            else
                isEscaped = (s, i, endIdx, val) => true;

            for (; startIndex < endIndex; startIndex++)
            {
                var c = @this[startIndex];

                if (c == escapeChar && isEscaped(@this, startIndex, endIndex, value))
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

        public static int LastIndexOfEscaped(this string @this, char escapeChar, char value)
        {
            var length = @this.Length;
            return @this.LastIndexOfEscaped(escapeChar, value, length - 1, length);
        }

        public static int LastIndexOfEscaped(this string @this, char escapeChar, char value, int startIndex)
        {
            return @this.LastIndexOfEscaped(escapeChar, value, startIndex, startIndex + 1);
        }

        public static int LastIndexOfEscaped(this string @this, char escapeChar, char value, int startIndex, int count)
        {
            var length = @this.Length;
            var endIndex = startIndex - count;

            if (startIndex < -1 || length <= startIndex)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count < 0 || endIndex < -1)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (; startIndex > endIndex; startIndex--)
            {
                var c = @this[startIndex];

                if (c == value)
                    if (IsEscaped(@this, startIndex, endIndex, escapeChar))
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

        public static IEnumerable<string> SplitEscaped(this string @this, char escapeChar, char separatorChar, StringSplitOptions options = StringSplitOptions.None)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this.Length == 0)
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

#if !NETSTANDARD1_0
        // http://weblogs.asp.net/fmarguerie/archive/2006/10/30/removing-diacritics-accents-from-strings.aspx
        public static string RemoveDiacritics(this string @this)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (@this.Length == 0)
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
