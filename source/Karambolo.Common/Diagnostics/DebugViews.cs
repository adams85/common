using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Karambolo.Common.Collections;

namespace Karambolo.Common.Diagnostics
{
    [DebuggerNonUserCode]
    class ReadOnlyCollectionDebugView<T>
    {
        readonly IReadOnlyCollection<T> _collection;

        public ReadOnlyCollectionDebugView(IReadOnlyCollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _collection.ToArray();
    }

    [DebuggerNonUserCode]
    class CollectionDebugView<T>
    {
        readonly ICollection<T> _collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _collection.ToArray();
    }

    [DebuggerNonUserCode]
    class ReadOnlyDictionaryDebugView<TKey, TValue>
    {
        readonly IReadOnlyDictionary<TKey, TValue> _dictionary;

        public ReadOnlyDictionaryDebugView(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items => _dictionary.ToArray();
    }

    [DebuggerNonUserCode]
    class DictionaryDebugView<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> _dictionary;

        public DictionaryDebugView(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items => _dictionary.ToArray();
    }

    [DebuggerNonUserCode]
    class KeyedCollectionDebugView<TKey, TValue>
    {
        readonly IKeyedCollection<TKey, TValue> _collection;

        public KeyedCollectionDebugView(IKeyedCollection<TKey, TValue> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items => _collection.AsKeyValuePairs().ToArray();
    }

    [DebuggerNonUserCode]
    class ReadOnlyCollectionConverterDebugView<TSource, TDest>
    {
        readonly IReadOnlyCollection<TDest> _collection;

        public ReadOnlyCollectionConverterDebugView(IReadOnlyCollection<TDest> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TDest[] Items => _collection.ToArray();
    }

    [DebuggerNonUserCode]
    class CollectionConverterDebugView<TSource, TDest>
    {
        readonly ICollection<TDest> _collection;

        public CollectionConverterDebugView(ICollection<TDest> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TDest[] Items => _collection.ToArray();
    }

    [DebuggerNonUserCode]
    class ReadOnlyDictionaryConverterDebugView<TKey, TSource, TDest>
    {
        readonly IReadOnlyDictionary<TKey, TDest> _dictionary;

        public ReadOnlyDictionaryConverterDebugView(IReadOnlyDictionary<TKey, TDest> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TDest>[] Items => _dictionary.ToArray();
    }

    [DebuggerNonUserCode]
    class DictionaryConverterDebugView<TKey, TSource, TDest>
    {
        readonly IDictionary<TKey, TDest> _dictionary;

        public DictionaryConverterDebugView(IDictionary<TKey, TDest> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TDest>[] Items => _dictionary.ToArray();
    }

    [DebuggerNonUserCode]
    class KeyedCollectionConverterDebugView<TKey, TSource, TDest>
    {
        readonly IKeyedCollection<TKey, TDest> _collection;

        public KeyedCollectionConverterDebugView(IKeyedCollection<TKey, TDest> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TDest>[] Items => _collection.AsKeyValuePairs().ToArray();
    }
}
