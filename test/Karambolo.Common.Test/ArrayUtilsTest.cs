using System;
using Xunit;

namespace Karambolo.Common
{
    public class ArrayUtilsTest
    {
        [Fact]
        public void EmptyTest()
        {
            Assert.Equal(0, ArrayUtils.Empty<int>().Length);
            Assert.Same(ArrayUtils.Empty<int>(), ArrayUtils.Empty<int>());
        }

        [Fact]
        public void IsNullOrEmptyTest()
        {
            Assert.True(ArrayUtils.IsNullOrEmpty<int>(null));
            Assert.True(ArrayUtils.IsNullOrEmpty(ArrayUtils.Empty<int>()));
            Assert.False(ArrayUtils.IsNullOrEmpty(ArrayUtils.From(1)));

            Assert.True(ArrayUtils.IsNullOrEmpty(null));
            Assert.True(ArrayUtils.IsNullOrEmpty((Array)ArrayUtils.Empty<int>()));
            Assert.False(ArrayUtils.IsNullOrEmpty((Array)ArrayUtils.From(1)));
        }

        [Fact]
        public void FromTest()
        {
            Assert.Equal(new[] { 1 }, ArrayUtils.From(1));
            Assert.Equal(new[] { 1, 2, 3 }, ArrayUtils.From(1, 2, 3));
            Assert.Throws<ArgumentNullException>(() => ArrayUtils.From<int>(null));
        }

        [Fact]
        public void GetDimensionsTest()
        {
            Array array = new byte[1];
            Assert.Equal(new[] { 0 }, array.GetLowerBounds());
            Assert.Equal(new[] { 0 }, array.GetUpperBounds());
            Assert.Equal(new[] { 1 }, array.GetLengths());

            array = new byte[1, 2, 3];
            Assert.Equal(new[] { 0, 0, 0 }, array.GetLowerBounds());
            Assert.Equal(new[] { 0, 1, 2 }, array.GetUpperBounds());
            Assert.Equal(new[] { 1, 2, 3 }, array.GetLengths());

            array = Array.CreateInstance(typeof(int), new[] { 1, 2, 3 }, new[] { 1, 0, -1 });
            Assert.Equal(new[] { 1, 0, -1 }, array.GetLowerBounds());
            Assert.Equal(new[] { 1, 1, 1 }, array.GetUpperBounds());
            Assert.Equal(new[] { 1, 2, 3 }, array.GetLengths());
        }

        [Fact]
        public void FillTest()
        {
            var array = new int[10];
            array.Fill(1);
            Assert.Equal(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, array);

            array.Fill(Identity<int>.Func);
            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, array);
        }

        [Fact]
        public void ShuffleTest()
        {
            var array = new int[10];
            array.Fill(Identity<int>.Func);
            var shuffledArray = (int[])array.Clone();

            shuffledArray.Shuffle(new Random(0));
            Assert.Equal(array.Length, shuffledArray.Length);
            Assert.NotEqual(array, shuffledArray);

            var otherShuffledArray = (int[])array.Clone();
            otherShuffledArray.Shuffle(new Random(1));
            Assert.Equal(array.Length, otherShuffledArray.Length);
            Assert.NotEqual(array, otherShuffledArray);
            Assert.NotEqual(shuffledArray, otherShuffledArray);
        }


        [Fact]
        public void ContentEqualsTest()
        {
            Assert.True(ArrayUtils.ContentEquals<int>(null, null));
            Assert.False(ArrayUtils.ContentEquals(ArrayUtils.Empty<int>(), null));
            Assert.False(ArrayUtils.ContentEquals(null, ArrayUtils.Empty<int>()));

            var array = new int[10];
            array.Fill(Identity<int>.Func);
            Assert.False(ArrayUtils.ContentEquals(array, ArrayUtils.Empty<int>()));

            var otherArray = (int[])array.Clone();
            Assert.True(ArrayUtils.ContentEquals(array, otherArray));
            otherArray[otherArray.Length - 1] = 0;
            Assert.False(ArrayUtils.ContentEquals(array, otherArray));

            var stringArray = new[] { "a", "b", "c" };
            Assert.False(ArrayUtils.ContentEquals(new[] { "a", "B", "c" }, stringArray));
            Assert.True(ArrayUtils.ContentEquals(new[] { "a", "B", "c" }, stringArray, StringComparer.OrdinalIgnoreCase));
        }
    }
}
