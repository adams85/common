using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    public sealed class ProjectionEqualityComparer<TSource, TKey> : IEqualityComparer<TSource>
    {
        private readonly Func<TSource, TKey> _projection;
        private readonly IEqualityComparer<TKey> _comparer;

        public ProjectionEqualityComparer(Func<TSource, TKey> projection)
            : this(projection, null) { }

        public ProjectionEqualityComparer(Func<TSource, TKey> projection, IEqualityComparer<TKey> comparer)
        {
            if (projection == null)
                throw new ArgumentNullException(nameof(projection));

            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _projection = projection;
        }

        #region IEqualityComparer<TSource> Members

        public bool Equals(TSource x, TSource y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            return _comparer.Equals(_projection(x), _projection(y));
        }

        public int GetHashCode(TSource obj)
        {
            return obj != null ? _comparer.GetHashCode(_projection(obj)) : 0;
        }

        #endregion
    }

    public static class ProjectionEqualityComparer
    {
        public static ProjectionEqualityComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection);
        }

        public static ProjectionEqualityComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection,
            IEqualityComparer<TKey> comparer)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection, comparer);
        }
    }

    public sealed class DelegatedEqualityComparer<TSource> : IEqualityComparer<TSource>
    {
        private readonly Func<TSource, TSource, bool> _comparer;
        private readonly Func<TSource, int> _hasher;

        public DelegatedEqualityComparer(Func<TSource, TSource, bool> comparer, Func<TSource, int> hasher)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            if (hasher == null)
                throw new ArgumentNullException(nameof(hasher));

            _comparer = comparer;
            _hasher = hasher;
        }

        #region IEqualityComparer<TSource> Members

        public bool Equals(TSource x, TSource y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            return _comparer(x, y);
        }

        public int GetHashCode(TSource obj)
        {
            return obj != null ? _hasher(obj) : 0;
        }

        #endregion
    }

    public static class DelegatedEqualityComparer
    {
        public static DelegatedEqualityComparer<TSource> Create<TSource>(Func<TSource, TSource, bool> comparer, Func<TSource, int> hasher)
        {
            return new DelegatedEqualityComparer<TSource>(comparer, hasher);
        }
    }

    public sealed class DelegatedComparer<TSource> : IComparer<TSource>
    {
        private readonly Func<TSource, TSource, int> _comparer;

        public DelegatedComparer(Func<TSource, TSource, int> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _comparer = comparer;
        }

        #region IComparer<TSource> Members

        public int Compare(TSource x, TSource y)
        {
            return _comparer(x, y);
        }

        #endregion
    }

    public static class DelegatedComparer
    {
        public static DelegatedComparer<TSource> Create<TSource>(Func<TSource, TSource, int> comparer)
        {
            return new DelegatedComparer<TSource>(comparer);
        }
    }
}
