using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Karambolo.Common.Properties;

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

            private MemberDescriptor(MemberInfo member)
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
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, MemberDescriptor[]> s_memberDescriptorCache;
#else
        private static Dictionary<Type, MemberDescriptor[]> s_memberDescriptorCache;
#endif

        public static bool AllowsNull(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return !type.IsValueType() || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsAssignableFrom(this Type type, object obj)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return obj != null ? type.IsAssignableFrom(obj.GetType()) : type.AllowsNull();
        }

        public static bool IsSameOrSubclassOf(this Type type, Type c)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type == c || type.IsSubclassOf(c);
        }

        public static bool IsDelegate(this Type type)
        {
            return type.IsSameOrSubclassOf(typeof(Delegate));
        }

        public static Type GetInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

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
                throw new ArgumentNullException(nameof(type));

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

        public static Type GetMemberType(this MemberInfo member, MemberTypes allowedMemberTypes = MemberTypes.All)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            switch (member)
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
        public static TAttribute[] GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider, bool inherit = false)
            where TAttribute : Attribute
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            // http://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
            return
                (!inherit || !(attributeProvider is MemberInfo memberInfo) ?
                (TAttribute[])attributeProvider.GetCustomAttributes(typeof(TAttribute), inherit) :
                (TAttribute[])Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute), inherit));
        }

        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider, bool inherit = false)
            where TAttribute : Attribute
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            // http://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
            return
                !inherit || !(attributeProvider is MemberInfo memberInfo) ?
                attributeProvider.IsDefined(typeof(TAttribute), inherit) :
                Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit);
        }
