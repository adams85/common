using System;
using System.Threading;

namespace Karambolo.Common.Localization
{
    public interface ILocalizer<in TLocalizable, out TValue>
        where TLocalizable : ILocalizableObject<TValue>
    {
        TValue Localize(TLocalizable localizableObject);
    }

    public interface ITextLocalizer : ILocalizer<ILocalizableText, string>
    {
        string this[string id, params object[] formatArgs] { get; }
    }

    public abstract class TextLocalizerBase : ITextLocalizer
    {
        public string this[string id, params object[] formatArgs]
        {
            get
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));

                var plural = default(Plural);
                var context = default(TextContext);

                if (!ArrayUtils.IsNullOrEmpty(formatArgs))
                {
                    var pluralIndex = Array.FindIndex(formatArgs, a => a is Plural);
                    if (pluralIndex >= 0)
                        plural = (Plural)formatArgs[pluralIndex];

                    var contextIndex = formatArgs.Length - 1;
                    object contextArg;
                    if (pluralIndex != contextIndex && (contextArg = formatArgs[contextIndex]) is TextContext)
                        context = (TextContext)contextArg;
                }

                var localizableText = new LocalizableText
                {
                    Culture = Thread.CurrentThread.CurrentUICulture,
                    Id = id,
                    Plural = plural,
                    Context = context,
                    FormatArgs = formatArgs,
                };

                return Localize(localizableText);
            }
        }

        public abstract string Localize(ILocalizableText localizableObject);
    }
}