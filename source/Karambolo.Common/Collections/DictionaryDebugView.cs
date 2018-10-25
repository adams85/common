using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Karambolo.Common.Collections
{
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
}
