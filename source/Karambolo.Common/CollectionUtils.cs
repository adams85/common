using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Karambolo.Common.Collections;

namespace Karambolo.Common
{
    public sealed class NotifyCollectionChangedHandler<T>
    {
        public delegate void AddHandler(T item, int index);
        public delegate void RemoveHandler(T item, int index);
        public delegate void ReplaceHandler(T oldItem, T newItem, int index);
        public delegate void MoveHandler(T item, int oldIndex, int newIndex);

        Action _resetHandler;
        AddHandler _addHandler;
        RemoveHandler _removeHandler;
        ReplaceHandler _replaceHandler;
        MoveHandler _moveHandler;
        bool _useAddRemoveOnReplace;
        bool _useAddRemoveOnMove;

        readonly NotifyCollectionChangedEventArgs _eventArgs;

        public NotifyCollectionChangedHandler(NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            _eventArgs = eventArgs;
        }

        public NotifyCollectionChangedHandler<T> OnReset(Action handler)
        {
            _resetHandler = handler;
            return this;
        }

        public NotifyCollectionChangedHandler<T> OnAdd(AddHandler handler)
        {
            _addHandler = handler;
            return this;
        }

        public NotifyCollectionChangedHandler<T> OnRemove(RemoveHandler handler)
        {
            _removeHandler = handler;
            return this;
        }

        public NotifyCollectionChangedHandler<T> OnReplace(ReplaceHandler handler)
        {
            _replaceHandler = handler;
            return this;
        }

        public NotifyCollectionChangedHandler<T> OnMove(MoveHandler handler)
        {
            _moveHandler = handler;
            return this;
        }

        public NotifyCollectionChangedHandler<T> UseAddRemoveOnReplace()
        {
            _useAddRemoveOnReplace = true;
            return this;
        }

        public NotifyCollectionChangedHandler<T> UseAddRemoveOnMove()
        {
            _useAddRemoveOnMove = true;
            return this;
        }

