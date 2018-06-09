using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public sealed class AssemblyNameBuilder
    {
        const string versionKey = "Version";
        const string cultureKey = "Culture";
        const string publicKeyTokenKey = "PublicKeyToken";

        const string neutralCultureValue = "neutral";
        const int publicKeyTokenLength = sizeof(ulong);

        static CultureInfo CultureNameToInfo(string value)
        {
            if (value == null)
                return null;

            if (neutralCultureValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                return CultureInfo.InvariantCulture;

            return new CultureInfo(value);
        }

        static string CultureInfoToName(CultureInfo value)
        {
            if (value == null)
                return null;

            if (value.Name == string.Empty)
                return neutralCultureValue;

            return value.Name;
        }

        public AssemblyNameBuilder() { }

        public AssemblyNameBuilder(string assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            Parse(assemblyName);
        }

        void Parse(string input)
        {
            var parts = input.Split(new[] { ',' }, StringSplitOptions.None);

            var name = parts[0].Trim();
            if (name.Length == 0)
                throw new FormatException();

            Version version = null;
            CultureInfo cultureInfo = null;
            ulong? publicKeyToken = null;

            string part;
            int index;
            var n = parts.Length;
            for (var i = 1; i < n; i++)
                if ((index = (part = parts[i]).IndexOf('=')) > 0)
                {
                    var key = part.Substring(0, index).Trim();
                    var value = part.Remove(0, index + 1).Trim();

                    if (versionKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (version == null && Version.TryParse(value, out Version parsedVersion))
                            version = parsedVersion;
                    }
                    else if (cultureKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (cultureInfo == null)
                            try { cultureInfo = CultureNameToInfo(value); }
                            catch (CultureNotFoundException) { }
                    }
                    else if (publicKeyTokenKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (publicKeyToken == null && value.Length == publicKeyTokenLength << 1 && 
                            ulong.TryParse(value,NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong publicKeyTokenValue))
                            publicKeyToken = publicKeyTokenValue;
                    }
                }

            Name = name;
            Version = version;
            CultureInfo = cultureInfo;
            PublicKeyToken = publicKeyToken;
        }

        public string Name { get; set; }
        public Version Version { get; set; }
        public CultureInfo CultureInfo { get; set; }
        public string CultureName
        {
            get => CultureInfoToName(CultureInfo);
            set => CultureInfo = CultureNameToInfo(value);
        }

        public ulong? PublicKeyToken { get; set; }

        public byte[] GetPublicKeyTokenBytes()
        {
            if (PublicKeyToken == null)
                return null;

            var value = PublicKeyToken.Value;

            var bytes = new byte[publicKeyTokenLength];
            for (var i = 0; i < publicKeyTokenLength; i++)
                bytes[i] = (byte)(value >> ((publicKeyTokenLength - 1 - i) << 3) & 0xFF);

            return bytes;
        }

        public void SetPublicKeyTokenBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                PublicKeyToken = null;
                return;
            }

            if (bytes.Length != publicKeyTokenLength)
                throw new ArgumentException(string.Format(Resources.InvalidPublicKeyToken, publicKeyTokenLength), nameof(bytes));

            ulong value = 0;
            for (var i = 0; i < publicKeyTokenLength; i++)
                value |= (ulong)bytes[i] << ((publicKeyTokenLength - 1 - i) << 3);

            PublicKeyToken = value;
        }

        public override string ToString()
        {
            if (Name == null)
                return string.Empty;

            if (Version == null && CultureInfo == null && PublicKeyToken == null)
                return Name;

            var builder = new StringBuilder(Name);

            if (Version != null)
            {
                builder.Append(',');
                builder.Append(' ');
                builder.Append(versionKey);
                builder.Append('=');
                builder.Append(Version.ToString());
            }

            if (CultureInfo != null)
            {
                builder.Append(',');
                builder.Append(' ');
                builder.Append(cultureKey);
                builder.Append('=');
                builder.Append(CultureInfoToName(CultureInfo));
            }

            if (PublicKeyToken != null)
            {
                builder.Append(',');
                builder.Append(' ');
                builder.Append(publicKeyTokenKey);
                builder.Append('=');
                builder.Append(PublicKeyToken.Value.ToString("x" + (publicKeyTokenLength << 1)));
            }

            return builder.ToString();
        }
    }

}
