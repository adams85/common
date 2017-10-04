using System.Globalization;

namespace Karambolo.Common.Localization
{
    public interface ILocalizableObject<out T>
    {
        CultureInfo Culture { get; }
        string Id { get; }
    }

    public interface ILocalizableText : ILocalizableObject<string>
    {
        Plural Plural { get; }
        TextContext Context { get; }
        object[] FormatArgs { get; }
    }

    public class LocalizableText : ILocalizableText
    {
        public CultureInfo Culture { get; set; }
        public string Id { get; set; }
        public Plural Plural { get; set; }
        public TextContext Context { get; set; }
        public object[] FormatArgs { get; set; }
    }
}
