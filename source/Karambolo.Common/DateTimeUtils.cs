using Karambolo.Common.Localization;
using System;
using System.Text;

namespace Karambolo.Common
{
    public static class DateTimeUtils
    {
        class PeriodDescriptor
        {
            public PeriodDescriptor(string singularName, string pluralName, int divisor)
            {
                SingularName = singularName;
                PluralName = pluralName;
                Divisor = divisor;
            }

            public string SingularName { get; private set; }
            public string PluralName { get; private set; }
            public int Divisor { get; private set; }
        }

        public const string DefaultPastFormat = "{0} ago";
        public const string DefaultNowText = "now";
        public const string DefaultFutureFormat = "in {0}";

        static readonly PeriodDescriptor[] periods = new[]
        {
            new PeriodDescriptor("year", "years", 31_536_000),
            new PeriodDescriptor("month", "months", 2_628_000),
            new PeriodDescriptor("week", "weeks", 604_800),
            new PeriodDescriptor("day", "days", 86_400),
            new PeriodDescriptor("hour", "hours", 3_600),
            new PeriodDescriptor("minute", "minutes", 60),
            new PeriodDescriptor("second", "seconds", 1),
        };

        public static string ToTimeReference(this TimeSpan @this)
        {
            return @this.ToTimeReference(2);
        }

        public static string ToTimeReference(this TimeSpan @this, int precision)
        {
            return @this.ToTimeReference(precision, NullTextLocalizer.Instance);
        }

        public static string ToTimeReference(this TimeSpan @this, int precision, TextLocalizer textLocalizer)
        {
            return @this.ToTimeReference(precision, DefaultPastFormat, DefaultNowText, DefaultFutureFormat, NullTextLocalizer.Instance);
        }

        public static string ToTimeReference(this TimeSpan @this, int precision, string pastFormat, string nowText, string futureFormat, TextLocalizer textLocalizer)
        {
            var periodCount = periods.Length;
            if (precision < 1 || periodCount < precision)
                throw new ArgumentOutOfRangeException(nameof(precision));

            var total = @this.Ticks / TimeSpan.TicksPerSecond;
            if (total == 0)
                return textLocalizer(nowText);

            var remainder = Math.Abs(total);

            int index, value = 0;
            for (index = 0; index < periods.Length; index++)
                if ((value = GetValue(ref remainder, index)) > 0)
                    break;

            var builder = new StringBuilder();
            AddValue(builder, index++, value, textLocalizer);
            precision--;

            for (; index < periods.Length && precision > 0; index++, precision--)
                if ((value = GetValue(ref remainder, index)) > 0)
                {
                    builder.Append(' ');
                    AddValue(builder, index, value, textLocalizer);
                }
                else
                    break;

            return textLocalizer(total > 0 ? futureFormat : pastFormat, args: new[] { builder.ToString() });

            int GetValue(ref long r, int i) => (int)Math.DivRem(r, periods[i].Divisor, out r);

            void AddValue(StringBuilder sb, int i, int v, TextLocalizer t)
            {
                sb.Append(v);
                sb.Append(' ');
                sb.Append(t(v > 1 ? periods[i].PluralName : periods[i].SingularName));
            }
        }
    }
}
