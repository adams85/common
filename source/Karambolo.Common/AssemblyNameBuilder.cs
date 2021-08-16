using System;
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

        public AssemblyNameBuilder(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            AssemblyName assemblyName = assembly.GetName();
            
            Name = assemblyName.Name;
            Version = assemblyName.Version;
            PublicKeyToken = assemblyName.GetPublicKeyToken();

#if !NETSTANDARD1_0
            CultureInfo = assemblyName.CultureInfo;
            ProcessorArchitecture = assemblyName.ProcessorArchitecture;
#else
            CultureName = assemblyName.CultureName;
#endif

#if !NET40
            ContentType = assemblyName.ContentType;
#else
            Retargetable = (assemblyName.Flags & AssemblyNameFlags.Retargetable) != 0;
#endif
        }

        private void Parse(string input)
        {
            var parts = input.Split(new[] { ',' }, StringSplitOptions.None);

            var value = parts[0].Trim();
            if (value.Length == 0)
                throw new FormatException();
            Name = value;

            const int versionAttribute = 0x1;
            const int cultureOrLanguageAttribute = 0x2;
            const int publicKeyOrTokenAttribute = 0x4;
            const int processorArchitectureAttribute = 0x8;
            const int retargetableAttribute = 0x10;
            const int contentTypeAttribute = 0x20;
            const int customAttribute = -0x8000_0000;

            int attributesParsed = 0;

            string part;
            int index;
            for (int i = 1, n = parts.Length; i < n; i++)
                if ((index = (part = parts[i]).IndexOf('=')) > 0)
                {
                    value = part.Remove(0, index + 1).Trim();
                    if (value.Length == 0)
                        continue;

                    var key = part.Substring(0, index).Trim();

                    if (ShouldParseAttribute(ref attributesParsed, versionAttribute) &&
                        VersionKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= versionAttribute;

                        if (Version.TryParse(value, out Version version))
                            Version = version;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, cultureOrLanguageAttribute) &&
                        CultureKey.Equals(key, StringComparison.OrdinalIgnoreCase) || LanguageKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= cultureOrLanguageAttribute;

                        try { CultureName = value; }
                        catch (CultureNotFoundException) { }
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, publicKeyOrTokenAttribute) &&
                        PublicKeyTokenKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= publicKeyOrTokenAttribute;

                        if (value.Length == PublicKeyTokenLength << 1)
                            try { PublicKeyTokenString = value; }
                            catch (FormatException) { }
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, publicKeyOrTokenAttribute) &&
                        PublicKeyKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= publicKeyOrTokenAttribute;

                        if (value.Length <= PublicKeyMaxLength << 1)
                            try { PublicKeyString = value; }
                            catch (FormatException) { }
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, processorArchitectureAttribute) &&
                        ProcessorArchitectureKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= processorArchitectureAttribute;

                        if (Enum.TryParse(value, ignoreCase: true, out _ProcessorArchitecture processorArchitecture) &&
                            processorArchitecture != _ProcessorArchitecture.None)
                            ProcessorArchitecture = processorArchitecture;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, retargetableAttribute) &&
                        RetargetableKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= retargetableAttribute;

                        if (YesValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                            Retargetable = true;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, contentTypeAttribute) &&
                        ContentTypeKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= contentTypeAttribute;

                        if (Enum.TryParse(value, ignoreCase: true, out AssemblyContentType contentType) &&
                            contentType != AssemblyContentType.Default)
                            ContentType = contentType;
                    }
                    else if (ShouldParseAttribute(ref attributesParsed, customAttribute) &&
                        CustomKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        attributesParsed |= customAttribute;

                        if (NullValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                            Custom = ArrayUtils.Empty<byte>();
                        else if (value.Length <= PublicKeyMaxLength << 1)
                            try { Custom = StringUtils.BytesFromHexString(value); }
                            catch (FormatException) { }
                    }
                }

            bool ShouldParseAttribute(ref int attrsParsed, int attr)
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
                StringUtils.BytesToHexString(PublicKeyToken);
            set => PublicKeyToken =
                value == null ? null :
                NullValue.Equals(value, StringComparison.OrdinalIgnoreCase) ? ArrayUtils.Empty<byte>() :
                StringUtils.BytesFromHexString(value);
        }

        public byte[] PublicKey { get; set; }

        public string PublicKeyString
        {
            get =>
                PublicKey == null ? null :
                PublicKey.Length == 0 ? NullValue :
                StringUtils.BytesToHexString(PublicKey);
            set => PublicKey =
                value == null ? null :
                NullValue.Equals(value, StringComparison.OrdinalIgnoreCase) ? ArrayUtils.Empty<byte>() :
                StringUtils.BytesFromHexString(value);
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
                sb.Append(',').Append(' ').Append(CustomKey).Append('=').Append(Custom.Length > 0 ? StringUtils.BytesToHexString(Custom) : NullValue);

            return sb.ToString();
        }
    }
}
