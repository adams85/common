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

        private enum GcpState
        {
            Initial,
            OneDotRead,
            TwoDotsRead,
            SlashRead,
            SkipUntilSlash,
        }

        public static string GetCanonicalPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            GcpState state = GcpState.Initial;
            int sectionStartIndex = 0, sectionEndIndex;
            int level = 0;
            StringBuilder sb = null;

            var length = path.Length;
            for (var index = 0; index < length; index++)
            {
                var c = path[index];

                switch (state)
                {
                    case GcpState.Initial:
                        if (c == '/')
                        {
                            sectionStartIndex++;
                            state = GcpState.SlashRead;
                        }
                        else if (c == '.')
                            state = GcpState.OneDotRead;
                        else
                            state = GcpState.SkipUntilSlash;
                        break;
                    case GcpState.OneDotRead:
                        if (c == '/')
                        {
                            sectionEndIndex = index + 1;
                            if (sb == null)
                                sb = new StringBuilder(path, 0, sectionStartIndex, length - (sectionEndIndex - sectionStartIndex));

                            sectionStartIndex = sectionEndIndex;
                            state = GcpState.SlashRead;
                        }
                        else if (c == '.')
                            state = GcpState.TwoDotsRead;
                        else
                            state = GcpState.SkipUntilSlash;
                        break;
                    case GcpState.TwoDotsRead:
                        if (c == '/')
                        {
                            sectionEndIndex = index + 1;
                            if (sb == null)
                                sb = new StringBuilder(path, 0, sectionStartIndex, length - (sectionEndIndex - sectionStartIndex));

                            AddSection(sb, path, sectionStartIndex, sectionEndIndex, -(--level));

                            sectionStartIndex = sectionEndIndex;
                            state = GcpState.SlashRead;
                        }
                        else
                            state = GcpState.SkipUntilSlash;
                        break;
                    case GcpState.SlashRead:
                        if (c == '/')
                        {
                            sectionEndIndex = index + 1;
                            if (sb == null)
                                sb = new StringBuilder(path, 0, sectionStartIndex, length - (sectionEndIndex - sectionStartIndex));

                            sectionStartIndex++;
                        }
                        else if (c == '.')
                            state = GcpState.OneDotRead;
                        else
                            state = GcpState.SkipUntilSlash;
                        break;
                    case GcpState.SkipUntilSlash:
                        if (c == '/')
                        {
                            sectionEndIndex = index + 1;
                            level++;
                            if (sb != null)
                                AddSection(sb, path, sectionStartIndex, sectionEndIndex, level);

                            sectionStartIndex = sectionEndIndex;
                            state = GcpState.SlashRead;
                        }
                        break;
                }
            }

            switch (state)
            {
                case GcpState.OneDotRead:
                    if (sb == null)
                        return path.Length > 2 ? path.Substring(0, length - 2) : path[0] == '/' ? "/" : string.Empty;

                    return (sb.Length > 1 ? sb.Remove(sb.Length - 1, 1) : sb).ToString();
                case GcpState.TwoDotsRead:
                    if (sb == null)
                        sb = new StringBuilder(path, 0, sectionStartIndex, sectionStartIndex);

                    return AddSection(sb, path, sectionStartIndex, length, -(--level)).ToString();
                case GcpState.SlashRead:
                    if (sb == null)
                        return path;

                    return (sb.Length > 0 && sb[sb.Length - 1] != '/' ? sb.Append('/') : sb).ToString();
                case GcpState.SkipUntilSlash:
                    if (sb == null)
                        return path;

                    AddSection(sb, path, sectionStartIndex, length, ++level);
                    return (sb.Length > 1 && sb[sectionEndIndex = sb.Length - 1] == '/' ? sb.Remove(sectionEndIndex, 1) : sb).ToString();
                default:
                    return path;
            }

            StringBuilder AddSection(StringBuilder builder, string pathVal, int startIndex, int endIndex, int lvl)
            {
                if (lvl < 0)
                {
                    var separatorIndex = -1;
                    for (int j = builder.Length - 2; j >= 0; j--)
                        if (builder[j] == '/')
                        {
                            separatorIndex = j;
                            break;
                        }

                    builder.Remove(++separatorIndex, builder.Length - separatorIndex);
                }
                else if (lvl > 0)
                    builder.Append(pathVal, startIndex, endIndex - startIndex);
                else if (builder.Length > 0 && builder[0] == '/')
                    builder.Clear().Append('/');
                else
                    builder.Clear();

                return builder;
            }
        }
    }
}
