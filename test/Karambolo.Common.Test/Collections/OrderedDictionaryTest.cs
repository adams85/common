using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Karambolo.Common.Collections
{
    public class OrderedDictionaryTest
    {
        [Fact]
        public void Ctors()
        {
            var dic = new OrderedDictionary<string, int>();
            dic["A"] = 1;
            dic["a"] = 2;
            Assert.Equal(2, dic.Count);
            Assert.Equal(1, dic["A"]);
            Assert.Equal(2, dic["a"]);
            Assert.True(dic.ContainsKey("A"));
            Assert.True(dic.ContainsKey("a"));
            Assert.True(dic.ContainsKey("a"));
            Assert.True(dic.Contains(new KeyValuePair<string, int>("A", 1)));
            Assert.True(dic.Contains(new KeyValuePair<string, int>("a", 2)));

            var dic2 = new OrderedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dic2["A"] = 1;
            dic2["a"] = 2;
            Assert.Equal(1, dic2.Count);
            Assert.Equal(2, dic2["A"]);
            Assert.Equal(2, dic2["a"]);
            Assert.True(dic2.ContainsKey("A"));
            Assert.True(dic2.ContainsKey("a"));
            Assert.True(dic2.Contains(new KeyValuePair<string, int>("A", 2)));
            Assert.True(dic2.Contains(new KeyValuePair<string, int>("a", 2)));

            dic2 = new OrderedDictionary<string, int>(dic);
            Assert.Equal(2, dic2.Count);
            Assert.Equal(1, dic2["A"]);
            Assert.Equal(2, dic2["a"]);
            Assert.True(dic2.ContainsKey("A"));
            Assert.True(dic2.ContainsKey("a"));
            Assert.True(dic2.Contains(new KeyValuePair<string, int>("A", 1)));
            Assert.True(dic2.Contains(new KeyValuePair<string, int>("a", 2)));

            Assert.Throws<ArgumentException>(() => new OrderedDictionary<string, int>(dic, StringComparer.OrdinalIgnoreCase));
        }

        [Fact]
        public void RetainsOrder()
        {
            var dic = new OrderedDictionary<int, string>();
            IDictionary<int, string> dicIntf = dic;
            IOrderedDictionary dicNonGeneric = dic;

            dic.Add(3, "three");
            dic.Add(2, "two");
            dic.Add(1, "one");
            dicIntf.Add(new KeyValuePair<int, string>(0, "zero"));

            dic.Remove(2);
            dicIntf.Add(5, "five");

            Assert.Equal(new[]
            {
                new KeyValuePair<int, string>(3, "three"),
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(0, "zero"),
                new KeyValuePair<int, string>(5, "five")
            }, dic);

            Assert.Equal(new[]
            {
                new KeyValuePair<int, string>(3, "three"),
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(0, "zero"),
                new KeyValuePair<int, string>(5, "five")
            }, dicNonGeneric.Cast<KeyValuePair<int, string>>());

            var entries = new List<object>();
            var enumerator = dicNonGeneric.GetEnumerator();
            while (enumerator.MoveNext())
                entries.Add(enumerator.Current);

            entries = new List<object>();
            enumerator = ((IDictionary)dic).GetEnumerator();
            while (enumerator.MoveNext())
                entries.Add(enumerator.Current);

            enumerator.Reset();
            enumerator.MoveNext();
            Assert.Equal(3, enumerator.Key);
            Assert.Equal("three", enumerator.Value);

            Assert.Equal(new object[]
            {
                new DictionaryEntry(3, "three"),
                new DictionaryEntry(1, "one"),
                new DictionaryEntry(0, "zero"),
                new DictionaryEntry(5, "five")
            }, entries);

            Assert.Equal(new[] { 3, 1, 0, 5 }, dic.Keys);
            Assert.Equal(new[] { 3, 1, 0, 5 }, dicNonGeneric.Keys);

            Assert.Equal(new[] { "three", "one", "zero", "five" }, dic.Values);
            Assert.Equal(new[] { "three", "one", "zero", "five" }, dicNonGeneric.Values);
        }

        [Fact]
        public void Querying()
        {
            var dic = new OrderedDictionary<int, string>()
            {
                { 3, "three" },
                { 2, "two" },
                { 1, "one" },
                { 4, "four" },
            };

            IDictionary<int, string> dicIntf = dic;
            IOrderedDictionary dicNonGeneric = dic;
            IReadOnlyOrderedDictionary<int, string> readOnlyDic = dic;

            Assert.Equal("one", dic[2]);
            Assert.Throws<ArgumentOutOfRangeException>(() => dic[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => dic[dic.Count]);

            Assert.Equal("two", dicIntf[2]);
            Assert.Throws<KeyNotFoundException>(() => dicIntf[0]);

            Assert.True(dic.TryGetValue(2, out var value));
            Assert.Equal("two", value);
            Assert.False(dic.TryGetValue(0, out value));
            Assert.Equal(null, value);

            Assert.True(dic.ContainsKey(2));
            Assert.False(dic.ContainsKey(0));
            Assert.True(dic.Keys.Contains(2));
            Assert.False(dic.Keys.Contains(0));
            Assert.True(readOnlyDic.Keys.Contains(2));
            Assert.False(readOnlyDic.Keys.Contains(0));

            Assert.True(dicNonGeneric.Contains(2));
            Assert.False(dicNonGeneric.Contains(0));

            Assert.True(dic.ContainsValue("two"));
            Assert.False(dic.ContainsValue("Two"));
            Assert.True(dic.Values.Contains("two"));
            Assert.False(dic.Values.Contains("Two"));
            Assert.True(readOnlyDic.Values.Contains("two"));
            Assert.False(readOnlyDic.Values.Contains("Two"));

            Assert.Equal(1, dic.GetKeyAt(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => dic.GetKeyAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => dic.GetKeyAt(dic.Count));

            Assert.Equal(2, dic.IndexOfKey(1));
            Assert.Equal(-1, dic.IndexOfKey(0));

            var array = new KeyValuePair<int, string>[dic.Count + 2];
            dicIntf.CopyTo(array, 1);
            Assert.Equal(new[] {
                default,
                new KeyValuePair<int, string>(3, "three"),
                new KeyValuePair<int, string>(2, "two"),
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(4, "four"),
                default,
            }, array);

            var array2 = new string[dic.Count + 2];
            dicIntf.Values.CopyTo(array2, 1);
            Assert.Equal(new[] {
                default,
                 "three",
                "two",
                "one",
                "four",
                default
            }, array2);

            array = new KeyValuePair<int, string>[dic.Count + 2];
            dicNonGeneric.CopyTo(array, 1);
            Assert.Equal(new[] {
                default,
                new KeyValuePair<int, string>(3, "three"),
                new KeyValuePair<int, string>(2, "two"),
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(4, "four"),
                default,
            }, array);

            var array3 = new DictionaryEntry[dic.Count + 2];
            dicNonGeneric.CopyTo(array3, 1);
            Assert.Equal(new[] {
                default,
                new DictionaryEntry(3, "three"),
                new DictionaryEntry(2, "two"),
                new DictionaryEntry(1, "one"),
                new DictionaryEntry(4, "four"),
                default,
            }, array3);

            var array4 = new object[dic.Count + 2];
            dicNonGeneric.CopyTo(array4, 1);
            Assert.Equal(new object[] {
                default,
                new KeyValuePair<int, string>(3, "three"),
                new KeyValuePair<int, string>(2, "two"),
                new KeyValuePair<int, string>(1, "one"),
                new KeyValuePair<int, string>(4, "four"),
                default,
            }, array4);


            var array5 = new string[dic.Count + 2];
            ((ICollection)dic.Values).CopyTo(array5, 1);
            Assert.Equal(new[] {
                default,
                "three",
                "two",
                "one",
                "four",
                default,
            }, array5);

            array4 = new object[dic.Count];
            ((ICollection)dic.Values).CopyTo(array4, 0);
            Assert.Equal(new object[] {
                "three",
                "two",
                "one",
                "four",
            }, array4);

            Assert.Throws<ArgumentException>(() => dicNonGeneric.CopyTo(new (int, string)[dic.Count + 2], 1));

            Assert.False(dic.IsReadOnly);
            Assert.False(dicNonGeneric.IsFixedSize);
            Assert.False(dicNonGeneric.IsSynchronized);
            Assert.NotNull(dicNonGeneric.SyncRoot);
        }

        [Fact]
        public void Modifying()
        {
            var dic = new OrderedDictionary<int, string>()
            {
                { 3, "" },
                { 2, "two" },
                { 1, "one" },
            };

            IDictionary<int, string> dicIntf = dic;
            IOrderedDictionary dicNonGeneric = dic;

            dicIntf[0] = "0";
            dicIntf[3] = "3";

            Assert.Equal(4, dic.Count);
            Assert.Equal(new[] { (3, "3"), (2, "two"), (1, "one"), (0, "0") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dic[0] = "three";
            dic[3] = "zero";
            Assert.Throws<ArgumentOutOfRangeException>(() => dic[-1] = "");
            Assert.Throws<ArgumentOutOfRangeException>(() => dic[dic.Count] = "");

            Assert.Equal(4, dic.Count);
            Assert.Equal(new[] { (3, "three"), (2, "two"), (1, "one"), (0, "zero") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dic.Add(5, "five");

            Assert.Equal(5, dic.Count);
            Assert.Equal(new[] { (3, "three"), (2, "two"), (1, "one"), (0, "zero"), (5, "five") }, dic.Select(kvp => (kvp.Key, kvp.Value)));
            Assert.Throws<ArgumentException>(() => dic.Add(5, "five"));

            Assert.False(dic.Remove(-1));
            Assert.True(dic.Remove(2));

            Assert.Equal(4, dic.Count);
            Assert.Equal(new[] { (3, "three"), (1, "one"), (0, "zero"), (5, "five") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dic.Insert(0, 2, "two");
            dic.Insert(dic.Count, 6, "six");
            Assert.Throws<ArgumentException>(() => dic.Insert(0, 2, "two"));
            Assert.Throws<ArgumentOutOfRangeException>(() => dic.Insert(-1, -1, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => dic.Insert(-1, dic.Count + 1, ""));

            Assert.Equal(6, dic.Count);
            Assert.Equal(new[] { (2, "two"), (3, "three"), (1, "one"), (0, "zero"), (5, "five"), (6, "six") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dic.RemoveAt(2);
            dic.RemoveAt(dic.Count - 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => dic.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => dic.RemoveAt(dic.Count));

            Assert.Equal(4, dic.Count);
            Assert.Equal(new[] { (2, "two"), (3, "three"), (0, "zero"), (5, "five") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dic.Clear();
            Assert.Equal(0, dic.Count);
            Assert.Equal(new (int, string)[] { }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric.Add(0, "0");
            Assert.Throws<ArgumentException>(() => dicNonGeneric.Add("0", "0"));
            Assert.Throws<ArgumentException>(() => dicNonGeneric.Add(0, 0));

            Assert.Equal(1, dic.Count);
            Assert.Equal(new[] { (0, "0") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric[0] = "zero";
            Assert.Throws<ArgumentOutOfRangeException>(() => dicNonGeneric[-1] = "");
            Assert.Throws<ArgumentOutOfRangeException>(() => dicNonGeneric[dicNonGeneric.Count] = "");

            Assert.Equal(1, dic.Count);
            Assert.Equal(new[] { (0, "zero") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric[(object)1] = "1";
            dicNonGeneric[(object)1] = "one";
            dicNonGeneric[(object)2] = "two";
            Assert.Throws<ArgumentException>(() => dicNonGeneric["0"] = "0");
            Assert.Throws<ArgumentException>(() => dicNonGeneric[(object)0] = 0);

            Assert.Equal(3, dic.Count);
            Assert.Equal(new[] { (0, "zero"), (1, "one"), (2, "two") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric.Insert(0, -1, "minus one");
            dicNonGeneric.Insert(4, 3, "three");
            Assert.Throws<ArgumentOutOfRangeException>(() => dicNonGeneric.Insert(-1, -1, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => dicNonGeneric.Insert(dicNonGeneric.Count + 1, -1, ""));
            Assert.Throws<ArgumentException>(() => dicNonGeneric.Insert(0, "0", "0"));
            Assert.Throws<ArgumentException>(() => dicNonGeneric.Insert(0, 0, 0));

            Assert.Equal(5, dic.Count);
            Assert.Equal(new[] { (-1, "minus one"), (0, "zero"), (1, "one"), (2, "two"), (3, "three") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric.Remove(-1);
            dicNonGeneric.Remove(-2);
            Assert.Throws<ArgumentException>(() => dicNonGeneric.Remove("0"));

            Assert.Equal(4, dic.Count);
            Assert.Equal(new[] { (0, "zero"), (1, "one"), (2, "two"), (3, "three") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric.RemoveAt(3);
            Assert.Throws<ArgumentOutOfRangeException>(() => dicNonGeneric.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => dicNonGeneric.RemoveAt(3));

            Assert.Equal(3, dic.Count);
            Assert.Equal(new[] { (0, "zero"), (1, "one"), (2, "two") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            Assert.False(dicIntf.Remove(new KeyValuePair<int, string>(0, "Tero")));
            Assert.True(dicIntf.Remove(new KeyValuePair<int, string>(0, "zero")));
            Assert.False(dicIntf.Remove(new KeyValuePair<int, string>(0, "one")));

            Assert.Equal(2, dic.Count);
            Assert.Equal(new[] { (1, "one"), (2, "two") }, dic.Select(kvp => (kvp.Key, kvp.Value)));

            dicNonGeneric.Clear();
            Assert.Equal(0, dic.Count);
            Assert.Equal(new (int, string)[] { }, dic.Select(kvp => (kvp.Key, kvp.Value)));
        }

        [Fact]
        public void PreventsModificationDuringEnumeration()
        {
            var dic = new OrderedDictionary<int, string>()
            {
                { 3, "three" },
                { 2, "two" },
                { 1, "one" },
                { 4, "four" },
            };
            IDictionary<int, string> dicIntf = dic;
            IOrderedDictionary dicNonGeneric = dic;


            #region Default enumerator

            using (var enumerator = dic.GetEnumerator())
            {
                enumerator.MoveNext();
                dic.Remove(1);
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.GetEnumerator())
            {
                enumerator.MoveNext();
                dicIntf[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.GetEnumerator())
            {
                enumerator.MoveNext();
                dicIntf[1] = "1";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.GetEnumerator())
            {
                enumerator.MoveNext();
                dic[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            #endregion

            #region Keys

            using (var enumerator = dic.Keys.GetEnumerator())
            {
                enumerator.MoveNext();
                dic.Remove(1);
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.Keys.GetEnumerator())
            {
                enumerator.MoveNext();
                dicIntf[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.Keys.GetEnumerator())
            {
                enumerator.MoveNext();
                dicIntf[1] = "1";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.Keys.GetEnumerator())
            {
                enumerator.MoveNext();
                dic[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            #endregion

            #region Values

            using (var enumerator = dic.Values.GetEnumerator())
            {
                enumerator.MoveNext();
                dic.Remove(1);
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.Values.GetEnumerator())
            {
                enumerator.MoveNext();
                dicIntf[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.Values.GetEnumerator())
            {
                enumerator.MoveNext();
                dicIntf[1] = "1";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            using (var enumerator = dic.Values.GetEnumerator())
            {
                enumerator.MoveNext();
                dic[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            #endregion

            #region IDictionaryEnumerator

            {
                var enumerator = dicNonGeneric.GetEnumerator();
                enumerator.MoveNext();
                dic.Remove(1);
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            {
                var enumerator = dicNonGeneric.GetEnumerator();
                enumerator.MoveNext();
                dicIntf[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            {
                var enumerator = dicNonGeneric.GetEnumerator();
                enumerator.MoveNext();
                dicIntf[1] = "1";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            {
                var enumerator = dicNonGeneric.GetEnumerator();
                enumerator.MoveNext();
                dic[0] = "zero";
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            }

            #endregion
        }

        [Fact]
        public void LegacyApis()
        {
            var dic = new OrderedDictionary<int, string>();
            IOrderedDictionary dicNonGeneric = dic;

            dicNonGeneric.Add(1, null);
            Assert.Equal(1, dic.Count);
            Assert.Equal(1, dicNonGeneric.Count);
            Assert.Null(dic.Values.Single());
            Assert.Null(dicNonGeneric.Values.Cast<object>().Single());
            Assert.Null(dic[0]);
            Assert.Null(dicNonGeneric[0]);

            var dic2 = new OrderedDictionary<int, int>();
            dicNonGeneric = dic2;

            Assert.Throws<ArgumentNullException>(() => dicNonGeneric.Add(1, null));
            Assert.Equal(0, dic2.Count);
            Assert.Equal(0, dicNonGeneric.Count);

            var dic3 = new OrderedDictionary<int, int?>();
            dicNonGeneric = dic3;

            dicNonGeneric.Add(1, null);
            Assert.Equal(1, dic.Count);
            Assert.Equal(1, dicNonGeneric.Count);
            Assert.Null(dic3.Values.Single());
            Assert.Null(dicNonGeneric.Values.Cast<object>().Single());
            Assert.Null(dic[0]);
            Assert.Null(dicNonGeneric[(object)1]);
        }
    }
}