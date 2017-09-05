using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using Karambolo.Common.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Karambolo.Common.Test
{


    /// <summary>
    ///This is a test class for CollectionUtilsTest and is intended
    ///to contain all CollectionUtilsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CollectionUtilsTest
    {


        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get => _testContextInstance;
            set => _testContextInstance = value;
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
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
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 13 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12, 13 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.Add(13);
            Assert.AreEqual(fireCount, 0);

            // multi add
            fireCount = 0;
            args = null;
            observable.AddRange(new [] { 5, 14 });
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 14 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12, 13, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.AddRange(new[] { 5, 14 });
            Assert.AreEqual(fireCount, 0);

            fireCount = 0;
            observable.AddRange(Enumerable.Empty<int>());
            Assert.AreEqual(fireCount, 0);

            // single remove
            fireCount = 0;
            args = null;
            Assert.IsTrue(observable.Remove(13));
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 13 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            Assert.IsFalse(observable.Remove(13));
            Assert.AreEqual(fireCount, 0);

            // multi remove
            fireCount = 0;
            args = null;
            observable.RemoveRange(new [] { 5, 0, 8 });
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 5, 8 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveRange(new[] { 5, 0, 8 });
            Assert.AreEqual(fireCount, 0);

            fireCount = 0;
            observable.RemoveRange(Enumerable.Empty<int>());
            Assert.AreEqual(fireCount, 0);

            // single replace
            fireCount = 0;
            args = null;
            observable.Replace(1, 2);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.IsTrue(new[] { 1 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsTrue(new[] { 2 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 2, 10, 12, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            args = null;
            try
            {
                observable.Replace(1, 2);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                Assert.AreEqual(fireCount, 0);
                Assert.IsTrue(new[] { 2, 10, 12, 14 }.SequenceEqualUnordered(observable));
            }

            fireCount = 0;
            args = null;
            try
            {
                observable.Replace(2, 10);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                Assert.AreEqual(fireCount, 0);
                Assert.IsTrue(new[] { 2, 10, 12, 14 }.SequenceEqualUnordered(observable));
            }

            // clear
            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            Assert.IsTrue(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.AreEqual(fireCount, 0);

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
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.AreEqual(fireCount, 0);

            // reset section
            fireCount = 0;
            args = null;
            using (observable.ResetSection())
            {
                observable.AddRange(new[] { 5, 6, 7 });
                observable.Remove(7);
                observable.Replace(5, 8);
            }
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 6, 8 }.SequenceEqualUnordered(observable));
        }

        [TestMethod()]
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
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 13 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(5, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12, 13 }.SequenceEqualUnordered(observable));

            // multi add
            fireCount = 0;
            args = null;
            observable.AddRange(new[] { 5, 14 });
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 5, 14 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(6, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.AddRange(Enumerable.Empty<int>());
            Assert.AreEqual(fireCount, 0);

            // single insert
            fireCount = 0;
            args = null;
            observable.Insert(2, 20);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(2, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            // multi insert
            fireCount = 0;
            args = null;
            observable.InsertRange(2, new[] { 5, 0 });
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 5, 0 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(2, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 5, 0, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.InsertRange(3, Enumerable.Empty<int>());
            Assert.AreEqual(fireCount, 0);

            // single remove
            fireCount = 0;
            args = null;
            Assert.IsTrue(observable.Remove(5));
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 5 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 5, 0, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            Assert.IsFalse(observable.Remove(30));
            Assert.AreEqual(0, fireCount);
            
            // single remove by index
            fireCount = 0;
            args = null;
            observable.RemoveAt(1);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 5 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 1, 0, 20, 8, 10, 12, 13, 5, 14 }.SequenceEqualUnordered(observable));

            // multi remove
            fireCount = 0;
            args = null;
            observable.RemoveRange(new [] { 1, 14 });
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 1, 14 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 0, 20, 8, 10, 12, 13, 5 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveRange(new[] { 30, 31 });
            Assert.AreEqual(0, fireCount);

            fireCount = 0;
            observable.RemoveRange(Enumerable.Empty<int>());
            Assert.AreEqual(0, fireCount);

            // multi remove by index and count
            fireCount = 0;
            args = null;
            observable.RemoveRange(5, 2);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 13, 5 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(5, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 0, 20, 8, 10, 12 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveRange(2, 0);
            Assert.AreEqual(0, fireCount);

            // multi remove by predicate
            fireCount = 0;
            args = null;
            observable.RemoveAll(it => it <= 8);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 0, 8 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 20, 10, 12 }.SequenceEqualUnordered(observable));

            fireCount = 0;
            observable.RemoveAll(it => it < 0);
            Assert.AreEqual(0, fireCount);

            // replace by index
            fireCount = 0;
            args = null;
            observable.ReplaceAt(1, 20);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.IsTrue(new[] { 10 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(1, args.OldStartingIndex);
            Assert.AreEqual(1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 20, 20, 12 }.SequenceEqualUnordered(observable));

            // replace
            fireCount = 0;
            args = null;
            observable.Replace(20, 10);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsTrue(new[] { 10 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(0, args.OldStartingIndex);
            Assert.AreEqual(0, args.NewStartingIndex);
            Assert.IsTrue(new[] { 10, 20, 12 }.SequenceEqualUnordered(observable));

            // move
            fireCount = 0;
            args = null;
            observable.Move(20, 2);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Move, args.Action);
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(1, args.OldStartingIndex);
            Assert.AreEqual(2, args.NewStartingIndex);
            Assert.IsTrue(new[] { 10, 12, 20 }.SequenceEqualUnordered(observable));

            // move by index
            fireCount = 0;
            args = null;
            observable.MoveAt(2, 0);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Move, args.Action);
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsTrue(new[] { 20 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(2, args.OldStartingIndex);
            Assert.AreEqual(0, args.NewStartingIndex);
            Assert.IsTrue(new[] { 20, 10, 12 }.SequenceEqualUnordered(observable));

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
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 5, 14 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(3, args.NewStartingIndex);
            Assert.IsTrue(new[] { 15, 4, 3, 5, 14 }.SequenceEqualUnordered(observable));

            // multi insert
            fireCount = 0;
            args = null;
            observable.InsertRange(1, new[] { 6, 7 });
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            Assert.IsNull(args.OldItems);
            Assert.IsTrue(new[] { 6, 7 }.SequenceEqual(args.NewItems.Cast<int>()));
            Assert.AreEqual(-1, args.OldStartingIndex);
            Assert.AreEqual(1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 15, 6, 7, 4, 3, 5, 14 }.SequenceEqualUnordered(observable));

            // multi remove by index and count
            fireCount = 0;
            args = null;
            observable.RemoveRange(2, 3);
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 7, 4, 3 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(2, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { 15, 6, 5, 14 }.SequenceEqualUnordered(observable));

            // clear
            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            Assert.IsTrue(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.AreEqual(fireCount, 0);

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
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { 1, 5, 8, 10, 12 }.SequenceEqual(args.OldItems.Cast<int>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(0, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(!observable.Any());

            fireCount = 0;
            args = null;
            observable.Clear();
            Assert.AreEqual(fireCount, 0);
        }

        [TestMethod()]
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
            Assert.IsTrue(observable.Remove(4));
            Assert.AreEqual(fireCount, 1);
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
            Assert.IsTrue(new[] { "accc" }.SequenceEqual(args.OldItems.Cast<string>()));
            Assert.IsNull(args.NewItems);
            Assert.AreEqual(1, args.OldStartingIndex);
            Assert.AreEqual(-1, args.NewStartingIndex);
            Assert.IsTrue(new[] { "c", "def" }.SequenceEqualUnordered(observable));
        }
    }
}
