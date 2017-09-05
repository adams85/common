using System;
using System.Collections.Generic;
using System.Linq;
using Karambolo.Common.Properties;
using System.Reflection;
using System.Linq.Expressions;

namespace Karambolo.Common
{
    public static class ReflectionUtils
    {
        const string instanceMemberName = "Instance";
        const string defaultMemberName = "Default";

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
            if (!interfaceType.IsInterface)
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
            if (!openInterfaceType.IsInterface || !openInterfaceType.IsGenericType || !openInterfaceType.IsGenericTypeDefinition)
                throw new ArgumentException(Resources.NotGenericInterfaceType, nameof(openInterfaceType));

            // even if type is an open generic type, GetInterfaces() returns closed types (parameterized with System.RuntimeType)
            return type.GetInterfaces()
                .Where(intf => intf.IsGenericType && intf.GetGenericTypeDefinition() == openInterfaceType);
        }

        public static bool HasClosedInterface(this Type type, Type openInterfaceType)
        {
            return GetClosedInterfaces(type, openInterfaceType).Any();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ICustomAttributeProvider attributeProvider, bool inherit = false)
            where TAttribute : Attribute
        {
            if (attributeProvider == null)
                throw new NullReferenceException();

            // http://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
            MemberInfo memberInfo;
            return (
                !inherit || (memberInfo = attributeProvider as MemberInfo) == null ?
                attributeProvider.GetCustomAttributes(typeof(TAttribute), inherit) :
                Attribute.GetCustomAttributes(memberInfo, typeof(TAttribute), inherit)).Cast<TAttribute>();
        }

        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider, bool inherit = false)
            where TAttribute : Attribute
        {
            if (attributeProvider == null)
                throw new NullReferenceException();

            // http://blog.seancarpenter.net/2012/12/15/getcustomattributes-and-overridden-properties/
            MemberInfo memberInfo;
            return
                !inherit || (memberInfo = attributeProvider as MemberInfo) == null ?
                attributeProvider.IsDefined(typeof(TAttribute), inherit) :
                Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit);
        }

        public static MemberInfo GetSingletonMember(Type objectType, string memberName)
        {
            var member = objectType.GetMember(instanceMemberName, MemberTypes.Field | MemberTypes.Property, BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly).FirstOrDefault();
            if (member != null)
                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property;
                    if (objectType.IsAssignableFrom((property = (PropertyInfo)member).PropertyType))
                        return property;
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field;
                    if (objectType.IsAssignableFrom((field = (FieldInfo)member).FieldType))
                        return field;
                }
                else
                    throw new InvalidOperationException();

            return null;
        }

        public static Func<object> CreateSingletonInstanceGetter(Type objectType)
        {
            var member = GetSingletonMember(objectType, instanceMemberName) ?? GetSingletonMember(objectType, defaultMemberName);

            return
                member != null ?
                Expression.Lambda<Func<object>>(Expression.MakeMemberAccess(null, member)).Compile() :
                null;
        }
    }
}
