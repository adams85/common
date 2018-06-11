using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public class TypeNameParseSettings
    {
        public static readonly TypeNameParseSettings Default = new TypeNameParseSettings();

        public Func<string, string> TypeNameTransform { get; set; } = Identity<string>.Func;
        public Func<string, string> AssemblyNameTransform { get; set; } = Identity<string>.Func;
    }

    public sealed class TypeNameBuilder
    {
        enum ParserState
        {
            BeforeBrackets,
            InGenericBracketsBeforeArg,
            InNestedBrackets,
            InGenericBracketsAfterArg,
            InArrayBrackets,
            AfterBrackets,
        }

        static void ParseCore(TypeNameBuilder builder, string input, int startIndex, int count, TypeNameParseSettings settings)
        {
            var bracketIndex = -1;
            var argumentIndex = -1;
            var counter = -1;
            var isGeneric = false;
            List<TypeNameBuilder> genericArguments = null;
            List<int> arrayDimensions = null;
            var state = ParserState.BeforeBrackets;

            int i, endIndex;
            for (i = startIndex, endIndex = startIndex + count; i < endIndex; i++)
            {
                var c = input[i];

                switch (state)
                {
                    case ParserState.BeforeBrackets:
                        if (c == ',')
                            goto EndOfLoop;
                        else if (c == '`')
                            isGeneric = true;
                        else if (c == '[')
                        {
                            bracketIndex = i;
                            counter = 0;
                            state = isGeneric ? ParserState.InGenericBracketsBeforeArg : ParserState.InArrayBrackets;
                        }
                        else if (c == ']')
                            throw new FormatException();
                        break;
                    case ParserState.InGenericBracketsBeforeArg:
                        if (c == '[')
                        {
                            argumentIndex = i + 1;
                            state = ParserState.InNestedBrackets;
                        }
                        else if (!char.IsWhiteSpace(c))
                            throw new FormatException();
                        break;
                    case ParserState.InNestedBrackets:
                        if (c == '[')
                            counter++;
                        else if (c == ']')
                        {
                            if (counter == 0)
                            {
                                var genericArgument = new TypeNameBuilder();
                                ParseCore(genericArgument, input, argumentIndex, i - argumentIndex, settings);

                                (genericArguments ?? (genericArguments = new List<TypeNameBuilder>())).Add(genericArgument);
                                state = ParserState.InGenericBracketsAfterArg;
                            }
                            else
                                counter--;
                        }
                        else if (c == '[')
                            throw new FormatException();
                        break;
                    case ParserState.InGenericBracketsAfterArg:
                        if (c == ']')
                            state = ParserState.AfterBrackets;
                        else if (c == ',')
                            state = ParserState.InGenericBracketsBeforeArg;
                        else if (!char.IsWhiteSpace(c))
                            throw new FormatException();
                        break;
                    case ParserState.InArrayBrackets:
                        if (c == ']')
                        {
                            (arrayDimensions ?? (arrayDimensions = new List<int>())).Add(counter + 1);
                            state = ParserState.AfterBrackets;
                        }
                        else if (c == ',')
                            counter++;
                        else if (!char.IsWhiteSpace(c))
                            throw new FormatException();
                        break;
                    case ParserState.AfterBrackets:
                        if (c == ',')
                            goto EndOfLoop;
                        else if (c == '[')
                        {
                            counter = 0;
                            state = ParserState.InArrayBrackets;
                        }
                        else if (!char.IsWhiteSpace(c))
                            throw new FormatException();
                        break;
                }
            }
            EndOfLoop:

            bool isAssemblyQualified;
            if (i >= endIndex)
                isAssemblyQualified =
                    state == ParserState.BeforeBrackets || state == ParserState.AfterBrackets ?
                    false :
                    throw new FormatException();
            else
                isAssemblyQualified = true;

            var baseName =
                bracketIndex >= 0 ?
                input.Substring(startIndex, bracketIndex - startIndex) :
                input.Substring(startIndex, i - startIndex);

            if (bracketIndex >= 0)
            {
                if (baseName.Length > 0 && char.IsWhiteSpace(baseName[baseName.Length - 1]))
                    throw new FormatException();

                if (isGeneric)
                {
                    var index = baseName.IndexOf('`');
                    if (!int.TryParse(baseName.Substring(index + 1), out int genericArgumentCount) || genericArgumentCount != genericArguments.Count)
                        throw new FormatException();

                    baseName = baseName.Substring(0, index);
                    if (baseName.Length > 0 && char.IsWhiteSpace(baseName[baseName.Length - 1]))
                        throw new FormatException();
                }
            }

            baseName = baseName.Trim();
            if (baseName.Length == 0)
                throw new FormatException();

            string assemblyName;
            if (isAssemblyQualified)
            {
                assemblyName = input.Substring(++i, endIndex - i).Trim();
                if (assemblyName.Length == 0)
                    throw new FormatException();
            }
            else
                assemblyName = null;

            builder.BaseName = settings.TypeNameTransform(baseName);
            builder.AssemblyName = assemblyName != null ? settings.AssemblyNameTransform(assemblyName) : null;
            builder._genericArguments = genericArguments;
            builder._arrayDimensions = arrayDimensions;
        }

        public TypeNameBuilder() { }

        public TypeNameBuilder(string typeName)
            : this(typeName, null) { }

        public TypeNameBuilder(string typeName, TypeNameParseSettings settings)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            if (settings != null)
            {
                if (settings.AssemblyNameTransform == null)
                    throw new ArgumentException(string.Format(Resources.PropertyCannotBeNull, nameof(settings.AssemblyNameTransform)), nameof(settings));

                if (settings.TypeNameTransform == null)
                    throw new ArgumentException(string.Format(Resources.PropertyCannotBeNull, nameof(settings.TypeNameTransform)), nameof(settings));
            }
            else
                settings = TypeNameParseSettings.Default;

            ParseCore(this, typeName, 0, typeName.Length, settings);
        }

        public string BaseName { get; set; }
        public string AssemblyName { get; set; }

        IList<TypeNameBuilder> _genericArguments;
        public IList<TypeNameBuilder> GenericArguments => _genericArguments ?? (_genericArguments = new List<TypeNameBuilder>());
        public bool IsGeneric => _genericArguments != null && _genericArguments.Count > 0;

        IList<int> _arrayDimensions;
        public IList<int> ArrayDimensions => _arrayDimensions ?? (_arrayDimensions = new List<int>());
        public bool IsArray => _arrayDimensions != null && _arrayDimensions.Count > 0;

        public string GetFullName()
        {
            if (BaseName == null)
                return string.Empty;

            var isGenericType = IsGeneric;
            var isArrayType = IsArray;
            if (!isGenericType && !isArrayType)
                return BaseName;

            var builder = new StringBuilder(BaseName);

            if (isGenericType)
            {
                var n = GenericArguments.Count;

                builder.Append('`');
                builder.Append(n);

                builder.Append('[');
                for (var i = 0; i < n; i++)
                {
                    if (i > 0)
                        builder.Append(',');
                    builder.Append('[');
                    builder.Append(GenericArguments[i].ToString());
                    builder.Append(']');
                }
                builder.Append(']');
            }

            if (isArrayType)
            {
                var n = ArrayDimensions.Count;
                for (var i = 0; i < n; i++)
                {
                    builder.Append('[');
                    var m = ArrayDimensions[i];
                    if (--m > 0)
                        builder.Append(new string(',', m));
                    builder.Append(']');
                }
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            if (BaseName == null)
                return string.Empty;

            var fullName = GetFullName();

            return
                AssemblyName == null ?
                fullName :
                string.Concat(fullName, ", ", AssemblyName);
        }
    }
}
