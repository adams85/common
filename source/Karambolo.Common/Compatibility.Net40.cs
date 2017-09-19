using System.Collections.Generic;
using System.Diagnostics;
using Karambolo.Common.Diagnostics;

namespace System.Collections.Generic
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

namespace System.Collections.ObjectModel
{
    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyDictionaryDebugView<,>))]
    public class ReadOnlyDictionary<TKey, TValue> : Karambolo.Common.Collections.ReadOnlyDictionary<TKey, TValue, IDictionary<TKey, TValue>>
    {
        public ReadOnlyDictionary(IDictionary<TKey, TValue> source) : base(source) { }
    }
}