#endif

        private static Expression BuildMemberAccessExpression<TContainer>(MemberInfo member, out ParameterExpression param)
        {
            param = Expression.Parameter(typeof(TContainer));

            Expression expression = param;
            if (member.DeclaringType != typeof(TContainer))
                expression = Expression.Convert(expression, member.DeclaringType);

            expression = Expression.MakeMemberAccess(expression, member);

            return expression;
        }

        private static Func<TContainer, TMember> MakeFastGetterCore<TContainer, TMember>(this MemberInfo member, Type memberType)
        {
            Expression memberAccess = BuildMemberAccessExpression<TContainer>(member, out ParameterExpression param);

            if (typeof(TMember) != memberType)
                memberAccess = Expression.Convert(memberAccess, typeof(TMember));

            return Expression.Lambda<Func<TContainer, TMember>>(memberAccess, param).Compile();
        }

        public static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(this MemberInfo member, MemberTypes allowedMemberTypes)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if ((allowedMemberTypes & ~(MemberTypes.Field | MemberTypes.Property)) != 0)
                throw new ArgumentException(Resources.FieldOrPropertyAllowedOnly, nameof(allowedMemberTypes));

            MemberTypes memberType = member.MemberType();
            if ((allowedMemberTypes & memberType) == 0)
                throw new ArgumentException(Resources.InvalidValue, nameof(member));

            return
                memberType == MemberTypes.Property ?
                ((PropertyInfo)member).MakeFastGetter<TContainer, TMember>() :
                ((FieldInfo)member).MakeFastGetter<TContainer, TMember>();
        }

        public static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(this FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            return MakeFastGetterCore<TContainer, TMember>(field, field.FieldType);
        }

        public static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(this PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

#if !NETSTANDARD1_0
            MethodInfo getMethod = property.GetGetMethod(nonPublic: true);
            if (getMethod != null)
            {
                Type getMethodDelegateType = typeof(Func<,>).MakeGenericType(getMethod.DeclaringType, getMethod.ReturnType);

                if (typeof(Func<TContainer, TMember>).IsAssignableFrom(getMethodDelegateType))
                    return (Func<TContainer, TMember>)Delegate.CreateDelegate(getMethodDelegateType, getMethod);
            }
#endif

            return MakeFastGetterCore<TContainer, TMember>(property, property.PropertyType);
        }

        public static Func<TContainer, TMember> MakeFastGetter<TContainer, TMember>(this Expression<Func<TContainer, TMember>> expression)
        {
            MemberExpression memberExpression = expression.GetMemberExpression(MemberTypes.Field | MemberTypes.Property);
            if (memberExpression == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return
                memberExpression.Member is PropertyInfo property ?
                property.MakeFastGetter<TContainer, TMember>() :
                ((FieldInfo)memberExpression.Member).MakeFastGetter<TContainer, TMember>();
        }

        private static Action<TContainer, TMember> MakeFastSetterCore<TContainer, TMember>(this MemberInfo member, Type memberType)
        {
            Expression expression = BuildMemberAccessExpression<TContainer>(member, out ParameterExpression param);

            ParameterExpression valueParam = Expression.Parameter(typeof(TMember));
            Expression valueExpression = valueParam;
            if (memberType != typeof(TMember))
                valueExpression = Expression.Convert(valueExpression, memberType);

            expression = Expression.Assign(expression, valueExpression);

            return Expression.Lambda<Action<TContainer, TMember>>(expression, param, valueParam).Compile();
        }

        public static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(this MemberInfo member, MemberTypes allowedMemberTypes)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if ((allowedMemberTypes & ~(MemberTypes.Field | MemberTypes.Property)) != 0)
                throw new ArgumentException(Resources.FieldOrPropertyAllowedOnly, nameof(allowedMemberTypes));

            MemberTypes memberType = member.MemberType();
            if ((allowedMemberTypes & memberType) == 0)
                throw new ArgumentException(Resources.InvalidValue, nameof(member));

            return
                memberType == MemberTypes.Property ?
                ((PropertyInfo)member).MakeFastSetter<TContainer, TMember>() :
                ((FieldInfo)member).MakeFastSetter<TContainer, TMember>();
        }

        public static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(this FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            return MakeFastSetterCore<TContainer, TMember>(field, field.FieldType);
        }

        public static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(this PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

#if !NETSTANDARD1_0
            MethodInfo setMethod = property.GetSetMethod(nonPublic: true);
            if (setMethod != null)
            {
                Type setMethodDelegateType = typeof(Action<,>).MakeGenericType(setMethod.DeclaringType, setMethod.GetParameters()[0].ParameterType);

                if (typeof(Action<TContainer, TMember>).IsAssignableFrom(setMethodDelegateType))
                    return (Action<TContainer, TMember>)Delegate.CreateDelegate(setMethodDelegateType, setMethod);
            }
#endif

            return MakeFastSetterCore<TContainer, TMember>(property, property.PropertyType);
        }

        public static Action<TContainer, TMember> MakeFastSetter<TContainer, TMember>(this Expression<Func<TContainer, TMember>> expression)
        {
            MemberExpression memberExpression = expression.GetMemberExpression(MemberTypes.Field | MemberTypes.Property);
            if (memberExpression == null)
                throw new ArgumentException(Resources.InvalidValue, nameof(expression));

            return
                memberExpression.Member is PropertyInfo property ?
                property.MakeFastSetter<TContainer, TMember>() :
                ((FieldInfo)memberExpression.Member).MakeFastSetter<TContainer, TMember>();
        }

        private static bool IsPropertyReadOnly(PropertyInfo property)
        {
            return property.GetSetMethod() == null;
        }

        private static IEnumerable<FieldInfo> GetObjectToDictionaryFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        }

        private static IEnumerable<PropertyInfo> GetObjectToDictionaryProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(prop =>
                {
                    MethodInfo getMethod = prop.GetGetMethod();
                    return getMethod != null && getMethod.GetParameters().Length == 0;
                });
        }

        private static IEnumerable<MemberInfo> GetObjectToDictionaryMembers(Type type)
        {
            IEnumerable<MemberInfo> members = GetObjectToDictionaryFields(type);
            members = members.Concat(GetObjectToDictionaryProperties(type));

            if (type.IsClass() && (type = type.BaseType()) != null)
            {
                var membersByName = members.ToDictionary(member => member.Name, CachedDelegates.Identity<MemberInfo>.Func);

                do
                {
                    members = GetObjectToDictionaryFields(type);
                    members = members.Concat(GetObjectToDictionaryProperties(type));

                    foreach (MemberInfo member in members)
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

            foreach (MemberInfo member in GetObjectToDictionaryMembers(obj.GetType()))
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

            Type type = obj.GetType();
#pragma warning disable IDE0018 // Inline variable declaration
            MemberDescriptor[] memberDescriptors;
#pragma warning restore IDE0018 // Inline variable declaration

#if !NETSTANDARD1_0
            LazyInitializer.EnsureInitialized(ref s_memberDescriptorCache, () => new System.Collections.Concurrent.ConcurrentDictionary<Type, MemberDescriptor[]>());
#else
            LazyInitializer.EnsureInitialized(ref s_memberDescriptorCache, () => new Dictionary<Type, MemberDescriptor[]>());
            lock (s_memberDescriptorCache)
#endif
                if (!s_memberDescriptorCache.TryGetValue(type, out memberDescriptors))
                {
                    memberDescriptors = GetObjectToDictionaryMembers(type).Select(MemberDescriptor.Create).ToArray();
#if !NETSTANDARD1_0
                    s_memberDescriptorCache.TryAdd(type, memberDescriptors);
#else
                    s_memberDescriptorCache.Add(type, memberDescriptors);
#endif
                }

            var excludeReadOnly = (flags & ObjectToDictionaryFlags.ExcludeReadOnlyProperties) != 0;

            var dictionary = new Dictionary<string, object>((flags & ObjectToDictionaryFlags.IgnoreCase) == 0 ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

            MemberDescriptor memberDescriptor;
            for (int i = 0, n = memberDescriptors.Length; i < n; i++)
                if (((memberDescriptor = memberDescriptors[i]).Member.MemberType() & memberTypes) != 0 &&
                    (!excludeReadOnly || !memberDescriptor.IsReadOnly))
                    dictionary.Add(memberDescriptor.Member.Name, memberDescriptor.ValueAccessor(obj));

            return dictionary;
        }
    }
}
