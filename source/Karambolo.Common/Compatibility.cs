using System;
using System.Reflection;

namespace Karambolo.Common
{
    static class MathShim
    {
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static int DivRem(int a, int b, out int result)
        {
#if !NETSTANDARD1_0
            return Math.DivRem(a, b, out result);
#else
            result = a % b;
            return a / b;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static long DivRem(long a, long b, out long result)
        {
#if !NETSTANDARD1_0
            return Math.DivRem(a, b, out result);
#else
            result = a % b;
            return a / b;
#endif
        }
    }

    static partial class ReflectionShim
    {
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static Assembly Assembly(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.Assembly;
#else
            return @this.GetTypeInfo().Assembly;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValueType(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.IsValueType;
#else
            return @this.GetTypeInfo().IsValueType;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsClass(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.IsClass;
#else
            return @this.GetTypeInfo().IsClass;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInterface(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.IsInterface;
#else
            return @this.GetTypeInfo().IsInterface;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsGenericType(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.IsGenericType;
#else
            return @this.GetTypeInfo().IsGenericType;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsGenericTypeDefinition(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.IsGenericTypeDefinition;
#else
            return @this.GetTypeInfo().IsGenericTypeDefinition;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static Type BaseType(this Type @this)
        {
#if !NETSTANDARD1_0
            return @this.BaseType;
#else
            return @this.GetTypeInfo().BaseType;
#endif
        }

        public static MemberTypes MemberType(this MemberInfo @this)
        {
#if !NETSTANDARD1_0
            return @this.MemberType;
#else
            if (@this == null)
                throw new NullReferenceException();

            switch (@this)
            {
                case ConstructorInfo ci: return MemberTypes.Constructor;
                case EventInfo ei: return MemberTypes.Event;
                case FieldInfo fi: return MemberTypes.Field;
                case MethodInfo mi: return MemberTypes.Method;
                case PropertyInfo pi: return MemberTypes.Property;
                case TypeInfo ti: return !ti.IsNested ? MemberTypes.TypeInfo : MemberTypes.NestedType;
                default: return MemberTypes.Custom;
            }
#endif
        }
    }
}
