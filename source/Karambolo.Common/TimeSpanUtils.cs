using System;
using System.Text;
using Karambolo.Common.Localization;

namespace Karambolo.Common
{
    public static class TimeSpanUtils
    {
        private class PeriodDescriptor
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

        private static readonly PeriodDescriptor[] s_periods = new[]
        {
            new PeriodDescriptor("year", "years", 31_536_000),
            new PeriodDescriptor("month", "months", 2_628_000),
            new PeriodDescriptor("week", "weeks", 604_800),
            new PeriodDescriptor("day", "days", 86_400),
            new PeriodDescriptor("hour", "hours", 3_600),
            new PeriodDescriptor("minute", "minutes", 60),
            new PeriodDescriptor("second", "seconds", 1),
        };

        public static string ToTimeReference(this TimeSpan timeSpan)
        {
            return timeSpan.ToTimeReference(2);
        }

        public static string ToTimeReference(this TimeSpan timeSpan, int precision)
        {
            return timeSpan.ToTimeReference(precision, DefaultTextLocalizer.Instance);
        }

        public static string ToTimeReference(this TimeSpan timeSpan, int precision, TextLocalizer localizer)
        {
            return timeSpan.ToTimeReference(precision, DefaultPastFormat, DefaultNowText, DefaultFutureFormat, localizer);
        }

        public static string ToTimeReference(this TimeSpan timeSpan, int precision, string pastFormat, string nowText, string futureFormat, TextLocalizer localizer)
        {
            if (localizer == null)
                throw new ArgumentNullException(nameof(localizer));

            var periodCount = s_periods.Length;
            if (precision < 1 || periodCount < precision)
                throw new ArgumentOutOfRangeException(nameof(precision));

            var total = timeSpan.Ticks / TimeSpan.TicksPerSecond;
            if (total == 0)
                return localizer(nowText);

            var remainder = Math.Abs(total);

            int index, value = 0;
            for (index = 0; index < s_periods.Length; index++)
                if ((value = GetValue(ref remainder, index)) > 0)
                    break;

            var builder = new StringBuilder();
            AddValue(builder, index++, value, localizer);
            precision--;

            for (; index < s_periods.Length && precision > 0; index++, precision--)
                if ((value = GetValue(ref remainder, index)) > 0)
                {
                    builder.Append(' ');
                    AddValue(builder, index, value, localizer);
                }
                else
                    break;

            return localizer(total > 0 ? futureFormat : pastFormat, builder.ToString());

            int GetValue(ref long rem, int i)
            {
                return (int)MathShim.DivRem(rem, s_periods[i].Divisor, out rem);
            }

            void AddValue(StringBuilder sb, int i, int val, TextLocalizer loc)
            {
                sb.Append(val);
                sb.Append(' ');
                sb.Append(loc(s_periods[i].SingularName, Plural.From(s_periods[i].PluralName, val)));
            }
        }
    }
}
