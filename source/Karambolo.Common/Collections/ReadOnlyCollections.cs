using Karambolo.Common.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Karambolo.Common.Collections
{
    #region Collections
    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionDebugView<>))]
    public class ReadOnlyCollection<T, TCollection> : ICollection<T>, IReadOnlyCollection<T>
        where TCollection : ICollection<T>
    {
        protected readonly TCollection _source;

        public ReadOnlyCollection(TCollection source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            _source = source;
        }

        public int Count => _source.Count;

        public bool IsReadOnly => true;

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return _source.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _source.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    #endregion

    #region Sets
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        bool Contains(T item);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
    }

    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionDebugView<>))]
    public class ReadOnlyEnabledHashSet<T> : HashSet<T>, IReadOnlySet<T>
    {
        public ReadOnlyEnabledHashSet() { }
        public ReadOnlyEnabledHashSet(IEqualityComparer<T> comparer) : base(comparer) { }
        public ReadOnlyEnabledHashSet(IEnumerable<T> collection) : base(collection) { }
        public ReadOnlyEnabledHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(collection, comparer) { }
        protected ReadOnlyEnabledHashSet(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionDebugView<>))]
    public class ReadOnlySet<T, TSet> : ReadOnlyCollection<T, TSet>, ISet<T>, IReadOnlySet<T>
        where TSet : ISet<T>
    {
        public ReadOnlySet(TSet source) : base(source) { }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _source.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _source.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _source.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _source.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _source.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _source.SetEquals(other);
        }

        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region Dictionaries
    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyDictionaryDebugView<,>))]
    public class ReadOnlyDictionary<TKey, TValue, TDictionary> : ReadOnlyCollection<KeyValuePair<TKey, TValue>, TDictionary>, 
        IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>
    {
        public ReadOnlyDictionary(TDictionary source) : base(source) { }

        public TValue this[TKey key] { get => _source[key]; set => throw new NotSupportedException(); }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = _source.Keys;
                return keys.IsReadOnly ? keys : new ReadOnlyCollection<TKey, ICollection<TKey>>(keys);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = _source.Values;
                return values.IsReadOnly ? values : new ReadOnlyCollection<TValue, ICollection<TValue>>(values);
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _source.TryGetValue(key, out value);
        }
    }

    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyDictionaryDebugView<,>))]
    public class ReadOnlyOrderedDictionary<TKey, TValue, TDictionary> : ReadOnlyDictionary<TKey, TValue, TDictionary>, IOrderedDictionary<TKey, TValue>
        where TDictionary : IOrderedDictionary<TKey, TValue>
    {
        public ReadOnlyOrderedDictionary(TDictionary source) : base(source) { }

        public TValue this[int index] { get => _source[index]; set => throw new NotSupportedException(); }

        public TKey GetKeyAt(int index)
        {
            return _source.GetKeyAt(index);
        }

        public int IndexOfKey(TKey key)
        {
            return _source.IndexOfKey(key);
        }

        public void Insert(int index, TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        int IOrderedDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region Keyed Collections
    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(KeyedCollectionDebugView<,>))]
    public class ReadOnlyKeyedCollection<TKey, TValue, TKeyedCollection> : ReadOnlyCollection<TValue, TKeyedCollection>, IKeyedCollection<TKey, TValue>
        where TKeyedCollection : IKeyedCollection<TKey, TValue>
    {
        public ReadOnlyKeyedCollection(TKeyedCollection source) : base(source) { }

        public TValue this[int index] { get => _source[index]; set => throw new NotSupportedException(); }

        public TValue this[TKey key] => _source[key];

        TValue IReadOnlyList<TValue>.this[int index] => this[index];

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = _source.Keys;
                return keys.IsReadOnly ? keys : new ReadOnlyCollection<TKey, ICollection<TKey>>(keys);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        public int IndexOf(TValue item)
        {
            return _source.IndexOf(item);
        }

        public void Insert(int index, TValue item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _source.TryGetValue(key, out value);
        }
    }

    [Serializable]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(KeyedCollectionDebugView<,>))]
    public class ReadOnlyObservableKeyedCollection<TKey, TValue, TKeyedCollection> : ReadOnlyKeyedCollection<TKey, TValue, TKeyedCollection>, 
        IObservableKeyedCollection<TKey, TValue>
        where TKeyedCollection : IObservableKeyedCollection<TKey, TValue>
    {
        public ReadOnlyObservableKeyedCollection(TKeyedCollection source) : base(source) { }

        public void AddRange(IEnumerable<TValue> items)
        {
            throw new NotSupportedException();
        }

        public void InsertRange(int index, IEnumerable<TValue> items)
        {
            throw new NotSupportedException();
        }

        public void Move(TValue item, int newIndex)
        {
            throw new NotSupportedException();
        }

        public void MoveAt(int currentIndex, int newIndex)
        {
            throw new NotSupportedException();
        }

        public void RemoveAll(Predicate<TValue> match)
        {
            throw new NotSupportedException();
        }

        public void RemoveRange(int index, int count)
        {
            throw new NotSupportedException();
        }

        public void RemoveRange(IEnumerable<TValue> items)
        {
            throw new NotSupportedException();
        }

        public void Replace(TValue currentItem, TValue newItem)
        {
            throw new NotSupportedException();
        }

        public void ReplaceAt(int index, TValue newItem)
        {
            throw new NotSupportedException();
        }

        public IDisposable ResetSection()
        {
            throw new NotSupportedException();
        }

        public void Subscribe(ICollectionChangePreviewer previewer)
        {
            _source.Subscribe(previewer);
        }

        public void Unsubscribe(ICollectionChangePreviewer previewer)
        {
            _source.Unsubscribe(previewer);
        }

        public void UnsubscribeOwned<TOwner>(TOwner owner) where TOwner : class
        {
            _source.UnsubscribeOwned(owner);
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add => _source.CollectionChanged += value;
            remove => _source.CollectionChanged -= value;
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => _source.PropertyChanged += value;
            remove => _source.PropertyChanged -= value;
        }
    }
    #endregion
}


