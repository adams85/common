using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Karambolo.Common
{
    public static class UriUtils
    {
        public static string BuildPath(params string[] parts)
        {
            return BuildUrl(parts, null, null);
        }

        public static string BuildQuery(object query)
        {
            return BuildUrl(null, ReflectionUtils.ObjectToDictionaryCached(query), null);
        }

        public static string BuildQuery(IDictionary<string, object> query)
        {
            return BuildUrl(null, query, null);
        }

        public static string BuildUrl(string[] parts, object query)
        {
            return BuildUrl(parts, ReflectionUtils.ObjectToDictionaryCached(query), null);
        }

        public static string BuildUrl(string[] parts, IDictionary<string, object> query)
        {
            return BuildUrl(parts, query, null);
        }

        public static string BuildUrl(string[] parts, object query, string fragment)
        {
            return BuildUrl(parts, ReflectionUtils.ObjectToDictionaryCached(query), fragment);
        }

        public static string BuildUrl(string[] parts, IDictionary<string, object> query, string fragment)
        {
            var builder = new StringBuilder();

            if (!ArrayUtils.IsNullOrEmpty(parts))
            {
                var part = parts[0];
                var delimiter = !string.IsNullOrEmpty(part) && part[0] == '/' ? "/" : null;

                var count = parts.Length;
                for (var i = 0; i < count; i++)
                {
                    builder.Append(delimiter);

                    part = parts[i];
                    if (!string.IsNullOrEmpty(part))
                    {
                        var endIndex = part.Length - 1;
                        if (part[0] == '/')
                            builder.Append(part, 1, endIndex);
                        else
                            builder.Append(part);

                        delimiter = endIndex > 0 && part[endIndex] != '/' ? "/" : null;
                    }
                    else
                        delimiter = null;
                }
            }

            if (query != null)
            {
                var separator = '?';
                foreach (var queryPart in query)
                    foreach (var value in 
                        queryPart.Value is string || !(queryPart.Value is IEnumerable enumerable) ?
                        EnumerableUtils.Return(queryPart.Value) : 
                        enumerable.Cast<object>())
                    {
                        builder.Append(separator);
                        separator = '&';

                        builder.Append(queryPart.Key);
                        builder.Append('=');
                        builder.Append(value != null ? Uri.EscapeDataString(value.ToString()) : string.Empty);
                    }
            }

            if (!string.IsNullOrEmpty(fragment))
            {
                builder.Append('#');
                builder.Append(Uri.EscapeDataString(fragment));
            }

            return builder.ToString();
        }
    }
}
