using System;

namespace Karambolo.Common.Localization
{
    public sealed class NullTextLocalizer : TextLocalizerBase
    {
        public static readonly NullTextLocalizer Instance = new NullTextLocalizer();

        private NullTextLocalizer() { }

        public override string Localize(ILocalizableText localizableObject)
        {
            if (localizableObject == null)
                throw new ArgumentNullException(nameof(localizableObject));

            if (localizableObject.Id == null)
                return string.Empty;

            var id =
                localizableObject.Plural.Id == null || localizableObject.Plural.Count == 1 ?
                localizableObject.Id :
                localizableObject.Plural.Id;

            return
                ArrayUtils.IsNullOrEmpty(localizableObject.FormatArgs) ?
                id :
                string.Format(id, localizableObject.FormatArgs);
        }
    }
}
