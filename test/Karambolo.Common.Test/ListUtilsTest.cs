using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace Karambolo.Common
{
    public class ListUtilsTest
    {
        class IntCollection : Collection<int>
        {
            public IntCollection() { }

            public IntCollection(IEnumerable<int> items) : base(items.ToList()) { }
        }

        static readonly Comparison<int> intComparison = (x, y) => x < y ? -1 : x > y ? 1 : 0;

        static readonly IComparer<int> intComparer = DelegatedComparer.Create(new Func<int, int, int>(intComparison));

        [Fact]
        public void BinarySearchTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(-1, ListUtils.BinarySearch(collection, 0));
            Assert.Equal(-1, ReadOnlyListUtils.BinarySearch(collection, 0));

            Assert.Equal(1, ListUtils.BinarySearch(collection, 8));
            Assert.Equal(1, ReadOnlyListUtils.BinarySearch(collection, 8));

            Assert.Equal(1, ListUtils.BinarySearch(collection, 8, intComparer));
            Assert.Equal(1, ReadOnlyListUtils.BinarySearch(collection, 8, intComparer));

            Assert.Equal(-2, ListUtils.BinarySearch(collection, 1, 2, 5));
            Assert.Equal(-2, ReadOnlyListUtils.BinarySearch(collection, 1, 2, 5));

            Assert.Equal(-4, ListUtils.BinarySearch(collection, 1, 2, 10));
            Assert.Equal(-4, ReadOnlyListUtils.BinarySearch(collection, 1, 2, 10));

            Assert.Equal(2, ListUtils.BinarySearch(collection, 1, 2, 9));
            Assert.Equal(2, ReadOnlyListUtils.BinarySearch(collection, 1, 2, 9));

            Assert.Equal(2, ListUtils.BinarySearch(collection, 1, 2, 9, intComparer));
            Assert.Equal(2, ReadOnlyListUtils.BinarySearch(collection, 1, 2, 9, intComparer));

            Assert.Equal(-1, ListUtils.BinarySearch(collection, 0, 0, 5));
            Assert.Equal(-1, ReadOnlyListUtils.BinarySearch(collection, 0, 0, 5));

            Assert.Equal(~collection.Count, ListUtils.BinarySearch(collection, collection.Count, 0, 12));
            Assert.Equal(~collection.Count, ReadOnlyListUtils.BinarySearch(collection, collection.Count, 0, 12));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.BinarySearch(collection, -1, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.BinarySearch(collection, -1, 1, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.BinarySearch(collection, 0, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.BinarySearch(collection, 0, -1, 0));

            Assert.Throws<ArgumentException>(() => ListUtils.BinarySearch(collection, collection.Count, 1, 0));
            Assert.Throws<ArgumentException>(() => ReadOnlyListUtils.BinarySearch(collection, collection.Count, 1, 0));
        }

        [Fact]
        public void ConvertAllTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(new[] { false, true, false, true, true }, ListUtils.ConvertAll(collection, item => item % 2 == 0));
            Assert.Equal(new[] { false, true, false, true, true }, ReadOnlyListUtils.ConvertAll(collection, item => item % 2 == 0));
        }

        [Fact]
        public void ExistsTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.True(ListUtils.Exists(collection, item => item > 10));
            Assert.True(ReadOnlyListUtils.Exists(collection, item => item > 10));

            Assert.False(ListUtils.Exists(collection, item => item > 12));
            Assert.False(ReadOnlyListUtils.Exists(collection, item => item > 12));
        }

        [Fact]
        public void FindTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(8, ListUtils.Find(collection, item => item % 2 == 0));
            Assert.Equal(8, ReadOnlyListUtils.Find(collection, item => item % 2 == 0));

            Assert.Equal(default, ListUtils.Find(collection, item => item > 12));
            Assert.Equal(default, ReadOnlyListUtils.Find(collection, item => item > 12));
        }

        [Fact]
        public void FindAllTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(new[] { 8, 10, 12 }, ListUtils.FindAll(collection, item => item % 2 == 0));
            Assert.Equal(new[] { 8, 10, 12 }, ReadOnlyListUtils.FindAll(collection, item => item % 2 == 0));

            Assert.Equal(new int[] { }, ListUtils.FindAll(collection, item => item > 12));
            Assert.Equal(new int[] { }, ReadOnlyListUtils.FindAll(collection, item => item > 12));
        }

        [Fact]
        public void FindIndexTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(1, ListUtils.FindIndex(collection, item => item % 2 == 0));
            Assert.Equal(1, ReadOnlyListUtils.FindIndex(collection, item => item % 2 == 0));

            Assert.Equal(-1, ListUtils.FindIndex(collection, item => item > 12));
            Assert.Equal(-1, ReadOnlyListUtils.FindIndex(collection, item => item > 12));

            Assert.Equal(3, ListUtils.FindIndex(collection, 2, item => item % 2 == 0));
            Assert.Equal(3, ReadOnlyListUtils.FindIndex(collection, 2, item => item % 2 == 0));

            Assert.Equal(-1, ListUtils.FindIndex(collection, 2, 1, item => item % 2 == 0));
            Assert.Equal(-1, ReadOnlyListUtils.FindIndex(collection, 2, 1, item => item % 2 == 0));

            Assert.Equal(3, ListUtils.FindIndex(collection, 2, 2, item => item % 2 == 0));
            Assert.Equal(3, ReadOnlyListUtils.FindIndex(collection, 2, 2, item => item % 2 == 0));

            Assert.Equal(-1, ListUtils.FindIndex(collection, 0, 0, True<int>.Predicate));
            Assert.Equal(-1, ReadOnlyListUtils.FindIndex(collection, 0, 0, True<int>.Predicate));

            Assert.Equal(-1, ListUtils.FindIndex(collection, collection.Count, 0, True<int>.Predicate));
            Assert.Equal(-1, ReadOnlyListUtils.FindIndex(collection, collection.Count, 0, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindIndex(collection, -1, 1, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindIndex(collection, -1, 1, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindIndex(collection, 0, -1, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindIndex(collection, 0, -1, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindIndex(collection, collection.Count, 1, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindIndex(collection, collection.Count, 1, True<int>.Predicate));
        }

        [Fact]
        public void FindLastTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(12, ListUtils.FindLast(collection, item => item % 2 == 0));
            Assert.Equal(12, ReadOnlyListUtils.FindLast(collection, item => item % 2 == 0));

            Assert.Equal(default, ListUtils.FindLast(collection, item => item > 12));
            Assert.Equal(default, ReadOnlyListUtils.FindLast(collection, item => item > 12));
        }

        [Fact]
        public void FindLastIndexTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(4, ListUtils.FindLastIndex(collection, item => item % 2 == 0));
            Assert.Equal(4, ReadOnlyListUtils.FindLastIndex(collection, item => item % 2 == 0));

            Assert.Equal(-1, ListUtils.FindLastIndex(collection, item => item > 12));
            Assert.Equal(-1, ReadOnlyListUtils.FindLastIndex(collection, item => item > 12));

            Assert.Equal(1, ListUtils.FindLastIndex(collection, 2, item => item % 2 == 0));
            Assert.Equal(1, ReadOnlyListUtils.FindLastIndex(collection, 2, item => item % 2 == 0));

            Assert.Equal(1, ListUtils.FindLastIndex(collection, 2, 2, item => item % 2 == 0));
            Assert.Equal(1, ReadOnlyListUtils.FindLastIndex(collection, 2, 2, item => item % 2 == 0));

            Assert.Equal(-1, ListUtils.FindLastIndex(collection, 2, 1, item => item % 2 == 0));
            Assert.Equal(-1, ReadOnlyListUtils.FindLastIndex(collection, 2, 1, item => item % 2 == 0));

            Assert.Equal(-1, ListUtils.FindLastIndex(collection, collection.Count - 1, 0, True<int>.Predicate));
            Assert.Equal(-1, ReadOnlyListUtils.FindLastIndex(collection, collection.Count - 1, 0, True<int>.Predicate));

            Assert.Equal(-1, ListUtils.FindLastIndex(collection, -1, 0, True<int>.Predicate));
            Assert.Equal(-1, ReadOnlyListUtils.FindLastIndex(collection, -1, 0, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindLastIndex(collection, -1, 1, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindLastIndex(collection, -1, 1, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindLastIndex(collection, 0, -1, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindLastIndex(collection, 0, -1, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindLastIndex(collection, collection.Count, 0, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindLastIndex(collection, collection.Count, 0, True<int>.Predicate));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.FindLastIndex(new int[0], 1, 0, True<int>.Predicate));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.FindLastIndex(new int[0], 1, 0, True<int>.Predicate));
        }

        [Fact]
        public void ForEach()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            int n = 0;
            ListUtils.ForEach(collection, item => n += item);
            Assert.Equal(44, n);

            n = 0;
            ReadOnlyListUtils.ForEach(collection, item => n += item);
            Assert.Equal(44, n);
        }

        [Fact]
        public void GetRangeTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(new[] { 8, 9, 10 }, ListUtils.GetRange(collection, 1, 3));
            Assert.Equal(new[] { 8, 9, 10 }, ReadOnlyListUtils.GetRange(collection, 1, 3));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.GetRange(collection, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.GetRange(collection, -1, 1));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.GetRange(collection, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.GetRange(collection, 0, -1));

            Assert.Throws<ArgumentException>(() => ListUtils.GetRange(collection, collection.Count, 1));
            Assert.Throws<ArgumentException>(() => ReadOnlyListUtils.GetRange(collection, collection.Count, 1));
        }

        [Fact]
        public void IndexOfTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(1, ReadOnlyListUtils.IndexOf(collection, 8));

            Assert.Equal(-1, ReadOnlyListUtils.IndexOf(collection, 13));

            Assert.Equal(3, ReadOnlyListUtils.IndexOf(collection, 10, 2));

            Assert.Equal(-1, ReadOnlyListUtils.IndexOf(collection, 10, 2, 1));

            Assert.Equal(3, ReadOnlyListUtils.IndexOf(collection, 10, 2, 2));

            Assert.Equal(-1, ReadOnlyListUtils.IndexOf(collection, 5, 0, 0));

            Assert.Equal(-1, ReadOnlyListUtils.IndexOf(collection, 12, collection.Count, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.IndexOf(collection, 5, -1, 1));

            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.IndexOf(collection, 5, 0, -1));

            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.IndexOf(collection, 5, collection.Count, 1));
        }

        [Fact]
        public void LastIndexOfTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.Equal(4, ListUtils.LastIndexOf(collection, 12));
            Assert.Equal(4, ReadOnlyListUtils.LastIndexOf(collection, 12));

            Assert.Equal(-1, ListUtils.LastIndexOf(collection, 13));
            Assert.Equal(-1, ReadOnlyListUtils.LastIndexOf(collection, 13));

            Assert.Equal(1, ListUtils.LastIndexOf(collection, 8, 2));
            Assert.Equal(1, ReadOnlyListUtils.LastIndexOf(collection, 8, 2));

            Assert.Equal(1, ListUtils.LastIndexOf(collection, 8, 2, 2));
            Assert.Equal(1, ReadOnlyListUtils.LastIndexOf(collection, 8, 2, 2));

            Assert.Equal(-1, ListUtils.LastIndexOf(collection, 8, 2, 1));
            Assert.Equal(-1, ReadOnlyListUtils.LastIndexOf(collection, 8, 2, 1));

            Assert.Equal(-1, ListUtils.LastIndexOf(collection, 12, collection.Count - 1, 0));
            Assert.Equal(-1, ReadOnlyListUtils.LastIndexOf(collection, 12, collection.Count - 1, 0));

            Assert.Equal(-1, ListUtils.LastIndexOf(collection, 5, -1, 0));
            Assert.Equal(-1, ReadOnlyListUtils.LastIndexOf(collection, 5, -1, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.LastIndexOf(collection, 5, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.LastIndexOf(collection, 5, -1, 1));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.LastIndexOf(collection, 5, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.LastIndexOf(collection, 5, 0, -1));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.LastIndexOf(collection, 5, collection.Count, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.LastIndexOf(collection, 5, collection.Count, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => ListUtils.LastIndexOf(new int[0], default, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ReadOnlyListUtils.LastIndexOf(new int[0], default, 1, 0));
        }

        [Fact]
        public void TrueForAllTest()
        {
            var collection = new IntCollection { 5, 8, 9, 10, 12 };

            Assert.True(ListUtils.TrueForAll(collection, item => item <= 12));
            Assert.True(ReadOnlyListUtils.TrueForAll(collection, item => item <= 12));

            Assert.False(ListUtils.TrueForAll(collection, item => item > 12));
            Assert.False(ReadOnlyListUtils.TrueForAll(collection, item => item > 12));
        }
    }
}
