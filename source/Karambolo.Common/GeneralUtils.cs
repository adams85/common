using System.Collections.Generic;

namespace Karambolo.Common
{
    public static class GeneralUtils
    {
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void Swap<T>(ref T value1, ref T value2)
        {
            T temp = value1;
            value1 = value2;
            value2 = temp;
        }

#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            // checking list argument for null is omitted intentionally because of performance considerations:
            // this method can often be used in tight loops

            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
    }
}
