using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Karambolo.Common.Properties;
using System.Diagnostics;
using Karambolo.Common.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Karambolo.Common.Collections
{
#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionConverterDebugView<,>))]
    public class ReadOnlyCollectionConverter<TSource, TDest, TCollection> : IReadOnlyCollection<TDest>
        where TCollection : IReadOnlyCollection<TSource>
    {
        protected readonly TCollection _source;
        protected readonly Func<TDest, TSource> _convertToSource;
        protected readonly Func<TSource, TDest> _convertToDest;

        public ReadOnlyCollectionConverter(TCollection source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (convertToSource == null)
                throw new ArgumentNullException(nameof(convertToSource));
            if (convertToDest == null)
                throw new ArgumentNullException(nameof(convertToDest));

            _source = source;
            _convertToSource = convertToSource;
            _convertToDest = convertToDest;
        }

        public int Count => _source.Count;

        public IEnumerator<TDest> GetEnumerator()
        {
            return _source.Select(_convertToDest).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TDest>)this).GetEnumerator();
        }

#if !NETSTANDARD1_2
        [System.Runtime.Serialization.OnSerializing]
        internal void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            // using expressions would be a much better solution but currently there is no simple way to serialize them
            // http://stackoverflow.com/questions/25721711/how-to-identify-a-lambda-closure-with-reflection
            if (_convertToSource.Target != null || _convertToDest.Target != null)
                throw new InvalidOperationException(Resources.DelegateSerializationNotSupported);
        }
#endif
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionConverterDebugView<,>))]
    public class CollectionConverter<TSource, TDest, TCollection> : ICollection<TDest>
        where TCollection : ICollection<TSource>
    {
        protected readonly TCollection _source;
        protected readonly Func<TDest, TSource> _convertToSource;
        protected readonly Func<TSource, TDest> _convertToDest;

        public CollectionConverter(TCollection source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (convertToSource == null)
                throw new ArgumentNullException(nameof(convertToSource));
            if (convertToDest == null)
                throw new ArgumentNullException(nameof(convertToDest));

            _source = source;
            _convertToSource = convertToSource;
            _convertToDest = convertToDest;
        }

        public void Add(TDest item)
        {
            _source.Add(_convertToSource(item));
        }

        public void Clear()
        {
            _source.Clear();
        }

        public bool Contains(TDest item)
        {
            return _source.Contains(_convertToSource(item));
        }

        public void CopyTo(TDest[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException(Resources.ArrayNotLongEnough, nameof(array));
            if (arrayIndex < 0 || array.Length < arrayIndex)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), Resources.IndexOutOfRange);

            var i = arrayIndex;
            foreach (var item in _source)
                array[i++] = _convertToDest(item);
        }

        public int Count => _source.Count;

        public bool IsReadOnly => _source.IsReadOnly;

        public bool Remove(TDest item)
        {
            return _source.Remove(_convertToSource(item));
        }

        public IEnumerator<TDest> GetEnumerator()
        {
            return _source.Select(_convertToDest).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TDest>)this).GetEnumerator();
        }

#if !NETSTANDARD1_2
        [System.Runtime.Serialization.OnSerializing]
        internal void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            // using expressions would be a much better solution but currently there is no simple way to serialize them
            // http://stackoverflow.com/questions/25721711/how-to-identify-a-lambda-closure-with-reflection
            if (_convertToSource.Target != null || _convertToDest.Target != null)
                throw new InvalidOperationException(Resources.DelegateSerializationNotSupported);
        }
