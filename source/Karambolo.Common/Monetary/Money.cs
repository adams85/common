using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Karambolo.Common.Properties;

namespace Karambolo.Common.Monetary
{
#if !NETSTANDARD1_0
    [Serializable]
    public readonly struct Money : IEquatable<Money>, IComparable<Money>, IFormattable, IConvertible, IComparable
#else
    public readonly struct Money : IEquatable<Money>, IComparable<Money>, IFormattable, IComparable
#endif
    {
        public static readonly Money Zero = new Money(0);

        public static Money operator -(Money value)
        {
            return new Money(-value._amount, value._currency);
        }

        public static Money operator +(Money value)
        {
            return value;
        }

        public static Money operator +(Money left, Money right)
        {
            if (!left.IsCompatibleWith(right))
                throw new InvalidOperationException(Resources.IncompatibleCurrencies);

            return new Money(left._amount + right._amount, left.Currency);
        }

        public static Money operator -(Money left, Money right)
        {
            if (!left.IsCompatibleWith(right))
                throw new InvalidOperationException(Resources.IncompatibleCurrencies);

            return new Money(left._amount - right._amount, left.Currency);
        }

        public static Money operator *(Money left, decimal right)
        {
            return new Money(left._amount * right, left.Currency);
        }

        public static Money operator *(decimal left, Money right)
        {
            return new Money(left * right._amount, right.Currency);
        }

        public static Money operator /(Money left, decimal right)
        {
            return new Money(left._amount / right, left.Currency);
        }

        public static Money operator /(decimal left, Money right)
        {
            return new Money(left / right._amount, right.Currency);
        }

        public static bool operator ==(Money left, Money right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Money left, Money right)
        {
            return !left.Equals(right);
        }

        public static bool operator >(Money left, Money right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(Money left, Money right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(Money left, Money right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(Money left, Money right)
        {
            return left.CompareTo(right) <= 0;
        }

        private static string RemoveSignFromCurrencyString(string part, NumberFormatInfo numberFormat)
        {
            if (part.StartsWith(numberFormat.PositiveSign))
                return part.Remove(0, numberFormat.PositiveSign.Length);

            if (part.StartsWith(numberFormat.NegativeSign))
                return part.Remove(0, numberFormat.NegativeSign.Length);

            if (part.EndsWith(numberFormat.PositiveSign))
                return part.Remove(part.Length - numberFormat.PositiveSign.Length, numberFormat.PositiveSign.Length);

            if (part.EndsWith(numberFormat.NegativeSign))
                return part.Remove(part.Length - numberFormat.NegativeSign.Length, numberFormat.NegativeSign.Length);

            return part;
        }

        private static string ExtractCurrencyString(string value, NumberFormatInfo numberFormat)
        {
            var length = value.Length;
            if (length == 0)
                return null;

            // pre-number
            char c;
            var sb = new StringBuilder();
            var separator = numberFormat.CurrencyDecimalSeparator.AsEnumerable().First();
            for (int i = 0, n = length - 1; i < n; i++)
                if (!char.IsDigit(c = value[i]) && c != separator)
                    sb.Append(c);
                else
                    break;

            if (sb.Length > 0)
            {
                var part = sb.ToString().Trim();
                var index = part.IndexOf('(');
                if (index < 0)
                    part = RemoveSignFromCurrencyString(part, numberFormat);
                else if (index == 0)
                    part = part.Substring(1);
                else
                    part = part.Substring(0, index);

                part = part.Trim();
                if (part.Length > 0)
                    return part;
            }

            // post-number
            sb.Clear();
            separator = numberFormat.CurrencyDecimalSeparator.AsEnumerable().Last();
            for (var i = length - 1; i > 0; i--)
                if (!char.IsDigit(c = value[i]) && c != separator)
                    sb.Insert(0, c);
                else
                    break;

            if (sb.Length > 0)
            {
                var part = sb.ToString().Trim();
                var index = part.LastIndexOf(')');
                if (index < 0)
                    part = RemoveSignFromCurrencyString(part, numberFormat);
                else if (index == part.Length - 1)
                    part = part.Remove(part.Length - 1);
                else
                    part = part.Remove(0, index + 1);

                part = part.Trim();
                if (part.Length > 0)
                    return part;
            }

            return null;
        }

        public static bool TryParse(string value, out Money result)
        {
            return TryParse(value, null, out result);
        }

        public static bool TryParse(string value, IFormatProvider formatProvider, out Money result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            NumberFormatInfo numberFormat =
                (NumberFormatInfo)formatProvider?.GetFormat(typeof(NumberFormatInfo)) ??
                NumberFormatInfo.CurrentInfo;

            var currencyString = ExtractCurrencyString(value, numberFormat);
            if (!Currency.TryParse(currencyString, out Currency currency))
                currency = Currency.None;

            numberFormat = (NumberFormatInfo)numberFormat.Clone();
            numberFormat.CurrencySymbol = currency != Currency.None ? currencyString : "¤";

            if (decimal.TryParse(value, NumberStyles.Currency, numberFormat, out decimal amount))
            {
                result = new Money(amount, currency);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static Money Parse(string value, IFormatProvider formatProvider)
        {
            if (TryParse(value, formatProvider, out Money result))
                return result;
            else
                throw new FormatException();
        }

        public static Money Parse(string value)
        {
            return Parse(value, null);
        }

        public static Money Round(Money value, int? decimals = null, MidpointRounding mode = MidpointRounding.ToEven)
        {
            return new Money(Math.Round(value._amount, decimals ?? value._currency.DefaultDecimals, mode), value._currency);
        }

        public static Money ChangeAmount(Money value, decimal amount)
        {
            return new Money(amount, value._currency);
        }

        public static Money ChangeCurrency(Money value, Currency currency)
        {
            return new Money(value._amount, currency);
        }

        public static Money ChangeCurrency(Money value, string currencyCode)
        {
            return ChangeCurrency(value, Currency.FromCode(currencyCode));
        }

        private readonly decimal _amount;
        private readonly Currency _currency;

        public Money(decimal amount)
            : this(amount, Currency.None) { }

        public Money(decimal amount, string currencyCode)
            : this(amount, Currency.FromCode(currencyCode)) { }

        public Money(decimal amount, Currency currency)
        {
            _amount = amount;
            _currency = currency;
        }

        public decimal Amount => _amount;

        public Currency Currency => _currency;

        public bool IsCompatibleWith(Money money)
        {
            return _currency == money._currency;
        }

        public override int GetHashCode()
        {
            int hashCode = -259941593;
            hashCode = hashCode * -1521134295 + _currency.GetHashCode();
            hashCode = hashCode * -1521134295 + _amount.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is Money money && Equals(money);
        }

        private NumberFormatInfo CustomizeNumberFormat(NumberFormatInfo numberFormat)
        {
            var clone = (NumberFormatInfo)numberFormat.Clone();
            clone.CurrencyDecimalDigits = _currency.DefaultDecimals;
            clone.CurrencySymbol = _currency.Symbol ?? _currency.Code ?? string.Empty;
            return clone;
        }

        public override string ToString()
        {
            return ToString("C", null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public bool Equals(Money other)
        {
            return IsCompatibleWith(other) && _amount == other._amount;
        }

        public int CompareTo(Money other)
        {
            if (!IsCompatibleWith(other))
                throw new InvalidOperationException(Resources.IncompatibleCurrencies);

            return _amount.CompareTo(other._amount);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is Money))
                throw new InvalidOperationException(string.Format(Resources.UnexpectedObjectType, typeof(Money)));

            return CompareTo((Money)obj);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            NumberFormatInfo numberFormat =
                (NumberFormatInfo)formatProvider?.GetFormat(typeof(NumberFormatInfo)) ??
                NumberFormatInfo.CurrentInfo;

            return _amount.ToString(format, CustomizeNumberFormat(numberFormat));
        }

#if !NETSTANDARD1_0
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(_amount);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(_amount);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_amount);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_amount);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(_amount);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_amount);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_amount);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_amount);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(_amount);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(_amount);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return _amount;
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return ToString("C", formatProvider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotSupportedException();
        }
#endif
    }
}
