using System;
using System.Linq;
using Xunit;

namespace Karambolo.Common
{
    public class StringUtilsTest
    {
        [Fact]
        public void ToHexStringTest()
        {
            byte[] bytes = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
            Assert.Equal("0123456789abcdef", StringUtils.ToHexString(bytes));
            Assert.Equal("0123456789ABCDEF", StringUtils.ToHexString(bytes, upperCase: true));
        }

        [Fact]
        public void FromHexStringTest()
        {
            byte[] bytes = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
            Assert.Equal(bytes, StringUtils.FromHexString("0123456789abcdef"));
            Assert.Equal(bytes, StringUtils.FromHexString("0123456789ABCDEF"));
            Assert.Equal(bytes, StringUtils.FromHexString("0123456789aBCDeF"));

            Assert.Throws<FormatException>(() => StringUtils.FromHexString("0123456789abcde"));
            Assert.Throws<FormatException>(() => StringUtils.FromHexString("0123456789abcdeg"));
            Assert.Throws<FormatException>(() => StringUtils.FromHexString("0123456789abcd-f"));
        }

        [Fact]
        public void FindIndexTest()
        {
            var text = "From the stars we came, to the stars we rise.";

            Assert.Equal(4, text.FindIndex(char.IsWhiteSpace));

            Assert.Equal(4, text.FindIndex(4, char.IsWhiteSpace));
            Assert.Equal(8, text.FindIndex(5, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindIndex(text.Length - 2, char.IsWhiteSpace));

            Assert.Equal(4, text.FindIndex(3, 2, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindIndex(3, 1, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindIndex(4, 0, char.IsWhiteSpace));

            Assert.Equal(0, text.FindIndex(True<char>.Predicate));
            Assert.Equal(text.Length - 1, text.FindIndex(text.Length - 1, True<char>.Predicate));
            Assert.Equal(-1, text.FindIndex(0, 0, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindIndex(text.Length, 0, char.IsWhiteSpace));

            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindIndex(-1, char.IsWhiteSpace));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindIndex(text.Length + 1, char.IsWhiteSpace));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindIndex(text.Length, -1, char.IsWhiteSpace));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindIndex(text.Length, 1, char.IsWhiteSpace));
        }

        [Fact]
        public void FindLastIndexTest()
        {
            var text = "From the stars we came, to the stars we rise.";

            Assert.Equal(39, text.FindLastIndex(char.IsWhiteSpace));

            Assert.Equal(39, text.FindLastIndex(39, char.IsWhiteSpace));
            Assert.Equal(36, text.FindLastIndex(38, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindLastIndex(2, char.IsWhiteSpace));

            Assert.Equal(39, text.FindLastIndex(40, 2, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindLastIndex(40, 1, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindLastIndex(39, 0, char.IsWhiteSpace));

            Assert.Equal(text.Length - 1, text.FindLastIndex(True<char>.Predicate));
            Assert.Equal(0, text.FindLastIndex(0, True<char>.Predicate));
            Assert.Equal(-1, text.FindLastIndex(text.Length - 1, 0, char.IsWhiteSpace));
            Assert.Equal(-1, text.FindLastIndex(-1, 0, char.IsWhiteSpace));

            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindLastIndex(-2, char.IsWhiteSpace));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindLastIndex(text.Length, char.IsWhiteSpace));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindLastIndex(-1, -1, char.IsWhiteSpace));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.FindLastIndex(-1, 1, char.IsWhiteSpace));
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

        [Fact]
        public void TruncateTest()
        {
            var text = "From the stars we came, to the stars we rise.";
            Assert.Equal("", text.Truncate(0));
            Assert.Equal("From", text.Truncate(4));
        }

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

            text = "This text is not escaped.";
            Assert.Same(text, text.Escape('\\', '"'));
            Assert.Same(text, text.Unescape('\\', '"'));
        }

