using System;

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
        public static readonly Func<T, bool> Func = _ => false;
        public static readonly Predicate<T> Predicate = new Predicate<T>(Func);
    }

    public static class True
    {
        public static readonly Func<bool> Func = () => true;
    }

    public static class True<T>
    {
        public static readonly Func<T, bool> Func = _ => true;
        public static readonly Predicate<T> Predicate = new Predicate<T>(Func);
    }
}
