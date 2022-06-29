using System;
using System.Collections.Generic;

namespace Karambolo.Common.Internal
{
    internal readonly struct ValueWrapper<TSource> : IEquatable<ValueWrapper<TSource>>
    {
#if !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public ValueWrapper(TSource item)
        {
            Value = item;
        }

        public readonly TSource Value;

        public override bool Equals(object obj)
        {
            return obj is ValueWrapper<TSource> wrapper && Equals(wrapper);
        }

        public bool Equals(ValueWrapper<TSource> other)
        {
            return EqualityComparer<TSource>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TSource>.Default.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
