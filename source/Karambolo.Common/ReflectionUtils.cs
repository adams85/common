using System;
using System.Collections.Generic;
using System.Linq;
using Karambolo.Common.Properties;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading;

namespace Karambolo.Common
{
    [Flags]
    public enum ObjectToDictionaryFlags
    {
        None = 0,
        IgnoreCase = 0x1,
        ExcludeReadOnlyProperties = 0x2,
    }

    public static class ReflectionUtils
    {
        internal sealed class MemberDescriptor
        {
            public static MemberDescriptor Create(MemberInfo member)
            {
                return new MemberDescriptor(member);
            }

            MemberDescriptor(MemberInfo member)
            {
                Member = member;
                ValueAccessor = MakeFastGetter<object, object>(member, MemberTypes.Field | MemberTypes.Property);
                IsReadOnly = member is PropertyInfo property ? IsPropertyReadOnly(property) : false;
            }

            public readonly MemberInfo Member;
            public readonly Func<object, object> ValueAccessor;
            public readonly bool IsReadOnly;
        }

#if !NETSTANDARD1_0
        internal static System.Collections.Concurrent.ConcurrentDictionary<Type, MemberDescriptor[]> memberDescriptorCache;
#else
        internal static Dictionary<Type, MemberDescriptor[]> memberDescriptorCache;
#endif

        public static bool AllowsNull(this Type type)
        {
            return !type.IsValueType() || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsAssignableFrom(this Type type, object obj)
        {
            return obj != null ? type.IsAssignableFrom(obj.GetType()) : type.AllowsNull();
        }

        public static bool IsDelegate(this Type type)
        {
            return type == typeof(Delegate) || type.IsSubclassOf(typeof(Delegate));
        }

        public static Type GetInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new NullReferenceException();

            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));
            if (!interfaceType.IsInterface())
                throw new ArgumentException(Resources.NotInterfaceType, nameof(interfaceType));

            return Array.Find(type.GetInterfaces(), t => t == interfaceType);
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            return type.GetInterface(interfaceType) != null;
        }

        public static IEnumerable<Type> GetClosedInterfaces(this Type type, Type openInterfaceType)
        {
            if (type == null)
                throw new NullReferenceException();

            if (openInterfaceType == null)
                throw new ArgumentNullException(nameof(openInterfaceType));
            if (!openInterfaceType.IsInterface() || !openInterfaceType.IsGenericTypeDefinition())
                throw new ArgumentException(Resources.NotGenericInterfaceType, nameof(openInterfaceType));

            // even if type is an open generic type, GetInterfaces() returns closed types (parameterized with System.RuntimeType)
            return type.GetInterfaces()
                .Where(intfType => intfType.IsGenericType() && intfType.GetGenericTypeDefinition() == openInterfaceType);
        }

        public static bool HasClosedInterface(this Type type, Type openInterfaceType)
        {
            return GetClosedInterfaces(type, openInterfaceType).Any();
        }

        public static Type GetMemberType(this MemberInfo type, MemberTypes allowedMemberTypes = MemberTypes.All)
        {
            switch (type)
            {
                case FieldInfo field when (allowedMemberTypes & MemberTypes.Field) != 0:
                    return field.FieldType;
                case PropertyInfo property when (allowedMemberTypes & MemberTypes.Property) != 0:
                    return property.PropertyType;
                case EventInfo @event when (allowedMemberTypes & MemberTypes.Event) != 0:
                    return @event.EventHandlerType;
                case MethodInfo method when (allowedMemberTypes & MemberTypes.Method) != 0:
                    return method.ReturnType;
                case ConstructorInfo ctor when (allowedMemberTypes & MemberTypes.Constructor) != 0:
                    return ctor.DeclaringType;
                default:
                    return null;
            }
        }

#if !NETSTANDARD1_0
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider, bool inherit = false)
            where TAttribute : Attribute
        {
            if (attributeProvider == null)
                throw new NullReferenceException();

            // http://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
            return
                (!inherit || !(attributeProvider is MemberInfo memberInfo) ?
                attributeProvider.GetCustomAttributes(typeof(TAttribute), inherit) :
                Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute), inherit)).Cast<TAttribute>();
        }

        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider, bool inherit = false)
            where TAttribute : Attribute
        {
            if (attributeProvider == null)
                throw new NullReferenceException();

            // http://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
            return
                !inherit || !(attributeProvider is MemberInfo memberInfo) ?
                attributeProvider.IsDefined(typeof(TAttribute), inherit) :
                Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit);
        }
