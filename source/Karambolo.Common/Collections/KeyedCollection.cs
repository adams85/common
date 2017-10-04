using Karambolo.Common.Diagnostics;
using Karambolo.Common.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Karambolo.Common.Collections
{
    public interface IReadOnlyKeyedCollection<TKey, TValue> : IReadOnlyList<TValue>
    {
        TValue this[TKey key] { get; }
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        ICollection<TKey> Keys { get; }
    }

    public interface IKeyedCollection<TKey, TValue> : IReadOnlyKeyedCollection<TKey, TValue>, IList<TValue>
    {
        bool Remove(TKey key);
        new int Count { get; }
        new TValue this[int index] { get; set; }
    }

#if !NETSTANDARD1_0
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(KeyedCollectionDebugView<,>))]
    public class GenericKeyedCollection<TKey, TValue> : KeyedCollection<TKey, TValue>, IKeyedCollection<TKey, TValue>
    {
        class KeyCollection : ICollection<TKey>
        {
            readonly GenericKeyedCollection<TKey, TValue> _owner;

            public KeyCollection(GenericKeyedCollection<TKey, TValue> owner)
            {
                _owner = owner;
            }

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey item)
            {
                return _owner.Contains(item);
            }

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (!(array.Length - index >= Count))
                    throw new ArgumentException(Resources.ArrayNotLongEnough, nameof(array));
                if (!(0 <= index && index <= array.Length))
                    throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

                var count = _owner.Items.Count;
                for (var i = 0; i < count; i++)
                    array[index++] = _owner.GetKeyForItem(_owner.Items[i]);
            }

            public int Count => _owner.Count;

            public bool IsReadOnly => true;

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                var count = _owner.Items.Count;
                for (var i = 0; i < count; i++)
                    yield return _owner.GetKeyForItem(_owner.Items[i]);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        readonly Func<TValue, TKey> _keyFromItemSelector;
#if !NETSTANDARD1_0
        [System.NonSerialized]
#endif
        KeyCollection _keyCollection;

        public GenericKeyedCollection(Func<TValue, TKey> keyFromItemSelector, IEqualityComparer<TKey> comparer = null, int dictionaryCreationThreshold = 0)
            : base(comparer, dictionaryCreationThreshold)
        {
            if (keyFromItemSelector == null)
                throw new ArgumentNullException(nameof(keyFromItemSelector));

            _keyFromItemSelector = keyFromItemSelector;
        }

        public GenericKeyedCollection(IKeyedCollection<TKey, TValue> collection, Func<TValue, TKey> keyFromItemSelector, IEqualityComparer<TKey> comparer = null, int dictionaryCreationThreshold = 0)
            : this(keyFromItemSelector, comparer, dictionaryCreationThreshold)
        {
            var count = collection.Count;
            for (var i = 0; i < count; i++)
                Add(collection[i]);
        }

        protected override TKey GetKeyForItem(TValue item)
        {
            return _keyFromItemSelector(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (Dictionary != null)
            {
                if (Dictionary.TryGetValue(key, out value))
                    return true;
            }
            else
            {
                TValue item;
                var count = Count;
                for (var i = 0; i < count; i++)
                    if (Comparer.Equals(GetKeyForItem(item = Items[i]), key))
                    {
                        value = item;
                        return true;
                    }
            }

            value = default(TValue);
            return false;
        }

        public ICollection<TKey> Keys => _keyCollection ?? (_keyCollection = new KeyCollection(this));

        bool IReadOnlyKeyedCollection<TKey, TValue>.ContainsKey(TKey key)
        {
            return Contains(key);
        }

#if !NETSTANDARD1_0
        [System.Runtime.Serialization.OnSerializing]
        internal void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            // using expressions would be a much better solution but currently there is no simple way to serialize them
            // http://stackoverflow.com/questions/25721711/how-to-identify-a-lambda-closure-with-reflection
            if (_keyFromItemSelector.Target != null)
                throw new InvalidOperationException(Resources.DelegateSerializationNotSupported);
        }
#endif
    }
}
