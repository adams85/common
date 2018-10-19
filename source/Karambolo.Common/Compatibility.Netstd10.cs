using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
    [Flags]
    public enum BindingFlags
    {
        Default = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64,
        InvokeMethod = 256,
        CreateInstance = 512,
        GetField = 1024,
        SetField = 2048,
        GetProperty = 4096,
        SetProperty = 8192
    }

    [Flags]
    public enum MemberTypes
    {
        Constructor = 1,
        Event = 2,
        Field = 4,
        Method = 8,
        Property = 16,
        TypeInfo = 32,
        Custom = 64,
        NestedType = 128,
        All = 191
    }
}

namespace Karambolo.Common
{
    static partial class StringShim
    {
        public static IEnumerable<char> AsEnumerable(this string source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var n = source.Length;
            for (var i = 0; i < n; i++)
                yield return source[i];
        }
    }

    static partial class ReflectionShim
    {
        public static FieldInfo[] GetFields(this Type type, BindingFlags bindingFlags)
        {
            if (type == null)
                throw new NullReferenceException();

            if (bindingFlags != (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                throw new NotImplementedException();

            var typeInfo = type.GetTypeInfo();

            var fields = typeInfo.DeclaredFields.Where(ShouldIncludeField);

            return fields.ToArray();

            bool ShouldIncludeField(FieldInfo field)
            {
                return !field.IsStatic && field.IsPublic;
            }
        }

        public static PropertyInfo[] GetProperties(this Type type, BindingFlags bindingFlags)
        {
            if (type == null)
                throw new NullReferenceException();

            if (bindingFlags != (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                throw new NotImplementedException();

            var typeInfo = type.GetTypeInfo();

            var properties = typeInfo.DeclaredProperties.Where(ShouldIncludeProperty);

            return properties.ToArray();

            bool ShouldIncludeProperty(PropertyInfo property)
            {
                var method = property.GetMethod ?? property.SetMethod;
                return !method.IsStatic && method.IsPublic;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAssignableFrom(this Type type, Type c)
        {
            if (type == null)
                throw new NullReferenceException();

            if (c == null)
                throw new ArgumentNullException(nameof(c));

            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSubclassOf(this Type type, Type c)
        {
            return type.GetTypeInfo().IsSubclassOf(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GetInterfaces(this Type type)
        {
            return System.Linq.Enumerable.ToArray(type.GetTypeInfo().ImplementedInterfaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetGetMethod(this PropertyInfo property)
        {
            var getMethod = property.GetMethod;
            return getMethod != null && getMethod.IsPublic ? getMethod : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetSetMethod(this PropertyInfo property)
        {
            var setMethod = property.SetMethod;
            return setMethod != null && setMethod.IsPublic ? setMethod : null;
        }
    }
}

namespace System.Collections.Specialized
{
    public interface IOrderedDictionary : IDictionary, ICollection, IEnumerable
    {
        object this[int index] { get; set; }
        new IDictionaryEnumerator GetEnumerator();
        void Insert(int index, object key, object value);
        void RemoveAt(int index);
    }
}
