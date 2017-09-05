using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Karambolo.Common.Test
{
    [TestClass()]
    public class DateTimeUtilsTest
    {
        [TestMethod]
        public void ToTimeReferenceTest()
        {
            Assert.AreEqual("now", TimeSpan.Zero.ToTimeReference());
            Assert.AreEqual("in 1 year", TimeSpan.FromDays(365).ToTimeReference());
            Assert.AreEqual("1 week ago", TimeSpan.FromDays(-7).ToTimeReference());
            Assert.AreEqual("2 days ago", TimeSpan.FromDays(-2).ToTimeReference());
            Assert.AreEqual("in 2 months", TimeSpan.FromDays(67).ToTimeReference());
            Assert.AreEqual("in 2 months 1 week", TimeSpan.FromDays(68).ToTimeReference());
            Assert.AreEqual("2 months 1 week ago", TimeSpan.FromDays(-70).ToTimeReference());
            Assert.AreEqual("2 months ago", TimeSpan.FromDays(-70).ToTimeReference(precision: 1));
            Assert.AreEqual("2 months 1 week 2 days ago", TimeSpan.FromDays(-70).ToTimeReference(precision: 3));
        }
    }
}
