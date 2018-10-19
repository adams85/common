using System;
using System.Collections.Specialized;
using System.Linq;

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
}
