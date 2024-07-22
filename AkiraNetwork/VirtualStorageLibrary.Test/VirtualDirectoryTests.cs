using System.Diagnostics;
using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualDirectoryTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            VirtualStorageSettings.Initialize();
            VirtualNodeName.ResetCounter();
        }

        [TestMethod]
        public void VirtualDirectoryConstructorDefault_CreatesObjectCorrectly()

        {
            // VirtualDirectory オブジェクトを作成
            VirtualDirectory directory = [];

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(directory);
            Assert.AreEqual($"{VirtualStorageState.State.PrefixDirectory}1", (string)directory.Name);
            Assert.AreEqual(0, directory.Count);
        }

        [TestMethod]
        public void VirtualDirectoryConstructorByName_CreatesObjectCorrectly()

        {
            // VirtualDirectory オブジェクトを作成
            VirtualNodeName name = "TestDirectory";
            VirtualDirectory directory = new(name);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(directory);
            Assert.AreEqual(name, directory.Name);
            Assert.AreEqual(0, directory.Count);
        }

        [TestMethod]
        public void VirtualDirectoryDeepClone_CreatesDistinctCopyWithSameData()
        {
            // VirtualDirectory オブジェクトを作成し、いくつかのノードを追加
            VirtualDirectory originalDirectory = new("TestDirectory");
            originalDirectory.AddDirectory("Node1");
            originalDirectory.AddDirectory("Node2");

            // DeepClone メソッドを使用してクローンを作成
            VirtualDirectory? clonedDirectory = originalDirectory.DeepClone() as VirtualDirectory;

            // クローンが正しく作成されたか検証
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreNotEqual(originalDirectory.CreatedDate, clonedDirectory.CreatedDate);
            Assert.AreNotEqual(originalDirectory.UpdatedDate, clonedDirectory.UpdatedDate);
            Assert.AreEqual(0, clonedDirectory.Count); // ディレクトリのコピーなのでノードはコピーされない
        }

        [TestMethod]
        public void DirectoryCount_WithMultipleDirectories_ReturnsCorrectCount()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.Root.AddDirectory("dir1");
            vs.Root.AddDirectory("dir2");

            Assert.AreEqual(2, vs.Root.DirectoryViewCount, "ルートディレクトリ内のディレクトリ数が正しくありません。");
        }

        [TestMethod]
        public void ItemCount_WithMultipleItems_ReturnsCorrectCount()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.Root.AddItem("item1", new object());
            vs.Root.AddItem("item2", new object());

            Assert.AreEqual(2, vs.Root.ItemViewCount, "ルートディレクトリ内のアイテム数が正しくありません。");
        }

        [TestMethod]
        public void SymbolicLinkCount_WithMultipleSymbolicLinks_ReturnsCorrectCount()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.Root.Add(new VirtualSymbolicLink("link1", "/path/to/target1"));
            vs.Root.Add(new VirtualSymbolicLink("link2", "/path/to/target2"));

            Assert.AreEqual(2, vs.Root.SymbolicLinkViewCount, "ルートディレクトリ内のシンボリックリンク数が正しくありません。");
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("original")
            {
                new VirtualItem<BinaryData>("item", [1, 2, 3])
            };

            // Act
            VirtualDirectory clonedDirectory = (VirtualDirectory)originalDirectory.DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(clonedDirectory.Count, 0); // ディレクトリのコピーなのでアイテムはコピーされない
        }

        [TestMethod]
        public void Add_NewNode_AddsNodeCorrectly()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> newNode = new("NewItem", new(testData));

            DateTime beforeDate = directory.UpdatedDate;

            directory.Add(newNode);

            Assert.IsTrue(directory.NodeExists("NewItem"));
            Assert.AreEqual(newNode, directory["NewItem"]);

            Assert.IsTrue(directory.UpdatedDate > beforeDate);

            VirtualItem<BinaryData> item = (VirtualItem<BinaryData>)directory.Get("NewItem")!;
            CollectionAssert.AreEqual(testData, item.ItemData!.Data);
        }

        [TestMethod]
        public void Add_ExistingNodeWithoutOverwrite_ThrowsInvalidOperationException()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> originalNode = new("OriginalItem", new BinaryData(testData));
            directory.Add(originalNode);

            VirtualItem<BinaryData> newNode = new("OriginalItem", new BinaryData(testData));

            DateTime beforeDate = directory.UpdatedDate;

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.Add(newNode);
            });

            Assert.IsTrue(directory.UpdatedDate == beforeDate);
        }

        [TestMethod]
        public void Add_ExistingNodeWithOverwrite_OverwritesNode()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> originalNode = new("OriginalItem", new(testData));
            directory.Add(originalNode);

            byte[] newTestData = [4, 5, 6];
            VirtualItem<BinaryData> newNode = new("OriginalItem", new(newTestData));

            directory.Add(newNode, allowOverwrite: true);

            Assert.AreEqual(newNode, directory["OriginalItem"]);

            VirtualItem<BinaryData> item = (VirtualItem<BinaryData>)directory.Get("OriginalItem")!;
            CollectionAssert.AreEqual(newTestData, item.ItemData!.Data);
        }

        [TestMethod]
        public void Add_NewDirectory_AddsDirectoryCorrectly()
        {
            VirtualDirectory parentDirectory = new("ParentDirectory");
            VirtualDirectory childDirectory = new("ChildDirectory");

            parentDirectory.Add(childDirectory);

            Assert.IsTrue(parentDirectory.NodeExists("ChildDirectory"));
            Assert.AreEqual(childDirectory, parentDirectory["ChildDirectory"]);
        }

        [TestMethod]
        public void Add_ExistingDirectoryWithoutOverwrite_ThrowsInvalidOperationException()
        {
            VirtualDirectory parentDirectory = new("ParentDirectory");
            VirtualDirectory originalDirectory = new("OriginalDirectory");
            parentDirectory.Add(originalDirectory);

            VirtualDirectory newDirectory = new("OriginalDirectory");

            Exception err = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.Add(newDirectory);
            });

            Assert.AreEqual("Node [OriginalDirectory] already exists. Overwriting is not allowed.", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void Add_ExistingDirectoryWithOverwrite_OverwritesDirectory()
        {
            VirtualDirectory parentDirectory = new("ParentDirectory");
            VirtualDirectory originalDirectory = new("OriginalDirectory");
            parentDirectory.Add(originalDirectory);

            VirtualDirectory newDirectory = new("OriginalDirectory");

            parentDirectory.Add(newDirectory, allowOverwrite: true);

            Assert.AreEqual(newDirectory, parentDirectory["OriginalDirectory"]);
        }

        [TestMethod]
        public void Add_InvalidNodeName_ThrowsArgumentException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new("Invalid/Name", new BinaryData([1, 2, 3]));

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => directory.Add(newNode));
            Assert.AreEqual("Node name [Invalid/Name] is invalid. Forbidden characters are used. (Parameter 'node')", ex.Message);

            Debug.WriteLine(ex.Message);
        }

        [TestMethod]
        public void Add_EmptyNodeName_ThrowsArgumentException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new("", new BinaryData([1, 2, 3]));

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => directory.Add(newNode));

            Debug.WriteLine(ex.Message);
        }

        [TestMethod]
        public void Add_InvalidNodeNameWithDot_ThrowsArgumentException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new(".", new BinaryData([1, 2, 3]));

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => directory.Add(newNode));

            Debug.WriteLine(ex.Message);
        }

        [TestMethod]
        public void Add_InvalidNodeNameWithDotDot_ThrowsArgumentException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new("..", new BinaryData([1, 2, 3]));

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => directory.Add(newNode));

            Debug.WriteLine(ex.Message);
        }

        [TestMethod]
        public void Add_InvalidNodeNameWithRoot_ThrowsArgumentException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new("/", new BinaryData([1, 2, 3]));

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => directory.Add(newNode));

            Assert.AreEqual("Cannot add the root to a directory. (Parameter 'node')", ex.Message);

            Debug.WriteLine(ex.Message);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully()
        {
            VirtualDirectory directory = new("TestDirectory");
            BinaryData itemData = [1, 2, 3];

            directory.AddItem("TestItem", itemData, false);

            Assert.IsTrue(directory.NodeExists("TestItem"));
            VirtualItem<BinaryData>? retrievedItem = directory.Get("TestItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(itemData!.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_BySimpleData()
        {
            VirtualDirectory directory = new("TestDirectory");
            SimpleData itemData = new(5);

            directory.AddItem("TestItem", itemData, false);

            Assert.IsTrue(directory.NodeExists("TestItem"));
            VirtualItem<SimpleData>? retrievedItem = directory.Get("TestItem") as VirtualItem<SimpleData>;
            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(itemData!.Value, retrievedItem.ItemData!.Value);
        }

        [TestMethod]
        public void AddItem_ThrowsWhenItemAlreadyExistsAndOverwriteIsFalse()
        {
            VirtualDirectory directory = new("TestDirectory");
            BinaryData itemData = [1, 2, 3];
            directory.AddItem("TestItem", itemData, false);

            Assert.ThrowsException<InvalidOperationException>(() =>
                directory.AddItem("TestItem", new BinaryData([4, 5, 6]), false));
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddItem("TestItem", new BinaryData([1, 2, 3]), false);

            BinaryData newItemData = new([4, 5, 6]);
            directory.AddItem("TestItem", newItemData, true);

            VirtualItem<BinaryData>? retrievedItem = directory.Get("TestItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItemData!.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddSymbolicLink_AddsNewLinkSuccessfully()
        {
            VirtualDirectory directory = new("TestDirectory");

            directory.AddSymbolicLink("TestLink", "/path/to/target", false);

            Assert.IsTrue(directory.NodeExists("TestLink"));
            VirtualSymbolicLink? retrievedLink = directory.Get("TestLink") as VirtualSymbolicLink;
            Assert.IsNotNull(retrievedLink);
            Assert.IsTrue(retrievedLink.TargetPath == "/path/to/target");
        }

        [TestMethod]
        public void AddSymbolicLink_ThrowsWhenLinkAlreadyExistsAndOverwriteIsFalse()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddSymbolicLink("TestLink", "/path/to/old-target", false);

            Assert.ThrowsException<InvalidOperationException>(() =>
                directory.AddSymbolicLink("TestLink", "/path/to/new-target", false));
        }

        [TestMethod]
        public void AddSymbolicLink_OverwritesExistingLinkWhenAllowed()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddSymbolicLink("TestLink", "/path/to/old-target", false);

            directory.AddSymbolicLink("TestLink", "/path/to/new-target", true);

            VirtualSymbolicLink? retrievedLink = directory.Get("TestLink") as VirtualSymbolicLink;
            Assert.IsNotNull(retrievedLink);
            Assert.IsTrue(retrievedLink.TargetPath == "/path/to/new-target");
        }

        [TestMethod]
        public void AddDirectory_NewDirectory_AddsDirectoryCorrectly()
        {
            VirtualDirectory parentDirectory = new("ParentDirectory");

            parentDirectory.AddDirectory("ChildDirectory");

            Assert.IsTrue(parentDirectory.NodeExists("ChildDirectory"));
            Assert.IsInstanceOfType(parentDirectory["ChildDirectory"], typeof(VirtualDirectory));
        }

        [TestMethod]
        public void AddDirectory_ExistingDirectoryWithoutOverwrite_ThrowsInvalidOperationException()
        {
            VirtualDirectory parentDirectory = new("ParentDirectory");
            parentDirectory.AddDirectory("OriginalDirectory");

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.AddDirectory("OriginalDirectory");
            });
        }

        [TestMethod]
        public void AddDirectory_ExistingDirectoryWithOverwrite_OverwritesDirectory()
        {
            VirtualDirectory parentDirectory = new("ParentDirectory");
            parentDirectory.AddDirectory("OriginalDirectory");

            parentDirectory.AddDirectory("OriginalDirectory", allowOverwrite: true);

            Assert.IsTrue(parentDirectory.NodeExists("OriginalDirectory"));
            Assert.IsInstanceOfType(parentDirectory["OriginalDirectory"], typeof(VirtualDirectory));
        }

        [TestMethod]
        public void Indexer_ValidKey_ReturnsNode()
        {
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> node = new("ItemData", []);
            directory.Add(node);

            VirtualNode result = directory["ItemData"];

            Assert.AreEqual(node, result);
        }

        [TestMethod]
        public void Indexer_InvalidKey_ThrowsVirtualNodeNotFoundException()
        {
            VirtualDirectory directory = new("TestDirectory");

            Exception err = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                VirtualNode result = directory["InvalidKey"];
            });

            Assert.AreEqual("Node not found. [InvalidKey]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void Indexer_Setter_UpdatesNode()
        {
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new("NewItem", []);

            directory["NewItemKey"] = newNode;
            VirtualNode result = directory["NewItemKey"];

            Assert.AreEqual(newNode, result);
        }

        [TestMethod]
        public void Remove_NonExistentNodeWithoutForce_ThrowsVirtualNodeNotFoundException()
        {
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> node = new("ExistingNode");

            DateTime beforeDate = directory.UpdatedDate;

            Exception err = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.Remove(node);
            });

            Assert.IsTrue(directory.UpdatedDate == beforeDate);
            Assert.AreEqual("Node not found. [ExistingNode]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void Remove_ExistingNode_RemovesNode()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> node = new("item", new BinaryData(testData));
            directory.Add(node);

            DateTime beforeDate = directory.UpdatedDate;

            directory.Remove(node);

            Assert.IsFalse(directory.NodeExists(node.Name));

            Assert.IsTrue(directory.UpdatedDate > beforeDate);
        }

        [TestMethod]
        public void Get_ExistingNode_ReturnsNode()
        {
            // VirtualDirectory オブジェクトを作成し、ノードを追加
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> existingNode = new("ExistingNode", new BinaryData(testData));
            directory.Add(existingNode);

            // Get メソッドを使用してノードを取得
            VirtualNode? retrievedNode = directory.Get("ExistingNode");

            // 取得したノードが期待通りであることを確認
            Assert.AreEqual(existingNode, retrievedNode);
        }

        [TestMethod]
        public void Get_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            VirtualDirectory directory = new("TestDirectory");

            // 存在しないノード名で Get メソッドを呼び出すと例外がスローされることを確認
            Exception err = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                VirtualNode? retrievedNode = directory.Get("NonExistingNode");
            });

            Assert.AreEqual("Node not found. [NonExistingNode]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void VirtualDirectory_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");

            // Act
            string result = directory.ToString();
            Debug.WriteLine(result);

            // Assert
            Assert.IsTrue(result.Contains("TestDirectory"));
        }

        [TestMethod]
        public void ItemExists_WithExistingItem_ReturnsTrue()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName nodeName = "TestItem";
            BinaryData itemData = [1, 2, 3];
            directory.AddItem(nodeName, itemData, false);

            // Act
            bool exists = directory.ItemExists(nodeName);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void ItemExists_WithNonExistingItem_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName nonExistingNodeName = "NonExistingItem";

            // Act
            bool exists = directory.ItemExists(nonExistingNodeName);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_WithExistingDirectory_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName subDirectoryNodeName = "SubDirectory";
            directory.AddDirectory(subDirectoryNodeName);

            // Act
            bool exists = directory.ItemExists(subDirectoryNodeName);

            // Assert
            Assert.IsFalse(exists); // ディレクトリはアイテムではないため、falseを返すべき
        }

        [TestMethod]
        public void ItemExists_WithExistingSymbolicLink_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName linkNodeName = "TestLink";
            directory.AddSymbolicLink(linkNodeName, "/path/to/target", false);

            // Act
            bool exists = directory.ItemExists(linkNodeName);

            // Assert
            Assert.IsFalse(exists); // シンボリックリンクはアイテムとして扱わず、falseを返す
        }

        [TestMethod]
        public void DirectoryExists_WithExistingDirectory_ReturnsTrue()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName subDirectoryNodeName = "SubDirectory";
            directory.AddDirectory(subDirectoryNodeName);

            // Act
            bool exists = directory.DirectoryExists(subDirectoryNodeName);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DirectoryExists_WithNonExistingDirectory_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName nonExistingDirectoryNodeName = "NonExistingDirectory";

            // Act
            bool exists = directory.DirectoryExists(nonExistingDirectoryNodeName);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void DirectoryExists_WithExistingItem_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName nodeName = "TestItem";
            BinaryData itemData = [1, 2, 3];
            directory.AddItem(nodeName, itemData, false);

            // Act
            bool exists = directory.DirectoryExists(nodeName);

            // Assert
            Assert.IsFalse(exists); // アイテムはディレクトリではないため、falseを返すべき
        }

        [TestMethod]
        public void DirectoryExists_WithExistingSymbolicLink_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName linkName = "TestLink";
            directory.AddSymbolicLink(linkName, "/path/to/target", false);

            // Act
            bool exists = directory.DirectoryExists(linkName);

            // Assert
            Assert.IsFalse(exists); // シンボリックリンクはディレクトリとして扱わず、falseを返す
        }

        [TestMethod]
        public void SymbolicLinkExists_WithExistingSymbolicLink_ReturnsTrue()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName linkName = "TestLink";
            directory.AddSymbolicLink(linkName, "/path/to/target", false);

            // Act
            bool exists = directory.SymbolicLinkExists(linkName);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WithNonExistingSymbolicLink_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName nonExistingLinkName = "NonExistingLink";

            // Act
            bool exists = directory.SymbolicLinkExists(nonExistingLinkName);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WithExistingDirectory_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName subDirectoryName = "SubDirectory";
            directory.AddDirectory(subDirectoryName);

            // Act
            bool exists = directory.SymbolicLinkExists(subDirectoryName);

            // Assert
            Assert.IsFalse(exists); // ディレクトリはシンボリックリンクではないため、falseを返すべき
        }

        [TestMethod]
        public void SymbolicLinkExists_WithExistingItem_ReturnsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualNodeName itemName = "TestItem";
            BinaryData itemData = [1, 2, 3];
            directory.AddItem(itemName, itemData, false);

            // Act
            bool exists = directory.SymbolicLinkExists(itemName);

            // Assert
            Assert.IsFalse(exists); // アイテムはシンボリックリンクではないため、falseを返すべき
        }

        [TestMethod]
        public void GetNodeList_DefaultOption()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // 仮想ディレクトリとアイテムを追加
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("dir1", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir3", (string)nodes[2].Name);
            Assert.AreEqual("item1", (string)nodes[3].Name);
            Assert.AreEqual("item2", (string)nodes[4].Name);
            Assert.AreEqual("item3", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeAsc()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.Name, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("dir1", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir3", (string)nodes[2].Name);
            Assert.AreEqual("item1", (string)nodes[3].Name);
            Assert.AreEqual("item2", (string)nodes[4].Name);
            Assert.AreEqual("item3", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetNodeList_CreatedDateAscAndTypeAsc()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualItem<BinaryData> item1 = new("item1", new BinaryData([1, 2, 3]));
            VirtualItem<BinaryData> item2 = new("item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddItem("/", item1);
            vs.AddItem("/", item2);
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.CreatedDate, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("dir1", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir3", (string)nodes[2].Name);
            Assert.AreEqual("item1", (string)nodes[3].Name);
            Assert.AreEqual("item2", (string)nodes[4].Name);
            Assert.AreEqual("item3", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetNodeList_CreatedDateDesAndTypeAsc()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualItem<BinaryData> item1 = new("item1", new BinaryData([1, 2, 3]));
            VirtualItem<BinaryData> item2 = new("item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddItem("/", item1);
            vs.AddItem("/", item2);
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.CreatedDate, false)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("dir3", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir1", (string)nodes[2].Name);
            Assert.AreEqual("item3", (string)nodes[3].Name);
            Assert.AreEqual("item2", (string)nodes[4].Name);
            Assert.AreEqual("item1", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetNodeList_NameDesAndTypeAsc()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.Name, false)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("dir3", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir1", (string)nodes[2].Name);
            Assert.AreEqual("item3", (string)nodes[3].Name);
            Assert.AreEqual("item2", (string)nodes[4].Name);
            Assert.AreEqual("item1", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDes()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new(node => node.ResolveNodeType(), false),
                [new(node => node.Name, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("item1", (string)nodes[0].Name);
            Assert.AreEqual("item2", (string)nodes[1].Name);
            Assert.AreEqual("item3", (string)nodes[2].Name);
            Assert.AreEqual("dir1", (string)nodes[3].Name);
            Assert.AreEqual("dir2", (string)nodes[4].Name);
            Assert.AreEqual("dir3", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDesWithOnlyDir()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.Directory | VirtualNodeTypeFilter.SymbolicLink,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.Name, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(3, nodes.Count);
            Assert.AreEqual("dir1", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir3", (string)nodes[2].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDesWithOnlyItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.Item | VirtualNodeTypeFilter.SymbolicLink,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.Name, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(3, nodes.Count);
            Assert.AreEqual("item1", (string)nodes[0].Name);
            Assert.AreEqual("item2", (string)nodes[1].Name);
            Assert.AreEqual("item3", (string)nodes[2].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDesWithOnlySymbolicLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.SymbolicLink,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.Name, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }

            // new(node => node.ResolveNodeType(), true)が指定されているため、SymbolicLink のみ取得される
            // dir3、item3 はディレクトリ、アイテムの扱いとなるため取得されない
            Assert.AreEqual(0, nodes.Count);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeAscWithSymbolicLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2");
            vs.AddItem("/item2");
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/item3", "/item1");
            vs.AddSymbolicLink("/dir3", "/dir1");

            // 条件を設定
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new(node => node.ResolveNodeType(), true),
                [new(node => node.Name, true)]);

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = vs.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(6, nodes.Count);
            Assert.AreEqual("dir1", (string)nodes[0].Name);
            Assert.AreEqual("dir2", (string)nodes[1].Name);
            Assert.AreEqual("dir3", (string)nodes[2].Name);
            Assert.AreEqual("item1", (string)nodes[3].Name);
            Assert.AreEqual("item2", (string)nodes[4].Name);
            Assert.AreEqual("item3", (string)nodes[5].Name);
        }

        [TestMethod]
        public void GetItem_ExistingItem_ReturnsItem()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> item = new("TestItem", new BinaryData(testData));
            directory.Add(item);

            // Act
            VirtualItem<BinaryData> result = directory.GetItem<BinaryData>("TestItem");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(item, result);
            CollectionAssert.AreEqual(testData, result.ItemData!.Data);
        }

        [TestMethod]
        public void GetItem_NonExistingItem_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");

            // Act & Assert
            Exception err = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.GetItem<BinaryData>("NonExistingItem");
            });

            Assert.AreEqual("Node not found. [NonExistingItem]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void GetItem_ItemWithDifferentType_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            directory.AddItem("TestItem", "This is a string item");

            // Act & Assert
            Exception err = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.GetItem<BinaryData>("TestItem");
            });

            // ノードの型がVirtualItemであり、かつ、Tの型がBinaryDataでなければならない
            Assert.AreEqual("The specified node [TestItem] is not of type VirtualItem<BinaryData>.", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void GetDirectory_ExistingDirectory_ReturnsDirectory()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualDirectory subDirectory = new("SubDirectory");
            directory.Add(subDirectory);

            // Act
            VirtualDirectory result = directory.GetDirectory("SubDirectory");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(subDirectory, result);
        }

        [TestMethod]
        public void GetDirectory_NonExistingDirectory_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");

            // Act & Assert
            Exception err = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.GetDirectory("NonExistingDirectory");
            });

            Assert.AreEqual("Node not found. [NonExistingDirectory]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void GetDirectory_NodeIsNotDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            directory.AddItem("TestItem", new object());

            // Act & Assert
            Exception err = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.GetDirectory("TestItem");
            });

            Assert.AreEqual("The specified node [TestItem] is not of type VirtualDirectory.", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void GetSymbolicLink_ExistingSymbolicLink_ReturnsSymbolicLink()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            VirtualSymbolicLink symbolicLink = new("TestLink", "/path/to/target");
            directory.Add(symbolicLink);

            // Act
            VirtualSymbolicLink result = directory.GetSymbolicLink("TestLink");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(symbolicLink, result);
        }

        [TestMethod]
        public void GetSymbolicLink_NonExistingSymbolicLink_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");

            // Act & Assert
            Exception err = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.GetSymbolicLink("NonExistingLink");
            });

            Assert.AreEqual("Node not found. [NonExistingLink]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void GetSymbolicLink_NodeIsNotSymbolicLink_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualDirectory directory = new("TestDirectory");
            directory.AddItem("TestItem", new object());

            // Act & Assert
            Exception err = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.GetSymbolicLink("TestItem");
            });

            Assert.AreEqual("The specified node [TestItem] is not of type VirtualSymbolicLink.", err.Message);

            Debug.WriteLine(err.Message);
        }

        // 文字列からの暗黙的な変換で VirtualDirectory オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromString_CreatesObjectCorrectly()
        {
            // 文字列から VirtualDirectory オブジェクトを作成
            VirtualNodeName directoryName = "MyDirectory";
            VirtualDirectory directory = directoryName;

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(directory);
            Assert.AreEqual(directoryName, directory.Name);
        }

        [TestMethod]
        public void Operator_Plus_AddsNodeToDirectory_And_VerifiesIsReferencedInStorageIsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("testDir");
            VirtualItem<string> item = new("testItem");

            // Act
            directory += item;

            // Assert
            Assert.IsTrue(directory.NodeExists("testItem"));
            Assert.IsFalse(item.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Minus_RemovesNodeFromDirectory_And_VerifiesIsReferencedInStorageIsFalse()
        {
            // Arrange
            VirtualDirectory directory = new("testDir");
            VirtualItem<string> item = new("testItem");
            directory += item;

            // Act
            directory -= item;

            // Assert
            Assert.IsFalse(directory.NodeExists("testItem"));
            Assert.IsFalse(item.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Plus_AddsNodeToDirectoryInVirtualStorage()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/testDir");
            VirtualItem<string> item = new("testItem");
            VirtualDirectory directory = vs.GetDirectory("/testDir");

            // Act
            directory += item;

            // Assert
            Assert.IsTrue(directory.NodeExists("testItem"));
            Assert.IsTrue(item.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Minus_RemovesNodeFromDirectoryInVirtualStorage()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/testDir");
            VirtualItem<string> item = new("testItem");
            VirtualDirectory directory = vs.GetDirectory("/testDir");
            directory += item;

            // Act
            directory -= item;

            // Assert
            Assert.IsFalse(directory.NodeExists("testItem"));
            Assert.IsFalse(item.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Plus_AddsDirectoryToDirectoryInVirtualStorage_And_VerifiesIsReferencedInStorageIsTrue()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/testDir");
            VirtualDirectory subDirectory = new("subDir");
            VirtualItem<string> item = new("testItem");
            subDirectory += item;
            VirtualDirectory directory = vs.GetDirectory("/testDir");

            // Verify flags before Act
            Assert.IsFalse(subDirectory.IsReferencedInStorage);
            Assert.IsFalse(item.IsReferencedInStorage);

            // Act
            directory += subDirectory;

            // Assert
            Assert.IsTrue(directory.NodeExists("subDir"));
            Assert.IsTrue(subDirectory.IsReferencedInStorage);
            Assert.IsTrue(item.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Minus_RemovesDirectoryFromDirectoryInVirtualStorage_And_VerifiesIsReferencedInStorageIsFalse()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/testDir");
            VirtualDirectory subDirectory = new("subDir");
            VirtualItem<string> item = new("testItem");
            subDirectory += item;
            VirtualDirectory directory = vs.GetDirectory("/testDir");
            directory += subDirectory;

            // Verify flags before Act
            Assert.IsTrue(subDirectory.IsReferencedInStorage);
            Assert.IsTrue(item.IsReferencedInStorage);

            // Act
            directory -= subDirectory;

            // Assert
            Assert.IsFalse(directory.NodeExists("subDir"));
            Assert.IsFalse(subDirectory.IsReferencedInStorage);
            Assert.IsFalse(item.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Plus_AddsExistingItemToRootDirectoryInVirtualStorage()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData data1 = [1, 2, 3];
            vs.AddDirectory("/dir1/dir2", true);
            vs.AddItem("/dir1/dir2/item1", data1);

            // Act
            VirtualItem<BinaryData> item1 = vs.GetItem("/dir1/dir2/item1");
            VirtualDirectory root = vs.GetDirectory("/");
            root += item1;

            // Assert
            Assert.IsTrue(root.NodeExists("item1"));
            Assert.IsTrue(item1.IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Plus_AddsAllNodeTypesToDirectoryInVirtualStorage()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1");
            VirtualDirectory dir2 = new("dir2");

            // Add various node types to dir2
            VirtualDirectory testSubDir = new("testSubDir");
            VirtualItem<string> testItem = new("testItem");
            VirtualSymbolicLink testLink = new("testLink", "/dir1/testItem");

            dir2 += testSubDir;
            dir2 += testItem;
            dir2 += testLink;

            // Get the directory to which dir2 will be added
            VirtualDirectory directory = vs.GetDirectory("/dir1");

            // Verify flags before Act
            Assert.IsFalse(dir2.IsReferencedInStorage);
            Assert.IsFalse(testSubDir.IsReferencedInStorage);
            Assert.IsFalse(testItem.IsReferencedInStorage);
            Assert.IsFalse(testLink.IsReferencedInStorage);

            // Act
            directory += dir2;

            // Assert
            Assert.IsTrue(directory.NodeExists("dir2"));
            Assert.IsTrue(vs.GetNode("/dir1/dir2").IsReferencedInStorage);
            Assert.IsTrue(vs.GetNode("/dir1/dir2/testSubDir").IsReferencedInStorage);
            Assert.IsTrue(vs.GetNode("/dir1/dir2/testItem").IsReferencedInStorage);
            Assert.IsTrue(vs.GetNode("/dir1/dir2/testLink").IsReferencedInStorage);
        }

        [TestMethod]
        public void Operator_Plus_AddsExistingDirectoryToNewDirectory_And_VerifiesIsReferencedInStorage()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir1/dir2");
            vs.AddDirectory("/dir1/dir2/testSubDir");
            vs.AddItem("/dir1/dir2/testItem");
            vs.AddSymbolicLink("/dir1/dir2/testLink", "/dir1/dir2/testItem");

            VirtualDirectory dir3 = new("dir3");

            // Verify flags before Act
            Assert.IsTrue(vs.GetNode("/dir1/dir2").IsReferencedInStorage);
            Assert.IsTrue(vs.GetNode("/dir1/dir2/testSubDir").IsReferencedInStorage);
            Assert.IsTrue(vs.GetNode("/dir1/dir2/testItem").IsReferencedInStorage);
            Assert.IsTrue(vs.GetNode("/dir1/dir2/testLink").IsReferencedInStorage);
            Assert.IsFalse(dir3.IsReferencedInStorage);

            // Act
            dir3 += vs.GetDirectory("/dir1/dir2");

            // Assert
            Assert.IsFalse(dir3.IsReferencedInStorage);

            // Verify the existence and IsReferencedInStorage flag of sub-nodes within dir3
            VirtualDirectory addedDir2 = dir3.GetDirectory("dir2");
            Assert.IsFalse(addedDir2.IsReferencedInStorage);

            VirtualDirectory addedTestSubDir = addedDir2.GetDirectory("testSubDir");
            VirtualItem<string> addedTestItem = addedDir2.GetItem<string>("testItem");
            VirtualSymbolicLink addedTestLink = addedDir2.GetSymbolicLink("testLink");

            Assert.IsFalse(addedTestSubDir.IsReferencedInStorage);
            Assert.IsFalse(addedTestItem.IsReferencedInStorage);
            Assert.IsFalse(addedTestLink.IsReferencedInStorage);
        }

        [TestMethod]
        public void AddDirectoryToAnotherDirectory_CreatesClone_And_VerifiesIsReferencedInStorage()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddDirectory("/dir1/dir2a/", true);

            VirtualDirectory dir3 = vs.GetDirectory("/dir1/dir2/dir3");
            VirtualDirectory dir2a = vs.GetDirectory("/dir1/dir2a");

            dir2a += dir3;
            vs.GetDirectory("/dir1/dir2/dir3").AddItem<string>("item1");

            Assert.IsTrue(dir2a.NodeExists("dir3"));
            Assert.AreNotSame(vs.GetDirectory("/dir1/dir2/dir3"), vs.GetDirectory("/dir1/dir2a/dir3"));

            Assert.IsTrue(vs.GetDirectory("/dir1").IsReferencedInStorage);
            Assert.IsTrue(vs.GetDirectory("/dir1/dir2").IsReferencedInStorage);
            Assert.IsTrue(vs.GetDirectory("/dir1/dir2/dir3").IsReferencedInStorage);
            Assert.IsTrue(vs.GetItem("/dir1/dir2/dir3/item1").IsReferencedInStorage);
            Assert.IsTrue(vs.GetDirectory("/dir1/dir2a").IsReferencedInStorage);
            Assert.IsTrue(vs.GetDirectory("/dir1/dir2a/dir3").IsReferencedInStorage);
            Assert.IsFalse(vs.NodeExists("/dir1/dir2a/dir3/item1"));
        }

        [TestMethod]
        public void NodeNames_ReturnsCorrectNodeNames()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            List<VirtualNodeName> expectedNodeNames =
            [
                "subDir1",
                "subDir2",
                "item1",
                "link1"
            ];

            // Act
            IEnumerable<VirtualNodeName> actualNodeNames = dir1.NodeNames;

            // Assert
            CollectionAssert.AreEquivalent(expectedNodeNames, actualNodeNames.ToList());
        }

        [TestMethod]
        public void Nodes_ReturnsCorrectNodes()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            VirtualDirectory subDir1 = new("subDir1");
            VirtualDirectory subDir2 = new("subDir2");
            VirtualItem<object> item1 = new("item1");
            VirtualSymbolicLink link1 = new("link1", "/target");

            dir1.Add(subDir1);
            dir1.Add(subDir2);
            dir1.Add(item1);
            dir1.Add(link1);

            List<VirtualNode> expectedNodes =
            [
                subDir1,
                subDir2,
                item1,
                link1
            ];

            // Act
            IEnumerable<VirtualNode> actualNodes = dir1.Nodes;

            // Assert
            CollectionAssert.AreEquivalent(expectedNodes, actualNodes.ToList());
        }

        [TestMethod]
        public void Count_ReturnsCorrectNodeCount()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualCount = dir1.Count;

            // Assert
            Assert.AreEqual(4, actualCount);
        }

        [TestMethod]
        public void DirectoryCount_ReturnsCorrectDirectoryCount()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualDirectoryCount = dir1.DirectoryCount;

            // Assert
            Assert.AreEqual(2, actualDirectoryCount);
        }

        [TestMethod]
        public void ItemCount_ReturnsCorrectItemCount()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualItemCount = dir1.ItemCount;

            // Assert
            Assert.AreEqual(1, actualItemCount);
        }

        [TestMethod]
        public void SymbolicLinkCount_ReturnsCorrectSymbolicLinkCount()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualSymbolicLinkCount = dir1.SymbolicLinkCount;

            // Assert
            Assert.AreEqual(1, actualSymbolicLinkCount);
        }

        [TestMethod]
        public void NodesView_ReturnsAllNodes_WithDefaultConditions()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            VirtualDirectory subDir1 = new("subDir1");
            VirtualDirectory subDir2 = new("subDir2");
            VirtualItem<object> item1 = new("item1");
            VirtualSymbolicLink link1 = new("link1", "/target");

            dir1.Add(subDir1);
            dir1.Add(subDir2);
            dir1.Add(item1);
            dir1.Add(link1);

            // Act
            IEnumerable<VirtualNode> actualNodesView = dir1.NodesView;

            List<VirtualNode> expectedNodesView =
            [
                subDir1,
                subDir2,
                item1,
                link1
            ];

            // Assert
            CollectionAssert.AreEquivalent(expectedNodesView, actualNodesView.ToList());
        }

        [TestMethod]
        public void NodesView_ReturnsFilteredNodes_ByType()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            VirtualDirectory subDir1 = new("subDir1");
            VirtualDirectory subDir2 = new("subDir2");
            VirtualItem<object> item1 = new("item1");
            VirtualSymbolicLink link1 = new("link1", "/target");

            dir1.Add(subDir1);
            dir1.Add(subDir2);
            dir1.Add(item1);
            dir1.Add(link1);

            // Apply filter to only include directories
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.Directory,
                null,
                null
            );

            // Act
            IEnumerable<VirtualNode> actualNodesView = dir1.NodesView;

            List<VirtualNode> expectedNodesView =
            [
                subDir1,
                subDir2
            ];

            // Assert
            CollectionAssert.AreEquivalent(expectedNodesView, actualNodesView.ToList());
        }

        [TestMethod]
        public void NodesView_ReturnsSortedNodes_ByName()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            VirtualDirectory subDir1 = new("subDir1");
            VirtualDirectory subDir2 = new("subDir2");
            VirtualItem<object> item1 = new("item1");
            VirtualSymbolicLink link1 = new("link1", "/target");

            dir1.Add(subDir1);
            dir1.Add(subDir2);
            dir1.Add(item1);
            dir1.Add(link1);

            // Apply sort condition by name
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                null,
                [
                    new(node => node.Name, true)
                ]
            );

            // Act
            IEnumerable<VirtualNode> actualNodesView = dir1.NodesView;

            List<VirtualNode> expectedNodesView =
            [
                item1,
                link1,
                subDir1,
                subDir2
            ];

            // Assert
            CollectionAssert.AreEqual(expectedNodesView, actualNodesView.ToList());
        }

        [TestMethod]
        public void NodesView_ReturnsGroupedNodes_ByType()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            VirtualDirectory subDir1 = new("subDir1");
            VirtualDirectory subDir2 = new("subDir2");
            VirtualItem<object> item1 = new("item1");
            VirtualSymbolicLink link1 = new("link1", "/target");

            dir1.Add(subDir1);
            dir1.Add(subDir2);
            dir1.Add(item1);
            dir1.Add(link1);

            // Apply group condition by type
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.All,
                new VirtualGroupCondition<VirtualNode, object>(node => node.NodeType),
                null
            );

            // Act
            IEnumerable<VirtualNode> actualNodesView = dir1.NodesView;

            List<VirtualNode> expectedNodesView =
            [
                subDir1,
                subDir2,
                item1,
                link1
            ];

            // Assert
            CollectionAssert.AreEquivalent(expectedNodesView, actualNodesView.ToList());
        }

        [TestMethod]
        public void NodesViewCount_ReturnsCorrectCount_WithDefaultConditions()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualNodesViewCount = dir1.NodesViewCount;

            // Assert
            Assert.AreEqual(4, actualNodesViewCount);
        }

        [TestMethod]
        public void NodesViewCount_ReturnsCorrectCount_WithDirectoryFilter()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Apply filter to only include directories
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.Directory,
                null,
                null
            );

            // Act
            int actualNodesViewCount = dir1.NodesViewCount;

            // Assert
            Assert.AreEqual(2, actualNodesViewCount);
        }

        [TestMethod]
        public void DirectoryViewCount_ReturnsCorrectCount_WithDefaultConditions()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualDirectoryViewCount = dir1.DirectoryViewCount;

            // Assert
            Assert.AreEqual(2, actualDirectoryViewCount);
        }

        [TestMethod]
        public void DirectoryViewCount_ReturnsCorrectCount_WithDirectoryFilter()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Apply filter to only include directories
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.Directory,
                null,
                null
            );

            // Act
            int actualDirectoryViewCount = dir1.DirectoryViewCount;

            // Assert
            Assert.AreEqual(2, actualDirectoryViewCount);
        }

        [TestMethod]
        public void ItemViewCount_ReturnsCorrectCount_WithDefaultConditions()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualItemViewCount = dir1.ItemViewCount;

            // Assert
            Assert.AreEqual(1, actualItemViewCount);
        }

        [TestMethod]
        public void ItemViewCount_ReturnsCorrectCount_WithItemFilter()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Apply filter to only include items
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.Item,
                null,
                null
            );

            // Act
            int actualItemViewCount = dir1.ItemViewCount;

            // Assert
            Assert.AreEqual(1, actualItemViewCount);
        }

        [TestMethod]
        public void SymbolicLinkViewCount_ReturnsCorrectCount_WithDefaultConditions()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Act
            int actualSymbolicLinkViewCount = dir1.SymbolicLinkViewCount;

            // Assert
            Assert.AreEqual(1, actualSymbolicLinkViewCount);
        }

        [TestMethod]
        public void SymbolicLinkViewCount_ReturnsCorrectCount_WithSymbolicLinkFilter()
        {
            // Arrange
            VirtualDirectory dir1 = new("dir1");
            dir1.AddDirectory("subDir1");
            dir1.AddDirectory("subDir2");
            dir1.AddItem<object>("item1");
            dir1.AddSymbolicLink("link1", "/target");

            // Apply filter to only include symbolic links
            VirtualStorageState.SetNodeListConditions(
                VirtualNodeTypeFilter.SymbolicLink,
                null,
                null
            );

            // Act
            int actualSymbolicLinkViewCount = dir1.SymbolicLinkViewCount;

            // Assert
            Assert.AreEqual(1, actualSymbolicLinkViewCount);
        }

        [TestMethod]
        public void Update_ValidDirectory_UpdatesCorrectly()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("OriginalDirectory");
            VirtualDirectory newDirectory = new("NewDirectory");
            newDirectory.AddDirectory("SubDirectory");
            newDirectory.AddItem("Item", new BinaryData([1, 2, 3]));

            // Act
            originalDirectory.Update(newDirectory);

            // Assert
            Assert.AreEqual(originalDirectory.CreatedDate, newDirectory.CreatedDate);
            Assert.IsTrue(originalDirectory.UpdatedDate > newDirectory.UpdatedDate);
            Assert.IsTrue(originalDirectory.NodeExists("SubDirectory"));
            Assert.IsTrue(originalDirectory.NodeExists("Item"));
            Assert.IsFalse(originalDirectory.IsReferencedInStorage);
        }

        [TestMethod]
        public void Update_InvalidNode_ThrowsArgumentException()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("OriginalDirectory");
            VirtualItem<BinaryData> invalidNode = new("ItemNode", new BinaryData([1, 2, 3]));

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => originalDirectory.Update(invalidNode));

            Assert.AreEqual("The specified node [ItemNode] is not of type VirtualDirectory. (Parameter 'node')", ex.Message);

            Debug.WriteLine(ex.Message);
        }

        [TestMethod]
        public void Update_ReferencedDirectory_CreatesClone()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("NewDirectory/SubDirectory", true);
            VirtualDirectory originalDirectory = new("OriginalDirectory");

            // Act
            VirtualDirectory newDirectory = vs.GetDirectory("/NewDirectory");
            originalDirectory.Update(newDirectory);

            // Assert
            Assert.AreNotSame(originalDirectory, newDirectory);
            Assert.IsTrue(originalDirectory.NodeExists("SubDirectory"));
            Assert.IsFalse(originalDirectory.IsReferencedInStorage);
        }

        [TestMethod]
        public void Update_SubNodeExists_UpdatesSubNode()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("OriginalDirectory");
            VirtualDirectory newDirectory = new("NewDirectory");
            originalDirectory.AddDirectory("SubDirectory");
            newDirectory.AddDirectory("SubDirectory");

            DateTime beforeDate = originalDirectory.UpdatedDate;

            // Act
            originalDirectory.Update(newDirectory);

            // Assert
            Assert.IsTrue(originalDirectory.UpdatedDate > beforeDate);
            Assert.IsTrue(originalDirectory.NodeExists("SubDirectory"));
            Assert.IsFalse(originalDirectory.IsReferencedInStorage);
        }

        [TestMethod]
        public void Update_SubNodeNotExists_AddsSubNode()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("OriginalDirectory");
            VirtualDirectory newDirectory = new("NewDirectory");
            newDirectory.AddDirectory("SubDirectory");

            DateTime beforeDate = originalDirectory.UpdatedDate;

            // Act
            originalDirectory.Update(newDirectory);

            // Assert
            Assert.IsTrue(originalDirectory.UpdatedDate > beforeDate);
            Assert.IsTrue(originalDirectory.NodeExists("SubDirectory"));
            Assert.IsFalse(originalDirectory.IsReferencedInStorage);
        }
    }
}
