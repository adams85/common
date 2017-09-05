using System;
using System.Diagnostics;

namespace Karambolo.Common.Diagnostics
{
    public static class Assert
    {
        [Conditional("DEBUG")]
        public static void Ensure(bool condition, string message = null)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
