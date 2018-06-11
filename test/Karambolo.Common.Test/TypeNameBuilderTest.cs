using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Globalization;

namespace Karambolo.Common.Test
{
    [TestClass()]
    public class TypeNameBuilderTest
    {
        [TestMethod]
        public void ParseTest()
        {
            // simple
            var builder = new TypeNameBuilder("System.String");
            Assert.AreEqual("System.String", builder.BaseName);
            Assert.IsNull(builder.AssemblyName);
            Assert.IsFalse(builder.IsGeneric);
            Assert.AreEqual(0, builder.GenericArguments.Count);
            Assert.IsFalse(builder.IsArray);
            Assert.AreEqual(0, builder.ArrayDimensions.Count);

            try
            {
                builder = new TypeNameBuilder("System.String, ");
                Assert.Fail();
            }
            catch (FormatException) { }

            // array
            builder = new TypeNameBuilder("System.String[],mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.AreEqual("System.String", builder.BaseName);
            Assert.AreEqual("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", builder.AssemblyName);
            Assert.IsFalse(builder.IsGeneric);
            Assert.AreEqual(0, builder.GenericArguments.Count);
            Assert.AreEqual(1, builder.ArrayDimensions.Count);
            Assert.AreEqual(1, builder.ArrayDimensions[0]);

            try
            {
                builder = new TypeNameBuilder("System.String []");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.String`1[]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.String[[]]");
                Assert.Fail();
            }
            catch (FormatException) { }

            // array of array
            builder = new TypeNameBuilder("System.String[ , ] [, ,][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.AreEqual("System.String", builder.BaseName);
            Assert.AreEqual("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", builder.AssemblyName);
            Assert.IsFalse(builder.IsGeneric);
            Assert.AreEqual(0, builder.GenericArguments.Count);
            Assert.AreEqual(3, builder.ArrayDimensions.Count);
            Assert.AreEqual(2, builder.ArrayDimensions[0]);
            Assert.AreEqual(3, builder.ArrayDimensions[1]);
            Assert.AreEqual(1, builder.ArrayDimensions[2]);

            // generic
            builder = new TypeNameBuilder("System.Collections.Generic.List`1[[System.String]]");
            Assert.AreEqual("System.Collections.Generic.List", builder.BaseName);
            Assert.IsNull(builder.AssemblyName);
            Assert.IsTrue(builder.IsGeneric);
            Assert.AreEqual(1, builder.GenericArguments.Count);
            Assert.IsFalse(builder.IsArray);
            Assert.AreEqual(0, builder.ArrayDimensions.Count);

            var builder2 = builder.GenericArguments[0];
            Assert.AreEqual("System.String", builder2.BaseName);
            Assert.IsNull(builder2.AssemblyName);
            Assert.IsFalse(builder2.IsGeneric);
            Assert.AreEqual(0, builder2.GenericArguments.Count);
            Assert.IsFalse(builder2.IsArray);
            Assert.AreEqual(0, builder2.ArrayDimensions.Count);

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.List`1 [[System.String]]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.List`1[]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.List`1[[]]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.List`2[[System.String]]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.Dictionary`2[System.String]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.Dictionary`2[System.String, ]");
                Assert.Fail();
            }
            catch (FormatException) { }

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.List`1[System.String]");
                Assert.Fail();
            }
            catch (FormatException) { }

            // array of generic
            builder = new TypeNameBuilder("System.Collections.Generic.List`1[[System.String[,]]] []");
            Assert.AreEqual("System.Collections.Generic.List", builder.BaseName);
            Assert.IsNull(builder.AssemblyName);
            Assert.IsTrue(builder.IsGeneric);
            Assert.AreEqual(1, builder.GenericArguments.Count);
            Assert.IsTrue(builder.IsArray);
            Assert.AreEqual(1, builder.ArrayDimensions.Count);
            Assert.AreEqual(1, builder.ArrayDimensions[0]);

            builder2 = builder.GenericArguments[0];
            Assert.AreEqual("System.String", builder2.BaseName);
            Assert.IsNull(builder2.AssemblyName);
            Assert.IsFalse(builder2.IsGeneric);
            Assert.AreEqual(0, builder2.GenericArguments.Count);
            Assert.IsTrue(builder2.IsArray);
            Assert.AreEqual(1, builder2.ArrayDimensions.Count);
            Assert.AreEqual(2, builder2.ArrayDimensions[0]);

            try
            {
                builder = new TypeNameBuilder("System.Collections.Generic.List`1[][[System.String]]");
                Assert.Fail();
            }
            catch (FormatException) { }

            // generic of generic
            builder = new TypeNameBuilder(" System.Collections.Generic.Dictionary`2[[ System.Int32 , mscorlib2 ], [System.Collections.Generic.List`1[[System.String, mscorlib4]] ,mscorlib3] ] , mscorlib1");
            Assert.AreEqual("System.Collections.Generic.Dictionary", builder.BaseName);
            Assert.AreEqual("mscorlib1", builder.AssemblyName);
            Assert.IsTrue(builder.IsGeneric);
            Assert.AreEqual(2, builder.GenericArguments.Count);
            Assert.IsFalse(builder.IsArray);
            Assert.AreEqual(0, builder.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[0];
            Assert.AreEqual("System.Int32", builder2.BaseName);
            Assert.AreEqual("mscorlib2", builder2.AssemblyName);
            Assert.IsFalse(builder2.IsGeneric);
            Assert.AreEqual(0, builder2.GenericArguments.Count);
            Assert.IsFalse(builder2.IsArray);
            Assert.AreEqual(0, builder2.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[1];
            Assert.AreEqual("System.Collections.Generic.List", builder2.BaseName);
            Assert.AreEqual("mscorlib3", builder2.AssemblyName);
            Assert.IsTrue(builder2.IsGeneric);
            Assert.AreEqual(1, builder2.GenericArguments.Count);
            Assert.IsFalse(builder2.IsArray);
            Assert.AreEqual(0, builder2.ArrayDimensions.Count);

            var builder3 = builder2.GenericArguments[0];
            Assert.AreEqual("System.String", builder3.BaseName);
            Assert.AreEqual("mscorlib4", builder3.AssemblyName);
            Assert.IsFalse(builder3.IsGeneric);
            Assert.AreEqual(0, builder3.GenericArguments.Count);
            Assert.IsFalse(builder3.IsArray);
            Assert.AreEqual(0, builder3.ArrayDimensions.Count);

            // transforms
            builder = new TypeNameBuilder(
                "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                new TypeNameParseSettings
                {
                    AssemblyNameTransform = n => new AssemblyNameBuilder(n) { Version = null, CultureInfo = null, PublicKeyToken = null }.ToString(),
                    TypeNameTransform = n => n.Replace("System.", "")
                });

            Assert.AreEqual("Collections.Generic.Dictionary", builder.BaseName);
            Assert.AreEqual("mscorlib", builder.AssemblyName);
            Assert.IsTrue(builder.IsGeneric);
            Assert.AreEqual(2, builder.GenericArguments.Count);
            Assert.IsFalse(builder.IsArray);
            Assert.AreEqual(0, builder.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[0];
            Assert.AreEqual("Int32", builder2.BaseName);
            Assert.AreEqual("mscorlib", builder2.AssemblyName);
            Assert.IsFalse(builder2.IsGeneric);
            Assert.AreEqual(0, builder2.GenericArguments.Count);
            Assert.IsFalse(builder2.IsArray);
            Assert.AreEqual(0, builder2.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[1];
            Assert.AreEqual("Collections.Generic.List", builder2.BaseName);
            Assert.AreEqual("mscorlib", builder2.AssemblyName);
            Assert.IsTrue(builder2.IsGeneric);
            Assert.AreEqual(1, builder2.GenericArguments.Count);
            Assert.IsFalse(builder2.IsArray);
            Assert.AreEqual(0, builder2.ArrayDimensions.Count);

            builder3 = builder2.GenericArguments[0];
            Assert.AreEqual("String", builder3.BaseName);
            Assert.AreEqual("mscorlib", builder3.AssemblyName);
            Assert.IsFalse(builder3.IsGeneric);
            Assert.AreEqual(0, builder3.GenericArguments.Count);
            Assert.IsFalse(builder3.IsArray);
            Assert.AreEqual(0, builder3.ArrayDimensions.Count);
        }

        [TestMethod]
        public void GenerateTest()
        {
            // simple
            var builder = new TypeNameBuilder
            {
                BaseName = "System.String",
                AssemblyName = "mscorlib"
            };
            Assert.AreEqual("System.String, mscorlib", builder.ToString());

            // complex
            builder = new TypeNameBuilder
            {
                BaseName = "System.Collections.Generic.Dictionary",
                AssemblyName = "mscorlib",
                GenericArguments =
                {
                    new TypeNameBuilder
                    {
                        BaseName = "System.Int32",
                        AssemblyName = "mscorlib",
                    },
                    new TypeNameBuilder
                    {
                        BaseName = "System.Collections.Generic.List",
                        AssemblyName = "mscorlib",
                        GenericArguments =
                        {
                            new TypeNameBuilder
                            {
                                BaseName = "System.String",
                                AssemblyName = "mscorlib",
                                ArrayDimensions = { 1, 2 }
                            },
                        }
                    },
                }
            };
            Assert.AreEqual("System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib],[System.Collections.Generic.List`1[[System.String[][,], mscorlib]], mscorlib]], mscorlib", builder.ToString());
        }
    }
}
