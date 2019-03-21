﻿using System;
using System.Globalization;
using System.Reflection;
using System.Text;

#if !NETSTANDARD1_0
using _ProcessorArchitecture = System.Reflection.ProcessorArchitecture;
#else
using _ProcessorArchitecture = Karambolo.Common.ProcessorArchitecture;
#endif

namespace Karambolo.Common
{
    public sealed class AssemblyNameBuilder
    {
        [Flags]
        private enum Attributes
        {
            None = 0,
            Version = 0x1,
            CultureOrLanguage = 0x2,
            PublicKeyOrToken = 0x4,
            ProcessorArchitecture = 0x8,
            Retargetable = 0x10,
            ContentType = 0x20,
            Custom = -0x8000_0000
        }

        private const string VersionKey = "Version";
        private const string CultureKey = "Culture";
        private const string LanguageKey = "Language";
        private const string PublicKeyTokenKey = "PublicKeyToken";
        private const string PublicKeyKey = "PublicKey";
        private const string ProcessorArchitectureKey = "ProcessorArchitecture";
        private const string RetargetableKey = "Retargetable";
        private const string ContentTypeKey = "ContentType";
        private const string CustomKey = "Custom";
        private const string NullValue = "null";
        private const string NeutralValue = "neutral";
        private const string YesValue = "yes";
        private const int PublicKeyTokenLength = sizeof(long);
        private const int PublicKeyMaxLength = 2048;

        public AssemblyNameBuilder() { }

        public AssemblyNameBuilder(string assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            Parse(assemblyName);
        }

        private void Parse(string input)
        {
            var parts = input.Split(new[] { ',' }, StringSplitOptions.None);

            var value = parts[0].Trim();
            if (value.Length == 0)
                throw new FormatException();
            Name = value;

            Attributes attributesParsed = Attributes.None;

            string part;
            int index;
            var n = parts.Length;
            for (var i = 1; i < n; i++)
                if ((index = (part = parts[i]).IndexOf('=')) > 0)
                {
                    value = part.Remove(0, index + 1).Trim();
                    if (value.Length == 0)
                        continue;

                    var key = part.Substring(0, index).Trim();

                    if (ShouldParseAttribute(ref attributesParsed, Attributes.Version) &&
                        VersionKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.Version;

                        if (Version.TryParse(value, out Version version))
                            Version = version;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.CultureOrLanguage) &&
                        CultureKey.Equals(key, StringComparison.OrdinalIgnoreCase) || LanguageKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.CultureOrLanguage;

                        try { CultureName = value; }
                        catch (CultureNotFoundException) { }
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.PublicKeyOrToken) &&
                        PublicKeyTokenKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.PublicKeyOrToken;

                        if (value.Length == PublicKeyTokenLength << 1)
                            try { PublicKeyTokenString = value; }
                            catch (FormatException) { }
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.PublicKeyOrToken) &&
                        PublicKeyKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.PublicKeyOrToken;

                        if (value.Length <= PublicKeyMaxLength << 1)
                            try { PublicKeyString = value; }
                            catch (FormatException) { }
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.ProcessorArchitecture) &&
                        ProcessorArchitectureKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.ProcessorArchitecture;

