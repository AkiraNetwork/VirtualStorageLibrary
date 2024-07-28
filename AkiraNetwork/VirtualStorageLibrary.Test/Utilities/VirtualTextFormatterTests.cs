using System.Diagnostics;
using AkiraNetwork.VirtualStorageLibrary.Utilities;

namespace AkiraNetwork.VirtualStorageLibrary.Test.Utilities
{
    [TestClass]
    public class VirtualTextFormatterTests
    {
        private static VirtualStorage<string> SetupVirtualStorage()
        {
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1", "test");
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/item2", "test");
            vs.AddDirectory("/dir1/subdir1", true);
            vs.AddItem("/dir1/subdir1/item3", "test");
            vs.AddSymbolicLink("/link-to-item", "/item1");
            vs.AddSymbolicLink("/link-to-dir", "/dir1");

            // ツリー構造の表示
            Debug.WriteLine("tree structure (SetupVirtualStorage):");
            string tree = vs.GenerateTreeDebugText("/", true, false);

            Debug.WriteLine(tree);

            return vs;
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_RecursiveWithLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_RecursiveNoLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonRecursiveWithLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", false, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonRecursiveNoLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", false, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonExistentPathWithLinks()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                string tree = vs.GenerateTreeDebugText("/nonexistent", true, true);
            });
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_RecursiveWithLinksFromSubDirectory()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/dir1", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonRecursiveWithLinksFromSubDirectory()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/dir1", false, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_BasePathIsItem()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/item1", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToItem_NoFollow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-item", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToItem_Follow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-item", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToDirectory_NoFollow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-dir", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToDirectory_Follow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-dir", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateLinkTableDebugText_EmptyLinkDictionary_ReturnsEmptyMessage()
        {
            // Arrange
            VirtualStorage<string> vs = new();

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Assert.AreEqual("(リンク辞書は空です。)", result);
        }

        [TestMethod]
        public void GenerateLinkTableDebugText_WithLinks1_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/linkToItem1", "/item1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void GenerateLinkTableDebugText_WithLinks2_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/linkToItem1", "/item1");
            vs.AddSymbolicLink("/linkToItem2", "/item1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void GenerateLinkTableDebugText_WithFullWidth1_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク1", "/アイテム1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void GenerateLinkTableDebugText_WithFullWidth2_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク1", "/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク2", "/アイテム1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void GenerateLinkTableDebugText_Test1_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/linkToItem1", "/item1");
            vs.AddSymbolicLink("/linkToItem2", "/item1");
            vs.AddItem("/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク1", "/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク2", "/アイテム1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestMethod]
        public void GenerateTableDebugText_EmptyTable_ReturnsEmptyMessage()
        {
            // Arrange
            List<string> messages = [];

            // Act
            string result = messages.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.AreEqual("(コレクションは空です。)", result);
        }

        [TestMethod]
        public void GenerateTableDebugText_AnyTable_ReturnsEmptyMessage()
        {
            // Arrange
            List<string> messages = ["Hello, ", "World!"];

            // Act
            string result = messages.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }
    }
}
