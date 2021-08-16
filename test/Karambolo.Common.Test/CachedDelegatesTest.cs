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
            Assert.Same(CachedDelegates.Noop.Action, CachedDelegates.Noop.Action);
            CachedDelegates.Noop.Action();

            Assert.Same(CachedDelegates.Noop<int>.Action, CachedDelegates.Noop<int>.Action);
            CachedDelegates.Noop<int>.Action(1);
        }

        [Fact]
        public void IdentityTest()
        {
            Assert.Same(CachedDelegates.Identity<int>.Func, CachedDelegates.Identity<int>.Func);
            Assert.Equal(0, CachedDelegates.Identity<int>.Func(0));
            Assert.Equal(1, CachedDelegates.Identity<int>.Func(1));
        }

        [Fact]
        public void DefaultTest()
        {
            Assert.Same(CachedDelegates.DefaultMap<int>.Func, CachedDelegates.DefaultMap<int>.Func);
            Assert.Equal(0, CachedDelegates.DefaultMap<int>.Func(0));
            Assert.Equal(0, CachedDelegates.DefaultMap<int>.Func(1));
            Assert.Equal(null, CachedDelegates.DefaultMap<int?>.Func(1));
            Assert.Equal(null, CachedDelegates.DefaultMap<string>.Func("1"));

            Assert.Same(CachedDelegates.Default<int>.Func, CachedDelegates.Default<int>.Func);
            Assert.Equal(0, CachedDelegates.Default<int>.Func());
            Assert.Equal(null, CachedDelegates.Default<int?>.Func());
            Assert.Equal(null, CachedDelegates.Default<string>.Func());
        }

        [Fact]
        public void FalseTest()
        {
            Assert.Same(CachedDelegates.False.Func, CachedDelegates.False.Func);
            Assert.False(CachedDelegates.False.Func());

            Assert.Same(CachedDelegates.False<int>.Func, CachedDelegates.False<int>.Func);
            Assert.False(CachedDelegates.False<int>.Func(1));
            Assert.False(CachedDelegates.False<int?>.Func(1));
            Assert.False(CachedDelegates.False<string>.Func("1"));

            Assert.Same(CachedDelegates.False<int>.Predicate, CachedDelegates.False<int>.Predicate);
            Assert.False(CachedDelegates.False<int>.Predicate(1));
            Assert.False(CachedDelegates.False<int?>.Predicate(1));
            Assert.False(CachedDelegates.False<string>.Predicate("1"));
        }

        [Fact]
        public void TrueTest()
        {
            Assert.Same(CachedDelegates.True.Func, CachedDelegates.True.Func);
            Assert.True(CachedDelegates.True.Func());

            Assert.Same(CachedDelegates.True<int>.Func, CachedDelegates.True<int>.Func);
            Assert.True(CachedDelegates.True<int>.Func(1));
            Assert.True(CachedDelegates.True<int?>.Func(1));
            Assert.True(CachedDelegates.True<string>.Func("1"));

            Assert.Same(CachedDelegates.True<int>.Predicate, CachedDelegates.True<int>.Predicate);
            Assert.True(CachedDelegates.True<int>.Predicate(1));
            Assert.True(CachedDelegates.True<int?>.Predicate(1));
            Assert.True(CachedDelegates.True<string>.Predicate("1"));
        }
    }
}
