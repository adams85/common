using System.Threading.Tasks;
using Xunit;

namespace Karambolo.Common
{
    public class CachedTasksTest
    {
        [Fact]
        public void DefaultTest()
        {
            Assert.Same(CachedTasks.Default<object>.Task, CachedTasks.Default<object>.Task);

            Assert.Equal(TaskStatus.RanToCompletion, CachedTasks.Default<object>.Task.Status);
            Assert.Equal(default, CachedTasks.Default<object>.Task.Result);

            Assert.Equal(TaskStatus.RanToCompletion, CachedTasks.Default<int>.Task.Status);
            Assert.Equal(default, CachedTasks.Default<int>.Task.Result);
        }

        [Fact]
        public void FalseTest()
        {
            Assert.Same(CachedTasks.False.Task, CachedTasks.False.Task);

            Assert.Equal(TaskStatus.RanToCompletion, CachedTasks.False.Task.Status);
            Assert.False(CachedTasks.False.Task.Result);
        }

        [Fact]
        public void TrueTest()
        {
            Assert.Same(CachedTasks.True.Task, CachedTasks.True.Task);

            Assert.Equal(TaskStatus.RanToCompletion, CachedTasks.True.Task.Status);
            Assert.True(CachedTasks.True.Task.Result);
        }
    }
}
