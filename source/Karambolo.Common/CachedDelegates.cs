using System;

namespace Karambolo.Common
{
    public static class CachedDelegates
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

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class Noop
    {
        public static readonly Action Action = CachedDelegates.Noop.Action;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class Noop<T>
    {
        public static readonly Action<T> Action = CachedDelegates.Noop<T>.Action;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class Identity<T>
    {
        public static readonly Func<T, T> Func = CachedDelegates.Identity<T>.Func;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class DefaultMap<T>
    {
        public static readonly Func<T, T> Func = CachedDelegates.DefaultMap<T>.Func;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class Default<T>
    {
        public static readonly Func<T> Func = CachedDelegates.Default<T>.Func;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class False
    {
        public static readonly Func<bool> Func = CachedDelegates.False.Func;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class False<T>
    {
        public static readonly Func<T, bool> Func = CachedDelegates.False<T>.Func;
        public static readonly Predicate<T> Predicate = CachedDelegates.False<T>.Predicate;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class True
    {
        public static readonly Func<bool> Func = CachedDelegates.True.Func;
    }

    [Obsolete("This type is moved into the CachedDelegates container class, thus the uncontained type will be removed in the next major version.")]
    public static class True<T>
    {
        public static readonly Func<T, bool> Func = CachedDelegates.True<T>.Func;
        public static readonly Predicate<T> Predicate = CachedDelegates.True<T>.Predicate;
    }
}