        public void Handle()
        {
            switch (_eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _resetHandler?.Invoke();
                    return;
                case NotifyCollectionChangedAction.Add:
                    if (_addHandler != null)
                    {
                        var index = _eventArgs.NewStartingIndex;
                        var count = _eventArgs.NewItems?.Count ?? 0;
                        for (var i = 0; i < count; i++)
                        {
                            var item = (T)_eventArgs.NewItems[i];
                            _addHandler(item, index);
                            if (index >= 0)
                                index++;
                        };
                    }
                    return;
                case NotifyCollectionChangedAction.Remove:
                    if (_removeHandler != null)
                    {
                        var index = _eventArgs.OldStartingIndex;
                        var count = _eventArgs.OldItems?.Count ?? 0;
                        for (var i = 0; i < count; i++)
                        {
                            var item = (T)_eventArgs.OldItems[i];
                            _removeHandler(item, index);
                            if (index >= 0)
                                index++;
                        };
                    }
                    return;
                case NotifyCollectionChangedAction.Replace:
                    if (!_useAddRemoveOnReplace && _replaceHandler != null ||
                        _useAddRemoveOnReplace && _addHandler != null && _removeHandler != null)
                    {
                        var handler = !_useAddRemoveOnReplace ? _replaceHandler : (oit, nit, idx) => { _removeHandler(oit, idx); _addHandler(nit, idx); };
                        var index = _eventArgs.OldStartingIndex;
                        var count = _eventArgs.OldItems?.Count ?? 0;
                        for (var i = 0; i < count; i++)
                        {
                            var oldItem = (T)_eventArgs.OldItems[i];
                            var newItem = (T)_eventArgs.NewItems[i];
                            handler(oldItem, newItem, index);
                            if (index >= 0)
                                index++;
                        };
                    }
                    return;
                case NotifyCollectionChangedAction.Move:
                    if (!_useAddRemoveOnMove && _moveHandler != null ||
                        _useAddRemoveOnMove && _addHandler != null && _removeHandler != null)
                    {
                        var handler = !_useAddRemoveOnMove ? _moveHandler : (it, oidx, nidx) => { _removeHandler(it, oidx); _addHandler(it, nidx); };
                        var oldIndex = _eventArgs.OldStartingIndex;
                        var newIndex = _eventArgs.NewStartingIndex;
                        var count = _eventArgs.OldItems?.Count ?? 0;
                        for (var i = 0; i < count; i++)
                        {
                            var item = (T)_eventArgs.OldItems[i];
                            handler(item, oldIndex++, newIndex++);
                        };
                    }
                    return;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public static class CollectionExtensions
    {
        #region Collections
        public static IReadOnlyCollection<T> AsReadOnly<T>(this ICollection<T> source)
        {
            return new ReadOnlyCollection<T, ICollection<T>>(source);
        }

        public static IReadOnlyCollection<TDest> ConvertReadOnly<TSource, TDest>(this IReadOnlyCollection<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ReadOnlyCollectionConverter<TSource, TDest, IReadOnlyCollection<TSource>>(source, convertToSource, convertToDest);
        }

        public static ICollection<TDest> Convert<TSource, TDest>(this ICollection<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new CollectionConverter<TSource, TDest, ICollection<TSource>>(source, convertToSource, convertToDest);
        }

        public static ICollection<TDest> AsCovariant<TSource, TDest>(this ICollection<TSource> source)
            where TSource : class, TDest
        {
            return Convert<TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static ICollection<TDest> AsContravariant<TSource, TDest>(this ICollection<TSource> source)
            where TDest : class, TSource
        {
            return Convert<TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static IObservableCollection<T> AsObservable<T>(this ICollection<T> source, bool treatClearAsRemove = false)
        {
            return new ObservableCollectionDecorator<T, ICollection<T>>(source, treatClearAsRemove);
        }

        public static IObservableCollection<TDest> Convert<TSource, TDest>(this IObservableCollection<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ObservableCollectionConverter<TSource, TDest, IObservableCollection<TSource>>(source, convertToSource, convertToDest);
        }

        public static IObservableCollection<TDest> AsCovariant<TSource, TDest>(this IObservableCollection<TSource> source)
            where TSource : class, TDest
        {
            return Convert<TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IObservableCollection<TDest> AsContravariant<TSource, TDest>(this IObservableCollection<TSource> source)
            where TDest : class, TSource
        {
            return Convert<TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }
        #endregion

        #region Sets
        public static IReadOnlySet<T> AsReadOnly<T>(this ISet<T> source)
        {
            return new ReadOnlySet<T, ISet<T>>(source);
        }

        public static IReadOnlySet<TDest> ConvertReadOnly<TSource, TDest>(this IReadOnlySet<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ReadOnlySetConverter<TSource, TDest, IReadOnlySet<TSource>>(source, convertToSource, convertToDest);
        }

        public static IReadOnlySet<TDest> AsCovariantReadOnly<TSource, TDest>(this IReadOnlySet<TSource> source)
            where TSource : class, TDest
        {
            return ConvertReadOnly<TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IReadOnlySet<TDest> AsContravariantReadOnly<TSource, TDest>(this IReadOnlySet<TSource> source)
            where TDest : class, TSource
        {
            return ConvertReadOnly<TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static ISet<TDest> Convert<TSource, TDest>(this ISet<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new SetConverter<TSource, TDest, ISet<TSource>>(source, convertToSource, convertToDest);
        }

        public static ISet<TDest> AsCovariant<TSource, TDest>(this ISet<TSource> source)
            where TSource : class, TDest
        {
            return Convert<TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static ISet<TDest> AsContravariant<TSource, TDest>(this ISet<TSource> source)
            where TDest : class, TSource
        {
            return Convert<TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if (source == null)
                throw new NullReferenceException();

            return new HashSet<T>(source, comparer);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return ToHashSet(source, null);
        }
        #endregion

        #region Lists
        public static IList<T> AsReadOnly<T>(this IList<T> source)
        {
            return new System.Collections.ObjectModel.ReadOnlyCollection<T>(source);
        }

        public static IReadOnlyList<TDest> ConvertReadOnly<TSource, TDest>(this IReadOnlyList<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ReadOnlyListConverter<TSource, TDest, IReadOnlyList<TSource>>(source, convertToSource, convertToDest);
        }

        public static IList<TDest> Convert<TSource, TDest>(this IList<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ListConverter<TSource, TDest, IList<TSource>>(source, convertToSource, convertToDest);
        }

        public static IList<TDest> AsCovariant<TSource, TDest>(this IList<TSource> source)
            where TSource : class, TDest
        {
            return Convert<TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IList<TDest> AsContravariant<TSource, TDest>(this IList<TSource> source)
            where TDest : class, TSource
        {
            return Convert<TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static IObservableList<T> AsObservable<T>(this IList<T> source, bool treatClearAsRemove = false)
        {
            var list = source as List<T>;
            return
                list != null ?
                (IObservableList<T>)new ObservableListClassDecorator<T, List<T>>(list, treatClearAsRemove) :
                new ObservableListDecorator<T, IList<T>>(source, treatClearAsRemove);
        }

        public static IObservableList<TDest> Convert<TSource, TDest>(this IObservableList<TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ObservableListConverter<TSource, TDest, IObservableList<TSource>>(source, convertToSource, convertToDest);
        }

        public static IObservableList<TDest> AsCovariant<TSource, TDest>(this IObservableList<TSource> source)
            where TSource : class, TDest
        {
            return Convert<TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IObservableList<TDest> AsContravariant<TSource, TDest>(this IObservableList<TSource> source)
            where TDest : class, TSource
        {
            return Convert<TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }
        #endregion

        #region Dictionaries
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>(source);
        }

        public static IReadOnlyDictionary<TKey, TDest> ConvertReadOnly<TKey, TSource, TDest>(this IReadOnlyDictionary<TKey, TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ReadOnlyDictionaryConverter<TKey, TSource, TDest, IReadOnlyDictionary<TKey, TSource>>(source, convertToSource, convertToDest);
        }

        public static IReadOnlyDictionary<TKey, TDest> AsCovariantReadOnly<TKey, TSource, TDest>(this IReadOnlyDictionary<TKey, TSource> source)
            where TSource : class, TDest
        {
            return ConvertReadOnly<TKey, TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IReadOnlyDictionary<TKey, TDest> AsContravariantReadOnly<TKey, TSource, TDest>(this IReadOnlyDictionary<TKey, TSource> source)
            where TDest : class, TSource
        {
            return ConvertReadOnly<TKey, TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static IDictionary<TKey, TDest> Convert<TKey, TSource, TDest>(this IDictionary<TKey, TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new DictionaryConverter<TKey, TSource, TDest, IDictionary<TKey, TSource>>(source, convertToSource, convertToDest);
        }

        public static IDictionary<TKey, TDest> AsCovariant<TKey, TSource, TDest>(this IDictionary<TKey, TSource> source)
            where TSource : class, TDest
        {
            return Convert<TKey, TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IDictionary<TKey, TDest> AsContravariant<TKey, TSource, TDest>(this IDictionary<TKey, TSource> source)
            where TDest : class, TSource
        {
            return Convert<TKey, TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static IReadOnlyOrderedDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IOrderedDictionary<TKey, TValue> source)
        {
            return new ReadOnlyOrderedDictionary<TKey, TValue, IOrderedDictionary<TKey, TValue>>(source);
        }

        public static IOrderedDictionary<TKey, TDest> Convert<TKey, TSource, TDest>(this IOrderedDictionary<TKey, TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new OrderedDictionaryConverter<TKey, TSource, TDest, IOrderedDictionary<TKey, TSource>>(source, convertToSource, convertToDest);
        }

        public static IOrderedDictionary<TKey, TDest> AsCovariant<TKey, TSource, TDest>(this IOrderedDictionary<TKey, TSource> source)
            where TSource : class, TDest
        {
            return Convert<TKey, TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IOrderedDictionary<TKey, TDest> AsContravariant<TKey, TSource, TDest>(this IOrderedDictionary<TKey, TSource> source)
            where TDest : class, TSource
        {
            return Convert<TKey, TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<T, TKey, TElement>(this IEnumerable<T> source,
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
                throw new NullReferenceException();

            var result = new OrderedDictionary<TKey, TElement>(comparer);
            foreach (var item in source)
                result.Add(keySelector(item), elementSelector(item));

            return result;
        }

        public static OrderedDictionary<TKey, TElement> ToOrderedDictionary<T, TKey, TElement>(this IEnumerable<T> source,
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
        {
            return ToOrderedDictionary(source, keySelector, elementSelector, null);
        }

        public static OrderedDictionary<TKey, T> ToOrderedDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return ToOrderedDictionary(source, keySelector, Identity<T>.Func, comparer);
        }

        public static OrderedDictionary<TKey, T> ToOrderedDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            return ToOrderedDictionary(source, keySelector, Identity<T>.Func, null);
        }
        #endregion

        #region Keyed Collections
        public static IReadOnlyKeyedCollection<TKey, TValue> AsReadOnly<TKey, TValue>(this IKeyedCollection<TKey, TValue> source)
        {
            return new ReadOnlyKeyedCollection<TKey, TValue, IKeyedCollection<TKey, TValue>>(source);
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> AsKeyValuePairs<TKey, TValue>(this IReadOnlyKeyedCollection<TKey, TValue> source)
        {
            var count = source.Count;
            foreach (var key in source.Keys)
                yield return new KeyValuePair<TKey, TValue>(key, source[key]);
        }

        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IReadOnlyKeyedCollection<TKey, TValue> source)
        {
            return source.Keys.ToDictionary(Identity<TKey>.Func, key => source[key]);
        }

        public static IKeyedCollection<TKey, TDest> Convert<TKey, TSource, TDest>(this IKeyedCollection<TKey, TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new KeyedCollectionConverter<TKey, TSource, TDest, IKeyedCollection<TKey, TSource>>(source, convertToSource, convertToDest);
        }

        public static IKeyedCollection<TKey, TDest> AsCovariant<TKey, TSource, TDest>(this IKeyedCollection<TKey, TSource> source)
            where TSource : class, TDest
        {
            return Convert<TKey, TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IKeyedCollection<TKey, TDest> AsContravariant<TKey, TSource, TDest>(this IKeyedCollection<TKey, TSource> source)
            where TDest : class, TSource
        {
            return Convert<TKey, TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static IObservableKeyedCollection<TKey, TValue> AsObservable<TKey, TValue>(this IKeyedCollection<TKey, TValue> source, bool treatClearAsRemove = false)
        {
            return new ObservableKeyedCollectionDecorator<TKey, TValue, IKeyedCollection<TKey, TValue>>(source, treatClearAsRemove);
        }

        public static IReadOnlyObservableKeyedCollection<TKey, TValue> AsReadOnly<TKey, TValue>(this IObservableKeyedCollection<TKey, TValue> source)
        {
            return new ReadOnlyObservableKeyedCollection<TKey, TValue, IObservableKeyedCollection<TKey, TValue>>(source);
        }

        public static IObservableKeyedCollection<TKey, TDest> Convert<TKey, TSource, TDest>(this IObservableKeyedCollection<TKey, TSource> source, Func<TDest, TSource> convertToSource, Func<TSource, TDest> convertToDest)
        {
            return new ObservableKeyedCollectionConverter<TKey, TSource, TDest, IObservableKeyedCollection<TKey, TSource>>(source, convertToSource, convertToDest);
        }

        public static IObservableKeyedCollection<TKey, TDest> AsCovariant<TKey, TSource, TDest>(this IObservableKeyedCollection<TKey, TSource> source)
            where TSource : class, TDest
        {
            return Convert<TKey, TSource, TDest>(source, d => (TSource)d, Identity<TDest>.Func);
        }

        public static IObservableKeyedCollection<TKey, TDest> AsContravariant<TKey, TSource, TDest>(this IObservableKeyedCollection<TKey, TSource> source)
            where TDest : class, TSource
        {
            return Convert<TKey, TSource, TDest>(source, Identity<TSource>.Func, s => (TDest)s);
        }

        public static GenericKeyedCollection<TKey, TElement> ToKeyedCollection<T, TKey, TElement>(this IEnumerable<T> source,
            Func<TElement, TKey> keyFromValueSelector, Func<T, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null, int dictionaryCreationThreshold = 0)
        {
            if (source == null)
                throw new NullReferenceException();

            var result = new GenericKeyedCollection<TKey, TElement>(keyFromValueSelector, comparer, dictionaryCreationThreshold);
            foreach (var item in source)
                result.Add(elementSelector(item));

            return result;
        }

        public static GenericKeyedCollection<TKey, T> ToKeyedCollection<TKey, T>(this IEnumerable<T> source, Func<T, TKey> keyFromValueSelector,
            IEqualityComparer<TKey> comparer = null, int dictionaryCreationThreshold = 0)
        {
            if (source == null)
                throw new NullReferenceException();

            var result = new GenericKeyedCollection<TKey, T>(keyFromValueSelector, comparer, dictionaryCreationThreshold);
            foreach (var item in source)
                result.Add(item);

            return result;
        }
        #endregion

        public static NotifyCollectionChangedHandler<T> AsHandler<T>(this NotifyCollectionChangedEventArgs eventArgs)
        {
            return new NotifyCollectionChangedHandler<T>(eventArgs);
        }
    }
}
