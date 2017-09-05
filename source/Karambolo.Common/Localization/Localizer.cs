namespace Karambolo.Common.Localization
{
    public interface ILocalizer<in TLocalizable, out TValue>
        where TLocalizable : ILocalizableObject<TValue>
    {
        TValue Localize(TLocalizable localizedObject);
    }

    public delegate string TextLocalizer(string hint, string differentiator = null, object[] args = null);

    public interface ITextLocalizer : ILocalizer<ILocalizableText, string> { }
}