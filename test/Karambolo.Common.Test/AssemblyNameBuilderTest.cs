using System;
using System.Globalization;
using Xunit;

namespace Karambolo.Common.Test
{
    public class AssemblyNameBuilderTest
    {
        [Fact]
        public void ParseTest()
        {
            // name only
            var builder = new AssemblyNameBuilder("mscorlib");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Null(builder.Version);
            Assert.Null(builder.CultureInfo);
            Assert.Null(builder.CultureName);
            Assert.Null(builder.PublicKeyToken);

            // full name
            builder = new AssemblyNameBuilder("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(4, 0, 0, 0), builder.Version);
            Assert.Equal(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.Equal("neutral", builder.CultureName);
            Assert.Equal(0xb77a5c561934e089UL, builder.PublicKeyToken);

            // nonstandard format
            builder = new AssemblyNameBuilder(" mscorlib,Version =4.0.0.0, Culture = neutral,PublicKeyToken= b77a5c561934e089");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(4, 0, 0, 0), builder.Version);
            Assert.Equal(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.Equal("neutral", builder.CultureName);
            Assert.Equal(0xb77a5c561934e089UL, builder.PublicKeyToken);

            // nonstandard order
            builder = new AssemblyNameBuilder("mscorlib, PublicKeyToken=b77a5c561934e089, Culture=neutral, Version=4.0.0.0");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(4, 0, 0, 0), builder.Version);
            Assert.Equal(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.Equal("neutral", builder.CultureName);
            Assert.True(ArrayUtils.ContentEquals(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, builder.GetPublicKeyTokenBytes()));

            // redundant attributes
            builder = new AssemblyNameBuilder("mscorlib, PublicKeyToken=b77a5c561934e089, Version=2.0.0.0, Culture=en-Us, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(2, 0, 0, 0), builder.Version);
            Assert.Equal("en-US", builder.CultureInfo.Name);
            Assert.Equal("en-US", builder.CultureName);
            Assert.Equal(0xb77a5c561934e089UL, builder.PublicKeyToken);

            // missing name
            Assert.Throws<FormatException>(() => new AssemblyNameBuilder("  , PublicKeyToken=b77a5c561934e089, Version=2.0.0.0, Culture=en-Us, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef"));
        }

        [Fact]
        public void GenerateTest()
        {
            // name only
            var builder = new AssemblyNameBuilder
            {
                Name = "mscorlib"
            };
            Assert.Equal("mscorlib", builder.ToString());

            // full name
            builder = new AssemblyNameBuilder
            {
                Name = "mscorlib",
                Version = new Version(4, 0, 0, 0),
                CultureName = "neutral",
                PublicKeyToken = 0xb77a5c561934e089UL
            };
            Assert.Equal("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", builder.ToString());

            // missing attributes
            builder.Version = null;
            builder.CultureName = null;
            builder.SetPublicKeyTokenBytes(new byte[] { 0x00, 0x22, 0x44, 0x66, 0x88, 0xaa, 0xcc, 0xee });
            Assert.Equal("mscorlib, PublicKeyToken=0022446688aaccee", builder.ToString());
        }
    }
}
