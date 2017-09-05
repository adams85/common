using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace Karambolo.Common.Test
{


    /// <summary>
    ///This is a test class for ModelExtensionsTest and is intended
    ///to contain all ModelExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ModelExtensionsTest
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


        [TestMethod()]
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
            Assert.AreEqual("F, B, A, D, C, E, G, I, H", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.PreOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: false);
            Assert.AreEqual("B, A, D, C, E, G, I, H", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.PostOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: true);
            Assert.AreEqual("A, C, E, D, B, H, I, G, F", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.PostOrder.Traverse(tree, t => t.Children.AsEnumerable().Reverse(), includeSelf: false);
            Assert.AreEqual("A, C, E, D, B, H, I, G", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.LevelOrder.Traverse(tree, t => t.Children, includeSelf: true);
            Assert.AreEqual("F, B, G, A, D, I, C, E, H", string.Join(", ", traversal.Select(n => n.Id)));

            traversal = TreeTraversal.LevelOrder.Traverse(tree, t => t.Children, includeSelf: false);
            Assert.AreEqual("B, G, A, D, I, C, E, H", string.Join(", ", traversal.Select(n => n.Id)));
        }

        /// <summary>
        ///A test for Leaves
        ///</summary>
        [TestMethod()]
        public void LeavesTest()
        {
            var tree = new TreeNode<int>(0);
            var result = TreeUtils.Leaves(tree, t => t.Children).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result.Any(r => r.Id == 0));

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

            Assert.AreEqual(5, result.Length);
            Assert.IsTrue(result.Any(r => r.Id == 2));
            Assert.IsTrue(result.Any(r => r.Id == 4));
            Assert.IsTrue(result.Any(r => r.Id == 5));
            Assert.IsTrue(result.Any(r => r.Id == 6));
            Assert.IsTrue(result.Any(r => r.Id == 8));
        }

        /// <summary>
        ///A test for EnumeratePaths
        ///</summary>
        [TestMethod()]
        public void EnumeratePathsTest()
        {
            var tree = new TreeNode<int>(0);
            var result = TreeUtils.EnumeratePaths(tree, n => n.Children).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result.Any(r => PathEquals(r, 0)));

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

            Assert.AreEqual(5, result.Length);
            Assert.IsTrue(result.Any(r => PathEquals(r, 0, 1, 2)));
            Assert.IsTrue(result.Any(r => PathEquals(r, 0, 1, 3, 4)));
            Assert.IsTrue(result.Any(r => PathEquals(r, 0, 1, 3, 5)));
            Assert.IsTrue(result.Any(r => PathEquals(r, 0, 6)));
            Assert.IsTrue(result.Any(r => PathEquals(r, 0, 7, 8)));
        }
    }
}
