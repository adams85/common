﻿using System;
using System.Linq;
using Xunit;

namespace Karambolo.Common.Test
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
        public void PrependTest()
        {
            Assert.Equal(new[] { 0 }, Enumerable.Empty<int>().Prepend(0));
            Assert.Equal(new[] { 0, 1, 2, 3 }, ArrayUtils.From(1, 2, 3).Prepend(0));
        }

        [Fact]
        public void AppendTest()
        {
            Assert.Equal(new[] { 0 }, Enumerable.Empty<int>().Append(0));
            Assert.Equal(new[] { 1, 2, 3, 0 }, ArrayUtils.From(1, 2, 3).Append(0));
        }

        [Fact]
        public void SequenceEqualUnorderedTest()
        {
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
    }
}