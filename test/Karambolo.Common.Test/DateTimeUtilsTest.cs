using Xunit;
using System;

namespace Karambolo.Common
{
    public class DateTimeUtilsTest
    {
        [Fact]
        public void ToTimeReferenceTest()
        {
            Assert.Equal("now", TimeSpan.Zero.ToTimeReference());
            Assert.Equal("in 1 year", TimeSpan.FromDays(365).ToTimeReference());
            Assert.Equal("1 week ago", TimeSpan.FromDays(-7).ToTimeReference());
            Assert.Equal("2 days ago", TimeSpan.FromDays(-2).ToTimeReference());
            Assert.Equal("in 2 months", TimeSpan.FromDays(67).ToTimeReference());
            Assert.Equal("in 2 months 1 week", TimeSpan.FromDays(68).ToTimeReference());
            Assert.Equal("2 months 1 week ago", TimeSpan.FromDays(-70).ToTimeReference());
            Assert.Equal("2 months ago", TimeSpan.FromDays(-70).ToTimeReference(precision: 1));
            Assert.Equal("2 months 1 week 2 days ago", TimeSpan.FromDays(-70).ToTimeReference(precision: 3));
        }
    }
}
