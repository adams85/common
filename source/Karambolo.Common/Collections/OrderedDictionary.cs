﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using Karambolo.Common.Properties;

namespace Karambolo.Common.Collections
{
    /// <summary>
    /// Represents a generic read-only collection of key/value pairs that are ordered independently of the key and value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    public interface IReadOnlyOrderedDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <value>The value of the item at the specified index.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is equal to or greater than <see cref="ICollection.Count"/>.</exception>
        TValue this[int index] { get; }

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the key to get.</param>
        /// <returns>The key of the item at the specified index.</returns>
        TKey GetKeyAt(int index);

        /// <summary>
        /// Returns the zero-based index of the specified key in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></param>
        /// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="ley"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, -1</returns>
        /// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
        int IndexOfKey(TKey key);
    }

    /// <summary>
    /// Represents a generic collection of key/value pairs that are ordered independently of the key and value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyOrderedDictionary<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <value>The value of the item at the specified index.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is equal to or greater than <see cref="ICollection.Count"/>.</exception>
        new TValue this[int index] { get; set; }

        /// <summary>
        /// Inserts a new entry into the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> collection with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is greater than <see cref="ICollection.Count"/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
        /// <exception cref="NotSupportedException">The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> is read-only.<br/>
        /// -or-<br/>
        /// The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> has a fized size.</exception>
        void Insert(int index, TKey key, TValue value);

        /// <summary>
        /// Removes the entry at the specified index from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// index is equal to or greater than <see cref="Count"/>.</exception>
        void RemoveAt(int index);

        new int Count { get; }
        new ICollection<TKey> Keys { get; }
        new ICollection<TValue> Values { get; }
        new TValue this[TKey key] { get; set; }
        new bool ContainsKey(TKey key);
        new bool TryGetValue(TKey key, out TValue value);
    }

    /// <summary>
    /// Represents a generic collection of key/value pairs that are ordered independently of the key and value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
#if !NETSTANDARD1_0
    [Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>, IOrderedDictionary
    {
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly bool _returnKeyValuePair;
            private readonly Dictionary<TKey, TValue> _dictionary;
            private List<TKey>.Enumerator _listEnumerator;

            internal Enumerator(OrderedDictionary<TKey, TValue> dictionary, bool returnKeyValuePair)
            {
                _dictionary = dictionary._dictionary;
                _listEnumerator = dictionary._list.GetEnumerator();
                _returnKeyValuePair = returnKeyValuePair;
            }

            public void Dispose()
            {
                _listEnumerator.Dispose();
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    TKey key = _listEnumerator.Current;
                    return new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
                }
            }

            object IEnumerator.Current => _returnKeyValuePair ? Current : (object)GetCurrentEntry();

            private DictionaryEntry GetCurrentEntry()
            {
                TKey key = _listEnumerator.Current;
                return new DictionaryEntry(key, _dictionary[key]);
            }

            DictionaryEntry IDictionaryEnumerator.Entry => GetCurrentEntry();

            object IDictionaryEnumerator.Key => _listEnumerator.Current;

            object IDictionaryEnumerator.Value => _dictionary[_listEnumerator.Current];

            public bool MoveNext()
            {
                return _listEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                var listEnumeratorBoxed = (IEnumerator)_listEnumerator;
                listEnumeratorBoxed.Reset();
                _listEnumerator = (List<TKey>.Enumerator)listEnumeratorBoxed;
            }
        }

#if !NETSTANDARD1_0
        [Serializable]
