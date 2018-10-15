using System;
using System.Linq;
using Xunit;

namespace Karambolo.Common.Test
{
    public class StringUtilsTest
    {
        [Fact]
        public void EscapeTest()
        {
            var text = "This is a quotation: 'blablabla\"!";
            var result = text.Escape('!', '\'', '"');
            Assert.Equal("This is a quotation: !'blablabla!\"!!", result);
            result = result.Escape('!', '\'', '"');
            Assert.Equal("This is a quotation: !!!'blablabla!!!\"!!!!", result);
            Assert.Equal(text, result.Unescape('!', '\'', '"').Unescape('!', '\'', '"'));

            text = "This is a quotation: !blablabla!\"!!";
            Assert.Throws<FormatException>(() => text.Unescape('!', '\'', '"'));
        }

        [Fact]
        public void IndexOfEscapedTest()
        {
            var text = "This is a quotation: \\\"blablabla\"";
            var result = text.IndexOfEscaped('\\', '"');
            Assert.Equal(text.Length - 1, result);

            text = "This is a quotation: \\\"blablabla\\\"";
            result = text.IndexOfEscaped('\\', '"');
            Assert.Equal(-1, result);

            result = text.IndexOfEscaped('\\', '"', text.Length - 1);
            Assert.Equal(text.Length - 1, result);

            result = text.IndexOfEscaped('\\', '"', text.Length);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void SplitTest()
        {
            var text = "From the stars we came, to the stars we rise.";
            Assert.True(new[] { "From the stars we came", " to the stars we rise", "" }.SequenceEqual(text.Split(char.IsPunctuation)));

            text = "From the stars we came, to the stars we rise???";
            Assert.True(new[] { "From the stars we came", " to the stars we rise", "", "", "" }.SequenceEqual(text.Split(char.IsPunctuation)));

            text = "From the stars we came, to the stars we rise???";
            Assert.True(new[] { "From the stars we came", " to the stars we rise" }.SequenceEqual(text.Split(char.IsPunctuation, StringSplitOptions.RemoveEmptyEntries)));
        }
    }
}
