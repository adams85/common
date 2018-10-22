using System;
using System.Collections.Generic;
using System.Linq;

namespace Karambolo.Common
{
    public delegate IEnumerable<T> TreeTraversal<T>(T node, Func<T, IEnumerable<T>> childrenSelector);

    public abstract class TreeTraversal
    {
        class PreOrderTraversal : TreeTraversal
        {
            static IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
            {
                var stack = new Stack<T>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    node = stack.Pop();
                    yield return node;

                    foreach (var child in childrenSelector(node) ?? Enumerable.Empty<T>())
                        stack.Push(child);
                }
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
            {
                var result = Traverse(node, childrenSelector);
                return includeSelf ? result : result.Skip(1);
            }
        }

        class PostOrderTraversal : TreeTraversal
        {
            static IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
            {
                var comparer = EqualityComparer<T>.Default;

                var stack = new Stack<T>();
                var visited = new Stack<T>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    node = stack.Peek();

                    if (visited.Count == 0 || !comparer.Equals(visited.Peek(), node))
                    {
                        using (var enumerator = (childrenSelector(node) ?? Enumerable.Empty<T>()).GetEnumerator())
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
                var result = Traverse(node, childrenSelector);
                return includeSelf ? result : result.SkipLast();
            }
        }

        class LevelOrderTraversal : TreeTraversal
        {
            static IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
            {
                var queue = new Queue<T>();
                queue.Enqueue(node);
                while (queue.Count > 0)
                {
                    node = queue.Dequeue();
                    yield return node;

                    foreach (var child in childrenSelector(node) ?? Enumerable.Empty<T>())
                        queue.Enqueue(child);
                }
            }

            public override IEnumerable<T> Traverse<T>(T node, Func<T, IEnumerable<T>> childrenSelector, bool includeSelf)
            {
                var result = Traverse(node, childrenSelector);
                return includeSelf ? result : result.Skip(1);
            }
        }

        /// <remarks>
        /// Provides right-to-left order because of performance considerations. Use Reverse() on children selector to get left-to-right order.
        /// </remarks>
        public static readonly TreeTraversal PreOrder = new PreOrderTraversal();
        /// <remarks>
        /// Provides right-to-left order because of performance considerations. Use Reverse() on children selector to get left-to-right order.
        /// </remarks>
        public static readonly TreeTraversal PostOrder = new PostOrderTraversal();
        /// <remarks>
        /// Provides left-to-right order because of performance considerations. Use Reverse() on children selector to get right-to-left order.
        /// </remarks>
        public static readonly TreeTraversal LevelOrder = new LevelOrderTraversal();

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

            var comparer = EqualityComparer<T>.Default;

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

            var comparer = EqualityComparer<T>.Default;

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

            var comparer = EqualityComparer<T>.Default;

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
            foreach (var nwc in TreeTraversal.PreOrder.Traverse(nodeWithChildren, n => n.Value.Select(cn => new KeyValuePair<T, IEnumerable<T>>(cn, childrenSelector(cn))), includeSelf: true))
                if (!nwc.Value.Any())
                    yield return nwc.Key;
        }

        public static IEnumerable<IEnumerable<T>> EnumeratePaths<T>(T node, Func<T, IEnumerable<T>> childrenSelector)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var paths = Enumerable.Empty<IEnumerable<T>>();
            var nodeWithPath = new KeyValuePair<T, IEnumerable<T>>(node, Enumerable.Empty<T>());
            foreach (var _ in TreeTraversal.PreOrder.Traverse(nodeWithPath, n =>
            {
                var path = n.Value.Append(n.Key);
                var children = childrenSelector(n.Key);
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
