using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using Karambolo.Common.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Karambolo.Common.Test
{
    public class CollectionUtilsTest
    {
        [Fact]
        public void AsObservableCollectionTest()
        {
            var observable = new HashSet<int> { 1, 5, 8, 10, 12 }.AsObservable();
            var fireCount = 0;
            NotifyCollectionChangedEventArgs args = null;
            observable.CollectionChanged += (s, e) =>
            {
                fireCount++;
                args = e;
            };

            // single add
            observable.Add(13);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 13 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 8, 10, 12, 13 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.Add(13);
            Assert.Equal(0, fireCount);

            // multi add
            fireCount = 0;
            args = null;
            observable.AddRange(new [] { 5, 14 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 14 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 8, 10, 12, 13, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.AddRange(new[] { 5, 14 });
            Assert.Equal(0, fireCount);

            fireCount = 0;
            observable.AddRange(Enumerable.Empty<int>());
            Assert.Equal(0, fireCount);

            // single remove
            fireCount = 0;
            args = null;
            Assert.True(observable.Remove(13));
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 13 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 8, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            Assert.False(observable.Remove(13));
            Assert.Equal(0, fireCount);

            // multi remove
            fireCount = 0;
            args = null;
            observable.RemoveRange(new [] { 5, 0, 8 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 5, 8 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 1, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveRange(new[] { 5, 0, 8 });
            Assert.Equal(0, fireCount);

            fireCount = 0;
            observable.RemoveRange(Enumerable.Empty<int>());
            Assert.Equal(0, fireCount);

            // single replace
            fireCount = 0;
            args = null;
            observable.Replace(1, 2);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.True(new[] { 1 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.True(new[] { 2 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 2, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            args = null;
            Assert.Throws<ArgumentException>(() => observable.Replace(1, 2));
            Assert.Equal(0, fireCount);
            Assert.True(new[] { 2, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            args = null;
            Assert.Throws<ArgumentException>(() => observable.Replace(2, 10));
            Assert.Equal(0, fireCount);
            Assert.True(new[] { 2, 10, 12, 14 }.SequenceEqualUnordered(observable));

            // clear
            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
            Assert.True(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.Equal(0, fireCount);

            // clear (treated as remove)
            observable = new HashSet<int> { 1, 5, 8, 10, 12 }.AsObservable(true);
            fireCount = 0;
            args = null;
            observable.CollectionChanged += (s, e) =>
            {
                fireCount++;
                args = e;
            };

            observable.Clear();
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 1, 5, 8, 10, 12 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.Equal(0, fireCount);

            // reset section
            fireCount = 0;
            args = null;
            using (observable.ResetSection())
            {
                observable.AddRange(new[] { 5, 6, 7 });
                observable.Remove(7);
                observable.Replace(5, 8);
            }
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
            Assert.Null(args.OldItems);
            Assert.Null(args.NewItems);
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 6, 8 }.SequenceEqualUnordered(observable));
        }

        [Fact]
        public void AsObservableListTest()
        {
            var observable = new List<int> { 1, 5, 8, 10, 12 }.AsObservable();
            var fireCount = 0;
            NotifyCollectionChangedEventArgs args = null;
            observable.CollectionChanged += (s, e) =>
            {
                fireCount++;
                args = e;
            };

            // single add
            observable.Add(13);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 13 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(5, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 8, 10, 12, 13 }.SequenceEqualUnordered(observable));

            // multi add
            fireCount = 0;
            args = null;
            observable.AddRange(new[] { 5, 14 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 5, 14 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(6, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.AddRange(Enumerable.Empty<int>());
            Assert.Equal(0, fireCount);

            // single insert
            fireCount = 0;
            args = null;
            observable.Insert(2, 20);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(2, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            // multi insert
            fireCount = 0;
            args = null;
            observable.InsertRange(2, new[] { 5, 0 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 5, 0 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(2, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 5, 0, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.InsertRange(3, Enumerable.Empty<int>());
            Assert.Equal(0, fireCount);

            // single remove
            fireCount = 0;
            args = null;
            Assert.True(observable.Remove(5));
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 5 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 1, 5, 0, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            Assert.False(observable.Remove(30));
            Assert.Equal(0, fireCount);
            
            // single remove by index
            fireCount = 0;
            args = null;
            observable.RemoveAt(1);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 5 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 1, 0, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            // multi remove
            fireCount = 0;
            args = null;
            observable.RemoveRange(new [] { 1, 14 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 1, 14 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 0, 20, 8, 10, 12, 13, 5 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveRange(new[] { 30, 31 });
            Assert.Equal(0, fireCount);

            fireCount = 0;
            observable.RemoveRange(Enumerable.Empty<int>());
            Assert.Equal(0, fireCount);

            // multi remove by index and count
            fireCount = 0;
            args = null;
            observable.RemoveRange(5, 2);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 13, 5 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(5, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 0, 20, 8, 10, 12 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveRange(2, 0);
            Assert.Equal(0, fireCount);

            // multi remove by predicate
            fireCount = 0;
            args = null;
            observable.RemoveAll(it => it <= 8);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 0, 8 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 20, 10, 12 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveAll(it => it < 0);
            Assert.Equal(0, fireCount);

            // replace by index
            fireCount = 0;
            args = null;
            observable.ReplaceAt(1, 20);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.True(new[] { 10 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.True(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(1, args.OldStartingIndex);
            Assert.Equal(1, args.NewStartingIndex);
            Assert.True(new[] { 20, 20, 12 }.SequenceEqualUnordered(observable));

            // replace
            fireCount = 0;
            args = null;
            observable.Replace(20, 10);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.True(new[] { 20 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.True(new[] { 10 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(0, args.OldStartingIndex);
            Assert.Equal(0, args.NewStartingIndex);
            Assert.True(new[] { 10, 20, 12 }.SequenceEqualUnordered(observable));

            // move
            fireCount = 0;
            args = null;
            observable.Move(20, 2);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Move, args.Action);
            Assert.True(new[] { 20 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.True(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(1, args.OldStartingIndex);
            Assert.Equal(2, args.NewStartingIndex);
            Assert.True(new[] { 10, 12, 20 }.SequenceEqualUnordered(observable));

            // move by index
            fireCount = 0;
            args = null;
            observable.MoveAt(2, 0);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Move, args.Action);
            Assert.True(new[] { 20 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.True(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(2, args.OldStartingIndex);
            Assert.Equal(0, args.NewStartingIndex);
            Assert.True(new[] { 20, 10, 12 }.SequenceEqualUnordered(observable));

            // non-list source
            observable = new Collection<int> { 15, 4, 3 }.AsObservable();
            observable.CollectionChanged += (s, e) =>
            {
                fireCount++;
                args = e;
            };

            // multi add
            fireCount = 0;
            args = null;
            observable.AddRange(new[] { 5, 14 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 5, 14 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(3, args.NewStartingIndex);
            Assert.True(new[] { 15, 4, 3, 5, 14 }.SequenceEqualUnordered(observable));

            // multi insert
            fireCount = 0;
            args = null;
            observable.InsertRange(1, new[] { 6, 7 });
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
            Assert.Null(args.OldItems);
            Assert.True(new[] { 6, 7 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.Equal(-1, args.OldStartingIndex);
            Assert.Equal(1, args.NewStartingIndex);
            Assert.True(new[] { 15, 6, 7, 4, 3, 5, 14 }.SequenceEqualUnordered(observable));

            // multi remove by index and count
            fireCount = 0;
            args = null;
            observable.RemoveRange(2, 3);
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 7, 4, 3 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(2, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { 15, 6, 5, 14 }.SequenceEqualUnordered(observable));

            // clear
            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
            Assert.True(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.Equal(0, fireCount);

            // clear (treated as remove)
            observable = new List<int> { 1, 5, 8, 10, 12 }.AsObservable(true);
            fireCount = 0;
            args = null;
            observable.CollectionChanged += (s, e) =>
            {
                fireCount++;
                args = e;
            };

            observable.Clear();
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { 1, 5, 8, 10, 12 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.Null(args.NewItems);
            Assert.Equal(0, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.Equal(0, fireCount);
        }

        [Fact]
        public void AsObservableKeyedCollectionTest()
        {
            var observable = new GenericKeyedCollection<int, string>(s => s.Length) { "c", "accc", "def" }.AsObservable();
            var fireCount = 0;
            NotifyCollectionChangedEventArgs args = null;
            observable.CollectionChanged += (s, e) =>
            {
                fireCount++;
                args = e;
            };

            // single remove by key
            fireCount = 0;
            args = null;
            Assert.True(observable.Remove(4));
            Assert.Equal(1, fireCount);
            Assert.Equal(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.True(new[] { "accc" }.SequenceEqual(args.OldItems.Cast<string>()));
            Assert.Null(args.NewItems);
            Assert.Equal(1, args.OldStartingIndex);
            Assert.Equal(-1, args.NewStartingIndex);
            Assert.True(new[] { "c", "def" }.SequenceEqualUnordered(observable));
        }
    }
}