#endif

        static Expression BuildMemberAccessExpression<TContainer>(MemberInfo member, out ParameterExpression param)
        {
            param = Expression.Parameter(typeof(TContainer));

            Expression expression = param;
            if (member.DeclaringType != typeof(TContainer))
                expression = Expression.Convert(expression, member.DeclaringType);

            expression = Expression.MakeMemberAccess(expression, member);

            return expression;
        }

        static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(MemberInfo member, MemberTypes allowedMemberTypes)
        {
            var memberType = member.GetMemberType(allowedMemberTypes);

            var memberAccess = BuildMemberAccessExpression<TContainer>(member, out var param);

            if (typeof(TMember) != memberType)
                memberAccess = Expression.Convert(memberAccess, typeof(TMember));

            return Expression.Lambda<Func<TContainer, TMember>>(memberAccess, param).Compile();
        }

        public static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(this FieldInfo field)
        {
            return MakeFastGetter<TContainer, TMember>(field, MemberTypes.Field);
        }

        public static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(this PropertyInfo property)
        {
            return MakeFastGetter<TContainer, TMember>(property, MemberTypes.Property);
        }

        static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(MemberInfo member, MemberTypes allowedMemberTypes)
        {
            var memberType = member.GetMemberType(allowedMemberTypes);

            var expression = BuildMemberAccessExpression<TContainer>(member, out var param);

            var valueParam = Expression.Parameter(typeof(TMember));
            Expression valueExpression = valueParam;
            if (memberType != typeof(TMember))
                valueExpression = Expression.Convert(valueExpression, memberType);

            expression = Expression.Assign(expression, valueExpression);

            return Expression.Lambda<Action<TContainer, TMember>>(expression, param, valueParam).Compile();
        }

        public static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(this FieldInfo field)
        {
            return MakeFastSetter<TContainer, TMember>(field, MemberTypes.Field);
        }

        public static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(this PropertyInfo property)
        {
            return MakeFastSetter<TContainer, TMember>(property, MemberTypes.Property);
        }

        static bool IsPropertyReadOnly(PropertyInfo property)
        {
            return property.GetSetMethod() == null;
        }

        static IEnumerable<FieldInfo> GetObjectToDictionaryFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        }

        static IEnumerable<PropertyInfo> GetObjectToDictionaryProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(prop =>
                {
                    var getMethod = prop.GetGetMethod();
                    return getMethod != null && getMethod.GetParameters().Length == 0;
                });
        }

        static IEnumerable<MemberInfo> GetObjectToDictionaryMembers(Type type)
        {
            IEnumerable<MemberInfo> members = GetObjectToDictionaryFields(type);
            members = members.Concat(GetObjectToDictionaryProperties(type));

            if (type.IsClass() && (type = type.BaseType()) != null)
            {
                var membersByName = members.ToDictionary(member => member.Name, Identity<MemberInfo>.Func);

                do
                {
                    members = GetObjectToDictionaryFields(type);
                    members = members.Concat(GetObjectToDictionaryProperties(type));

                    foreach (var member in members)
                        if (!membersByName.ContainsKey(member.Name))
                            membersByName.Add(member.Name, member);
                }
                while ((type = type.BaseType()) != null);

                members = membersByName.Values;
            }

            return members;
        }

        public static IDictionary<string, object> ObjectToDictionary(object obj, MemberTypes memberTypes = MemberTypes.Property,
            ObjectToDictionaryFlags flags = ObjectToDictionaryFlags.None)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if ((memberTypes & ~(MemberTypes.Field | MemberTypes.Property)) != 0)
                throw new ArgumentException(Resources.FieldOrPropertyAllowedOnly, nameof(memberTypes));

            var excludeReadOnly = (flags & ObjectToDictionaryFlags.ExcludeReadOnlyProperties) != 0;

            var dictionary = new Dictionary<string, object>((flags & ObjectToDictionaryFlags.IgnoreCase) == 0 ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

            foreach (var member in GetObjectToDictionaryMembers(obj.GetType()))
                if ((member.MemberType() & memberTypes) != 0)
                {
                    object value;

                    if (member is PropertyInfo property)
                    {
                        if (excludeReadOnly && IsPropertyReadOnly(property))
                            continue;

                        value = property.GetValue(obj, null);
                    }
                    else
                        value = ((FieldInfo)member).GetValue(obj);

                    dictionary.Add(member.Name, value);
                }

            return dictionary;
        }

        public static IDictionary<string, object> ObjectToDictionaryCached(object obj, MemberTypes memberTypes = MemberTypes.Property,
    ObjectToDictionaryFlags flags = ObjectToDictionaryFlags.None)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if ((memberTypes & ~(MemberTypes.Field | MemberTypes.Property)) != 0)
                throw new ArgumentException(Resources.FieldOrPropertyAllowedOnly, nameof(memberTypes));

            var type = obj.GetType();
            MemberDescriptor[] memberDescriptors;

#if !NETSTANDARD1_0
            LazyInitializer.EnsureInitialized(ref memberDescriptorCache, () => new System.Collections.Concurrent.ConcurrentDictionary<Type, MemberDescriptor[]>());
#else
            LazyInitializer.EnsureInitialized(ref memberDescriptorCache, () => new Dictionary<Type, MemberDescriptor[]>());
            lock (memberDescriptorCache)
#endif
            if (!memberDescriptorCache.TryGetValue(type, out memberDescriptors))
            {
                memberDescriptors = GetObjectToDictionaryMembers(type).Select(MemberDescriptor.Create).ToArray();
#if !NETSTANDARD1_0
                memberDescriptorCache.TryAdd(type, memberDescriptors);
#else
                memberDescriptorCache.Add(type, memberDescriptors);
#endif
            }

            var excludeReadOnly = (flags & ObjectToDictionaryFlags.ExcludeReadOnlyProperties) != 0;

            var dictionary = new Dictionary<string, object>((flags & ObjectToDictionaryFlags.IgnoreCase) == 0 ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

            MemberDescriptor memberDescriptor;
            var n = memberDescriptors.Length;
            for (var i = 0; i < n; i++)
                if (((memberDescriptor = memberDescriptors[i]).Member.MemberType() & memberTypes) != 0 &&
                    (!excludeReadOnly || !memberDescriptor.IsReadOnly))
                    dictionary.Add(memberDescriptor.Member.Name, memberDescriptor.ValueAccessor(obj));

            return dictionary;
        }
    }
}
