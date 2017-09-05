﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Karambolo.Common.Finances
{
    [Serializable]
    public struct Currency : IEquatable<Currency>
    {
        class Metadata
        {
            public string Symbol { get; set; }
            public int DefaultDecimals { get; set; }
        }

        public static readonly Currency None = new Currency();

        static readonly Dictionary<string, Metadata> metadataLookup = new Dictionary<string, Metadata>(StringComparer.OrdinalIgnoreCase);

        static Currency()
        {
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var lcid = culture.LCID;
                var regionInfo = new RegionInfo(lcid);
                var code = regionInfo.ISOCurrencySymbol;
                metadataLookup[code] = new Metadata
                {
                    Symbol = regionInfo.CurrencySymbol,
                    DefaultDecimals = culture.NumberFormat.CurrencyDecimalDigits
                };
            }
        }

        public static void Register(string code, string symbol, int defaultDecimals)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (code.Length != 3 ||
                (code = code.ToUpperInvariant()).Any(c => c < 'A' || 'Z' < c))
                throw new ArgumentException(null, nameof(code));
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            metadataLookup[code] = new Metadata
            {
                Symbol = symbol,
                DefaultDecimals = defaultDecimals
            };
        }

        static Metadata GetMetadata(string code)
        {
            return !string.IsNullOrEmpty(code) && metadataLookup.TryGetValue(code, out Metadata metadata) ? metadata : null;
        }

        public static bool operator ==(Currency left, Currency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Currency left, Currency right)
        {
            return !left.Equals(right);
        }

        static int? Encode(string code)
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

        static string Decode(int data)
        {
            if (data == 0)
                return null;

            var chars = new char[3];
            for (var i = 0; i < 3; i++)
                chars[i] = (char)((data >> (i << 3) & 0xFF));

            return new string(chars);
        }

        static string SymbolToCode(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return null;

            return metadataLookup
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
                result = default(Currency);
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
                throw new ArgumentException(null, nameof(symbol));

            return new Currency(code);
        }

        readonly int _data;

        public Currency(string code)
        {
            var data = Encode(code);
            if (data == null)
                throw new ArgumentException(null, nameof(code));

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
            return obj is Currency && Equals((Currency)obj);
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
