using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Karambolo.Common
{
    public class EnumerableUtilsTest
    {
        [Fact]
        public void ReturnTest()
        {
            Assert.Equal(new[] { 1 }, EnumerableUtils.Return(1));
        }

        [Fact]
        public void RepeatTest()
        {
            Assert.Equal(new[] { 1, 1, 1, 1, 1 }, EnumerableUtils.Repeat(1).Take(5));
            Assert.Equal(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, EnumerableUtils.Repeat(1).Take(10));
        }

        [Fact]
        public void SkipLastTest()
        {
            Assert.Equal(Enumerable.Empty<int>(), Enumerable.Empty<int>().SkipLast());
            Assert.Equal(Enumerable.Empty<string>(), new[] { "a" }.SkipLast());
            Assert.Equal(new[] { "a", "b" }, new[] { "a", "b", "c" }.SkipLast());
        }

        [Fact]
        public void TakeLastTest()
        {
            Assert.Equal(Enumerable.Empty<int>(), Enumerable.Empty<int>().TakeLast());
            Assert.Equal(new[] { "a" }, new[] { "a" }.TakeLast());
            Assert.Equal(new[] { "c" }, new[] { "a", "b", "c" }.TakeLast());
        }

        [Fact]
        public void ScanTest()
        {
            Assert.Equal(new[] { 1, 3, 6, 10, 15 }, Enumerable.Range(1, 5).Scan((a, b) => a + b));
            Assert.Equal(new[] { 0, 1, 3, 6, 10, 15 }, Enumerable.Range(1, 5).Scan(0, (a, b) => a + b));
        }

        [Fact]
        public void PrependTest()
        {
            Assert.Equal(new[] { 0 }, Enumerable.Empty<int>().Prepend(0));
            Assert.Equal(new[] { 0, 1, 2, 3 }, new[] { 1, 2, 3 }.Prepend(0));
        }

        [Fact]
        public void AppendTest()
        {
            Assert.Equal(new[] { 0 }, Enumerable.Empty<int>().Append(0));
            Assert.Equal(new[] { 1, 2, 3, 0 }, new[] { 1, 2, 3 }.Append(0));
        }

        [Fact]
        public void SequenceEqualUnorderedTest()
        {
            Assert.True(Enumerable.Empty<object>().SequenceEqualUnordered(Enumerable.Empty<object>()));
            Assert.False(Enumerable.Empty<object>().SequenceEqualUnordered(new object[] { "a" }));
            Assert.False(new object[] { "a" }.SequenceEqualUnordered(Enumerable.Empty<object>()));

            Assert.False(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "a", "b" }));
            Assert.False(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "a", "b", "c", "d" }));
            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "a", "b", "c" }));
            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "a", "c", "b" }));
            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "b", "a", "c" }));
            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "b", "c", "a" }));
            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "c", "a", "b" }));
            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "c", "b", "a" }));

            Assert.True(new[] { "a", "a", "b", "b", "c" }.SequenceEqualUnordered(new[] { "a", "b", "c", "b", "a" }));

            Assert.True(new[] { "a", "b", "c" }.SequenceEqualUnordered(new[] { "C", "b", "a" }, StringComparer.OrdinalIgnoreCase));

            Assert.False(new[] { null, "b", "c" }.SequenceEqualUnordered(new[] { null, "b" }));
            Assert.False(new[] { null, "b", "c" }.SequenceEqualUnordered(new[] { null, "b", "c", "d" }));
            Assert.True(new[] { null, "b", "c" }.SequenceEqualUnordered(new[] { "c", "b", null }));

            Assert.True(new[] { null, null, "b", "b", "c" }.SequenceEqualUnordered(new[] { null, "b", "c", "b", null }));

            IEqualityComparer<string> comparer = DelegatedEqualityComparer.Create<string>(
                comparer: (x, y) => StringComparer.OrdinalIgnoreCase.Equals(x, y),
                hasher: (obj) => obj != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj) : 0);

            Assert.True(new[] { null, "b", "c" }.SequenceEqualUnordered(new[] { "C", "b", null }, comparer));
        }
    }
}
