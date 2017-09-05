using System;
using System.Collections.Generic;

namespace Karambolo.Common
{
    public sealed class ProjectionEqualityComparer<TSource, TKey>
        : IEqualityComparer<TSource>
    {
        private readonly Func<TSource, TKey> _projection;
        private readonly IEqualityComparer<TKey> _comparer;

        public ProjectionEqualityComparer(Func<TSource, TKey> projection)
            : this(projection, null)
        {
        }

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
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return _comparer.GetHashCode(_projection(obj));
        }

        #endregion
    }

    public static class ProjectionEqualityComparer<TSource>
    {
        public static ProjectionEqualityComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection);
        }

        public static ProjectionEqualityComparer<TSource, TKey> Create<TKey>(Func<TSource, TKey> projection,
            IEqualityComparer<TKey> comparer)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection, comparer);
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