#endif
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionConverterDebugView<,>))]
    public class ReadOnlySetConverter<TSource, TDest, TList> : ReadOnlyCollectionConverter<TSource, TDest, TList>, IReadOnlySet<TDest>
        where TList : IReadOnlySet<TSource>
    {
        public ReadOnlySetConverter(TList source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public bool Contains(TDest item)
        {
            return _source.Contains(_convertToSource(item));
        }

        public bool IsProperSubsetOf(IEnumerable<TDest> other)
        {
            return _source.IsProperSubsetOf(other.Select(_convertToSource));
        }

        public bool IsProperSupersetOf(IEnumerable<TDest> other)
        {
            return _source.IsProperSupersetOf(other.Select(_convertToSource));
        }

        public bool IsSubsetOf(IEnumerable<TDest> other)
        {
            return _source.IsSubsetOf(other.Select(_convertToSource));
        }

        public bool IsSupersetOf(IEnumerable<TDest> other)
        {
            return _source.IsSupersetOf(other.Select(_convertToSource));
        }

        public bool Overlaps(IEnumerable<TDest> other)
        {
            return _source.Overlaps(other.Select(_convertToSource));
        }

        public bool SetEquals(IEnumerable<TDest> other)
        {
            return _source.SetEquals(other.Select(_convertToSource));
        }
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionConverterDebugView<,>))]
    public class SetConverter<TSource, TDest, TList> : CollectionConverter<TSource, TDest, TList>, ISet<TDest>
        where TList : ISet<TSource>
    {
        public SetConverter(TList source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public new bool Add(TDest item)
        {
            return _source.Add(_convertToSource(item));
        }

        public void ExceptWith(IEnumerable<TDest> other)
        {
            _source.ExceptWith(other.Select(_convertToSource));
        }

        public void IntersectWith(IEnumerable<TDest> other)
        {
            _source.IntersectWith(other.Select(_convertToSource));
        }

        public bool IsProperSubsetOf(IEnumerable<TDest> other)
        {
            return _source.IsProperSubsetOf(other.Select(_convertToSource));
        }

        public bool IsProperSupersetOf(IEnumerable<TDest> other)
        {
            return _source.IsProperSupersetOf(other.Select(_convertToSource));
        }

        public bool IsSubsetOf(IEnumerable<TDest> other)
        {
            return _source.IsSubsetOf(other.Select(_convertToSource));
        }

        public bool IsSupersetOf(IEnumerable<TDest> other)
        {
            return _source.IsSupersetOf(other.Select(_convertToSource));
        }

        public bool Overlaps(IEnumerable<TDest> other)
        {
            return _source.Overlaps(other.Select(_convertToSource));
        }

        public bool SetEquals(IEnumerable<TDest> other)
        {
            return _source.SetEquals(other.Select(_convertToSource));
        }

        public void SymmetricExceptWith(IEnumerable<TDest> other)
        {
            _source.SymmetricExceptWith(other.Select(_convertToSource));
        }

        public void UnionWith(IEnumerable<TDest> other)
        {
            _source.UnionWith(other.Select(_convertToSource));
        }
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(ReadOnlyCollectionConverterDebugView<,>))]
    public class ReadOnlyListConverter<TSource, TDest, TList> : ReadOnlyCollectionConverter<TSource, TDest, TList>, IReadOnlyList<TDest>
        where TList : IReadOnlyList<TSource>
    {
        public ReadOnlyListConverter(TList source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public TDest this[int index] => _convertToDest(_source[index]);
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionConverterDebugView<,>))]
    public class ListConverter<TSource, TDest, TList> : CollectionConverter<TSource, TDest, TList>, IList<TDest>
        where TList : IList<TSource>
    {
        public ListConverter(TList source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public int IndexOf(TDest item)
        {
            return _source.IndexOf(_convertToSource(item));
        }

        public void Insert(int index, TDest item)
        {
            _source.Insert(index, _convertToSource(item));
        }

        public void RemoveAt(int index)
        {
            _source.RemoveAt(index);
        }

        public TDest this[int index]
        {
            get => _convertToDest(_source[index]);
            set => _source[index] = _convertToSource(value);
        }
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DictionaryConverterDebugView<,,>))]
    public class ReadOnlyDictionaryConverter<TKey, TSource, TDest, TDictionary> :
        ReadOnlyCollectionConverter<KeyValuePair<TKey, TSource>, KeyValuePair<TKey, TDest>, TDictionary>,
        IReadOnlyDictionary<TKey, TDest>
        where TDictionary : IReadOnlyDictionary<TKey, TSource>
    {
        protected new readonly Func<TDest, TSource> _convertToSource;
        protected new readonly Func<TSource, TDest> _convertToDest;

        public ReadOnlyDictionaryConverter(TDictionary source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source,
                kvp => new KeyValuePair<TKey, TSource>(kvp.Key, convertToSource(kvp.Value)),
                kvp => new KeyValuePair<TKey, TDest>(kvp.Key, convertToDest(kvp.Value)))
        {
            if (convertToSource == null)
                throw new ArgumentNullException(nameof(convertToSource));
            if (convertToDest == null)
                throw new ArgumentNullException(nameof(convertToDest));

            _convertToSource = convertToSource;
            _convertToDest = convertToDest;
        }

#if !NETSTANDARD1_2
        [System.Runtime.Serialization.OnSerializing]
        internal new void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            base.OnSerializing(context);

            // using expressions would be a much better solution but currently there is no simple way to serialize them
            // http://stackoverflow.com/questions/25721711/how-to-identify-a-lambda-closure-with-reflection
            if (_convertToSource.Target != null || _convertToDest.Target != null)
                throw new InvalidOperationException(Resources.DelegateSerializationNotSupported);
        }
