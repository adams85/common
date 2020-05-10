using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Karambolo.Common
{
    [Obsolete("This type is unuseful, thus it will be removed in the next major version.")]
    public delegate IEnumerable<T> TreeTraversal<T>(T node, Func<T, IEnumerable<T>> childrenSelector);

    public abstract class TreeTraversal
    {
        #region Pre-order

        private class PreOrderImpl : TreeTraversal
        {
            private static IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
            {
                var stack = new Stack<T>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    node = stack.Pop();
                    yield return node;

                    foreach (T child in childrenSelector(node) ?? Enumerable.Empty<T>())
                        stack.Push(child);
                }
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
            {
                IEnumerable<T> traversal = Traverse(node, childrenSelector);
                return includeSelf ? traversal : traversal.Skip(1);
            }
        }

        /// <summary>
        /// Non-recursive, heap-based, pre-order (depth-first) tree traversal. Provides right-to-left order. Use <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> in children selector to get left-to-right order.
        /// </summary>
        public static readonly TreeTraversal PreOrder = new PreOrderImpl();

        private class PreOrderRecursiveImpl : TreeTraversal
        {
            private readonly struct Traversal<T> : IEnumerable<T>
            {
                private readonly T _node;
                private readonly Func<T, IEnumerable<T>> _childrenSelector;

                public Traversal(T node, Func<T, IEnumerable<T>> childrenSelector)
                {
                    _node = node;
                    _childrenSelector = childrenSelector;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    yield return _node;

                    foreach (T child in _childrenSelector(_node) ?? Enumerable.Empty<T>())
                        foreach (T node in new Traversal<T>(child, _childrenSelector))
                            yield return node;
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf = false)
            {
                IEnumerable<T> traversal = new Traversal<T>(node, childrenSelector);
                return includeSelf ? traversal : traversal.Skip(1);
            }
        }

        /// <summary>
        /// Recursive, stack-based, pre-order (depth-first) tree traversal. Provides left-to-right order. Use <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> in children selector to get right-to-left order.
        /// </summary>
        public static readonly TreeTraversal PreOrderRecursive = new PreOrderRecursiveImpl();

        #endregion

        #region Post-order

        private class PostOrderImpl : TreeTraversal
        {
            private static IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
            {
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;

                var stack = new Stack<T>();
                var visited = new Stack<T>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    node = stack.Peek();

                    if (visited.Count == 0 || !comparer.Equals(visited.Peek(), node))
                    {
                        using (IEnumerator<T> enumerator = (childrenSelector(node) ?? Enumerable.Empty<T>()).GetEnumerator())
                            if (enumerator.MoveNext())
                            {
                                visited.Push(node);
                                stack.Push(enumerator.Current);
                                while (enumerator.MoveNext())
                                    stack.Push(enumerator.Current);
                                continue;
                            }
                    }
                    else
                        visited.Pop();

                    stack.Pop();
                    yield return node;
                }
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
            {
                IEnumerable<T> traversal = Traverse(node, childrenSelector);
                return includeSelf ? traversal : traversal.SkipLast();
            }
        }

        /// <summary>
        /// Non-recursive, heap-based, post-order (depth-first) tree traversal. Provides right-to-left order. Use <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> in children selector to get left-to-right order.
        /// </summary>
        /// <remarks>
        /// For correct behavior, tree nodes must be comparable by <see cref="EqualityComparer.Default"/>.
        /// </remarks>
        public static readonly TreeTraversal PostOrder = new PostOrderImpl();

        private class PostOrderRecursiveImpl : TreeTraversal
        {
            private readonly struct Traversal<T> : IEnumerable<T>
            {
                private readonly T _node;
                private readonly Func<T, IEnumerable<T>> _childrenSelector;

                public Traversal(T node, Func<T, IEnumerable<T>> childrenSelector)
                {
                    _node = node;
                    _childrenSelector = childrenSelector;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    foreach (T child in _childrenSelector(_node) ?? Enumerable.Empty<T>())
                        foreach (T node in new Traversal<T>(child, _childrenSelector))
                            yield return node;

                    yield return _node;
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf = false)
            {
                IEnumerable<T> traversal = new Traversal<T>(node, childrenSelector);
                return includeSelf ? traversal : traversal.SkipLast();
            }
        }

        /// <summary>
        /// Recursive, stack-based, post-order (depth-first) tree traversal. Provides left-to-right order. Use <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> in children selector to get right-to-left order.
        /// </summary>
        public static readonly TreeTraversal PostOrderRecursive = new PostOrderRecursiveImpl();

        #endregion

        #region Level-order

        private class LevelOrderImpl : TreeTraversal
        {
            private static IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
            {
                var queue = new Queue<T>();
                queue.Enqueue(node);
                while (queue.Count > 0)
                {
                    node = queue.Dequeue();
                    yield return node;

                    foreach (T child in childrenSelector(node) ?? Enumerable.Empty<T>())
                        queue.Enqueue(child);
                }
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
            {
                IEnumerable<T> traversal = Traverse(node, childrenSelector);
                return includeSelf ? traversal : traversal.Skip(1);
            }
        }

        /// <summary>
        /// Non-recursive, heap-based, level-order (breadth-first) tree traversal. Provides left-to-right order. Use <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> in children selector to get right-to-left order.
        /// </summary>
        public static readonly TreeTraversal LevelOrder = new LevelOrderImpl();

        private class LevelOrderRecursiveImpl : TreeTraversal
        {
            private readonly struct Traversal<T> : IEnumerable<T>
            {
                private readonly T _node;
                private readonly Func<T, IEnumerable<T>> _childrenSelector;

                public Traversal(T node, Func<T, IEnumerable<T>> childrenSelector)
                {
                    _node = node;
                    _childrenSelector = childrenSelector;
                }

                private IEnumerator<T> VisitChildren(IEnumerator<T> treeEnumerator, int nodeCount)
                {
                    var childCount = 0;

                    for (; nodeCount > 0; nodeCount--)
                    {
                        treeEnumerator.MoveNext();

                        foreach (T child in _childrenSelector(treeEnumerator.Current) ?? Enumerable.Empty<T>())
                        {
                            yield return child;
                            childCount++;
                        }
                    }

                    if (childCount > 0)
                        using (IEnumerator<T> enumerator = VisitChildren(treeEnumerator, childCount))
                            while (enumerator.MoveNext())
                                yield return enumerator.Current;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    yield return _node;

                    using (IEnumerator<T> treeEnumerator = new Traversal<T>(_node, _childrenSelector).GetEnumerator())
                    using (IEnumerator<T> enumerator = VisitChildren(treeEnumerator, 1))
                        while (enumerator.MoveNext())
                            yield return enumerator.Current;
                }

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf = false)
            {
                IEnumerable<T> traversal = new Traversal<T>(node, childrenSelector);
                return includeSelf ? traversal : traversal.Skip(1);
            }
        }

        /// <summary>
        /// Recursive, stack-based, level-order (breadth-first) tree traversal. Provides left-to-right order. Use <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> in children selector to get right-to-left order.
        /// </summary>
        /// <remarks>
        /// Use only when enumerating children is not an expensive operation as children are enumerated twice.
        /// </remarks>
        public static readonly TreeTraversal LevelOrderRecursive = new LevelOrderRecursiveImpl();

        #endregion

        public abstract IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf = false);
    }

    public static class TreeUtils
    {
        public static int Level<T>(T node, Func<T, T> parentSelector)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (parentSelector == null)
                throw new ArgumentNullException(nameof(parentSelector));

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            var level = 0;
            T parent;
            while (!comparer.Equals(parent = parentSelector(node), default))
            {
                node = parent;
                level++;
            }

            return level;
        }

        public static T Root<T>(T node, Func<T, T> parentSelector)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (parentSelector == null)
                throw new ArgumentNullException(nameof(parentSelector));

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            T parent;
            while (!comparer.Equals(parent = parentSelector(node), default))
                node = parent;

            return node;
        }

        public static IEnumerable<T> Ancestors<T>(T node, Func<T, T> parentSelector, bool includeSelf = false)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (parentSelector == null)
                throw new ArgumentNullException(nameof(parentSelector));

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            if (includeSelf)
                yield return node;

            while (!comparer.Equals(node = parentSelector(node), default))
                yield return node;
        }

        public static IEnumerable<T> Descendants<T>(T node, Func<T, IEnumerable<T>> childrenSelector, TreeTraversal traversal = null, bool includeSelf = false)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (childrenSelector == null)
                throw new ArgumentNullException(nameof(childrenSelector));

            return (traversal ?? TreeTraversal.PreOrder).Traverse(node, childrenSelector, includeSelf);
        }

        public static IEnumerable<T> Leaves<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (childrenSelector == null)
                throw new ArgumentNullException(nameof(childrenSelector));

            var nodeWithChildren = new KeyValuePair<T, IEnumerable<T>>(node, childrenSelector(node));
            foreach (KeyValuePair<T, IEnumerable<T>> nwc in TreeTraversal.PreOrder.Traverse(nodeWithChildren, n => n.Value.Select(cn => new KeyValuePair<T, IEnumerable<T>>(cn, childrenSelector(cn))), includeSelf: true))
                if (!nwc.Value.Any())
                    yield return nwc.Key;
        }

        public static IEnumerable<IEnumerable<T>> EnumeratePaths<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            IEnumerable<IEnumerable<T>> paths = Enumerable.Empty<IEnumerable<T>>();
            var nodeWithPath = new KeyValuePair<T, IEnumerable<T>>(node, Enumerable.Empty<T>());
            foreach (KeyValuePair<T, IEnumerable<T>> _ in TreeTraversal.PreOrder.Traverse(nodeWithPath, n =>
            {
                IEnumerable<T> path = n.Value.Append(n.Key);
                IEnumerable<T> children = childrenSelector(n.Key);
                if (!children.Any())
                {
                    paths = paths.Append(path);
                    return Enumerable.Empty<KeyValuePair<T, IEnumerable<T>>>();
                }
                return children.Select(c => new KeyValuePair<T, IEnumerable<T>>(c, path));
            })) { }

            return paths;
        }
    }
}
