using System;
using System.Reflection;

namespace Karambolo.Common
{
    internal static class Platform
    {
        public static readonly bool? IsWindowsOS =
#if NET40 || NET45
            true;
#elif NETSTANDARD1_0
            null;
#else
            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
#endif
    }

    internal static class MathShim
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

    internal static partial class ReflectionShim
    {
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static Assembly Assembly(this Type type)
        {
#if !NETSTANDARD1_0
            return type.Assembly;
#else
            return type.GetTypeInfo().Assembly;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValueType(this Type type)
        {
#if !NETSTANDARD1_0
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsClass(this Type type)
        {
#if !NETSTANDARD1_0
            return type.IsClass;
#else
            return type.GetTypeInfo().IsClass;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInterface(this Type type)
        {
#if !NETSTANDARD1_0
            return type.IsInterface;
#else
            return type.GetTypeInfo().IsInterface;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsGenericType(this Type type)
        {
#if !NETSTANDARD1_0
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsGenericTypeDefinition(this Type type)
        {
#if !NETSTANDARD1_0
            return type.IsGenericTypeDefinition;
#else
            return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static Type BaseType(this Type type)
        {
#if !NETSTANDARD1_0
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }

        public static MemberTypes MemberType(this MemberInfo member)
        {
#if !NETSTANDARD1_0
            return member.MemberType;
#else
            if (member == null)
                throw new NullReferenceException();

            switch (member)
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
