#if NETCOREAPP1_0
extern alias common;
using CommonProcessorArchitecture = common::System.Reflection.ProcessorArchitecture;
#else
using CommonProcessorArchitecture = System.Reflection.ProcessorArchitecture;
#endif

using System;
using System.Globalization;
using System.Reflection;
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
            Assert.Null(builder.VersionString);
            Assert.Null(builder.CultureInfo);
            Assert.Null(builder.CultureName);
            Assert.Null(builder.PublicKeyToken);
            Assert.Null(builder.PublicKeyTokenString);
            Assert.Null(builder.PublicKey);
            Assert.Null(builder.PublicKeyString);
            Assert.Equal(CommonProcessorArchitecture.None, builder.ProcessorArchitecture);
            Assert.False(builder.Retargetable);
            Assert.Equal(AssemblyContentType.Default, builder.ContentType);
            Assert.Null(builder.Custom);
            Assert.False(builder.HasAttributes);

            // full name
            builder = new AssemblyNameBuilder("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(4, 0, 0, 0), builder.Version);
            Assert.Equal(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.Equal("neutral", builder.CultureName);
            Assert.Equal(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, builder.PublicKeyToken);
            Assert.True(builder.HasAttributes);

            // nonstandard format
            builder = new AssemblyNameBuilder(" mscorlib,Version =4.0.0.0, Culture = neutral,PublicKeyToken= b77a5c561934e089");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(4, 0, 0, 0), builder.Version);
            Assert.Equal(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.Equal("neutral", builder.CultureName);
            Assert.Equal(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, builder.PublicKeyToken);

            // nonstandard order
            builder = new AssemblyNameBuilder("mscorlib, PublicKeyToken=b77a5c561934e089, Language=neutral, Version=4.0.0.0");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(4, 0, 0, 0), builder.Version);
            Assert.Equal(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.Equal("neutral", builder.CultureName);
            Assert.Equal(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, builder.PublicKeyToken);

            // redundant attributes
            builder = new AssemblyNameBuilder("mscorlib, PublicKeyToken=b77a5c561934e089, Version=2.0.0.0, Culture=en-Us, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Equal(new Version(2, 0, 0, 0), builder.Version);
            Assert.Equal("en-US", builder.CultureInfo.Name);
            Assert.Equal("en-US", builder.CultureName);
            Assert.Equal(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, builder.PublicKeyToken);

            // redundant attributes with invalid format
            builder = new AssemblyNameBuilder("mscorlib, Version=1.x, Version=4.0.0.0");
            Assert.Equal("mscorlib", builder.Name);
            Assert.Null(builder.Version);
            Assert.False(builder.HasAttributes);

            // other attributes
            builder = new AssemblyNameBuilder("lib, PublicKey=01234567, PublicKeyToken=ignored, ProcessorArchitecture=amd64, Retargetable=yes, ContentType=WindowsRuntime, Custom=00112233");
            Assert.Equal("lib", builder.Name);
            Assert.Equal(new byte[] { 0x01, 0x23, 0x45, 0x67 }, builder.PublicKey);
            Assert.Null(builder.PublicKeyToken);
            Assert.Equal(CommonProcessorArchitecture.Amd64, builder.ProcessorArchitecture);
            Assert.True(builder.Retargetable);
            Assert.Equal(AssemblyContentType.WindowsRuntime, builder.ContentType);
            Assert.Equal(new byte[] { 0x00, 0x11, 0x22, 0x33 }, builder.Custom);

            // invalid values
            builder = new AssemblyNameBuilder("lib, Version=, Language=null, PublicKeyToken=02, Custom=bcdefg");
            Assert.False(builder.HasAttributes);

            builder = new AssemblyNameBuilder("lib, Version=, Language=null, PublicKeyToken=0123456789abcdeg");
            Assert.False(builder.HasAttributes);

            builder = new AssemblyNameBuilder("lib, Version=, Language=null, PublicKey=0, Custom=null");
            Assert.Empty(builder.Custom);
            Assert.True(builder.HasAttributes);

            // missing name
            Assert.Throws<FormatException>(() => new AssemblyNameBuilder("  , PublicKeyToken=b77a5c561934e089, Version=2.0.0.0, Culture=en-Us, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef"));
        }

        [Fact]
        public void GenerateTest()
        {
            var builder = new AssemblyNameBuilder();
            Assert.Empty(builder.ToString());

            // name only
            builder.Name = "mscorlib";
            Assert.Equal("mscorlib", builder.ToString());

            // full name
            builder = new AssemblyNameBuilder
            {
                Name = "mscorlib",
                Version = new Version(4, 0, 0, 0),
                CultureName = "neutral",
                PublicKeyToken = new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }
            };
            Assert.Equal("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089", builder.ToString());

            // missing attributes
            builder.Version = null;
            builder.CultureName = null;
            builder.PublicKeyToken = ArrayUtils.Empty<byte>();
            Assert.Equal("mscorlib, PublicKeyToken=null", builder.ToString());

            // all (with defaults)
            builder.Name = "lib";
            builder.VersionString = "1.0.0.0";
            builder.CultureName = "neutral";
            builder.PublicKeyString = "null";
            builder.PublicKeyTokenString = null;
            builder.ProcessorArchitecture = CommonProcessorArchitecture.X86;
            builder.Retargetable = true;
            builder.ContentType = AssemblyContentType.WindowsRuntime;
            builder.Custom = new byte[0];
            Assert.Equal("lib, Version=1.0.0.0, Culture=neutral, PublicKey=null, ProcessorArchitecture=X86, Retargetable=yes, ContentType=WindowsRuntime, Custom=null", builder.ToString());

            // all
            builder.Name = "lib";
            builder.VersionString = "1.0.0.0";
            builder.CultureName = "en";
            builder.PublicKeyString = "0123456789abcdef";
            builder.ProcessorArchitecture = CommonProcessorArchitecture.None;
            builder.Retargetable = false;
            builder.ContentType = AssemblyContentType.Default;
            builder.Custom = new byte[] { 0, 1 };
            Assert.Equal("lib, Version=1.0.0.0, Culture=en, PublicKey=0123456789ABCDEF, Custom=0001", builder.ToString());

            // remove attributes

            builder.RemoveAttributes();
            Assert.Equal("lib", builder.ToString());
        }
    }
}