#endif

        public bool ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        public IEnumerable<TKey> Keys => _source.Keys;

        public bool TryGetValue(TKey key, out TDest value)
        {
            var result = _source.TryGetValue(key, out TSource sourceValue);

            value = _convertToDest(sourceValue);
            return result;
        }

        public IEnumerable<TDest> Values => _source.Values.Select(_convertToDest);

        public TDest this[TKey key] => _convertToDest(_source[key]);
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DictionaryConverterDebugView<,,>))]
    public class DictionaryConverter<TKey, TSource, TDest, TDictionary> :
        CollectionConverter<KeyValuePair<TKey, TSource>, KeyValuePair<TKey, TDest>, TDictionary>,
        IDictionary<TKey, TDest>
        where TDictionary : IDictionary<TKey, TSource>
    {
        protected new readonly Func<TDest, TSource> _convertToSource;
        protected new readonly Func<TSource, TDest> _convertToDest;

        public DictionaryConverter(TDictionary source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source,
                kvp => new KeyValuePair<TKey, TSource>(kvp.Key, convertToSource(kvp.Value)),
                kvp => new KeyValuePair<TKey, TDest>(kvp.Key, convertToDest(kvp.Value)))
        {
            if (convertToSource == null)
                throw new ArgumentNullException(nameof(convertToSource));
            if (convertToDest == null)
                throw new ArgumentNullException(nameof(convertToDest));

            _convertToSource = convertToSource;
            _convertToDest = convertToDest;
        }

        public void Add(TKey key, TDest value)
        {
            _source.Add(key, _convertToSource(value));
        }

        public bool ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        public ICollection<TKey> Keys => _source.Keys;

        public bool Remove(TKey key)
        {
            return _source.Remove(key);
        }

        public bool TryGetValue(TKey key, out TDest value)
        {
            var result = _source.TryGetValue(key, out TSource sourceValue);

            value = _convertToDest(sourceValue);
            return result;
        }

        ICollection<TDest> _values;
        public ICollection<TDest> Values => _values ?? (_values = new CollectionConverter<TSource, TDest, ICollection<TSource>>(_source.Values, _convertToSource, _convertToDest));

        public TDest this[TKey key]
        {
            get => _convertToDest(_source[key]);
            set => _source[key] = _convertToSource(value);
        }

#if !NETSTANDARD1_2
        [System.Runtime.Serialization.OnSerializing]
        internal new void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            base.OnSerializing(context);

            // using expressions would be a much better solution but currently there is no simple way to serialize them
            // http://stackoverflow.com/questions/25721711/how-to-identify-a-lambda-closure-with-reflection
            if (_convertToSource.Target != null || _convertToDest.Target != null)
                throw new InvalidOperationException(Resources.DelegateSerializationNotSupported);
        }
