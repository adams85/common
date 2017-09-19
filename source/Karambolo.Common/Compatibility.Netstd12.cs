using System;
using System.Linq;
using System.Reflection;

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
    static partial class ReflectionExtensions
    {
        public static PropertyInfo[] GetProperties(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredProperties.ToArray();
        }

        public static bool IsAssignableFrom(this Type @this, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return @this.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }
    }
}