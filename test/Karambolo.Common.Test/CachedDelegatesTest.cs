using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Karambolo.Common
{
    public class CachedDelegatesTest
    {
        [Fact]
        public void NoopTest()
        {
            Assert.Same(Noop.Action, Noop.Action);
            Noop.Action();

            Assert.Same(Noop<int>.Action, Noop<int>.Action);
            Noop<int>.Action(1);
        }

        [Fact]
        public void IdentityTest()
        {
            Assert.Same(Identity<int>.Func, Identity<int>.Func);
            Assert.Equal(0, Identity<int>.Func(0));
            Assert.Equal(1, Identity<int>.Func(1));
        }

        [Fact]
        public void DefaultTest()
        {
            Assert.Same(DefaultMap<int>.Func, DefaultMap<int>.Func);
            Assert.Equal(0, DefaultMap<int>.Func(0));
            Assert.Equal(0, DefaultMap<int>.Func(1));
            Assert.Equal(null, DefaultMap<int?>.Func(1));
            Assert.Equal(null, DefaultMap<string>.Func("1"));

            Assert.Same(Default<int>.Func, Default<int>.Func);
            Assert.Equal(0, Default<int>.Func());
            Assert.Equal(null, Default<int?>.Func());
            Assert.Equal(null, Default<string>.Func());
        }

        [Fact]
        public void FalseTest()
        {
            Assert.Same(False.Func, False.Func);
            Assert.False(False.Func());

            Assert.Same(False<int>.Func, False<int>.Func);
            Assert.False(False<int>.Func(1));
            Assert.False(False<int?>.Func(1));
            Assert.False(False<string>.Func("1"));
        }

        [Fact]
        public void TrueTest()
        {
            Assert.Same(True.Func, True.Func);
            Assert.True(True.Func());

            Assert.Same(True<int>.Func, True<int>.Func);
            Assert.True(True<int>.Func(1));
            Assert.True(True<int?>.Func(1));
            Assert.True(True<string>.Func("1"));
        }
    }
}
