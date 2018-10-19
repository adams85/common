using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
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

namespace Karambolo.Common
{
    static partial class StringExtensions
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

    static partial class ReflectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo[] GetProperties(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredProperties.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAssignableFrom(this Type @this, Type type)
        {
            if (@this == null)
                throw new NullReferenceException();

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return @this.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }
    }
}