#endif
        [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        public class KeyCollection : ICollection<TKey>, IReadOnlyCollection<TKey>, ICollection
        {
            public struct Enumerator : IEnumerator<TKey>
            {
                private List<TKey>.Enumerator _listEnumerator;

                internal Enumerator(OrderedDictionary<TKey, TValue> dictionary)
                {
                    _listEnumerator = dictionary._list.GetEnumerator();
                }

                public void Dispose()
                {
                    _listEnumerator.Dispose();
                }

                public TKey Current => _listEnumerator.Current;

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    return _listEnumerator.MoveNext();
                }

                void IEnumerator.Reset()
                {
                    var listEnumeratorBoxed = (IEnumerator)_listEnumerator;
                    listEnumeratorBoxed.Reset();
                    _listEnumerator = (List<TKey>.Enumerator)listEnumeratorBoxed;
                }
            }

            private readonly OrderedDictionary<TKey, TValue> _dictionary;

            public KeyCollection(OrderedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                    throw new ArgumentNullException(nameof(dictionary));

                _dictionary = dictionary;
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return _dictionary.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int index)
            {
                _dictionary._list.CopyTo(array, index);
            }

            public int Count => _dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;

            bool ICollection<TKey>.Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                ((ICollection)_dictionary._list).CopyTo(array, index);
            }

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;
        }

#if !NETSTANDARD1_0
        [Serializable]
#endif
        [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        public class ValueCollection : ICollection<TValue>, IReadOnlyCollection<TValue>, ICollection
        {
            public struct Enumerator : IEnumerator<TValue>
            {
                private readonly Dictionary<TKey, TValue> _dictionary;
                private List<TKey>.Enumerator _listEnumerator;

                internal Enumerator(OrderedDictionary<TKey, TValue> dictionary)
                {
                    _dictionary = dictionary._dictionary;
                    _listEnumerator = dictionary._list.GetEnumerator();
                }

                public void Dispose()
                {
                    _listEnumerator.Dispose();
                }

                public TValue Current => _dictionary[_listEnumerator.Current];

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    return _listEnumerator.MoveNext();
                }

                void IEnumerator.Reset()
                {
                    var listEnumeratorBoxed = (IEnumerator)_listEnumerator;
                    listEnumeratorBoxed.Reset();
                    _listEnumerator = (List<TKey>.Enumerator)listEnumeratorBoxed;
                }
            }

            private readonly OrderedDictionary<TKey, TValue> _dictionary;

            public ValueCollection(OrderedDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                    throw new ArgumentNullException(nameof(dictionary));

                _dictionary = dictionary;
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return _dictionary.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (array.Length - index < Count)
                    throw new ArgumentException(Resources.ArrayNotLongEnough, nameof(array));
                if (index < 0 || array.Length < index)
                    throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

                for (int i = 0, n = _dictionary._list.Count; i < n; i++)
                    array[index++] = _dictionary._dictionary[_dictionary._list[i]];
            }

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(_dictionary);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (array.Rank != 1)
                    throw new ArgumentException(Resources.MultiDimArrayNotSupported, nameof(array));
                if (array.GetLowerBound(0) != 0)
                    throw new ArgumentException(Resources.NonZeroLowerBoundArrayNotSupported, nameof(array));
                if (array.Length - index < Count)
                    throw new ArgumentException(Resources.ArrayNotLongEnough, nameof(array));
                if (index < 0 || array.Length < index)
                    throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

                if (array is TValue[] array2)
                {
                    CopyTo(array2, index);
                    return;
                }

                if (!(array is object[] array3))
                    throw new ArgumentException(Resources.InvalidArrayType, nameof(array));

                try
                {
                    for (int i = 0, n = _dictionary._list.Count; i < n; i++)
                        array3[index++] = _dictionary._dictionary[_dictionary._list[i]];
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(Resources.InvalidArrayType, nameof(array));
                }
            }

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;
        }

        private const int DefaultInitialCapacity = 0;

        private static readonly bool s_valueTypeAllowsNull = typeof(TValue).AllowsNull();

        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly List<TKey> _list;

#if !NETSTANDARD1_0
        [NonSerialized]
#endif
        private KeyCollection _keys;

#if !NETSTANDARD1_0
        [NonSerialized]
#endif
        private ValueCollection _values;

#if !NETSTANDARD1_0
        [NonSerialized]
