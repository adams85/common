using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using Karambolo.Common.Properties;
using System.Threading;
using System.Diagnostics;
using Karambolo.Common.Diagnostics;

// TODO: supporting multi-replace and multi-move?
// In short, the value of the Action property determines the validity of other properties in this class. NewItems and OldItems are null when they are invalid; NewStartingIndex and OldStartingIndex are -1 when they are invalid.
// If Action is NotifyCollectionChangedAction.Add, then NewItems contains the items that were added. In addition, if NewStartingIndex is not -1, then it contains the index where the new items were added.
// If Action is NotifyCollectionChangedAction.Remove, then OldItems contains the items that were removed. In addition, if OldStartingIndex is not -1, then it contains the index where the old items were removed.
// If Action is NotifyCollectionChangedAction.Replace, then OldItems contains the replaced items and NewItems contains the replacement items. In addition, NewStartingIndex and OldStartingIndex are equal, and if they are not -1, then they contain the index where the items were replaced.
// If Action is NotifyCollectionChangedAction.Move, then NewItems and OldItems are logically equivalent (i.e., they are SequenceEqual, even if they are different instances), and they contain the items that moved. In addition, OldStartingIndex contains the index where the items were moved from, and NewStartingIndex contains the index where the items were moved to. A Move operation is logically treated as a Remove followed by an Add, so NewStartingIndex is interpreted as though the items had already been removed.
// If Action is NotifyCollectionChangedAction.Reset, then no other properties are valid.
// http://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
namespace Karambolo.Common.Collections
{
    public interface ICollectionChangePreviewer
    {
        bool OnPropertyChanged(ICollectionChangePreviewable sender, PropertyChangedEventArgs args);
        bool OnCollectionChanged(ICollectionChangePreviewable sender, NotifyCollectionChangedEventArgs args);
    }

    public interface ICollectionChangePreviewable
    {
        void Subscribe(ICollectionChangePreviewer previewer);
        void Unsubscribe(ICollectionChangePreviewer previewer);
        void UnsubscribeOwned<TOwner>(TOwner owner) where TOwner : class;
    }

    public interface IReadOnlyObservableCollection<T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged, ICollectionChangePreviewable { }

    public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, ICollection<T>
    {
        IDisposable ResetSection();
        void AddRange(IEnumerable<T> items);
        void RemoveRange(IEnumerable<T> items);
        void Replace(T currentItem, T newItem);

        new int Count { get; }
    }

    public interface IReadOnlyObservableList<T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T> { }

    public interface IObservableList<T> : IObservableCollection<T>, IReadOnlyObservableList<T>, IList<T>
    {
        void InsertRange(int index, IEnumerable<T> items);
        void RemoveRange(int index, int count);
        void RemoveAll(Predicate<T> match);
        void ReplaceAt(int index, T newItem);
        void Move(T item, int newIndex);
        void MoveAt(int currentIndex, int newIndex);

        new T this[int index] { get; set; }
    }

    public interface IReadOnlyObservableKeyedCollection<TKey, TValue> : IReadOnlyObservableList<TValue>, IReadOnlyKeyedCollection<TKey, TValue> { }

    public interface IObservableKeyedCollection<TKey, TValue> : IObservableList<TValue>, IReadOnlyObservableKeyedCollection<TKey, TValue>, IKeyedCollection<TKey, TValue> { }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class ObservableCollectionDecorator<T, TCollection> : IObservableCollection<T>, ICollection
        where TCollection : ICollection<T>
    {
        protected const string countPropertyName = "Count";
        protected const string indexerPropertyName = "Item[]";

#if !NETSTANDARD1_2
        [System.Serializable]
#endif
        class SimpleMonitor : IDisposable
        {
            int _busyCount;
            public bool Busy => _busyCount > 0;

            public void Enter()
            {
                _busyCount++;
            }

            public void Dispose()
            {
                _busyCount--;
            }
        }

#if !NETSTANDARD1_2
        [System.Serializable]
#endif
        class ResetSectionImpl : IDisposable
        {
            HashSet<string> _changedProperties;

            readonly ObservableCollectionDecorator<T, TCollection> _owner;

            public ResetSectionImpl(ObservableCollectionDecorator<T, TCollection> owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                // Thread synchronization is not necessary:
                // http://stackoverflow.com/questions/11745440/what-operations-are-atomic-in-c
                _owner._resetSection = null;

                if (_changedProperties != null)
                    foreach (var changedProperty in _changedProperties)
                        _owner.OnPropertyChanged(changedProperty);

                _owner.OnCollectionMultiChanged(NotifyCollectionChangedAction.Reset, null);
            }

            public void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (_changedProperties == null)
                    _changedProperties = new HashSet<string>();
                _changedProperties.Add(e.PropertyName);
            }
        }