                        if (Enum.TryParse(value, ignoreCase: true, out _ProcessorArchitecture processorArchitecture) &&
                            processorArchitecture != _ProcessorArchitecture.None)
                            ProcessorArchitecture = processorArchitecture;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.Retargetable) &&
                        RetargetableKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.Retargetable;

                        if (YesValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                            Retargetable = true;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.ContentType) &&
                        ContentTypeKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.ContentType;

                        if (Enum.TryParse(value, ignoreCase: true, out AssemblyContentType contentType) &&
                            contentType != AssemblyContentType.Default)
                            ContentType = contentType;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, Attributes.Custom) &&
                        CustomKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= Attributes.Custom;

                        if (NullValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                            Custom = ArrayUtils.Empty<byte>();
                        else if (value.Length <= PublicKeyMaxLength << 1)
                            try { Custom = StringUtils.FromHexString(value); }
                            catch (FormatException) { }
                    }
                }

            bool ShouldParseAttribute(ref Attributes attrsParsed, Attributes attr)
            {
                return (attrsParsed & attr) == 0;
            }
        }

        public string Name { get; set; }

        public Version Version { get; set; }

        public string VersionString
        {
            get => Version?.ToString();
            set => Version = value != null ? Version.Parse(value) : null;
        }

        public CultureInfo CultureInfo { get; set; }

        public string CultureName
        {
            get =>
                CultureInfo == null ? null :
                CultureInfo.Name == string.Empty ? NeutralValue :
                CultureInfo.Name;
            set => CultureInfo =
                value == null ? null :
                NeutralValue.Equals(value, StringComparison.OrdinalIgnoreCase) ? CultureInfo.InvariantCulture :
                new CultureInfo(value);
        }

        public byte[] PublicKeyToken { get; set; }

        public string PublicKeyTokenString
        {
            get =>
                PublicKeyToken == null ? null :
                PublicKeyToken.Length == 0 ? NullValue :
                StringUtils.ToHexString(PublicKeyToken);
            set => PublicKeyToken =
                value == null ? null :
                NullValue.Equals(value, StringComparison.OrdinalIgnoreCase) ? ArrayUtils.Empty<byte>() :
                StringUtils.FromHexString(value);
        }

        public byte[] PublicKey { get; set; }

        public string PublicKeyString
        {
            get =>
                PublicKey == null ? null :
                PublicKey.Length == 0 ? NullValue :
                StringUtils.ToHexString(PublicKey);
            set => PublicKey =
                value == null ? null :
                NullValue.Equals(value, StringComparison.OrdinalIgnoreCase) ? ArrayUtils.Empty<byte>() :
                StringUtils.FromHexString(value);
        }

        public _ProcessorArchitecture ProcessorArchitecture { get; set; }

        public bool Retargetable { get; set; }

        public AssemblyContentType ContentType { get; set; }

        public byte[] Custom { get; set; }

        public bool HasAttributes =>
            Version != null || CultureInfo != null || PublicKeyToken != null || PublicKey != null ||
            ProcessorArchitecture != _ProcessorArchitecture.None || Retargetable || ContentType != AssemblyContentType.Default || Custom != null;

        public void RemoveAttributes()
        {
            Version = null;
            CultureInfo = null;
            PublicKeyToken = null;
            PublicKey = null;
            ProcessorArchitecture = _ProcessorArchitecture.None;
            Retargetable = false;
            ContentType = AssemblyContentType.Default;
            Custom = null;
        }

        public override string ToString()
        {
            if (Name == null)
                return string.Empty;

            if (!HasAttributes)
                return Name;

            var sb = new StringBuilder(Name);

            if (Version != null)
                sb.Append(',').Append(' ').Append(VersionKey).Append('=').Append(VersionString);

            if (CultureInfo != null)
                sb.Append(',').Append(' ').Append(CultureKey).Append('=').Append(CultureName);

            if (PublicKeyToken != null)
                sb.Append(',').Append(' ').Append(PublicKeyTokenKey).Append('=').Append(PublicKeyTokenString);
            else if (PublicKey != null)
                sb.Append(',').Append(' ').Append(PublicKeyKey).Append('=').Append(PublicKeyString);

            if (ProcessorArchitecture != _ProcessorArchitecture.None)
                sb.Append(',').Append(' ').Append(ProcessorArchitectureKey).Append('=').Append(ProcessorArchitecture);

            if (Retargetable)
                sb.Append(',').Append(' ').Append(RetargetableKey).Append('=').Append(YesValue);

            if (ContentType != AssemblyContentType.Default)
                sb.Append(',').Append(' ').Append(ContentTypeKey).Append('=').Append(ContentType);

            if (Custom != null)
                sb.Append(',').Append(' ').Append(CustomKey).Append('=').Append(Custom.Length > 0 ? StringUtils.ToHexString(Custom) : NullValue);

            return sb.ToString();
        }
    }
}