        [Fact]
        public void IndexOfEscapedTest()
        {
            var text = "This is a quotation: \\\"blablabla\"";
            Assert.Equal(text.Length - 1, text.IndexOfEscaped('\\', '"'));

            text = "This is a quotation: \\\"blablabla\\\"";
            Assert.Equal(-1, text.IndexOfEscaped('\\', '"'));

            Assert.Equal(text.Length - 1, text.IndexOfEscaped('\\', '"', text.Length - 1));
            Assert.Equal(-1, text.IndexOfEscaped('\\', '"', text.Length - 2));

            Assert.Equal(-1, text.IndexOfEscaped('\\', '"', text.Length));

            text = "This is a quotation: \"\"blablabla\"";
            Assert.Equal(text.Length - 1, text.IndexOfEscaped('"', '"'));
        }

        [Fact]
        public void LastIndexOfEscapedTest()
        {
            var text = "\"blablabla\\\" is a quotation.";
            Assert.Equal(0, text.LastIndexOfEscaped('\\', '"'));

            text = "\\\"blablabla\\\" is a quotation.";
            Assert.Equal(-1, text.LastIndexOfEscaped('\\', '"'));

            Assert.Equal(12, text.LastIndexOfEscaped('\\', '"', text.Length - 1, 17));
            Assert.Equal(-1, text.LastIndexOfEscaped('\\', '"', text.Length - 1, 18));

            Assert.Equal(-1, text.LastIndexOfEscaped('\\', '"', text.Length - 1));
        }


        [Fact]
        public void SplitEscapedTest()
        {
            Assert.Equal(new[] { "" }, string.Empty.SplitEscaped(';', ';', StringSplitOptions.None));
            Assert.Equal(new string[] { }, string.Empty.SplitEscaped(';', ';', StringSplitOptions.RemoveEmptyEntries));

            var text = "item1;item;;2;;;item3;item4;;;";
            Assert.Equal(new[] { "item1", "item;2;", "item3", "item4;", "" }, text.SplitEscaped(';', ';', StringSplitOptions.None));
            Assert.Equal(new[] { "item1", "item;2;", "item3", "item4;" }, text.SplitEscaped(';', ';', StringSplitOptions.RemoveEmptyEntries));

            text = "item1;item!;2;;!;item3;item4!;;";
            Assert.Equal(new[] { "item1", "item;2", "", ";item3", "item4;", "" }, text.SplitEscaped('!', ';', StringSplitOptions.None));
            Assert.Equal(new[] { "item1", "item;2", ";item3", "item4;" }, text.SplitEscaped('!', ';', StringSplitOptions.RemoveEmptyEntries));
        }

        [Fact]
        public void JoinEscapedTest()
        {
            Assert.Equal(new[] { "" }, string.Empty.SplitEscaped(';', ';', StringSplitOptions.None));
            Assert.Equal(new string[] { }, string.Empty.SplitEscaped(';', ';', StringSplitOptions.RemoveEmptyEntries));

            var parts = new[] { "item1", "item;2", "", ";item3", "item4;", "" };
            Assert.Equal("item1;item!;2;;!;item3;item4!;;", StringUtils.JoinEscaped('!', ';', parts));

            parts = new[] { "item1", "item;2", ";item3", "item4;"};
            Assert.Equal("item1;item!;2;!;item3;item4!;", StringUtils.JoinEscaped('!', ';', parts));
        }

#if !NETCOREAPP1_0
        [Fact]
        public void RemoveDiacriticsTest()
        {
            Assert.Equal("", "".RemoveDiacritics());
            Assert.Equal("arvizturo tukorfurogep", "árvíztűrő tükörfúrógép".RemoveDiacritics());
            Assert.Equal("ARVIZTURO TUKORFUROGEP", "ÁRVÍZTŰRŐ TÜKÖRFÚRÓGÉP".RemoveDiacritics());
        }
#endif
    }
}
