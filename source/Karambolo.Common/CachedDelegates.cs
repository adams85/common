using System;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    public static class Noop
    {
        public static readonly Action Action = () => { };
    }

    public static class Noop<T>
    {
        public static readonly Action<T> Action = _ => { };
    }

    public static class Identity<T>
    {
        public static readonly Func<T, T> Func = arg => arg;
    }

    public static class DefaultMap<T>
    {
        public static readonly Func<T, T> Func = _ => default;
    }

    public static class Default<T>
    {
        public static readonly Func<T> Func = () => default;
    }

    public static class False
    {
        public static readonly Func<bool> Func = () => false;
    }

    public static class False<T>
    {
        private static bool Impl(T _) => false;
        public static readonly Func<T, bool> Func = Impl;
        public static readonly Predicate<T> Predicate = Impl;
    }

    public static class True
    {
        public static readonly Func<bool> Func = () => true;
    }

    public static class True<T>
    {
        private static bool Impl(T _) => true;
        public static readonly Func<T, bool> Func = Impl;
        public static readonly Predicate<T> Predicate = Impl;
    }
}