#endif
        private object _syncRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class.
        /// </summary>
        public OrderedDictionary()
            : this(DefaultInitialCapacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> can contain.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
        public OrderedDictionary(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified comparer.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
            : this(DefaultInitialCapacity, comparer)
        {
        }

        public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        public OrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            _list = new List<TKey>(dictionary.Keys);
            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified initial capacity and comparer.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection can contain.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (0 > capacity)
                throw new ArgumentOutOfRangeException(nameof(capacity), Resources.ValueMustBeNonNegative);

            comparer = comparer ?? EqualityComparer<TKey>.Default;
            _list = new List<TKey>(capacity);
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        /// <summary>
        /// Converts the object passed as a key to the key type of the dictionary
        /// </summary>
        /// <param name="key">The key object to check</param>
        /// <returns>The key object, cast as the key type of the dictionary</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.</exception>
        /// <exception cref="ArgumentException">The key type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="key"/>.</exception>
        private static TKey ConvertToKeyType(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (!(key is TKey))
                throw new ArgumentException(string.Format(Resources.InvalidKeyType, typeof(TKey)), nameof(key));

            return (TKey)key;
        }

        /// <summary>
        /// Converts the object passed as a value to the value type of the dictionary
        /// </summary>
        /// <param name="value">The object to convert to the value type of the dictionary</param>
        /// <returns>The value object, converted to the value type of the dictionary</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <null/>, and the value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is a value type.</exception>
        /// <exception cref="ArgumentException">The value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="valueObject"/>.</exception>
        private static TValue ConvertToValueType(object value)
        {
            if (value == null)
            {
                if (!s_valueTypeAllowsNull)
                    throw new ArgumentNullException(nameof(value));

                return default;
            }

            if (!(value is TValue))
                throw new ArgumentException(string.Format(Resources.InvalidValueType, typeof(TValue)), nameof(value));

            return (TValue)value;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary)this).GetEnumerator();
        }

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return new Enumerator(this, returnKeyValuePair: false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, returnKeyValuePair: true);
        }

        /// <summary>
        /// Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
        public void Insert(int index, TKey key, TValue value)
        {
            if (index > Count || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            _dictionary.Add(key, value);
            _list.Insert(index, key);
        }

        /// <summary>
        /// Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.<br/>
        /// -or-<br/>
        /// <paramref name="value"/> is <null/>, and the value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is a value type.</exception>
        /// <exception cref="ArgumentException">The key type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="key"/>.<br/>
        /// -or-<br/>
        /// The value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="value"/>.<br/>
        /// -or-<br/>
        /// An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
        void IOrderedDictionary.Insert(int index, object key, object value)
        {
            Insert(index, ConvertToKeyType(key), ConvertToValueType(value));
        }

        /// <summary>
        /// Removes the entry at the specified index from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// index is equal to or greater than <see cref="Count"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index >= Count || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

            TKey key = _list[index];
            _list.RemoveAt(index);
            _dictionary.Remove(key);
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <value>The value of the item at the specified index.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// index is equal to or greater than <see cref="Count"/>.</exception>
        public TValue this[int index]
        {
            get => _dictionary[_list[index]];
            set
            {
                if (index >= Count || index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

                TKey key = _list[index];
                _list[index] = key; // forcing the list to update its internal version
                _dictionary[key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <value>The value of the item at the specified index.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
        /// -or-<br/>
        /// index is equal to or greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueObject"/> is a null reference, and the value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is a value type.</exception>
        /// <exception cref="ArgumentException">The value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="valueObject"/>.</exception>
        object IOrderedDictionary.this[int index]
        {
            get => this[index];
            set => this[index] = ConvertToValueType(value);
        }

        /// <summary>
        /// Adds an entry with the specified key and value into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection with the lowest available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <null/>.</param>
        /// <remarks>A key cannot be <null/>, but a value can be.
        /// <para>You can also use the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property to add new elements by setting the value of a key that does not exist in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection; however, if the specified key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>, setting the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property overwrites the old value. In contrast, the <see cref="M:Add"/> method does not modify existing elements.</para></remarks>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></exception>
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            _list.Add(key);
        }

        /// <summary>
        /// Adds an entry with the specified key and value into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection with the lowest available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <null/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.<br/>
        /// -or-<br/>
        /// <paramref name="value"/> is <null/>, and the value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is a value type.</exception>
        /// <exception cref="ArgumentException">The key type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="key"/>.<br/>
        /// -or-<br/>
        /// The value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="value"/>.</exception>
        void IDictionary.Add(object key, object value)
        {
            Add(ConvertToKeyType(key), ConvertToValueType(value));
        }

        /// <summary>
        /// Removes all elements from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <remarks>The capacity is not changed as a result of calling this method.</remarks>
        public void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.</param>
        /// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains a specific value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection. The value can be null for reference types.</param>
        /// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains an element with the specified value; otherwise, <see langword="false"/>.</returns>
        public bool ContainsValue(TValue value)
        {
            return _dictionary.ContainsValue(value);
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.</param>
        /// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
        /// <exception cref="ArgumentException">The key type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="key"/>.</exception>
        bool IDictionary.Contains(object key)
        {
            return ContainsKey(ConvertToKeyType(key));
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> has a fixed size.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> has a fixed size; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        bool IDictionary.IsFixedSize => false;

        /// <summary>
        /// Gets a value indicating whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection is read-only.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is read-only; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>
        /// A collection that is read-only does not allow the addition, removal, or modification of elements after the collection is created.
        /// <para>A collection that is read-only is simply a collection with a wrapper that prevents modification of the collection; therefore, if changes are made to the underlying collection, the read-only collection reflects those changes.</para>
        /// </remarks>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets an <see cref="ICollection"/> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
        /// </summary>
        /// <value>An <see cref="ICollection"/> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</value>
        /// <remarks>The returned <see cref="ICollection"/> object is not a static copy; instead, the collection refers back to the keys in the original <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the key collection.</remarks>
        ICollection IDictionary.Keys => (ICollection)Keys;

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the key to get.</param>
        /// <returns>The key of the item at the specified index.</returns>
        public TKey GetKeyAt(int index)
        {
            return _list[index];
        }

        /// <summary>
        /// Returns the zero-based index of the specified key in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></param>
        /// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="ley"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, -1</returns>
        /// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
        public int IndexOfKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            IEqualityComparer<TKey> comparer = _dictionary.Comparer;
            for (int i = 0, n = _list.Count; i < n; i++)
                if (comparer.Equals(_list[i], key))
                    return i;

            return -1;
        }

        /// <summary>
        /// Removes the entry with the specified key from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <param name="key">The key of the entry to remove</param>
        /// <returns><see langword="true"/> if the key was found and the corresponding element was removed; otherwise, <see langword="false"/></returns>
        public bool Remove(TKey key)
        {
            var index = IndexOfKey(key);

            if (index < 0)
                return false;

            _list.RemoveAt(index);
            _dictionary.Remove(key);
            return true;
        }

        /// <summary>
        /// Removes the entry with the specified key from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <param name="key">The key of the entry to remove</param>
        void IDictionary.Remove(object key)
        {
            Remove(ConvertToKeyType(key));
        }

        /// <summary>
        /// Gets an <see cref="ICollection"/> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <value>An <see cref="ICollection"/> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.</value>
        /// <remarks>The returned <see cref="ICollection"/> object is not a static copy; instead, the <see cref="ICollection"/> refers back to the values in the original <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the <see cref="ICollection"/>.</remarks>
        ICollection IDictionary.Values => (ICollection)Values;

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>The value associated with the specified key. If the specified key is not found, attempting to get it returns <null/>, and attempting to set it creates a new element using the specified key.</value>
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    _list[0] = _list[0]; // forcing the list to update its internal version
                    _dictionary[key] = value;
                }
                else
                    Add(key, value);
            }
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>The value associated with the specified key. If the specified key is not found, attempting to get it returns <null/>, and attempting to set it creates a new element using the specified key.</value>
        object IDictionary.this[object key]
        {
            get => this[ConvertToKeyType(key)];
            set => this[ConvertToKeyType(key)] = ConvertToValueType(value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> elements to a one-dimensional Array object at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> object that is the destination of the <see cref="T:KeyValuePair`2>"/> objects copied from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <remarks>The <see cref="M:CopyTo"/> method preserves the order of the elements in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></remarks>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1)
                throw new ArgumentException(Resources.MultiDimArrayNotSupported, nameof(array));
            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException(Resources.NonZeroLowerBoundArrayNotSupported, nameof(array));
            if (array.Length - index < Count)
                throw new ArgumentException(Resources.ArrayNotLongEnough, nameof(array));
            if (index < 0 || array.Length < index)
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);

            if (array is KeyValuePair<TKey, TValue>[] kvpArray)
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)this).CopyTo(kvpArray, index);
                return;
            }

            if (array is DictionaryEntry[] deArray)
            {
                TKey key;
                for (int i = 0, n = _list.Count; i < n; i++)
                    deArray[index++] = new DictionaryEntry(key = _list[i], _dictionary[key]);
                return;
            }

            if (array is object[] objArray)
                try
                {
                    TKey key;
                    for (int i = 0, n = _list.Count; i < n; i++)
                        objArray[index++] = new KeyValuePair<TKey, TValue>(key = _list[i], _dictionary[key]);
                    return;
                }
                catch (ArrayTypeMismatchException) { }

            throw new ArgumentException(Resources.InvalidArrayType, nameof(array));
        }

        /// <summary>
        /// Gets the number of key/values pairs contained in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
        /// </summary>
        /// <value>The number of key/value pairs contained in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.</value>
        public int Count => _list.Count;

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> object is synchronized (thread-safe).
        /// </summary>
        /// <value>This method always returns false.</value>
        bool ICollection.IsSynchronized => false;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> object.
        /// </summary>
        /// <value>An object that can be used to synchronize access to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> object.</value>
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</value>
        /// <remarks>The returned <see cref="T:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object is not a static copy; instead, the collection refers back to the keys in the original <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the key collection.
        /// It is guaranteed that the order of the keys is the same as in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</remarks>
        public KeyCollection Keys => _keys ?? (_keys = new KeyCollection(this));

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TKey> IOrderedDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of <paramref name="value"/>. This parameter can be passed uninitialized.</param>
        /// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:ICollection{TValue}">ICollection&lt;TValue&gt;</see> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
        /// </summary>
        /// <value>An <see cref="T:ICollection{TValue}">ICollection&lt;TValue&gt;</see> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</value>
        /// <remarks>The returned <see cref="T:ICollection{TValue}">ICollection&lt;TKey&gt;</see> object is not a static copy; instead, the collection refers back to the values in the original <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the value collection.
        /// It is guaranteed that the order of the values is the same as in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</remarks>
        public ValueCollection Values => _values ?? (_values = new ValueCollection(this));

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        ICollection<TValue> IOrderedDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        /// <summary>
        /// Adds the specified value to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> with the specified key.
        /// </summary>
        /// <param name="item">The <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure representing the key and value to add to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains a specific key and value.
        /// </summary>
        /// <param name="item">The <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
        /// <returns><see langword="true"/> if <paramref name="keyValuePair"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, <see langword="false"/>.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> to an array of type <see cref="T:KeyValuePair`2>"/>, starting at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional array of type <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> that is the destination of the <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> elements copied from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException(Resources.ArrayNotLongEnough, nameof(array));
            if (arrayIndex < 0 || array.Length < arrayIndex)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), Resources.IndexOutOfRange);

            TKey key;
            for (int i = 0, n = _list.Count; i < n; i++)
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key = _list[i], _dictionary[key]);
        }

        /// <summary>
        /// Removes a key and value from the dictionary.
        /// </summary>
        /// <param name="item">The <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure representing the key and value to remove from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
        /// <returns><see langword="true"/> if the key and value represented by <paramref name="keyValuePair"/> is successfully found and removed; otherwise, <see langword="false"/>. This method returns <see langword="false"/> if <paramref name="keyValuePair"/> is not found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item) ? Remove(item.Key) : false;
        }
    }
}
