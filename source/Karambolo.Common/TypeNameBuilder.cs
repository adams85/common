using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Karambolo.Common
{
    public class TypeName
    {
        private static string GetBaseName(string input, int startIndex, int endIndex, int dotIndex, bool isNested, ref string @namespace)
        {
            // name is missing or ends with dot?
            if (startIndex == endIndex || dotIndex == endIndex - 1)
                throw new FormatException();

            if (dotIndex >= 0)
            {
                // nested type has namespace?
                if (isNested)
                    throw new FormatException();

                @namespace = input.Substring(startIndex, dotIndex - startIndex);
                return input.Substring(++dotIndex, endIndex - dotIndex);
            }
            else
                return input.Substring(startIndex, endIndex - startIndex);
        }

        private static List<TypeNameBuilder> CreateGenericArguments(string input, int startIndex, int endIndex)
        {
            if (!int.TryParse(input.Substring(startIndex, endIndex - startIndex), NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
                throw new FormatException();

            var genericArguments = new List<TypeNameBuilder>(count);
            for (; count > 0; count--)
                genericArguments.Add(null);

            return genericArguments;
        }

        internal static int ParseTypeName(string input, int startIndex, int endIndex, TypeName typeName, out string @namespace)
        {
            @namespace = null;
            var sectionStartIndex = -1;
            var dotIndex = -1;
            var isNested = false;

            const int beforeTypeNameState = 0;
            const int inTypeNameState = 1;
            const int inGenericArgCountState = 2;
            const int afterTypeNameState = 3;

            int state = beforeTypeNameState;
            int i;
            for (i = startIndex; i < endIndex; i++)
            {
                var c = input[i];

                switch (state)
                {
                    case beforeTypeNameState:
                        switch (c)
                        {
                            case '`':
                            case '+':
                            case '.':
                            case '[':
                            case ']':
                            case ',':
                                throw new FormatException();
                            default:
                                if (!char.IsWhiteSpace(c))
                                {
                                    sectionStartIndex = i;
                                    state = inTypeNameState;
                                }
                                break;
                        }
                        break;
                    case inTypeNameState:
                        switch (c)
                        {
                            case '[':
                            case ',':
                                typeName._baseName = GetBaseName(input, sectionStartIndex, i, dotIndex, isNested, ref @namespace);
                                return i;
                            case '.':
                                // name starts with dot or has two consecutive dots?
                                if (sectionStartIndex == i || dotIndex == i - 1)
                                    throw new FormatException();

                                dotIndex = i;
                                break;
                            case '`':
                                typeName._baseName = GetBaseName(input, sectionStartIndex, i, dotIndex, isNested, ref @namespace);

                                sectionStartIndex = i + 1;
                                state = inGenericArgCountState;
                                break;
                            case '+':
                                typeName._baseName = GetBaseName(input, sectionStartIndex, i, dotIndex, isNested, ref @namespace);

                                typeName = typeName._nested = new TypeName();
                                isNested = true;

                                sectionStartIndex = i + 1;
                                dotIndex = -1;
                                break;
                            default:
                                if (char.IsWhiteSpace(c))
                                {
                                    typeName._baseName = GetBaseName(input, sectionStartIndex, i, dotIndex, isNested, ref @namespace);

                                    state = afterTypeNameState;
                                }
                                break;
                        }
                        break;
                    case inGenericArgCountState:
                        switch (c)
                        {
                            case '[':
                            case ',':
                                typeName._genericArguments = CreateGenericArguments(input, sectionStartIndex, i);
                                return i;
                            case '+':
                                typeName._genericArguments = CreateGenericArguments(input, sectionStartIndex, i);

                                typeName = typeName._nested = new TypeName();
                                isNested = true;

                                sectionStartIndex = i + 1;
                                dotIndex = -1;
                                state = inTypeNameState;
                                break;
                            default:
                                if (char.IsWhiteSpace(c))
                                {
                                    typeName._genericArguments = CreateGenericArguments(input, sectionStartIndex, i);

                                    state = afterTypeNameState;
                                }
                                else if (!char.IsDigit(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                    case afterTypeNameState:
                        switch (c)
                        {
                            case ',':
                                return i;
                            case '[':
                                // whitespace between type name and bracket?
                                throw new FormatException();
                            default:
                                if (!char.IsWhiteSpace(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                }
            }

            switch (state)
            {
                case inTypeNameState:
                    typeName._baseName = GetBaseName(input, sectionStartIndex, i, dotIndex, isNested, ref @namespace);
                    return i;
                case inGenericArgCountState:
                    typeName._genericArguments = CreateGenericArguments(input, sectionStartIndex, i);
                    return i;
                case afterTypeNameState:
                    return i;
                case beforeTypeNameState:
                    throw new FormatException();
                default:
                    throw new InvalidOperationException();
            }
        }

        internal static StringBuilder BuildTypeName(TypeName typeName, StringBuilder sb, int index)
        {
            if (typeName.IsGeneric)
                sb.Insert(index, typeName._genericArguments.Count).Insert(index, '`');

            if (!string.IsNullOrEmpty(typeName._baseName))
                sb.Insert(index, typeName._baseName);
            else
                sb.Insert(index, '?');

            return sb;
        }

        public TypeName() { }

        public TypeName(string typeName)
            : this(typeName, out var _) { }

        public TypeName(string typeName, out string @namespace)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            ParseTypeName(typeName, 0, typeName.Length, this, out @namespace);
        }

        internal string _baseName;
        public string BaseName
        {
            get { return _baseName; }
            set { _baseName = value; }
        }

        internal TypeName _nested;
        public TypeName Nested
        {
            get { return _nested; }
            set { _nested = value; }
        }

        public bool HasNested => _nested != null;

        internal IList<TypeNameBuilder> _genericArguments;
        public IList<TypeNameBuilder> GenericArguments => _genericArguments ?? (_genericArguments = new List<TypeNameBuilder>());

        public bool IsGeneric => _genericArguments != null && _genericArguments.Count > 0;

        public bool IsOpenGeneric
        {
            get
            {
                if (!IsGeneric)
                    return false;

                for (int i = 0, n = _genericArguments.Count; i < n; i++)
                    if (_genericArguments[i] == null)
                        return true;

                return false;
            }
        }

        public string GetName()
        {
            if (_baseName == null)
                return string.Empty;

            return BuildTypeName(this, new StringBuilder(), 0).ToString();
        }

        public override string ToString()
        {
            return GetName();
        }
    }

    public sealed class TypeNameBuilder : TypeName
    {
        private static void SeekNextGenericArg(ref TypeName typeName, ref int genericArgIndex)
        {
            genericArgIndex++;
            while (typeName._genericArguments == null || genericArgIndex >= typeName._genericArguments.Count)
                if ((typeName = typeName.Nested) != null)
                    genericArgIndex = 0;
                else
                    return;
        }

        private static int ParseBrackets(string input, int startIndex, int endIndex, TypeNameBuilder builder)
        {
            TypeName typeName = builder;
            var genericArgIndex = -1;
            var sectionStartIndex = -1;
            var counter = 0;

            const int inBracketsState = 0;
            const int inGenericBracketsBeforeArgState = 1;
            const int inNestedBracketsState = 2;
            const int inGenericBracketsAfterArgState = 3;
            const int inArrayBracketsState = 4;
            const int inAfterBracketsState = 5;

            int state = inBracketsState;
            int i;
            for (i = startIndex; i < endIndex; i++)
            {
                var c = input[i];

                switch (state)
                {
                    case inBracketsState:
                        switch (c)
                        {
                            case '[':
                                sectionStartIndex = i + 1;
                                state = inNestedBracketsState;
                                break;
                            case ']':
                                builder.ArrayDimensions.Add(counter + 1);
                                state = inAfterBracketsState;
                                break;
                            case ',':
                                counter++;
                                state = inArrayBracketsState;
                                break;
                            default:
                                if (!char.IsWhiteSpace(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                    case inGenericBracketsBeforeArgState:
                        switch (c)
                        {
                            case '[':
                                sectionStartIndex = i + 1;
                                state = inNestedBracketsState;
                                break;
                            default:
                                if (!char.IsWhiteSpace(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                    case inNestedBracketsState:
                        switch (c)
                        {
                            case '[':
                                counter++;
                                break;
                            case ']':
                                if (counter == 0)
                                {
                                    SeekNextGenericArg(ref typeName, ref genericArgIndex);
                                    // too much generic argument?
                                    if (typeName == null)
                                        throw new FormatException();

                                    Parse(input, sectionStartIndex, i, typeName._genericArguments[genericArgIndex] = new TypeNameBuilder());

                                    state = inGenericBracketsAfterArgState;
                                }
                                else
                                    counter--;
                                break;
                        }
                        break;
                    case inGenericBracketsAfterArgState:
                        switch (c)
                        {
                            case ']':
                                SeekNextGenericArg(ref typeName, ref genericArgIndex);
                                // too few generic argument?
                                if (typeName != null)
                                    throw new FormatException();

                                state = inAfterBracketsState;
                                break;
                            case ',':
                                state = inGenericBracketsBeforeArgState;
                                break;
                            default:
                                if (!char.IsWhiteSpace(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                    case inArrayBracketsState:
                        switch (c)
                        {
                            case ']':
                                builder.ArrayDimensions.Add(counter + 1);
                                state = inAfterBracketsState;
                                break;
                            case ',':
                                counter++;
                                break;
                            default:
                                if (!char.IsWhiteSpace(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                    case inAfterBracketsState:
                        switch (c)
                        {
                            case ',':
                                return i;
                            case '[':
                                counter = 0;
                                state = inArrayBracketsState;
                                break;
                            default:
                                if (!char.IsWhiteSpace(c))
                                    throw new FormatException();
                                break;
                        }
                        break;
                }
            }

            switch (state)
            {
                case inAfterBracketsState:
                    return i;
                case inBracketsState:
                case inGenericBracketsBeforeArgState:
                case inGenericBracketsAfterArgState:
                case inArrayBracketsState:
                case inNestedBracketsState:
                    throw new FormatException();
                default:
                    throw new InvalidOperationException();
            }
        }

        private static string GetAssemblyName(string input, int startIndex, int endIndex)
        {
            // skipping whitespaces
            for (; startIndex < endIndex && char.IsWhiteSpace(input[startIndex]); startIndex++) { }
            for (endIndex--; endIndex >= startIndex && char.IsWhiteSpace(input[endIndex]); endIndex--) { }

            // missing assembly name?
            if (startIndex > endIndex)
                throw new FormatException();

            return input.Substring(startIndex, endIndex - startIndex + 1);
        }

        private static void Parse(string input, int startIndex, int endIndex, TypeNameBuilder builder)
        {
            startIndex = ParseTypeName(input, startIndex, endIndex, builder, out builder._namespace);

            if (startIndex < endIndex)
            {
                if (input[startIndex] == '[')
                    startIndex = ParseBrackets(input, startIndex + 1, endIndex, builder);

                if (startIndex < endIndex && input[startIndex] == ',')
                    builder._assemblyName = GetAssemblyName(input, startIndex + 1, endIndex);
            }
        }

        private static StringBuilder Build(TypeNameBuilder builder, StringBuilder sb, int index, bool appendAssemblyName)
        {
            var length = sb.Length;
            var isNestedType = false;
            var isClosedGenericType = false;
            var genericIndex = -1;
            int i, n;

            TypeName typeName = builder;
            do
            {
                BuildTypeName(typeName, sb, index);

                if (!isNestedType)
                {
                    if (!string.IsNullOrEmpty(builder.Namespace))
                        sb.Insert(index, '.').Insert(index, builder.Namespace);

                    isNestedType = true;
                }
                else
                    sb.Insert(index, '+');

                n = sb.Length - length;
                index += n;
                genericIndex += n;
                length = sb.Length;

                if (typeName.IsGeneric && !typeName.IsOpenGeneric)
                {
                    if (genericIndex < index)
                    {
                        sb.Insert(genericIndex = index, '[');
                        genericIndex++;
                        length++;
                    }

                    for (i = 0, n = typeName._genericArguments.Count; i < n; i++)
                    {
                        sb.Insert(genericIndex, ']');
                        Build(typeName._genericArguments[i], sb, genericIndex, appendAssemblyName: true);
                        sb.Insert(genericIndex, '[');

                        if (!isClosedGenericType)
                            isClosedGenericType = true;
                        else
                            sb.Insert(genericIndex, ',');

                        genericIndex += sb.Length - length;
                        length = sb.Length;
                    }
                }
            }
            while ((typeName = typeName._nested) != null);

            if (isClosedGenericType)
            {
                sb.Insert(index = genericIndex, ']');
                index++;
                length++;
            }

            if (builder.IsArray)
            {
                for (i = builder._arrayDimensions.Count - 1; i >= 0; i--)
                {
                    sb.Insert(index, ']');
                    n = builder._arrayDimensions[i];
                    if (--n > 0)
                        sb.Insert(index, new string(',', n));
                    sb.Insert(index, '[');
                }

                index += sb.Length - length;
            }

            if (appendAssemblyName && !string.IsNullOrEmpty(builder._assemblyName))
                sb.Insert(index, builder._assemblyName).Insert(index, ' ').Insert(index, ',');

            return sb;
        }

        private static TypeNameBuilder Initialize(TypeNameBuilder builder, Type type, Func<Assembly, string> getAssemblyName)
        {
            int i, j, n, m;

            if (type.IsArray)
            {
                do
                {
                    builder.ArrayDimensions.Add(type.GetArrayRank());
                    type = type.GetElementType();
                }
                while (type.IsArray);

                for (i = 0, j = builder.ArrayDimensions.Count - 1; i < j; i++, j--)
                    GeneralUtils.Swap(builder.ArrayDimensions, i, j);
            }

            Type[] genericTypeArguments;
            bool isOpenGeneric;
            if (type.IsGenericType())
            {
                genericTypeArguments = type.GetGenericTypeArguments();
                isOpenGeneric = type.IsGenericTypeDefinition();
            }
            else
            {
                genericTypeArguments = null;
                isOpenGeneric = false;
            }

            TypeName currentTypeName = null;
            do
            {
                if (type.DeclaringType == null)
                {
                    builder.Nested = currentTypeName;
                    currentTypeName = builder;

                    builder.AssemblyName = getAssemblyName(type.Assembly());

                    builder.Namespace = type.Namespace;
                }
                else
                    currentTypeName = new TypeName { Nested = currentTypeName };

                var index = type.Name.LastIndexOf('`');
                if (index >= 0)
                {
                    currentTypeName.BaseName = type.Name.Substring(0, index);
                    n = int.Parse(type.Name.Substring(index + 1), CultureInfo.InvariantCulture);
                    for (; n > 0; n--)
                        currentTypeName.GenericArguments.Add(null);
                }
                else
                    currentTypeName.BaseName = type.Name;

                type = type.DeclaringType;
            }
            while (type != null);

            if (genericTypeArguments != null && !isOpenGeneric)
                for (i = 0, n = genericTypeArguments.Length; i < n;)
                {
                    for (j = 0, m = currentTypeName.GenericArguments.Count; j < m; j++)
                        currentTypeName.GenericArguments[j] = Initialize(new TypeNameBuilder(), genericTypeArguments[i++], getAssemblyName);

                    currentTypeName = currentTypeName.Nested;
                }

            return builder;
        }

        public TypeNameBuilder() { }

        public TypeNameBuilder(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            Parse(typeName, 0, typeName.Length, this);
        }

        public TypeNameBuilder(Type type) : this(type, null) { }

        public TypeNameBuilder(Type type, Func<Assembly, string> getAssemblyName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Initialize(this, type, getAssemblyName ?? (assembly => assembly.FullName));
        }

        private string _assemblyName;
        public string AssemblyName
        {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        private IList<int> _arrayDimensions;
        public IList<int> ArrayDimensions => _arrayDimensions ?? (_arrayDimensions = new List<int>());
        public bool IsArray => _arrayDimensions != null && _arrayDimensions.Count > 0;

        public TypeNameBuilder Transform(Action<TypeNameBuilder> action)
        {
            action(this);

            TypeName typeName = this;
            do
            {
                if (typeName.IsGeneric)
                    for (int i = 0, n = typeName._genericArguments.Count; i < n; i++)
                        typeName._genericArguments[i].Transform(action);
            }
            while ((typeName = typeName._nested) != null);

            return this;
        }

        public string GetFullName()
        {
            if (_baseName == null)
                return string.Empty;

            return Build(this, new StringBuilder(), 0, appendAssemblyName: false).ToString();
        }

        public override string ToString()
        {
            if (_baseName == null)
                return string.Empty;

            return Build(this, new StringBuilder(), 0, appendAssemblyName: true).ToString();
        }
    }
}