        protected readonly TCollection _source;
        protected readonly bool _treatClearAsRemove;
        readonly SimpleMonitor _monitor = new SimpleMonitor();
        readonly Lazy<List<ICollectionChangePreviewer>> _previewers = new Lazy<List<ICollectionChangePreviewer>>();
        ResetSectionImpl _resetSection;

        public ObservableCollectionDecorator(TCollection source, bool treatClearAsRemove = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            _source = source;
            _treatClearAsRemove = treatClearAsRemove;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_resetSection == null)
            {
                if (_previewers.IsValueCreated)
                {
                    var previewerCount = _previewers.Value.Count;
                    for (var i = 0; i < previewerCount; i++)
                        if (!_previewers.Value[i].OnPropertyChanged(this, e))
                            return;
                }

                PropertyChanged?.Invoke(this, e);
            }
            else
                _resetSection.OnPropertyChanged(e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_resetSection == null)
            {
                if (_previewers.IsValueCreated)
                {
                    var previewerCount = _previewers.Value.Count;
                    for (var i = 0; i < previewerCount; i++)
                        if (!_previewers.Value[i].OnCollectionChanged(this, e))
                            return;
                }

                var collectionChanged = CollectionChanged;
                if (collectionChanged != null)
                    using (BlockReentrancy())
                        collectionChanged(this, e);
            }
        }

        protected void CheckReentrancy()
        {
            var collectionChanged = CollectionChanged;
            if (_monitor.Busy && collectionChanged != null && collectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException(Resources.ChangeDuringCollectionChangedEventNotSupported);
        }

        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnCollectionSingleChanged(NotifyCollectionChangedAction action, object item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
        }

        protected void OnCollectionMultiChanged(NotifyCollectionChangedAction action, IList items)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items));
        }

        protected void OnCollectionSingleReplaced(object oldItem, object newItem)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
        }

        public IDisposable ResetSection()
        {
            if (Interlocked.CompareExchange(ref _resetSection, new ResetSectionImpl(this), null) != null)
                throw new InvalidOperationException(Resources.ParallelResetSectionsNotSupported);

            return _resetSection;
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var addedItems = new List<T>();
            var currentCount = _source.Count;
            foreach (var item in items)
            {
                _source.Add(item);
                var newCount = _source.Count;
                if (newCount > currentCount)
                {
                    addedItems.Add(item);
                    currentCount = newCount;
                }
            }

            if (addedItems.Count > 0)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Add, addedItems);
            }
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            CheckReentrancy();

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var removedItems = new List<T>();
            foreach (var item in items)
                if (_source.Remove(item))
                    removedItems.Add(item);

            if (removedItems.Count > 0)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Remove, removedItems);
            }
        }

        public virtual void Replace(T currentItem, T newItem)
        {
            CheckReentrancy();
            var removed = _source.Remove(currentItem);
            if (!removed)
                throw new ArgumentException(Resources.ItemToReplacedCannotRemoved, nameof(currentItem));

            var currentCount = _source.Count;
            _source.Add(newItem);
            if (_source.Count <= currentCount)
            {
                _source.Add(currentItem);
                throw new ArgumentException(Resources.ItemToReplaceCannotAdded, nameof(newItem));
            }

            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleReplaced(currentItem, newItem);
        }

        public virtual void Clear()
        {
            CheckReentrancy();

            if (_source.Count > 0)
            {
                var removedItems = _treatClearAsRemove ? _source.ToList() : null;

                _source.Clear();
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(_treatClearAsRemove ? NotifyCollectionChangedAction.Remove : NotifyCollectionChangedAction.Reset, removedItems);
            }
        }

        public virtual void Add(T item)
        {
            CheckReentrancy();

            var currentCount = _source.Count;
            _source.Add(item);
            if (_source.Count > currentCount)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionSingleChanged(NotifyCollectionChangedAction.Add, item);
            }
        }

        public bool Contains(T item)
        {
            return _source.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _source.CopyTo(array, arrayIndex);
        }

        public int Count => _source.Count;

        public bool IsReadOnly => _source.IsReadOnly;

        public virtual bool Remove(T item)
        {
            CheckReentrancy();

            if (_source.Remove(item))
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionSingleChanged(NotifyCollectionChangedAction.Remove, item);
                return true;
            }
            else
                return false;
        }

        ICollection AsCollection()
        {
            var result = _source as ICollection;
            if (result == null)
                throw new NotSupportedException();
            return result;
        }

        int ICollection.Count => AsCollection().Count;

        void ICollection.CopyTo(Array array, int index)
        {
            AsCollection().CopyTo(array, index);
        }

        bool ICollection.IsSynchronized => AsCollection().IsSynchronized;

        object ICollection.SyncRoot => AsCollection().SyncRoot;

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        void ICollectionChangePreviewable.Subscribe(ICollectionChangePreviewer previewer)
        {
            _previewers.Value.Add(previewer);
        }

        void ICollectionChangePreviewable.Unsubscribe(ICollectionChangePreviewer previewer)
        {
            if (!_previewers.IsValueCreated)
                return;

            _previewers.Value.Remove(previewer);
        }

        void ICollectionChangePreviewable.UnsubscribeOwned<TOwner>(TOwner owner)
        {
            if (!_previewers.IsValueCreated)
                return;

            _previewers.Value.RemoveAll(p =>
            {
                var owned = p as IOwned<TOwner>;
                return owned != null && owned.Owner == owner;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class ObservableListDecorator<T, TCollection> : ObservableCollectionDecorator<T, TCollection>, IObservableList<T>, IList
        where TCollection : IList<T>
    {
        public ObservableListDecorator(TCollection source, bool treatClearAsRemove = false)
            : base(source, treatClearAsRemove)
        {
        }

        protected void OnCollectionSingleChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        protected void OnCollectionMultiChanged(NotifyCollectionChangedAction action, IList items, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, index));
        }

        protected void OnCollectionSingleReplaced(object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected void OnCollectionSingleMoved(object oldItem, int oldIndex, int newIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItem, newIndex, oldIndex));
        }

        public virtual void InsertRange(int index, IEnumerable<T> items)
        {
            CheckReentrancy();

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var insertedItems = new List<T>();
            var i = index;
            foreach (var item in items)
            {
                _source.Insert(i++, item);
                insertedItems.Add(item);
            }

            if (insertedItems.Count > 0)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Add, insertedItems, index);
            }
        }

        public virtual void RemoveRange(int index, int count)
        {
            CheckReentrancy();

            if (!(index >= 0))
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);
            if (!(index + count <= _source.Count))
                throw new ArgumentException(Resources.RangeExceedsCollectionSize, nameof(count));

            var removedItems = new List<T>();
            for (var i = index + count - 1; i >= index; i--)
            {
                var item = _source[i];
                _source.RemoveAt(i);
                removedItems.Add(item);
            }

            if (removedItems.Count > 0)
            {
                removedItems.Reverse();
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Remove, removedItems, index);
            }
        }

        public virtual void RemoveAll(Predicate<T> match)
        {
            CheckReentrancy();

            var removedItems = new List<T>();
            var count = _source.Count;
            T item;
            for (var i = count - 1; i >= 0; i--)
                if (match(item = _source[i]))
                {
                    _source.RemoveAt(i);
                    removedItems.Add(item);
                }

            if (removedItems.Count > 0)
            {
                removedItems.Reverse();
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Remove, removedItems);
            }
        }

        public virtual void ReplaceAt(int index, T newItem)
        {
            CheckReentrancy();

            var currentItem = _source[index];
            _source[index] = newItem;

            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleReplaced(currentItem, newItem, index);
        }

        public virtual void Move(T item, int newIndex)
        {
            CheckReentrancy();

            if (!(0 <= newIndex && newIndex < _source.Count))
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            var currentIndex = _source.IndexOf(item);
            if (!(currentIndex >= 0))
                throw new ArgumentException(Resources.ItemToMovedNotFound, nameof(item));
            if (currentIndex != newIndex)
            {
                _source.RemoveAt(currentIndex);
                _source.Insert(newIndex, item);

                OnPropertyChanged(indexerPropertyName);
                OnCollectionSingleMoved(item, currentIndex, newIndex);
            }
        }

        public virtual void MoveAt(int currentIndex, int newIndex)
        {
            CheckReentrancy();

            if (!(0 <= newIndex && newIndex < _source.Count))
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            var item = _source[currentIndex];
            if (currentIndex != newIndex)
            {
                _source.RemoveAt(currentIndex);
                _source.Insert(newIndex, item);

                OnPropertyChanged(indexerPropertyName);
                OnCollectionSingleMoved(item, currentIndex, newIndex);
            }
        }

        public override void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var index = _source.Count;

            var addedItems = new List<T>();
            foreach (var item in items)
            {
                _source.Add(item);
                addedItems.Add(item);
            }

            if (addedItems.Count > 0)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Add, addedItems, index);
            }
        }

        public override void Replace(T currentItem, T newItem)
        {
            CheckReentrancy();

            var index = _source.IndexOf(currentItem);
            if (!(index >= 0))
                throw new ArgumentException(Resources.ItemToReplacedCannotRemoved, nameof(currentItem));
            _source[index] = newItem;

            OnPropertyChanged(countPropertyName);
            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleReplaced(currentItem, newItem, index);
        }

        public int IndexOf(T item)
        {
            return _source.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            CheckReentrancy();

            _source.Insert(index, item);

            OnPropertyChanged(countPropertyName);
            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public virtual void RemoveAt(int index)
        {
            CheckReentrancy();

            var item = _source[index];
            _source.RemoveAt(index);

            OnPropertyChanged(countPropertyName);
            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        public T this[int index]
        {
            get => _source[index];
            set => _source[index] = value;
        }

        public override void Clear()
        {
            CheckReentrancy();

            if (_source.Count > 0)
            {
                List<T> removedItems;
                int index;
                if (_treatClearAsRemove)
                {
                    removedItems = _source.ToList();
                    index = 0;
                }
                else
                {
                    removedItems = null;
                    index = -1;
                }

                _source.Clear();
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(_treatClearAsRemove ? NotifyCollectionChangedAction.Remove : NotifyCollectionChangedAction.Reset, removedItems, index);
            }
        }

        public override void Add(T item)
        {
            CheckReentrancy();

            var index = _source.Count;
            _source.Add(item);

            OnPropertyChanged(countPropertyName);
            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public override bool Remove(T item)
        {
            CheckReentrancy();

            var index = _source.IndexOf(item);
            if (index >= 0)
            {
                _source.RemoveAt(index);
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionSingleChanged(NotifyCollectionChangedAction.Remove, item, index);
                return true;
            }
            else
                return false;
        }

        IList AsList()
        {
            var result = _source as IList;
            if (result == null)
                throw new NotSupportedException();
            return result;
        
        }

        int IList.Add(object value)
        {
            return AsList().Add(value);
        }

        void IList.Clear()
        {
            AsList().Clear();
        }

        bool IList.Contains(object value)
        {
            return AsList().Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return AsList().IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            AsList().Insert(index, value);
        }

        bool IList.IsFixedSize => AsList().IsFixedSize;

        bool IList.IsReadOnly => AsList().IsReadOnly;

        void IList.Remove(object value)
        {
            AsList().Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            AsList().RemoveAt(index);
        }

        object IList.this[int index]
        {
            get => AsList()[index];
            set => AsList()[index] = value;
        }
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    public class ObservableListClassDecorator<T, TCollection> : ObservableListDecorator<T, TCollection>, IObservableList<T>
        where TCollection : List<T>
    {
        public ObservableListClassDecorator(TCollection source, bool treatClearAsRemove = false)
            : base(source, treatClearAsRemove)
        {
        }

        public override void InsertRange(int index, IEnumerable<T> items)
        {
            CheckReentrancy();

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var insertedItems = items.ToList();
            _source.InsertRange(index, insertedItems);

            if (insertedItems.Count > 0)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Add, insertedItems, index);
            }
        }

        public override void RemoveRange(int index, int count)
        {
            CheckReentrancy();

            if (!(index >= 0))
                throw new ArgumentOutOfRangeException(nameof(index), Resources.IndexOutOfRange);
            if (!(index + count <= _source.Count))
                throw new ArgumentException(Resources.RangeExceedsCollectionSize, nameof(count));

            var removedItems = new List<T>(); ;
            for (var i = index + count - 1; i >= index; i--)
                removedItems.Add(_source[i]);
            _source.RemoveRange(index, count);

            if (removedItems.Count > 0)
            {
                removedItems.Reverse();
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Remove, removedItems, index);
            }
        }

        public override void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var index = _source.Count;

            var addedItems = items.ToList();
            _source.AddRange(addedItems);

            if (addedItems.Count > 0)
            {
                OnPropertyChanged(countPropertyName);
                OnPropertyChanged(indexerPropertyName);
                OnCollectionMultiChanged(NotifyCollectionChangedAction.Add, addedItems, index);
            }
        }
    }

#if !NETSTANDARD1_2
    [System.Serializable]
#endif
    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(KeyedCollectionDebugView<,>))]
    public class ObservableKeyedCollectionDecorator<TKey, TValue, TCollection> : ObservableListDecorator<TValue, TCollection>, IObservableKeyedCollection<TKey, TValue>
        where TCollection : IKeyedCollection<TKey, TValue>
    {
        public ObservableKeyedCollectionDecorator(TCollection source, bool treatClearAsRemove = false)
            : base(source, treatClearAsRemove)
        {
        }

        public TValue this[TKey key] => _source[key];

        public bool ContainsKey(TKey key)
        {
            return _source.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            CheckReentrancy();

            if (!_source.TryGetValue(key, out TValue item))
                return false;

            var index = _source.IndexOf(item);
            if (index < 0)
                throw new InvalidOperationException();
            if (!_source.Remove(key))
                throw new InvalidOperationException();

            OnPropertyChanged(countPropertyName);
            OnPropertyChanged(indexerPropertyName);
            OnCollectionSingleChanged(NotifyCollectionChangedAction.Remove, item, index);
            return true;
        }

        public ICollection<TKey> Keys => _source.Keys;

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _source.TryGetValue(key, out value);
        }
    }
}
