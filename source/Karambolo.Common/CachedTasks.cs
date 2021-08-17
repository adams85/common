using System;
using System.Threading.Tasks;

namespace Karambolo.Common
{
    public static class CachedTasks
    {
        public static class Default<T>
        {
            public static readonly Task<T> Task = TaskUtils.FromResult<T>(default);
        }

        public static class False
        {
            public static readonly Task<bool> Task = TaskUtils.FromResult(false);
        }

        public static class True
        {
            public static readonly Task<bool> Task = TaskUtils.FromResult(true);
        }
    }
}
