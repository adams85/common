using System;

namespace Karambolo.Common.Localization
{
    public delegate string TextLocalizer(string id, params object[] formatArgs);

    public class DefaultTextLocalizer
    {
        public static readonly TextLocalizer Instance = (id, args) =>
        {
            if (ArrayUtils.IsNullOrEmpty(args))
                return id;

            var pluralIndex = Array.FindIndex(args, a => a is Plural);
            Plural plural;
            if (pluralIndex >= 0 && (plural = (Plural)args[pluralIndex]).Id != null && plural.Count != 1)
                id = plural.Id;

            return string.Format(id, args);
        };
    }
}
