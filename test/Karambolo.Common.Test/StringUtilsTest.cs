using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Karambolo.Common.Test
{
    [TestClass()]
    public class StringUtilsTest
    {
        [TestMethod]
        public void EscapeTest1()
        {
            var text = "This is a quotation: 'blablabla\"!";
            var result = text.Escape('!', '\'', '"');
            Assert.AreEqual("This is a quotation: !'blablabla!\"!!", result);
            result = result.Escape('!', '\'', '"');
            Assert.AreEqual("This is a quotation: !!!'blablabla!!!\"!!!!", result);
            Assert.AreEqual(text, result.Unescape('!', '\'', '"').Unescape('!', '\'', '"'));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void EscapeTest2()
        {
            var text = "This is a quotation: !blablabla!\"!!";
            text.Unescape('!', '\'', '"');
        }

        [TestMethod]
        public void SplitTest()
        {
            var text = "From the stars we came, to the stars we rise.";
            Assert.IsTrue(new[] { "From the stars we came", " to the stars we rise", "" }.SequenceEqual(text.Split(char.IsPunctuation)));

            text = "From the stars we came, to the stars we rise???";
            Assert.IsTrue(new[] { "From the stars we came", " to the stars we rise", "", "", "" }.SequenceEqual(text.Split(char.IsPunctuation)));

            text = "From the stars we came, to the stars we rise???";
            Assert.IsTrue(new[] { "From the stars we came", " to the stars we rise" }.SequenceEqual(text.Split(char.IsPunctuation, StringSplitOptions.RemoveEmptyEntries)));
        }
    }
}
