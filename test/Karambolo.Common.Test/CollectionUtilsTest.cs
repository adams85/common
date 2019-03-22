using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Karambolo.Common
{
    public class CollectionUtilsTest
    {
        private class ReadOnlyCollection<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection<T> _collection;

            public ReadOnlyCollection(ICollection<T> collection) => _collection = collection;

            public int Count => _collection.Count;

            public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void IsNullOrEmptyTest()
        {
            var list = new List<int>();

            Assert.True(ReadOnlyCollectionUtils.IsNullOrEmpty((IReadOnlyCollection<int>)null));
            Assert.True(ReadOnlyCollectionUtils.IsNullOrEmpty(ArrayUtils.Empty<int>()));
            Assert.False(ReadOnlyCollectionUtils.IsNullOrEmpty(new[] { 1 }));

            Assert.True(CollectionUtils.IsNullOrEmpty((ICollection<int>)null));
            Assert.True(CollectionUtils.IsNullOrEmpty(ArrayUtils.Empty<int>()));
            Assert.False(CollectionUtils.IsNullOrEmpty(new[] { 1 }));
        }
    }
}
