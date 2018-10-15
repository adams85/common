using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Karambolo.Common.Test
{
    public class ModelExtensionsTest
    {
        class TreeNode<T>
        {
            public TreeNode(T id)
            {
                Id = id;
                Children = new List<TreeNode<T>>();
            }

            public T Id { get; set; }

            public TreeNode<T> Parent { get; private set; }
            public List<TreeNode<T>> Children { get; private set; }

            public TreeNode<T> AddChild(T id)
            {
                var child = new TreeNode<T>(id) { Parent = this };
                Children.Add(child);
                return child;
            }

            public override string ToString()
            {
                return Id.ToString();
            }
        }

        bool PathEquals(IEnumerable<TreeNode<int>> path, params int[] ids)
        {
            return ids.SequenceEqual(path.Select(n => n.Id));
        }


        [Fact]
        public void TraversalTest()
        {
            var tree = new TreeNode<string>("F");
            tree
                .AddChild("B")
                    .AddChild("A").Parent
                    .AddChild("D")
                        .AddChild("C").Parent
                        .AddChild("E").Parent.Parent.Parent
                .AddChild("G")
                    .AddChild("I")
                        .AddChild("H");

            var traversal = TreeTraversal.PreOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: true);
            Assert.Equal("F, B, A, D, C, E, G, I, H", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.PreOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: false);
            Assert.Equal("B, A, D, C, E, G, I, H", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.PostOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: true);
            Assert.Equal("A, C, E, D, B, H, I, G, F", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.PostOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: false);
            Assert.Equal("A, C, E, D, B, H, I, G", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.LevelOrder.Traverse(tree, t => t.Children, includeSelf: true);
            Assert.Equal("F, B, G, A, D, I, C, E, H", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.LevelOrder.Traverse(tree, t => t.Children, includeSelf: false);
            Assert.Equal("B, G, A, D, I, C, E, H", string.Join(", ", traversal.Select(n => n.Id)));
        }

        /// <summary>
        ///A test for Leaves
        ///</summary>
        [Fact]
        public void LeavesTest()
        {
            var tree = new TreeNode<int>(0);
            var result = TreeUtils.Leaves(tree, t => t.Children).ToArray();
            Assert.Single(result);
            Assert.Contains(result, r => r.Id == 0);

            tree
                .AddChild(1)
                    .AddChild(2).Parent
                    .AddChild(3)
                        .AddChild(4).Parent
                        .AddChild(5).Parent.Parent.Parent
                .AddChild(6).Parent
                .AddChild(7)
                    .AddChild(8);

            result = TreeUtils.Leaves(tree, t => t.Children).ToArray();

            Assert.Equal(5, result.Length);
            Assert.Contains(result, r => r.Id == 2);
            Assert.Contains(result, r => r.Id == 4);
            Assert.Contains(result, r => r.Id == 5);
            Assert.Contains(result, r => r.Id == 6);
            Assert.Contains(result, r => r.Id == 8);
        }

        /// <summary>
        ///A test for EnumeratePaths
        ///</summary>
        [Fact]
        public void EnumeratePathsTest()
        {
            var tree = new TreeNode<int>(0);
            var result = TreeUtils.EnumeratePaths(tree, n => n.Children).ToArray();
            Assert.Single(result);
            Assert.Contains(result, r => PathEquals(r, 0));

            tree
                .AddChild(1)
                    .AddChild(2).Parent
                    .AddChild(3)
                        .AddChild(4).Parent
                        .AddChild(5).Parent.Parent.Parent
                .AddChild(6).Parent
                .AddChild(7)
                    .AddChild(8);

            result = TreeUtils.EnumeratePaths(tree, n => n.Children).ToArray();

            Assert.Equal(5, result.Length);
            Assert.Contains(result, r => PathEquals(r, 0, 1, 2));
            Assert.Contains(result, r => PathEquals(r, 0, 1, 3, 4));
            Assert.Contains(result, r => PathEquals(r, 0, 1, 3, 5));
            Assert.Contains(result, r => PathEquals(r, 0, 6));
            Assert.Contains(result, r => PathEquals(r, 0, 7, 8));
        }
    }
}
