using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace Karambolo.Common.Test
{
    [TestClass()]
    public class LambdaTest
    {
        [TestMethod]
        public void GetMemberPath()
        {
            Assert.AreEqual("Now.Day", string.Join(".", Lambda.GetMemberPath(() => DateTime.Now.Day).Select(m => m.Name)));
            try 
            {
                Lambda.GetMemberPath(() => DateTime.MaxValue);
                Assert.Fail();
            }
            catch (ArgumentException) { }
            Assert.AreEqual("MaxValue", string.Join(".", Lambda.GetMemberPath(() => DateTime.MaxValue, MemberTypes.Field).Select(m => m.Name)));

            Assert.AreEqual("Date.Day", string.Join(".", Lambda.GetMemberPath((DateTime dt) => dt.Date.Day).Select(m => m.Name)));
        }
    }
}
