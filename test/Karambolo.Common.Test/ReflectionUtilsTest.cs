#if NETCOREAPP1_0
using SystemBindingFlags = System.Reflection.BindingFlags;
using CommonMemberTypes = Karambolo.Common.MemberTypes;
#else
using SystemBindingFlags = System.Reflection.BindingFlags;
using CommonMemberTypes = System.Reflection.MemberTypes;
#endif

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Karambolo.Common.Test
{
    public class ReflectionUtilsTest
    {
        [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
        class NonInheritedAttribute : Attribute
        {
            public NonInheritedAttribute(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
        class InheritedAttribute : Attribute
        {
            public InheritedAttribute(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        [NonInherited(1), NonInherited(2)]
        [Inherited(1), Inherited(2)]
        class TestClass
        {
            public TestClass() { }
            public TestClass(string value) { _field = value; }
            internal string _field;

            public string PublicField;
            public string ShadowedPublicMember = "";

            [NonInherited(0), Inherited(0)]
            public int Property { get; set; }

            [NonInherited(0), Inherited(0)]
            public virtual int VirtualProperty { get; set; }

            public event EventHandler Event;
            public bool Method() => default;
        }

        interface ITestInterface
        {
            int VirtualProperty { get; }
            int ReadOnlyProperty { get; }
            object ExplicitProperty { get; set; }
        }

        class DerivedTestClass : TestClass, ITestInterface
        {
            internal int InternalProperty { get; set; }

            public override int VirtualProperty { get => base.VirtualProperty; set => base.VirtualProperty = value; }

            public int ReadOnlyProperty => 3;

            public new int ShadowedPublicMember => 4;

            object ITestInterface.ExplicitProperty { get; set; }
        }

        [Fact]
        public void AllowsNullTest()
        {
            Assert.True(typeof(object).AllowsNull());
            Assert.True(typeof(string).AllowsNull());
            Assert.True(typeof(ValueType).AllowsNull());
            Assert.False(typeof(int).AllowsNull());
            Assert.True(typeof(int?).AllowsNull());
        }

        [Fact]
        public void IsAssignableFrom()
        {
            Assert.True(typeof(object).IsAssignableFrom(""));
            Assert.True(typeof(object).IsAssignableFrom(1));
            Assert.True(typeof(object).IsAssignableFrom((object)null));

            Assert.True(typeof(string).IsAssignableFrom(""));
            Assert.False(typeof(string).IsAssignableFrom(1));
            Assert.True(typeof(string).IsAssignableFrom((object)null));

            Assert.False(typeof(ValueType).IsAssignableFrom(""));
            Assert.True(typeof(ValueType).IsAssignableFrom(1));
            Assert.True(typeof(ValueType).IsAssignableFrom((object)null));

            Assert.False(typeof(int).IsAssignableFrom(""));
            Assert.True(typeof(int).IsAssignableFrom(1));
            Assert.False(typeof(int).IsAssignableFrom((object)null));

            Assert.False(typeof(int?).IsAssignableFrom(""));
            Assert.True(typeof(int?).IsAssignableFrom(1));
            Assert.True(typeof(int?).IsAssignableFrom((object)null));
        }

        [Fact]
        public void IsDelegateTest()
        {
            Assert.True(typeof(EventHandler).IsDelegate());
            Assert.True(typeof(MulticastDelegate).IsDelegate());
            Assert.True(typeof(Delegate).IsDelegate());
            Assert.False(typeof(string).IsDelegate());
            Assert.False(typeof(object).IsDelegate());
        }

        [Fact]
        public void GetInterfaceTest()
        {
            Assert.Equal(typeof(IEnumerable), typeof(List<int>).GetInterface(typeof(IEnumerable)));
            Assert.Equal(typeof(IEnumerable<int>), typeof(List<int>).GetInterface(typeof(IEnumerable<int>)));
            Assert.Null(typeof(List<int>).GetInterface(typeof(IEnumerable<string>)));
            Assert.Throws<ArgumentNullException>(() => typeof(List<int>).GetInterface(null));
            Assert.Throws<ArgumentException>(() => typeof(List<int>).GetInterface(typeof(object)));
        }

        [Fact]
        public void HasInterfaceTest()
        {
            Assert.True(typeof(List<int>).HasInterface(typeof(IEnumerable)));
            Assert.True(typeof(List<int>).HasInterface(typeof(IEnumerable<int>)));
            Assert.False(typeof(List<int>).HasInterface(typeof(IEnumerable<string>)));
            Assert.Throws<ArgumentNullException>(() => typeof(List<int>).HasInterface(null));
            Assert.Throws<ArgumentException>(() => typeof(List<int>).HasInterface(typeof(object)));
        }

        [Fact]
        public void GetClosedInterfacesTest()
        {
            Assert.Equal(new[] { typeof(IEnumerable<int>) }, typeof(List<int>).GetClosedInterfaces(typeof(IEnumerable<>)));
            Assert.Equal(Type.EmptyTypes, typeof(List<int>).GetClosedInterfaces(typeof(IDictionary<,>)));
            Assert.Throws<ArgumentNullException>(() => typeof(List<int>).GetClosedInterfaces(null));
            Assert.Throws<ArgumentException>(() => typeof(List<int>).GetClosedInterfaces(typeof(IEnumerable)));
            Assert.Throws<ArgumentException>(() => typeof(List<int>).GetClosedInterfaces(typeof(object)));
        }

        [Fact]
        public void HasClosedInterfaceTest()
        {
            Assert.True(typeof(List<int>).HasClosedInterface(typeof(IEnumerable<>)));
            Assert.False(typeof(List<int>).HasClosedInterface(typeof(IDictionary<,>)));
            Assert.Throws<ArgumentNullException>(() => typeof(List<int>).HasClosedInterface(null));
            Assert.Throws<ArgumentException>(() => typeof(List<int>).HasClosedInterface(typeof(IEnumerable)));
            Assert.Throws<ArgumentException>(() => typeof(List<int>).HasClosedInterface(typeof(object)));
        }

        [Fact]
        public void GetMemberTypeTest()
        {
            var type = typeof(TestClass);

            var field = type.GetField(nameof(TestClass._field), SystemBindingFlags.Instance | SystemBindingFlags.NonPublic);
            Assert.Equal(typeof(string), field.GetMemberType(CommonMemberTypes.Field));
            Assert.Equal(null, field.GetMemberType(0));

            var property = type.GetProperty(nameof(TestClass.Property));
            Assert.Equal(typeof(int), property.GetMemberType(CommonMemberTypes.Property));
            Assert.Equal(null, property.GetMemberType(0));

            var @event = type.GetEvent(nameof(TestClass.Event));
            Assert.Equal(typeof(EventHandler), @event.GetMemberType(CommonMemberTypes.Event));
            Assert.Equal(null, @event.GetMemberType(0));

            var method = type.GetMethod(nameof(TestClass.Method));
            Assert.Equal(typeof(bool), method.GetMemberType(CommonMemberTypes.Method));
            Assert.Equal(null, method.GetMemberType(0));

            var explicitCtor = type.GetConstructor(new[] { typeof(string) });
            Assert.Equal(typeof(TestClass), explicitCtor.GetMemberType(CommonMemberTypes.Constructor));
            Assert.Equal(null, method.GetMemberType(0));

            type = typeof(DerivedTestClass);
            var implicitCtor = type.GetConstructor(Type.EmptyTypes);
            Assert.Equal(typeof(DerivedTestClass), implicitCtor.GetMemberType(CommonMemberTypes.Constructor));
            Assert.Equal(null, method.GetMemberType(0));
        }

#if !NETCOREAPP1_0
        [Fact]
        public void GetAttributeseTest()
        {
            var type = typeof(TestClass);
            var derivedType = typeof(DerivedTestClass);

            var property = type.GetProperty(nameof(TestClass.Property));
            var derivedProperty = derivedType.GetProperty(nameof(DerivedTestClass.Property));

            var virtualProperty = type.GetProperty(nameof(TestClass.VirtualProperty));
            var derivedVirtualProperty = derivedType.GetProperty(nameof(DerivedTestClass.VirtualProperty));

            #region non-inherited on type

            Assert.Equal(
                new[] { new NonInheritedAttribute(1), new NonInheritedAttribute(2) },
                type.GetAttributes<NonInheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new NonInheritedAttribute(1), new NonInheritedAttribute(2) },
                type.GetAttributes<NonInheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new NonInheritedAttribute[] { },
                derivedType.GetAttributes<NonInheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new NonInheritedAttribute[] { },
                derivedType.GetAttributes<NonInheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            #endregion

            #region non-inherited on member (static property)

            Assert.Equal(
                new[] { new NonInheritedAttribute(0) },
                property.GetAttributes<NonInheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new NonInheritedAttribute(0) },
                property.GetAttributes<NonInheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new NonInheritedAttribute(0) },
                derivedProperty.GetAttributes<NonInheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new NonInheritedAttribute(0) },
                derivedProperty.GetAttributes<NonInheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            #endregion

            #region non-inherited on member (virtual property)

            Assert.Equal(
                new[] { new NonInheritedAttribute(0) },
                virtualProperty.GetAttributes<NonInheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new NonInheritedAttribute(0) },
                virtualProperty.GetAttributes<NonInheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new NonInheritedAttribute[] { },
                derivedVirtualProperty.GetAttributes<NonInheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new NonInheritedAttribute[] { },
                derivedVirtualProperty.GetAttributes<NonInheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<NonInheritedAttribute, int>(attr => attr.Id));

            #endregion

            #region inherited on type

            Assert.Equal(
                new[] { new InheritedAttribute(1), new InheritedAttribute(2) },
                type.GetAttributes<InheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new InheritedAttribute(1), new InheritedAttribute(2) },
                type.GetAttributes<InheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new InheritedAttribute[] { },
                derivedType.GetAttributes<InheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new InheritedAttribute(1), new InheritedAttribute(2) },
                derivedType.GetAttributes<InheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            #endregion

            #region inherited on member (static property)

            Assert.Equal(
                new[] { new InheritedAttribute(0) },
                property.GetAttributes<InheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new InheritedAttribute(0) },
                property.GetAttributes<InheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new InheritedAttribute(0) },
                derivedProperty.GetAttributes<InheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new InheritedAttribute(0) },
                derivedProperty.GetAttributes<InheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            #endregion

            #region inherited on member (virtual property)

            Assert.Equal(
                new[] { new InheritedAttribute(0) },
                virtualProperty.GetAttributes<InheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new[] { new InheritedAttribute(0) },
                virtualProperty.GetAttributes<InheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new InheritedAttribute[] { },
                derivedVirtualProperty.GetAttributes<InheritedAttribute>(inherit: false).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            Assert.Equal(
                new InheritedAttribute[] { new InheritedAttribute(0) },
                derivedVirtualProperty.GetAttributes<InheritedAttribute>(inherit: true).OrderBy(attr => attr.Id),
                ProjectionEqualityComparer.Create<InheritedAttribute, int>(attr => attr.Id));

            #endregion
        }

        [Fact]
        public void HasAttributeseTest()
        {
            var type = typeof(TestClass);
            var derivedType = typeof(DerivedTestClass);

            var property = type.GetProperty(nameof(TestClass.Property));
            var derivedProperty = derivedType.GetProperty(nameof(DerivedTestClass.Property));

            var virtualProperty = type.GetProperty(nameof(TestClass.VirtualProperty));
            var derivedVirtualProperty = derivedType.GetProperty(nameof(DerivedTestClass.VirtualProperty));

            #region non-inherited on type

            Assert.True(type.HasAttribute<NonInheritedAttribute>(inherit: false));
            Assert.True(type.HasAttribute<NonInheritedAttribute>(inherit: true));
            Assert.False(derivedType.HasAttribute<NonInheritedAttribute>(inherit: false));
            Assert.False(derivedType.HasAttribute<NonInheritedAttribute>(inherit: true));

            #endregion

            #region non-inherited on member (static property)

            Assert.True(property.HasAttribute<NonInheritedAttribute>(inherit: false));
            Assert.True(property.HasAttribute<NonInheritedAttribute>(inherit: true));
            Assert.True(derivedProperty.HasAttribute<NonInheritedAttribute>(inherit: false));
            Assert.True(derivedProperty.HasAttribute<NonInheritedAttribute>(inherit: true));

            #endregion

            #region non-inherited on member (virtual property)

            Assert.True(virtualProperty.HasAttribute<NonInheritedAttribute>(inherit: false));
            Assert.True(virtualProperty.HasAttribute<NonInheritedAttribute>(inherit: true));
            Assert.False(derivedVirtualProperty.HasAttribute<NonInheritedAttribute>(inherit: false));
            Assert.False(derivedVirtualProperty.HasAttribute<NonInheritedAttribute>(inherit: true));

            #endregion

            #region inherited on type

            Assert.True(type.HasAttribute<InheritedAttribute>(inherit: false));
            Assert.True(type.HasAttribute<InheritedAttribute>(inherit: true));
            Assert.False(derivedType.HasAttribute<InheritedAttribute>(inherit: false));
            Assert.True(derivedType.HasAttribute<InheritedAttribute>(inherit: true));

            #endregion

            #region inherited on member (static property)

            Assert.True(property.HasAttribute<InheritedAttribute>(inherit: false));
            Assert.True(property.HasAttribute<InheritedAttribute>(inherit: true));
            Assert.True(derivedProperty.HasAttribute<InheritedAttribute>(inherit: false));
            Assert.True(derivedProperty.HasAttribute<InheritedAttribute>(inherit: true));

            #endregion

            #region inherited on member (virtual property)

            Assert.True(virtualProperty.HasAttribute<InheritedAttribute>(inherit: false));
            Assert.True(virtualProperty.HasAttribute<InheritedAttribute>(inherit: true));
            Assert.False(derivedVirtualProperty.HasAttribute<InheritedAttribute>(inherit: false));
            Assert.True(derivedVirtualProperty.HasAttribute<InheritedAttribute>(inherit: true));

            #endregion
        }
#endif

        [Fact]
        public void MakeFastGetterTest()
        {
            var type = typeof(TestClass);

            var obj = new DerivedTestClass { _field = "x", Property = 1 };

            var field = type.GetField(nameof(TestClass._field), SystemBindingFlags.Instance | SystemBindingFlags.NonPublic);
            Assert.Equal(obj._field, field.MakeFastGetter<TestClass, string>()(obj));
            Assert.Equal(obj._field, field.MakeFastGetter<TestClass, object>()(obj));
            Assert.Equal(obj._field, field.MakeFastGetter<object, object>()(obj));
            Assert.Throws<InvalidOperationException>(() => field.MakeFastGetter<object, StringBuilder>()(obj));
            Assert.Throws<InvalidCastException>(() => field.MakeFastGetter<object, string>()(new object()));

            var property = type.GetProperty(nameof(TestClass.Property));
            Assert.Equal(obj.Property, property.MakeFastGetter<TestClass, int>()(obj));
            Assert.Equal(obj.Property, property.MakeFastGetter<TestClass, int?>()(obj));
            Assert.Equal(obj.Property, property.MakeFastGetter<TestClass, short>()(obj));
            Assert.Equal(obj.Property, property.MakeFastGetter<TestClass, object>()(obj));
            Assert.Equal(obj.Property, property.MakeFastGetter<object, object>()(obj));
            Assert.Throws<InvalidOperationException>(() => property.MakeFastGetter<object, DateTime>()(obj));
            Assert.Throws<InvalidCastException>(() => property.MakeFastGetter<object, int>()(new object()));
        }

        [Fact]
        public void MakeFastSetterTest()
        {
            var type = typeof(TestClass);

            var field = type.GetField(nameof(TestClass._field), SystemBindingFlags.Instance | SystemBindingFlags.NonPublic);

            var obj = new DerivedTestClass { _field = "x" };
            field.MakeFastSetter<TestClass, string>()(obj, "y");
            Assert.Equal("y", obj._field);

            obj = new DerivedTestClass { _field = "x" };
            field.MakeFastSetter<TestClass, object>()(obj, "y");
            Assert.Equal("y", obj._field);

            obj = new DerivedTestClass { _field = "x" };
            field.MakeFastSetter<object, object>()(obj, "y");
            Assert.Equal("y", obj._field);

            Assert.Throws<InvalidOperationException>(() => field.MakeFastSetter<object, StringBuilder>()(obj, new StringBuilder()));
            Assert.Throws<InvalidCastException>(() => field.MakeFastSetter<object, string>()(new object(), "y"));

            var property = type.GetProperty(nameof(TestClass.Property));

            obj = new DerivedTestClass { Property = 1 };
            property.MakeFastSetter<TestClass, int>()(obj, 2);
            Assert.Equal(2, obj.Property);

            obj = new DerivedTestClass { Property = 1 };
            property.MakeFastSetter<TestClass, int?>()(obj, 2);
            Assert.Equal(2, obj.Property);

            obj = new DerivedTestClass { Property = 1 };
            property.MakeFastSetter<TestClass, short>()(obj, 2);
            Assert.Equal(2, obj.Property);

            obj = new DerivedTestClass { Property = 1 };
            property.MakeFastSetter<TestClass, object>()(obj, 2);
            Assert.Equal(2, obj.Property);

            obj = new DerivedTestClass { Property = 1 };
            property.MakeFastSetter<object, object>()(obj, 2);
            Assert.Equal(2, obj.Property);

            Assert.Throws<InvalidOperationException>(() => property.MakeFastSetter<object, DateTime>()(obj, default));
            Assert.Throws<InvalidCastException>(() => property.MakeFastSetter<object, int>()(new object(), 2));
        }

        [Fact]
        public void ObjectToDictionaryTest()
        {
            var cache = new ConcurrentDictionary<object, object>();

            var obj = new DerivedTestClass { Property = 1, VirtualProperty = 2, PublicField = "x" };
            var baseObj = new TestClass { Property = 1, VirtualProperty = 2, PublicField = "x", ShadowedPublicMember = "y" };

            #region default (properties only, include read-only properties, case-sensitive)

            var dic = ReflectionUtils.ObjectToDictionary(obj);
            Assert.Equal(4, dic.Count);
            Assert.Equal(obj.Property, dic[nameof(DerivedTestClass.Property)]);
            Assert.Equal(obj.VirtualProperty, dic[nameof(DerivedTestClass.VirtualProperty)]);
            Assert.Equal(obj.ReadOnlyProperty, dic[nameof(DerivedTestClass.ReadOnlyProperty)]);
            Assert.Equal(obj.ShadowedPublicMember, dic[nameof(DerivedTestClass.ShadowedPublicMember)]);

            dic = ReflectionUtils.ObjectToDictionaryCached(obj);
            Assert.Equal(4, dic.Count);
            Assert.Equal(obj.Property, dic[nameof(DerivedTestClass.Property)]);
            Assert.Equal(obj.VirtualProperty, dic[nameof(DerivedTestClass.VirtualProperty)]);
            Assert.Equal(obj.ReadOnlyProperty, dic[nameof(DerivedTestClass.ReadOnlyProperty)]);
            Assert.Equal(obj.ShadowedPublicMember, dic[nameof(DerivedTestClass.ShadowedPublicMember)]);

            #endregion

            #region properties only, exclude read-only properties, case-sensitive

            dic = ReflectionUtils.ObjectToDictionary(obj, flags: ObjectToDictionaryFlags.ExcludeReadOnlyProperties);
            Assert.Equal(2, dic.Count);
            Assert.Equal(obj.Property, dic[nameof(DerivedTestClass.Property)]);
            Assert.Equal(obj.VirtualProperty, dic[nameof(DerivedTestClass.VirtualProperty)]);

            dic = ReflectionUtils.ObjectToDictionaryCached(obj, flags: ObjectToDictionaryFlags.ExcludeReadOnlyProperties);
            Assert.Equal(2, dic.Count);
            Assert.Equal(obj.Property, dic[nameof(DerivedTestClass.Property)]);
            Assert.Equal(obj.VirtualProperty, dic[nameof(DerivedTestClass.VirtualProperty)]);

            #endregion

            #region fields only, case-sensitive

            dic = ReflectionUtils.ObjectToDictionary(obj, memberTypes: CommonMemberTypes.Field);
            Assert.Equal(1, dic.Count);
            Assert.Equal(obj.PublicField, dic[nameof(DerivedTestClass.PublicField)]);

            dic = ReflectionUtils.ObjectToDictionaryCached(obj, memberTypes: CommonMemberTypes.Field);
            Assert.Equal(1, dic.Count);
            Assert.Equal(obj.PublicField, dic[nameof(DerivedTestClass.PublicField)]);

            dic = ReflectionUtils.ObjectToDictionary(baseObj, memberTypes: CommonMemberTypes.Field);
            Assert.Equal(2, dic.Count);
            Assert.Equal(baseObj.PublicField, dic[nameof(TestClass.PublicField)]);
            Assert.Equal(baseObj.ShadowedPublicMember, dic[nameof(TestClass.ShadowedPublicMember)]);

            dic = ReflectionUtils.ObjectToDictionaryCached(baseObj, memberTypes: CommonMemberTypes.Field);
            Assert.Equal(2, dic.Count);
            Assert.Equal(baseObj.PublicField, dic[nameof(TestClass.PublicField)]);
            Assert.Equal(baseObj.ShadowedPublicMember, dic[nameof(TestClass.ShadowedPublicMember)]);

            #endregion
        }
    }
}
