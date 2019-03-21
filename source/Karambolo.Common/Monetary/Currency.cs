using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Karambolo.Common.Properties;

namespace Karambolo.Common.Monetary
{
#if !NETSTANDARD1_0
    [Serializable]
#endif
    public readonly struct Currency : IEquatable<Currency>
    {
        private class Metadata
        {
            public string Symbol { get; set; }
            public int DefaultDecimals { get; set; }
        }

        public static readonly Currency None = new Currency();
        private static readonly Dictionary<string, Metadata> s_metadataLookup = new Dictionary<string, Metadata>(StringComparer.OrdinalIgnoreCase);

#if !NETSTANDARD1_0
        public static void RegisterSystemDefaults()
        {
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var lcid = culture.LCID;
                var regionInfo = new RegionInfo(lcid);
                var code = regionInfo.ISOCurrencySymbol;
                s_metadataLookup[code] = new Metadata
                {
                    Symbol = regionInfo.CurrencySymbol,
                    DefaultDecimals = culture.NumberFormat.CurrencyDecimalDigits
                };
            }
        }
#endif

        public static void Register(string code, string symbol, int defaultDecimals)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (code.Length != 3 ||
                (code = code.ToUpperInvariant()).AsEnumerable().Any(c => c < 'A' || 'Z' < c))
                throw new ArgumentException(Resources.InvalidValue, nameof(code));
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            s_metadataLookup[code] = new Metadata
            {
                Symbol = symbol,
                DefaultDecimals = defaultDecimals
            };
        }

        public static bool IsRegistered(string code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            return s_metadataLookup.ContainsKey(code);
        }

        private static Metadata GetMetadata(string code)
        {
            return !string.IsNullOrEmpty(code) && s_metadataLookup.TryGetValue(code, out Metadata metadata) ? metadata : null;
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

            if (code.Length != 3)
                return null;

            var result = 0;
            for (var i = 0; i < 3; i++)
            {
                var c = char.ToUpperInvariant(code[i]);
                if (c < 'A' || 'Z' < c)
                    return null;

                result |= c << (i << 3);
            }
            return result;
        }

        private static string Decode(int data)
        {
            if (data == 0)
                return null;

            var chars = new char[3];
            for (var i = 0; i < 3; i++)
                chars[i] = (char)((data >> (i << 3) & 0xFF));

            return new string(chars);
        }

        private static string SymbolToCode(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return null;

            return s_metadataLookup
                .FirstOrDefault(kvp => string.Equals(kvp.Value.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
                .Key;
        }

        public static bool TryParse(string value, out Currency result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = None;
                return true;
            }

            var code = SymbolToCode(value) ?? value;
            if (Encode(code) == null)
            {
                result = default;
                return false;
            }

            result = new Currency(code);
            return true;
        }

        public static Currency FromCode(string code)
        {
            return new Currency(code);
        }

        public static Currency FromSymbol(string symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            var code = SymbolToCode(symbol);
            if (code == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(symbol));

            return new Currency(code);
        }

        private readonly int _data;

        public Currency(string code)
        {
            var data = Encode(code);
            if (data == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(code));

            _data = data.Value;
        }

        public string Code => Decode(_data);

        public string Symbol => GetMetadata(Code)?.Symbol;

        public int DefaultDecimals => GetMetadata(Code)?.DefaultDecimals ?? 2;

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Currency currency && Equals(currency);
        }

        public override string ToString()
        {
            return Code ?? string.Empty;
        }

        public bool Equals(Currency other)
        {
            return _data == other._data;
        }
    }

}
