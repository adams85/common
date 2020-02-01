using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Karambolo.Common
{
    public class TypeNameBuilderTest
    {
        private class Foo<T1, T2>
        {
            public class Bar<T3> { }
        }

        [Fact]
        public void ParseTest()
        {
            #region simple

            var typeName = new TypeName("System.String");
            Assert.Equal("String", typeName.BaseName);
            Assert.False(typeName.HasNested);
            Assert.Null(typeName.Nested);
            Assert.False(typeName.IsGeneric);
            Assert.False(typeName.IsOpenGeneric);
            Assert.Equal(0, typeName.GenericArguments.Count);

            var builder = new TypeNameBuilder("System.String");
            Assert.Equal("System", builder.Namespace);
            Assert.Equal("String", builder.BaseName);
            Assert.False(builder.HasNested);
            Assert.Null(builder.Nested);
            Assert.Null(builder.AssemblyName);
            Assert.False(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(0, builder.GenericArguments.Count);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String, "));
            Assert.Throws<FormatException>(() => new TypeNameBuilder(" .System.String, "));
            Assert.Throws<FormatException>(() => new TypeNameBuilder(" System..String, "));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String., "));
            Assert.Throws<FormatException>(() => new TypeNameBuilder(" ]System.String "));
            Assert.Throws<FormatException>(() => new TypeNameBuilder(" [System.String "));
            Assert.Throws<FormatException>(() => new TypeNameBuilder(", System.String "));
            new TypeNameBuilder("System.String  ");

            #endregion

            #region nested simple

            builder = new TypeNameBuilder("X+Y");
            Assert.Null(builder.AssemblyName);
            Assert.Null(builder.Namespace);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            Assert.Equal("X", builder.BaseName);
            Assert.True(builder.HasNested);
            Assert.False(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(0, builder.GenericArguments.Count);

            Assert.Equal("Y", builder.Nested.BaseName);
            Assert.False(builder.Nested.HasNested);
            Assert.False(builder.Nested.IsGeneric);
            Assert.False(builder.Nested.IsOpenGeneric);
            Assert.Equal(0, builder.Nested.GenericArguments.Count);

            Assert.Null(builder.Nested.Nested);

            builder = new TypeNameBuilder(" X+Y+Z ");
            Assert.Null(builder.AssemblyName);
            Assert.Null(builder.Namespace);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            Assert.Equal("X", builder.BaseName);
            Assert.False(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(0, builder.GenericArguments.Count);

            Assert.Equal("Y", builder.Nested.BaseName);
            Assert.False(builder.Nested.IsGeneric);
            Assert.False(builder.Nested.IsOpenGeneric);
            Assert.Equal(0, builder.Nested.GenericArguments.Count);

            Assert.Equal("Z", builder.Nested.Nested.BaseName);
            Assert.False(builder.Nested.Nested.IsGeneric);
            Assert.False(builder.Nested.Nested.IsOpenGeneric);
            Assert.Equal(0, builder.Nested.Nested.GenericArguments.Count);

            Assert.Null(builder.Nested.Nested.Nested);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("+X"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+ Y"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X +Y"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+Y+"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+.Y"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+Y."));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+NS.Y"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X[]+Y"));
            new TypeNameBuilder("X+Y[]");

            #endregion

            #region array

            builder = new TypeNameBuilder("System.String[],mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("System", builder.Namespace);
            Assert.Equal("String", builder.BaseName);
            Assert.Equal("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", builder.AssemblyName);
            Assert.False(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(0, builder.GenericArguments.Count);
            Assert.Equal(1, builder.ArrayDimensions.Count);
            Assert.Equal(1, builder.ArrayDimensions[0]);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String []"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String[[]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String["));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String]["));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String[]["));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.String[[["));

            #endregion

            #region array of array

            builder = new TypeNameBuilder("System.String[ , ] [, ,][], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("System", builder.Namespace);
            Assert.Equal("String", builder.BaseName);
            Assert.Equal("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", builder.AssemblyName);
            Assert.False(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(0, builder.GenericArguments.Count);
            Assert.Equal(3, builder.ArrayDimensions.Count);
            Assert.Equal(2, builder.ArrayDimensions[0]);
            Assert.Equal(3, builder.ArrayDimensions[1]);
            Assert.Equal(1, builder.ArrayDimensions[2]);

            #endregion

            #region generic

            builder = new TypeNameBuilder("System.Collections.Generic.List`1[[System.String]]");
            Assert.Equal("System.Collections.Generic", builder.Namespace);
            Assert.Equal("List", builder.BaseName);
            Assert.Null(builder.AssemblyName);
            Assert.True(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(1, builder.GenericArguments.Count);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            TypeNameBuilder builder2 = builder.GenericArguments[0];
            Assert.Equal("System", builder2.Namespace);
            Assert.Equal("String", builder2.BaseName);
            Assert.Null(builder2.AssemblyName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);

            builder = new TypeNameBuilder("System.Collections.Generic.List`1 , mscorlib ");
            Assert.Equal("System.Collections.Generic", builder.Namespace);
            Assert.Equal("List", builder.BaseName);
            Assert.Equal("mscorlib", builder.AssemblyName);
            Assert.True(builder.IsGeneric);
            Assert.True(builder.IsOpenGeneric);
            Assert.Equal(1, builder.GenericArguments.Count);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);
            Assert.Null(builder.GenericArguments[0]);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("`1[[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`a [[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.`1[[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`[[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1 [[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1[[]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`2[[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.Dictionary`2[System.String]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.Dictionary`2[System.String, ]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.Dictionary`2[[System.String], System.Int32]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1[System.String]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1[[System.String]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1[[System.String] System.Int32]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1[System.String]]"));
            new TypeNameBuilder("System.Collections.Generic.List`1");

            #endregion

            #region nested generic

            builder = new TypeNameBuilder("X`1+Y`2[[A[]], [B], [C]]");
            Assert.Null(builder.AssemblyName);
            Assert.Null(builder.Namespace);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            Assert.Equal("X", builder.BaseName);
            Assert.True(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(1, builder.GenericArguments.Count);

            builder2 = builder.GenericArguments[0];
            Assert.Null(builder2.AssemblyName);
            Assert.Null(builder2.Namespace);
            Assert.True(builder2.IsArray);
            Assert.Equal(1, builder2.ArrayDimensions.Count);
            Assert.Equal(1, builder2.ArrayDimensions[0]);
            Assert.Equal("A", builder2.BaseName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.Null(builder2.Nested);

            Assert.Equal("Y", builder.Nested.BaseName);
            Assert.True(builder.Nested.IsGeneric);
            Assert.False(builder.Nested.IsOpenGeneric);
            Assert.Equal(2, builder.Nested.GenericArguments.Count);

            builder2 = builder.Nested.GenericArguments[0];
            Assert.Null(builder2.AssemblyName);
            Assert.Null(builder2.Namespace);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);
            Assert.Equal("B", builder2.BaseName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.Null(builder2.Nested);

            builder2 = builder.Nested.GenericArguments[1];
            Assert.Null(builder2.AssemblyName);
            Assert.Null(builder2.Namespace);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);
            Assert.Equal("C", builder2.BaseName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.Null(builder2.Nested);

            Assert.Null(builder.Nested.Nested);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("X`1+Y`1[[A]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X`1+Y`1[[A],[B],[C]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X`2+Y[[A],[B],[C]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+Y`1[[A],[B],[C]]"));
            new TypeNameBuilder("X+Y`1[[A]]");

            #endregion

            #region array of generic

            builder = new TypeNameBuilder("System.Collections.Generic.List`1[[System.String[,]]] []");
            Assert.Equal("System.Collections.Generic", builder.Namespace);
            Assert.Equal("List", builder.BaseName);
            Assert.Null(builder.AssemblyName);
            Assert.True(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(1, builder.GenericArguments.Count);
            Assert.True(builder.IsArray);
            Assert.Equal(1, builder.ArrayDimensions.Count);
            Assert.Equal(1, builder.ArrayDimensions[0]);

            builder2 = builder.GenericArguments[0];
            Assert.Equal("System", builder2.Namespace);
            Assert.Equal("String", builder2.BaseName);
            Assert.Null(builder2.AssemblyName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.True(builder2.IsArray);
            Assert.Equal(1, builder2.ArrayDimensions.Count);
            Assert.Equal(2, builder2.ArrayDimensions[0]);

            builder = new TypeNameBuilder("System.Collections.Generic.List`1[],mscorlib");
            Assert.Equal("System.Collections.Generic", builder.Namespace);
            Assert.Equal("List", builder.BaseName);
            Assert.Equal("mscorlib", builder.AssemblyName);
            Assert.True(builder.IsGeneric);
            Assert.True(builder.IsOpenGeneric);
            Assert.Equal(1, builder.GenericArguments.Count);
            Assert.True(builder.IsArray);
            Assert.Equal(1, builder.ArrayDimensions.Count);
            Assert.Null(builder.GenericArguments[0]);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1[][[System.String]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("System.Collections.Generic.List`1 [],mscorlib"));

            #endregion

            #region array of nested generic

            builder = new TypeNameBuilder("X`1+Y`2[[A[]], [B], [C]] [,,][]");
            Assert.Null(builder.AssemblyName);
            Assert.Null(builder.Namespace);
            Assert.True(builder.IsArray);
            Assert.Equal(2, builder.ArrayDimensions.Count);
            Assert.Equal(3, builder.ArrayDimensions[0]);
            Assert.Equal(1, builder.ArrayDimensions[1]);

            Assert.Equal("X", builder.BaseName);
            Assert.True(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(1, builder.GenericArguments.Count);

            builder2 = builder.GenericArguments[0];
            Assert.Null(builder2.AssemblyName);
            Assert.Null(builder2.Namespace);
            Assert.True(builder2.IsArray);
            Assert.Equal(1, builder2.ArrayDimensions.Count);
            Assert.Equal(1, builder2.ArrayDimensions[0]);
            Assert.Equal("A", builder2.BaseName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.Null(builder2.Nested);

            Assert.Equal("Y", builder.Nested.BaseName);
            Assert.True(builder.Nested.IsGeneric);
            Assert.False(builder.Nested.IsOpenGeneric);
            Assert.Equal(2, builder.Nested.GenericArguments.Count);

            builder2 = builder.Nested.GenericArguments[0];
            Assert.Null(builder2.AssemblyName);
            Assert.Null(builder2.Namespace);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);
            Assert.Equal("B", builder2.BaseName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.Null(builder2.Nested);

            builder2 = builder.Nested.GenericArguments[1];
            Assert.Null(builder2.AssemblyName);
            Assert.Null(builder2.Namespace);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);
            Assert.Equal("C", builder2.BaseName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.Null(builder2.Nested);

            Assert.Null(builder.Nested.Nested);

            builder = new TypeNameBuilder("X+Y`16[ ]");
            Assert.Null(builder.AssemblyName);
            Assert.Null(builder.Namespace);
            Assert.True(builder.IsArray);
            Assert.Equal(1, builder.ArrayDimensions.Count);
            Assert.Equal(1, builder.ArrayDimensions[0]);

            Assert.Equal("X", builder.BaseName);
            Assert.False(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(0, builder.GenericArguments.Count);

            Assert.Equal("Y", builder.Nested.BaseName);
            Assert.True(builder.Nested.IsGeneric);
            Assert.True(builder.Nested.IsOpenGeneric);
            Assert.Equal(16, builder.Nested.GenericArguments.Count);
            Assert.True(builder.Nested.GenericArguments.All(arg => arg == null));

            Assert.Null(builder.Nested.Nested);

            Assert.Throws<FormatException>(() => new TypeNameBuilder("X`1+Y`2[, ,][[A[]], [B], [C]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+Y`16[[]]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+Y`16[, []]"));
            Assert.Throws<FormatException>(() => new TypeNameBuilder("X+Y`16[[], ]"));

            #endregion

            #region generic of generic

            builder = new TypeNameBuilder(" System.Collections.Generic.Dictionary`2[[ System.Int32 , mscorlib2 ], [System.Collections.Generic.List`1[[System.String, mscorlib4]] ,mscorlib3] ] , mscorlib1");
            Assert.Equal("System.Collections.Generic", builder.Namespace);
            Assert.Equal("Dictionary", builder.BaseName);
            Assert.Equal("mscorlib1", builder.AssemblyName);
            Assert.True(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(2, builder.GenericArguments.Count);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[0];
            Assert.Equal("System", builder2.Namespace);
            Assert.Equal("Int32", builder2.BaseName);
            Assert.Equal("mscorlib2", builder2.AssemblyName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[1];
            Assert.Equal("System.Collections.Generic", builder2.Namespace);
            Assert.Equal("List", builder2.BaseName);
            Assert.Equal("mscorlib3", builder2.AssemblyName);
            Assert.True(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(1, builder2.GenericArguments.Count);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);

            TypeNameBuilder builder3 = builder2.GenericArguments[0];
            Assert.Equal("System", builder3.Namespace);
            Assert.Equal("String", builder3.BaseName);
            Assert.Equal("mscorlib4", builder3.AssemblyName);
            Assert.False(builder3.IsGeneric);
            Assert.False(builder3.IsOpenGeneric);
            Assert.Equal(0, builder3.GenericArguments.Count);
            Assert.False(builder3.IsArray);
            Assert.Equal(0, builder3.ArrayDimensions.Count);

            #endregion

            #region transforms

            builder = new TypeNameBuilder("System.Collections.Generic.Dictionary`2+Enumerator[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Collections.Generic.List`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                .Transform(b =>
                {
                    b.AssemblyName = new AssemblyNameBuilder(b.AssemblyName) { Version = null, CultureInfo = null, PublicKeyToken = null }.ToString();
                    b.Namespace = Regex.Replace(b.Namespace, @"^System(.|$)", "");
                });

            Assert.Equal("mscorlib", builder.AssemblyName);
            Assert.Equal("Collections.Generic", builder.Namespace);
            Assert.False(builder.IsArray);
            Assert.Equal(0, builder.ArrayDimensions.Count);

            Assert.Equal("Dictionary", builder.BaseName);
            Assert.True(builder.IsGeneric);
            Assert.False(builder.IsOpenGeneric);
            Assert.Equal(2, builder.GenericArguments.Count);

            builder2 = builder.GenericArguments[0];
            Assert.Equal("", builder2.Namespace);
            Assert.Equal("Int32", builder2.BaseName);
            Assert.Equal("mscorlib", builder2.AssemblyName);
            Assert.False(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(0, builder2.GenericArguments.Count);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);

            builder2 = builder.GenericArguments[1];
            Assert.Equal("Collections.Generic", builder2.Namespace);
            Assert.Equal("List", builder2.BaseName);
            Assert.Equal("mscorlib", builder2.AssemblyName);
            Assert.True(builder2.IsGeneric);
            Assert.False(builder2.IsOpenGeneric);
            Assert.Equal(1, builder2.GenericArguments.Count);
            Assert.False(builder2.IsArray);
            Assert.Equal(0, builder2.ArrayDimensions.Count);

            builder3 = builder2.GenericArguments[0];
            Assert.Equal("", builder3.Namespace);
            Assert.Equal("String", builder3.BaseName);
            Assert.Equal("mscorlib", builder3.AssemblyName);
            Assert.False(builder3.IsGeneric);
            Assert.False(builder3.IsOpenGeneric);
            Assert.Equal(0, builder3.GenericArguments.Count);
            Assert.False(builder3.IsArray);
            Assert.Equal(0, builder3.ArrayDimensions.Count);

            Assert.Equal("Enumerator", builder.Nested.BaseName);
            Assert.False(builder.Nested.IsGeneric);
            Assert.False(builder.Nested.IsOpenGeneric);
            Assert.Equal(0, builder.Nested.GenericArguments.Count);

            Assert.Null(builder.Nested.Nested);

            #endregion
        }

        [Fact]
        public void GenerateTest()
        {
            #region simple

            var builder = new TypeNameBuilder
            {
                BaseName = "System.String",
                AssemblyName = "mscorlib"
            };
            Assert.Equal("System.String, mscorlib", builder.ToString());

            #endregion

            #region generic

            builder = new TypeNameBuilder
            {
                AssemblyName = "mscorlib",
                Namespace = "System.Collections.Generic",
                BaseName = "List",
                GenericArguments =
                {
                    new TypeNameBuilder
                    {
                        AssemblyName = "mscorlib",
                        Namespace = "System",
                        BaseName = "Int32",
                    },
                }
            };
            Assert.Equal("System.Collections.Generic.List`1[[System.Int32, mscorlib]], mscorlib", builder.ToString());

            builder.GenericArguments[0] = null;

            Assert.Equal("System.Collections.Generic.List`1, mscorlib", builder.ToString());

            #endregion

            #region array

            builder = new TypeNameBuilder
            {
                AssemblyName = "mscorlib",
                Namespace = "System.Collections.Generic",
                BaseName = "List",
                GenericArguments =
                {
                    new TypeNameBuilder
                    {
                        AssemblyName = "mscorlib",
                        Namespace = "System",
                        BaseName = "Int32",
                        ArrayDimensions = { 1 }
                    },
                },
                ArrayDimensions = { 2, 3 }
            };
            Assert.Equal("System.Collections.Generic.List`1[[System.Int32[], mscorlib]][,][,,], mscorlib", builder.ToString());
            Assert.Equal("List`1", builder.GetName());

            builder.GenericArguments[0] = null;

            Assert.Equal("System.Collections.Generic.List`1[,][,,]", builder.GetFullName());

            builder.BaseName = "";

            Assert.Equal("System.Collections.Generic.?`1[,][,,]", builder.GetFullName());

            #endregion

            #region nested

            builder = new TypeNameBuilder
            {
                BaseName = "X",
                Nested = new TypeName { BaseName = "Y" },
                Namespace = "NS"
            };
            Assert.Equal("NS.X+Y", builder.ToString());

            builder.ArrayDimensions.Add(1);
            Assert.Equal("NS.X+Y[]", builder.ToString());

            builder.Nested.GenericArguments.Add(new TypeNameBuilder { BaseName = "T" });
            Assert.Equal("NS.X+Y`1[[T]][]", builder.ToString());

            #endregion
            #region complex

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
                },
                Nested = new TypeName { BaseName = "Enumerator" }
            };
            Assert.Equal("System.Collections.Generic.Dictionary`2+Enumerator[[System.Int32, mscorlib],[System.Collections.Generic.List`1[[System.String[][,], mscorlib]], mscorlib]], mscorlib", builder.ToString());

            #endregion
        }

        [Fact]
        public void InitializeTest()
        {
            var type = typeof(string);
            var builder = new TypeNameBuilder(type);
            Assert.Equal(builder.AssemblyName, type.Assembly().FullName);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            builder = new TypeNameBuilder(type, assembly => assembly.GetName().Name);
            Assert.Equal(builder.AssemblyName, type.Assembly().GetName().Name);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(string[]);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(string[][,]);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<,>);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<int, string>);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<int, string>[][,][,,]);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<,>.Bar<>);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<int, string>.Bar<bool>);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<int, string>.Bar<bool>[]);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<int, List<string>>.Bar<bool>);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));

            type = typeof(Foo<(int, string), List<string>[][,]>.Bar<bool>[]);
            builder = new TypeNameBuilder(type);
            Assert.Equal(type, Type.GetType(builder.ToString()));
        }
    }
}
