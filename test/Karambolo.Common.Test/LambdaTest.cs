#if NETCOREAPP1_0
using CommonMemberTypes = Karambolo.Common.MemberTypes;
#else
using CommonMemberTypes = System.Reflection.MemberTypes;
#endif

using System;
using System.Linq;
using Xunit;
using System.Reflection;

namespace Karambolo.Common.Test
{
    public class LambdaTest
    {
        [Fact]
        public void GetMemberPath()
        {
            Assert.Equal("Now.Day", string.Join(".", Lambda.GetMemberPath(() => DateTime.Now.Day).Select(m => m.Name)));

            Assert.Throws<ArgumentException>(() => Lambda.GetMemberPath(() => DateTime.MaxValue));

            Assert.Equal("MaxValue", string.Join(".", Lambda.GetMemberPath(() => DateTime.MaxValue, CommonMemberTypes.Field).Select(m => m.Name)));

            Assert.Equal("Date.Day", string.Join(".", Lambda.GetMemberPath((DateTime dt) => dt.Date.Day).Select(m => m.Name)));
        }
    }
}
