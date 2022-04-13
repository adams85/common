using System;
using System.Collections.Generic;
using System.Globalization;
using Karambolo.Common.Properties;

namespace Karambolo.Common.Monetary
{
#if !NETSTANDARD1_0
    [Serializable]
#endif
    public readonly struct Currency : IEquatable<Currency>
    {
        private sealed class Metadata
        {
            public Metadata(string symbol, int defaultDecimals)
            {
                Symbol = symbol;
                DefaultDecimals = defaultDecimals;
            }

            public readonly string Symbol;
            public readonly int DefaultDecimals;
        }

        private static readonly Dictionary<int, Metadata> s_metadataLookup = new Dictionary<int, Metadata>();

        private const int NumberOfLetters = 'Z' - 'A' + 1;

        public static readonly Currency None = new Currency();

#if !NETSTANDARD1_0
        public static void RegisterSystemDefaults()
        {
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var lcid = culture.LCID;
                var regionInfo = new RegionInfo(lcid);
                var code = regionInfo.ISOCurrencySymbol;

                var encodedValue = Encode(code);
                if (encodedValue == null || encodedValue.Value == 0)
                    continue;

                s_metadataLookup[encodedValue.Value] = new Metadata(regionInfo.CurrencySymbol, culture.NumberFormat.CurrencyDecimalDigits);
            }
        }
#endif

        public static void Register(string code, string symbol, int defaultDecimals)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            var encodedValue = Encode(code);
            if (encodedValue == null || encodedValue.Value == 0)
                throw new ArgumentException(Resources.InvalidValue, nameof(code));

            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            // https://github.com/dotnet/runtime/blob/v6.0.4/src/libraries/System.Private.CoreLib/src/System/Decimal.cs#L668
            if ((uint)defaultDecimals > 28)
                throw new ArgumentOutOfRangeException(nameof(defaultDecimals));

            s_metadataLookup[encodedValue.Value] = new Metadata(symbol, defaultDecimals);
        }

        public static bool IsRegistered(string code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            var encodedValue = Encode(code);
            if (encodedValue == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(code));

            return encodedValue.Value != 0 ? s_metadataLookup.ContainsKey(encodedValue.Value) : false;
        }

        private static Metadata GetMetadata(int encodedValue)
        {
            return encodedValue != 0 && s_metadataLookup.TryGetValue(encodedValue, out Metadata metadata) ? metadata : null;
        }

        public static bool operator ==(Currency left, Currency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Currency left, Currency right)
        {
            return !left.Equals(right);
        }

        private static int? Encode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return 0;

            int length = code.Length;
            if (length < 3 || 6 < length)
                return null;

            var value = 0;
            for (var i = length - 1; i >= 0; i--)
            {
                char c = code[i];

                int letterValue;
                if ('A' <= c && c <= 'Z')
                    letterValue = c - 'A';
                else if ('a' <= c && c <= 'z')
                    letterValue = c - 'a';
                else
                    return null;

                value = value * NumberOfLetters + letterValue;
            }

            return length << 29 | value;
        }

        private static string Decode(int encodedValue)
        {
            if (encodedValue == 0)
                return null;

            var length = (int)((uint)encodedValue >> 29);
            const int valueBitMask = ~(7 << 29);

#if !NETSTANDARD2_1_OR_GREATER
            var chars = new char[length];

            var value = encodedValue & valueBitMask;
#else
            return string.Create(length, encodedValue & valueBitMask, (chars, value) =>
            {
#endif
                for (int i = 0, n = chars.Length; i < n; i++)
                {
                    value = MathShim.DivRem(value, NumberOfLetters, out var letterValue);
                    chars[i] = (char)(letterValue + 'A');
                }

#if NETSTANDARD2_1_OR_GREATER
            });
#else
            return new string(chars);
#endif
        }

        private static int? SymbolToEncodedValue(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return 0;

            foreach (KeyValuePair<int, Metadata> kvp in s_metadataLookup)
                if (string.Equals(kvp.Value.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;

            return null;
        }

        public static bool TryParse(string value, out Currency result)
        {
            var encodedValue = SymbolToEncodedValue(value) ?? Encode(value);
            if (encodedValue == null)
            {
                result = default;
                return false;
            }

            result = new Currency(encodedValue.Value);
            return true;
        }

        public static Currency Parse(string value)
        {
            return TryParse(value, out Currency result) ? result : throw new FormatException();
        }

        public static Currency FromCode(string code)
        {
            return new Currency(code);
        }

        public static Currency FromSymbol(string symbol)
        {
            var encodedValue = SymbolToEncodedValue(symbol);
            if (encodedValue == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(symbol));

            return new Currency(encodedValue.Value);
        }

        private readonly int _encodedValue;

        private Currency(int encodedValue)
        {
            _encodedValue = encodedValue;
        }

        public Currency(string code)
            : this(Encode(code) ?? throw new ArgumentException(Resources.InvalidValue, nameof(code))) { }

        public string Code => Decode(_encodedValue);

        public string Symbol => GetMetadata(_encodedValue)?.Symbol;

        public int DefaultDecimals => GetMetadata(_encodedValue)?.DefaultDecimals ?? 2;

        public bool Equals(Currency other)
        {
            return _encodedValue == other._encodedValue;
        }

        public override bool Equals(object obj)
        {
            return obj is Currency currency && Equals(currency);
        }

        public override int GetHashCode()
        {
            return _encodedValue.GetHashCode();
        }

        public override string ToString()
        {
            return Code ?? string.Empty;
        }
    }
}
