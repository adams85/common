using System;
using Karambolo.Common.Properties;

namespace Karambolo.Common.Finances
{
#if !NETSTANDARD1_0
    [System.Serializable]
#endif
    public sealed class Conversion
    {
        readonly Currency _from;
        readonly Currency _to;
        readonly decimal _rate;

        public Conversion(Currency from, Currency to, decimal rate)
        {
            if (rate <= 0 ||
                from == to && rate != 1m)
                throw new ArgumentException(Resources.InvalidValue, nameof(rate));

            _from = from;
            _to = to;
            _rate = rate;
        }

        public Conversion(string fromCode, string toCode, decimal rate)
            : this(Currency.FromCode(fromCode), Currency.FromCode(toCode), rate) { }

        public Currency From => _from;

        public Currency To => _to;

        public decimal Rate => _rate;

        public Money Convert(decimal value, string currency)
        {
            return Convert(new Money(value, currency));
        }

        public Money Convert(Money value)
        {
            if (value.Currency != _from)
                throw new ArgumentException(Resources.IncompatibleCurrencies, nameof(value));

            return Money.ChangeCurrency(value, _to) * _rate;
        }

        public Money ConvertBack(decimal value, string currency)
        {
            return Convert(new Money(value, currency));
        }

        public Money ConvertBack(Money value)
        {
            if (value.Currency != _to)
                throw new ArgumentException(Resources.IncompatibleCurrencies, nameof(value));

            return Money.ChangeCurrency(value, _from) / _rate;
        }

        public Conversion Invert()
        {
            return new Conversion(_to, _from, 1m / _rate);
        }

        public override string ToString()
        {
            return $"{_from}/{_to} = {_rate:#,##0.00000}";
        }
    }
}
