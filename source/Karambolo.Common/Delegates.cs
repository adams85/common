using System;

namespace Karambolo.Common
{
    public static class Empty
    {
        public static readonly Action Action = () => { };
    }

    public static class Empty<T>
    {
        public static readonly Action<T> Action = arg => { };
    }

    public static class Empty<T1, T2>
    {
        public static readonly Action<T1, T2> Action = (arg1, arg2) => { };
    }

    public static class Identity<T>
    {
        public static readonly Func<T, T> Func = arg => arg;
    }

    public static class False
    {
        public static readonly Func<bool> Func = () => false;
    }

    public static class False<T>
    {
        public static readonly Func<T, bool> Func = arg => false;
        public static readonly Predicate<T> Predicate = arg => false;
    }

    public static class True
    {
        public static readonly Func<bool> Func = () => true;
    }

    public static class True<T>
    {
        public static readonly Func<T, bool> Func = arg => true;
        public static readonly Predicate<T> Predicate = arg => true;
    }

    public static class Default<T>
    {
        public static readonly Func<T> Func = () => default(T);
    }

    public static class Default<T1, T2>
    {
        public static readonly Func<T1, T2> Func = arg => default(T2);
    }
}
