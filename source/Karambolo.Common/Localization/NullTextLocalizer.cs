using System;

namespace Karambolo.Common.Localization
{
    public class NullTextLocalizer : ITextLocalizer
    {
        static readonly NullTextLocalizer instance = new NullTextLocalizer();
        public static readonly TextLocalizer Instance = (h, d, a) => new LocalizedText(instance) { Hint = h, FormatArgs = a }.Value;

        public virtual string Localize(ILocalizableText localizedObject)
        {
            if (localizedObject == null)
                throw new ArgumentNullException(nameof(localizedObject));

            if (localizedObject.ObjectId == null)
                return string.Empty;

            if (localizedObject.FormatArgs == null || localizedObject.FormatArgs.Length == 0)
                return localizedObject.ObjectId;
            else
                return string.Format(localizedObject.ObjectId, localizedObject.FormatArgs);
        }
    }
}
