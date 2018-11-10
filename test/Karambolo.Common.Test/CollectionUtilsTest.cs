using System;
using System.Collections.Generic;
using Xunit;

namespace Karambolo.Common
{
    public class CollectionUtilsTest
    {
        [Fact]
        public void IsNullOrEmptyTest()
        {
            var list = new List<int>();

            Assert.True(ReadOnlyCollectionUtils.IsNullOrEmpty((IReadOnlyCollection<int>)null));
            Assert.True(ReadOnlyCollectionUtils.IsNullOrEmpty(ArrayUtils.Empty<int>()));
            Assert.False(ReadOnlyCollectionUtils.IsNullOrEmpty(ArrayUtils.From(1)));

            Assert.True(CollectionUtils.IsNullOrEmpty((ICollection<int>)null));
            Assert.True(CollectionUtils.IsNullOrEmpty(ArrayUtils.Empty<int>()));
            Assert.False(CollectionUtils.IsNullOrEmpty(ArrayUtils.From(1)));
        }
    }
}
