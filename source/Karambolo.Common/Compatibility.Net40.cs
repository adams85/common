using System.Collections;
using System.Collections.Generic;

namespace Karambolo.Common.Collections
{
    public interface IReadOnlyCollection<out T> : IEnumerable<T>, IEnumerable
    {
        int Count { get; }
    }

    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        T this[int index] { get; }
    }

    public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
        TValue this[TKey key] { get; }
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }
}
