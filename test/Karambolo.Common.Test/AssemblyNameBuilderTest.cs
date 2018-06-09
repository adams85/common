using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Globalization;

namespace Karambolo.Common.Test
{
    [TestClass()]
    public class AssemblyNameBuilderTest
    {
        [TestMethod]
        public void ParseTest()
        {
            // name only
            var builder = new AssemblyNameBuilder("mscorlib");
            Assert.AreEqual("mscorlib", builder.Name);
            Assert.IsNull(builder.Version);
            Assert.IsNull(builder.CultureInfo);
            Assert.IsNull(builder.CultureName);
            Assert.IsNull(builder.PublicKeyToken);

            // full name
            builder = new AssemblyNameBuilder("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.AreEqual("mscorlib", builder.Name);
            Assert.AreEqual(new Version(4, 0, 0, 0), builder.Version);
            Assert.AreEqual(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.AreEqual("neutral", builder.CultureName);
            Assert.AreEqual(0xb77a5c561934e089UL, builder.PublicKeyToken);

            // nonstandard format
            builder = new AssemblyNameBuilder(" mscorlib,Version =4.0.0.0, Culture = neutral,PublicKeyToken= b77a5c561934e089");
            Assert.AreEqual("mscorlib", builder.Name);
            Assert.AreEqual(new Version(4, 0, 0, 0), builder.Version);
            Assert.AreEqual(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.AreEqual("neutral", builder.CultureName);
            Assert.AreEqual(0xb77a5c561934e089UL, builder.PublicKeyToken);

            // nonstandard order
            builder = new AssemblyNameBuilder("mscorlib, PublicKeyToken=b77a5c561934e089, Culture=neutral, Version=4.0.0.0");
            Assert.AreEqual("mscorlib", builder.Name);
            Assert.AreEqual(new Version(4, 0, 0, 0), builder.Version);
            Assert.AreEqual(CultureInfo.InvariantCulture.Name, builder.CultureInfo.Name);
            Assert.AreEqual("neutral", builder.CultureName);
            Assert.IsTrue(ArrayUtils.ContentEquals(new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }, builder.GetPublicKeyTokenBytes()));

            // redundant attributes
            builder = new AssemblyNameBuilder("mscorlib, PublicKeyToken=b77a5c561934e089, Version=2.0.0.0, Culture=en-Us, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef");
            Assert.AreEqual("mscorlib", builder.Name);
            Assert.AreEqual(new Version(2, 0, 0, 0), builder.Version);
            Assert.AreEqual("en-US", builder.CultureInfo.Name);
            Assert.AreEqual("en-US", builder.CultureName);
            Assert.AreEqual(0xb77a5c561934e089UL, builder.PublicKeyToken);

            // missing name
            try
            {
                builder = new AssemblyNameBuilder("  , PublicKeyToken=b77a5c561934e089, Version=2.0.0.0, Culture=en-Us, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0123456789abcdef");
                Assert.Fail();
            }
            catch (FormatException) { }
        }

        [TestMethod]
        public void GenerateTest()
        {
            // name only
            var builder = new AssemblyNameBuilder
            {
                Name = "mscorlib"
            };
            Assert.AreEqual("mscorlib", builder.ToString());

            // full name
            builder = new AssemblyNameBuilder
            {
                Name = "mscorlib",
                Version = new Version(4, 0, 0, 0),
                CultureName = "neutral",
                PublicKeyToken = 0xb77a5c561934e089UL
            };
            Assert.AreEqual("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", builder.ToString());

            // missing attributes
            builder.Version = null;
            builder.CultureName = null;
            builder.SetPublicKeyTokenBytes(new byte[] { 0x00, 0x22, 0x44, 0x66, 0x88, 0xaa, 0xcc, 0xee });
            Assert.AreEqual("mscorlib, PublicKeyToken=0022446688aaccee", builder.ToString());
        }
    }
}