#endif
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DictionaryConverterDebugView<,,>))]
    public class OrderedDictionaryConverter<TKey, TSource, TDest, TDictionary> :
        DictionaryConverter<TKey, TSource, TDest, TDictionary>,
        IOrderedDictionary<TKey, TDest>
        where TDictionary : IOrderedDictionary<TKey, TSource>
    {
        public OrderedDictionaryConverter(TDictionary source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public new int Add(TKey key, TDest value)
        {
            return _source.Add(key, _convertToSource(value));
        }

        public void Insert(int index, TKey key, TDest value)
        {
            _source.Insert(index, key, _convertToSource(value));
        }

        public TDest this[int index]
        {
            get => _convertToDest(_source[index]);
            set => _source[index] = _convertToSource(value);
        }

        public TKey GetKeyAt(int index)
        {
            return _source.GetKeyAt(index);
        }

        public int IndexOfKey(TKey key)
        {
            return _source.IndexOfKey(key);
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TDest>.Keys => Keys;

        IEnumerable<TDest> IReadOnlyDictionary<TKey, TDest>.Values => Values;
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(KeyedCollectionConverterDebugView<,,>))]
    public class KeyedCollectionConverter<TKey, TSource, TDest, TCollection> :
        ListConverter<TSource, TDest, TCollection>,
        IKeyedCollection<TKey, TDest>
        where TCollection : IKeyedCollection<TKey, TSource>
    {
        public KeyedCollectionConverter(TCollection source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public TDest this[TKey key] => _convertToDest(_source[key]);

        public bool ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        public ICollection<TKey> Keys => _source.Keys;

        public bool Remove(TKey key)
        {
            return _source.Remove(key);
        }

        public bool TryGetValue(TKey key, out TDest value)
        {
            var result = _source.TryGetValue(key, out TSource sourceValue);

            value = _convertToDest(sourceValue);
            return result;
        }
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionConverterDebugView<,>))]
    public class ObservableCollectionConverter<TSource, TDest, TList> : CollectionConverter<TSource, TDest, TList>, IObservableCollection<TDest>
        where TList : IObservableCollection<TSource>
    {
        public ObservableCollectionConverter(TList source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public IDisposable ResetSection()
        {
            return _source.ResetSection();
        }

        public void AddRange(IEnumerable<TDest> items)
        {
            _source.AddRange(items.Select(_convertToSource));
        }

        public void RemoveRange(IEnumerable<TDest> items)
        {
            _source.RemoveRange(items.Select(_convertToSource));
        }

        public void Replace(TDest currentItem, TDest newItem)
        {
            _source.Replace(_convertToSource(currentItem), _convertToSource(newItem));
        }

        void ICollectionChangePreviewable.Subscribe(ICollectionChangePreviewer previewer)
        {
            _source.Subscribe(previewer);
        }

        void ICollectionChangePreviewable.Unsubscribe(ICollectionChangePreviewer previewer)
        {
            _source.Unsubscribe(previewer);
        }

        void ICollectionChangePreviewable.UnsubscribeOwned<TOwner>(TOwner owner)
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

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionConverterDebugView<,>))]
    public class ObservableListConverter<TSource, TDest, TList> : ListConverter<TSource, TDest, TList>, IObservableList<TDest>
        where TList : IObservableList<TSource>
    {
        public ObservableListConverter(TList source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        public void InsertRange(int index, IEnumerable<TDest> items)
        {
            _source.InsertRange(index, items.Select(_convertToSource));
        }

        public void RemoveRange(int index, int count)
        {
            _source.RemoveRange(index, count);
        }

        public void RemoveAll(Predicate<TDest> match)
        {
            _source.RemoveAll(it => match(_convertToDest(it)));
        }

        public void ReplaceAt(int index, TDest newItem)
        {
            _source.ReplaceAt(index, _convertToSource(newItem));
        }

        public void Move(TDest item, int newIndex)
        {
            _source.Move(_convertToSource(item), newIndex);
        }

        public void MoveAt(int currentIndex, int newIndex)
        {
            _source.MoveAt(currentIndex, newIndex);
        }

        public IDisposable ResetSection()
        {
            return _source.ResetSection();
        }

        public void AddRange(IEnumerable<TDest> items)
        {
            _source.AddRange(items.Select(_convertToSource));
        }

        public void RemoveRange(IEnumerable<TDest> items)
        {
            _source.RemoveRange(items.Select(_convertToSource));
        }

        public void Replace(TDest currentItem, TDest newItem)
        {
            _source.Replace(_convertToSource(currentItem), _convertToSource(newItem));
        }

        void ICollectionChangePreviewable.Subscribe(ICollectionChangePreviewer previewer)
        {
            _source.Subscribe(previewer);
        }

        void ICollectionChangePreviewable.Unsubscribe(ICollectionChangePreviewer previewer)
        {
            _source.Unsubscribe(previewer);
        }

        void ICollectionChangePreviewable.UnsubscribeOwned<TOwner>(TOwner owner)
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

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(KeyedCollectionConverterDebugView<,,>))]
    public class ObservableKeyedCollectionConverter<TKey, TSource, TDest, TCollection> :
        ObservableListConverter<TSource, TDest, TCollection>,
        IObservableKeyedCollection<TKey, TDest>
        where TCollection : IObservableKeyedCollection<TKey, TSource>
    {
        public ObservableKeyedCollectionConverter(TCollection source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
            : base(source, convertToSource, convertToDest)
        {
        }

        TDest IReadOnlyKeyedCollection<TKey, TDest>.this[TKey key] => _convertToDest(_source[key]);

        bool IReadOnlyKeyedCollection<TKey, TDest>.ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        ICollection<TKey> IReadOnlyKeyedCollection<TKey, TDest>.Keys => _source.Keys;

        bool IKeyedCollection<TKey, TDest>.Remove(TKey key)
        {
            return _source.Remove(key);
        }

        bool IReadOnlyKeyedCollection<TKey, TDest>.TryGetValue(TKey key, out TDest value)
        {
            var result = _source.TryGetValue(key, out TSource sourceValue);

            value = _convertToDest(sourceValue);
            return result;
        }
    }
}
