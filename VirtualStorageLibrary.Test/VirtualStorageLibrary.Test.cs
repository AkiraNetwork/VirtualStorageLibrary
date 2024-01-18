namespace VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualItemTest
    {
        [TestMethod]
        public void VirtualItemConstructor_CreatesObjectCorrectly()
        {
            // テストデータ
            var testData = new byte[] { 1, 2, 3 };

            // BinaryData オブジェクトを作成
            var binaryData = new BinaryData(testData);

            // VirtualItem<BinaryData> オブジェクトを作成
            string name = "TestBinaryItem";
            var virtualItem = new VirtualItem<BinaryData>(name, binaryData);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(virtualItem);
            Assert.AreEqual(name, virtualItem.Name);
            Assert.AreEqual(binaryData, virtualItem.Item);
            CollectionAssert.AreEqual(virtualItem.Item.Data, testData);
        }

        [TestMethod]
        public void VirtualItemDeepClone_CreatesDistinctCopyWithSameData()
        {
            // BinaryData オブジェクトを作成
            byte[] testData = new byte[] { 1, 2, 3 };
            var originalItem = new VirtualItem<BinaryData>("TestItem", new BinaryData(testData));

            // DeepClone メソッドを使用してクローンを作成
            var clonedItem = originalItem.DeepClone() as VirtualItem<BinaryData>;

            // クローンが正しく作成されたか検証
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            Assert.AreNotSame(originalItem.Item, clonedItem.Item);

            // BinaryData の Data プロパティが適切にクローンされていることを検証
            CollectionAssert.AreEqual(originalItem.Item.Data, clonedItem.Item.Data);
            Assert.AreNotSame(originalItem.Item.Data, clonedItem.Item.Data);
        }
    }

    [TestClass]
    public class VirtualDirectoryTests
    {
        [TestMethod]
        public void VirtualDirectoryConstructor_CreatesObjectCorrectly()
        {
            // VirtualDirectory オブジェクトを作成
            string name = "TestDirectory";
            var virtualDirectory = new VirtualDirectory(name);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(virtualDirectory);
            Assert.AreEqual(name, virtualDirectory.Name);
            Assert.AreEqual(0, virtualDirectory.Count);
        }

        [TestMethod]
        public void VirtualDirectoryDeepClone_CreatesDistinctCopyWithSameData()
        {
            // VirtualDirectory オブジェクトを作成し、いくつかのノードを追加
            var originalDirectory = new VirtualDirectory("TestDirectory");
            originalDirectory.AddDirectory("Node1");
            originalDirectory.AddDirectory("Node2");

            // DeepClone メソッドを使用してクローンを作成
            var clonedDirectory = originalDirectory.DeepClone() as VirtualDirectory;

            // クローンが正しく作成されたか検証
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.CreateDate, clonedDirectory.CreateDate);
            Assert.AreEqual(originalDirectory.UpdateDate, clonedDirectory.UpdateDate);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);

            // 各ノードも適切にクローンされていることを検証
            foreach (var name in originalDirectory.NodeNames)
            {
                Assert.AreNotSame(originalDirectory[name], clonedDirectory[name]);
                Assert.AreEqual(originalDirectory[name].Name, clonedDirectory[name].Name);
            }
        }
        
        [TestMethod]
        public void Add_NewNode_AddsNodeCorrectly()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            var newNode = new VirtualItem<BinaryData>("NewItem", new BinaryData(testData));

            directory.Add(newNode);

            Assert.IsTrue(directory.IsExists("NewItem"));
            Assert.AreEqual(newNode, directory["NewItem"]);
            CollectionAssert.AreEqual(testData, ((BinaryData)((VirtualItem<BinaryData>)directory["NewItem"]).Item).Data);
        }

        [TestMethod]
        public void Add_ExistingNodeWithoutOverwrite_ThrowsInvalidOperationException()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            var originalNode = new VirtualItem<BinaryData>("OriginalItem", new BinaryData(testData));
            directory.Add(originalNode);

            var newNode = new VirtualItem<BinaryData>("OriginalItem", new BinaryData(testData));

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.Add(newNode);
            });
        }

        [TestMethod]
        public void Add_ExistingNodeWithOverwrite_OverwritesNode()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            var originalNode = new VirtualItem<BinaryData>("OriginalItem", new BinaryData(testData));
            directory.Add(originalNode);

            var newTestData = new byte[] { 4, 5, 6 };
            var newNode = new VirtualItem<BinaryData>("OriginalItem", new BinaryData(newTestData));

            directory.Add(newNode, allowOverwrite: true);

            Assert.AreEqual(newNode, directory["OriginalItem"]);
            CollectionAssert.AreEqual(newTestData, ((BinaryData)((VirtualItem<BinaryData>)directory["OriginalItem"]).Item).Data);
        }

        [TestMethod]
        public void Add_NewDirectory_AddsDirectoryCorrectly()
        {
            var parentDirectory = new VirtualDirectory("ParentDirectory");
            var childDirectory = new VirtualDirectory("ChildDirectory");

            parentDirectory.Add(childDirectory);

            Assert.IsTrue(parentDirectory.IsExists("ChildDirectory"));
            Assert.AreEqual(childDirectory, parentDirectory["ChildDirectory"]);
        }

        [TestMethod]
        public void Add_ExistingDirectoryWithoutOverwrite_ThrowsInvalidOperationException()
        {
            var parentDirectory = new VirtualDirectory("ParentDirectory");
            var originalDirectory = new VirtualDirectory("OriginalDirectory");
            parentDirectory.Add(originalDirectory);

            var newDirectory = new VirtualDirectory("OriginalDirectory");

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.Add(newDirectory);
            });
        }

        [TestMethod]
        public void Add_ExistingDirectoryWithOverwrite_OverwritesDirectory()
        {
            var parentDirectory = new VirtualDirectory("ParentDirectory");
            var originalDirectory = new VirtualDirectory("OriginalDirectory");
            parentDirectory.Add(originalDirectory);

            var newDirectory = new VirtualDirectory("OriginalDirectory");

            parentDirectory.Add(newDirectory, allowOverwrite: true);

            Assert.AreEqual(newDirectory, parentDirectory["OriginalDirectory"]);
        }

        [TestMethod]
        public void AddDirectory_NewDirectory_AddsDirectoryCorrectly()
        {
            var parentDirectory = new VirtualDirectory("ParentDirectory");

            parentDirectory.AddDirectory("ChildDirectory");

            Assert.IsTrue(parentDirectory.IsExists("ChildDirectory"));
            Assert.IsInstanceOfType(parentDirectory["ChildDirectory"], typeof(VirtualDirectory));
        }

        [TestMethod]
        public void AddDirectory_ExistingDirectoryWithoutOverwrite_ThrowsInvalidOperationException()
        {
            var parentDirectory = new VirtualDirectory("ParentDirectory");
            parentDirectory.AddDirectory("OriginalDirectory");

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.AddDirectory("OriginalDirectory");
            });
        }

        [TestMethod]
        public void AddDirectory_ExistingDirectoryWithOverwrite_OverwritesDirectory()
        {
            var parentDirectory = new VirtualDirectory("ParentDirectory");
            parentDirectory.AddDirectory("OriginalDirectory");

            parentDirectory.AddDirectory("OriginalDirectory", allowOverwrite: true);

            Assert.IsTrue(parentDirectory.IsExists("OriginalDirectory"));
            Assert.IsInstanceOfType(parentDirectory["OriginalDirectory"], typeof(VirtualDirectory));
        }
        
        [TestMethod]
        public void Indexer_ValidKey_ReturnsNode()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var node = new VirtualItem<BinaryData>("Item", new BinaryData());
            directory.Add(node);

            var result = directory["Item"];

            Assert.AreEqual(node, result);
        }

        [TestMethod]
        public void Indexer_InvalidKey_ThrowsKeyNotFoundException()
        {
            var directory = new VirtualDirectory("TestDirectory");

            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                var result = directory["InvalidKey"];
            });
        }

        [TestMethod]
        public void Indexer_Setter_UpdatesNode()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var newNode = new VirtualItem<BinaryData>("NewItem", new BinaryData());

            directory["NewItemKey"] = newNode;
            var result = directory["NewItemKey"];

            Assert.AreEqual(newNode, result);
        }

        [TestMethod]
        public void Remove_NonExistentNodeWithoutForce_ThrowsKeyNotFoundException()
        {
            var directory = new VirtualDirectory("TestDirectory");

            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                directory.Remove("NonExistentNode");
            });
        }

        [TestMethod]
        public void Remove_NonExistentNodeWithForce_DoesNotThrow()
        {
            var directory = new VirtualDirectory("TestDirectory");

            // 例外がスローされないことを確認
            directory.Remove("NonExistentNode", forceRemove: true);
        }

        [TestMethod]
        public void Remove_ExistingNode_RemovesNode()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            string nodeName = "ExistingNode";
            var node = new VirtualItem<BinaryData>(nodeName, new BinaryData(testData));
            directory.Add(node);

            directory.Remove(nodeName);

            Assert.IsFalse(directory.IsExists(nodeName));
        }

        [TestMethod]
        public void Get_ExistingNode_ReturnsNode()
        {
            // VirtualDirectory オブジェクトを作成し、ノードを追加
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            var existingNode = new VirtualItem<BinaryData>("ExistingNode", new BinaryData(testData));
            directory.Add(existingNode);

            // Get メソッドを使用してノードを取得
            var retrievedNode = directory.Get("ExistingNode");

            // 取得したノードが期待通りであることを確認
            Assert.AreEqual(existingNode, retrievedNode);
        }

        [TestMethod]
        public void Get_NonExistingNode_ThrowsKeyNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            var directory = new VirtualDirectory("TestDirectory");

            // 存在しないノード名で Get メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                var retrievedNode = directory.Get("NonExistingNode");
            });
        }

        [TestMethod]
        public void Rename_ExistingNode_RenamesNodeCorrectly()
        {
            // VirtualDirectory オブジェクトを作成し、ノードを追加
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            var existingNode = new VirtualItem<BinaryData>("ExistingNode", new BinaryData(testData));
            directory.Add(existingNode);

            // Rename メソッドを使用してノードの名前を変更
            var newName = "RenamedNode";
            directory.Rename("ExistingNode", newName);

            // 名前が変更されたノードが存在し、元のノードが存在しないことを確認
            Assert.IsTrue(directory.IsExists(newName));
            Assert.IsFalse(directory.IsExists("ExistingNode"));
        }

        [TestMethod]
        public void Rename_NonExistingNode_ThrowsKeyNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            var directory = new VirtualDirectory("TestDirectory");

            // 存在しないノード名で Rename メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                directory.Rename("NonExistingNode", "NewName");
            });
        }

        [TestMethod]
        public void Rename_ToExistingNodeName_ThrowsInvalidOperationException()
        {
            // VirtualDirectory オブジェクトを作成し、2つのノードを追加
            var directory = new VirtualDirectory("TestDirectory");
            var testData = new byte[] { 1, 2, 3 };
            var existingNode1 = new VirtualItem<BinaryData>("ExistingNode1", new BinaryData(testData));
            var existingNode2 = new VirtualItem<BinaryData>("ExistingNode2", new BinaryData(testData));
            directory.Add(existingNode1);
            directory.Add(existingNode2);

            // 既に存在するノード名に Rename メソッドを使用してノードの名前を変更しようとすると例外がスローされることを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.Rename("ExistingNode1", "ExistingNode2");
            });
        }

        [TestMethod]
        public void NodeNames_EmptyDirectory_ReturnsEmpty()
        {
            var directory = new VirtualDirectory("TestDirectory");

            var nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(0, nodeNames.Count());
        }

        [TestMethod]
        public void NodeNames_DirectoryWithNodes_ReturnsNodeNames()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");

            var nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(2, nodeNames.Count());
            CollectionAssert.Contains(nodeNames.ToList(), "Node1");
            CollectionAssert.Contains(nodeNames.ToList(), "Node2");
        }

        [TestMethod]
        public void NodeNames_DirectoryWithNodesAfterRemovingOne_ReturnsRemainingNodeNames()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");
            directory.Remove("Node1");

            var nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(1, nodeNames.Count());
            CollectionAssert.DoesNotContain(nodeNames.ToList(), "Node1");
            CollectionAssert.Contains(nodeNames.ToList(), "Node2");
        }

        [TestMethod]
        public void Nodes_EmptyDirectory_ReturnsEmpty()
        {
            var directory = new VirtualDirectory("TestDirectory");

            var nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(0, nodes.Count());
        }

        [TestMethod]
        public void Nodes_DirectoryWithNodes_ReturnsNodes()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");

            var nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(2, nodes.Count());
            CollectionAssert.Contains(nodes.ToList(), directory["Node1"]);
            CollectionAssert.Contains(nodes.ToList(), directory["Node2"]);
        }

        [TestMethod]
        public void Nodes_DirectoryWithNodesAfterRemovingOne_ReturnsRemainingNodes()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");
            directory.Remove("Node1");

            var nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(1, nodes.Count());
            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                // "Node1"が正しく削除されていることを確認
                var node = directory["Node1"];
            });
            CollectionAssert.Contains(nodes.ToList(), directory["Node2"]);
        }

    }
}
