using System.Globalization;

namespace Karambolo.Common.Localization
{
    public interface ILocalizableObject<out T>
    {
        CultureInfo Culture { get; }
        string ContextId { get; }
        string ObjectId { get; }
    }

    public interface ILocalizableText : ILocalizableObject<string>
    {
        string Differentiator { get; }
        object[] FormatArgs { get; }
    }

    public class LocalizedText : ILocalizableText
    {
        readonly ITextLocalizer _localizer;

        public LocalizedText(ITextLocalizer localizer)
        {
            _localizer = localizer;
        }

        public CultureInfo Culture { get; set; }
        public string ContextId { get; set; }
        public string Hint { get; set; }
        public string Differentiator { get; set; }
        public object[] FormatArgs { get; set; }

        public string Value => _localizer.Localize(this);

        string ILocalizableObject<string>.ObjectId => Hint;
    }
}