using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Karambolo.Common.Collections
{
    [DebuggerNonUserCode]
    sealed class DictionaryDebugView<TKey, TValue>
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
    sealed class DictionaryKeyCollectionDebugView<TKey, TValue>
    {
        readonly ICollection<TKey> _collection;

        public DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items => _collection.ToArray();
    }

    [DebuggerNonUserCode]
    sealed class DictionaryValueCollectionDebugView<TKey, TValue>
    {
        readonly ICollection<TValue> _collection;

        public DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items => _collection.ToArray();
    }
}
