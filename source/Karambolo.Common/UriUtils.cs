using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            var sb = new StringBuilder();

            if (!ArrayUtils.IsNullOrEmpty(parts))
            {
                var part = parts[0];
                var delimiter = !string.IsNullOrEmpty(part) && part[0] == '/' ? "/" : null;

                var count = parts.Length;
                for (var i = 0; i < count; i++)
                {
                    sb.Append(delimiter);

                    part = parts[i];
                    if (!string.IsNullOrEmpty(part))
                    {
                        var endIndex = part.Length - 1;
                        if (part[0] == '/')
                            sb.Append(part, 1, endIndex);
                        else
                            sb.Append(part);

                        delimiter = endIndex > 0 && part[endIndex] != '/' ? "/" : null;
                    }
                    else
                        delimiter = null;
                }
            }

            if (query != null)
            {
                var separator = '?';
                foreach (KeyValuePair<string, object> queryPart in query)
                    foreach (var value in
                        queryPart.Value is string || !(queryPart.Value is IEnumerable enumerable) ?
                        EnumerableUtils.Return(queryPart.Value) :
                        enumerable.Cast<object>())
                    {
                        sb.Append(separator);
                        separator = '&';

                        sb.Append(queryPart.Key);
                        sb.Append('=');
                        sb.Append(value != null ? Uri.EscapeDataString(value.ToString()) : string.Empty);
                    }
            }

            if (!string.IsNullOrEmpty(fragment))
            {
                sb.Append('#');
                sb.Append(Uri.EscapeDataString(fragment));
            }

            return sb.ToString();
        }

        public static string GetCanonicalPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!Regex.IsMatch(path, @"\.{1,2}(?:/|$)|//", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
                return path;

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            int i = 0;
            while (i < segments.Count)
            {
                var segment = segments[i];

                if (segment == ".")
                {
                    segments.RemoveAt(i);
                }
                else if (segment == ".." && i > 0)
                {
                    segments.RemoveRange(i - 1, 2);
                    i--;
                }
                else
                    i++;
            }

            if (path.StartsWith("/"))
                segments.Insert(0, string.Empty);

            return string.Join("/", segments);
        }
    }
}
