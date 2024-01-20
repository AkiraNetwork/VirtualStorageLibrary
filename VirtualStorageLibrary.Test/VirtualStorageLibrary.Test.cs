using System.Diagnostics;

namespace VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualPathTest
    {
        [TestMethod]
        public void GetDirectoryPath_ReturnsCorrectPath_ForAbsolutePath()
        {
            // テストデータ
            string absolutePath = "/directory/subdirectory/file";

            // メソッドを実行
            string result = VirtualPath.GetDirectoryPath(absolutePath);

            // 結果を検証
            Assert.AreEqual("/directory/subdirectory", result);
        }

        [TestMethod]
        public void GetDirectoryPath_ReturnsRoot_ForRootPath()
        {
            // テストデータ
            string rootPath = "/";

            // メソッドを実行
            string result = VirtualPath.GetDirectoryPath(rootPath);

            // 結果を検証
            Assert.AreEqual("/", result);
        }

        [TestMethod]
        public void GetDirectoryPath_ReturnsSamePath_ForRelativePath()
        {
            // テストデータ
            string relativePath = "file";

            // メソッドを実行
            string result = VirtualPath.GetDirectoryPath(relativePath);

            // 結果を検証
            Assert.AreEqual("file", result);
        }

        [TestMethod]
        public void GetNodeName_WithFullPath_ReturnsNodeName()
        {
            string path = "/path/to/node";
            string expectedNodeName = "node";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithSingleNodeName_ReturnsSameName()
        {
            string path = "node";
            string expectedNodeName = "node";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithEmptyString_ReturnsEmptyString()
        {
            string path = "";
            string expectedNodeName = "";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithRootPath_ReturnsRootPath()
        {
            string path = "/";
            string expectedNodeName = "/";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithDot_ReturnsDot()
        {
            string path = ".";
            string expectedNodeName = ".";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithDotDot_ReturnsDotDot()
        {
            string path = "..";
            string expectedNodeName = "..";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithRelativePathUsingDot_ReturnsNodeName()
        {
            string path = "./node";
            string expectedNodeName = "node";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void GetNodeName_WithRelativePathUsingDotDot_ReturnsNodeName()
        {
            string path = "../node";
            string expectedNodeName = "node";

            string actualNodeName = VirtualPath.GetNodeName(path);

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void Combine_Path1EndsWithSlash_CombinesCorrectly()
        {
            string path1 = "path/to/directory/";
            string path2 = "file.txt";
            string expected = "path/to/directory/file.txt";

            string result = VirtualPath.Combine(path1, path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_Path1DoesNotEndWithSlash_CombinesCorrectly()
        {
            string path1 = "path/to/directory";
            string path2 = "file.txt";
            string expected = "path/to/directory/file.txt";

            string result = VirtualPath.Combine(path1, path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_Path2StartsWithSlash_CombinesCorrectly()
        {
            string path1 = "path/to/directory";
            string path2 = "/file.txt";
            string expected = "path/to/directory/file.txt";

            string result = VirtualPath.Combine(path1, path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_BothPathsAreEmpty_ReturnsSlash()
        {
            string path1 = "";
            string path2 = "";
            string expected = "/";

            string result = VirtualPath.Combine(path1, path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetParentPath_WithRootPath_ReturnsEmpty()
        {
            string path = "/";
            string expected = "/";

            string actual = VirtualPath.GetParentPath(path);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithSingleLevelPath_ReturnsRoot()
        {
            string path = "/level1";
            string expected = "/";

            string actual = VirtualPath.GetParentPath(path);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithMultiLevelPath_ReturnsParentPath()
        {
            string path = "/level1/level2/level3";
            string expected = "/level1/level2";

            string actual = VirtualPath.GetParentPath(path);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithTrailingSlash_ReturnsParentPath()
        {
            string path = "/level1/level2/level3/";
            string expected = "/level1/level2";

            string actual = VirtualPath.GetParentPath(path);

            Assert.AreEqual(expected, actual);
        }
    }

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

            Assert.IsTrue(directory.NodeExists("NewItem"));
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

            Assert.IsTrue(parentDirectory.NodeExists("ChildDirectory"));
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

            Assert.IsTrue(parentDirectory.NodeExists("ChildDirectory"));
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

            Assert.IsTrue(parentDirectory.NodeExists("OriginalDirectory"));
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

            Assert.IsFalse(directory.NodeExists(nodeName));
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
            Assert.IsTrue(directory.NodeExists(newName));
            Assert.IsFalse(directory.NodeExists("ExistingNode"));
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

    [TestClass]
    public class VirtualStorageTests
    {
        [TestMethod]
        public void ConvertToAbsolutePath_WhenCurrentPathIsRootAndPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/TestDirectory");
            virtualStorage.ChangeDirectory("/");

            var result = virtualStorage.ConvertToAbsolutePath("TestDirectory");

            Assert.AreEqual("/TestDirectory", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathStartsWithSlash_ReturnsSamePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("/test/path");

            Assert.AreEqual("/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("test/path");

            Assert.AreEqual("/root/subdirectory/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDot_ReturnsAbsolutePathWithoutDot()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("./test/path");

            Assert.AreEqual("/root/subdirectory/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDoubleDot_ReturnsParentDirectoryPath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("../test/path");

            Assert.AreEqual("/root/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsMultipleDoubleDots_ReturnsCorrectPath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("../../test/path");

            Assert.AreEqual("/test/path", result);
        }

        [TestMethod]
        public void MakeDirectory_WithValidPath_CreatesDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.MakeDirectory("/test/directory", true);

            // Assert
            Assert.IsTrue(virtualStorage.DirectoryExists("/test/directory"));
        }

        [TestMethod]
        public void MakeDirectory_WithNestedPathAndCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<Exception>(() => virtualStorage.MakeDirectory("/test/directory", false));
        }

        [TestMethod]
        public void MakeDirectory_WithNestedPathAndCreateSubdirectoriesTrue_CreatesDirectories()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.MakeDirectory("/test/directory/subdirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.DirectoryExists("/test/directory/subdirectory"));
        }

        [TestMethod]
        public void MakeDirectory_WithExistingDirectory_DoesNotThrowException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/test/directory", true);

            // Act & Assert
            virtualStorage.MakeDirectory("/test/directory", true);
        }

        [TestMethod]
        public void DirectoryExists_WithExistingDirectory_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/test/directory", true);

            // Act
            bool result = virtualStorage.DirectoryExists("/test/directory");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DirectoryExists_WithNonExistingDirectory_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.DirectoryExists("/test/directory");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DirectoryExists_WithNestedExistingDirectory_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/test/directory/subdirectory", true);

            // Act
            bool result = virtualStorage.DirectoryExists("/test/directory/subdirectory");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DirectoryExists_WithNestedNonExistingDirectory_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/test/directory", true);

            // Act
            bool result = virtualStorage.DirectoryExists("/test/directory/subdirectory");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetDirectory_WithValidPath_ReturnsCorrectDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/TestDirectory/SubDirectory", true);

            // Act
            var directory = virtualStorage.GetDirectory("/TestDirectory/SubDirectory");

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual("SubDirectory", directory.Name);
        }

        [TestMethod]
        public void GetDirectory_WithRootPath_ReturnsRootDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            var directory = virtualStorage.GetDirectory("/");

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual("/", directory.Name); // Assuming the root directory has an empty name
        }

        [TestMethod]
        public void GetDirectory_WithNonexistentPath_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(() => virtualStorage.GetDirectory("/NonexistentDirectory"));
        }

        [TestMethod]
        public void GetDirectory_WithEmptyPath_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.GetDirectory(""));
        }

        [TestMethod]
        public void RemoveDirectory_WhenDirectoryIsEmpty_RemovesDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("ParentDirectory/ChildDirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.DirectoryExists("ParentDirectory/ChildDirectory"));

            // Act
            virtualStorage.RemoveDirectory("ParentDirectory/ChildDirectory");

            // Assert
            Assert.IsFalse(virtualStorage.DirectoryExists("ParentDirectory/ChildDirectory"));
        }

        [TestMethod]
        public void RemoveDirectory_WhenDirectoryIsNotEmptyAndForceDeleteIsFalse_ThrowsInvalidOperationException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("ParentDirectory/ChildDirectory", true);
            virtualStorage.MakeDirectory("ParentDirectory/ChildDirectory/GrandChildDirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.DirectoryExists("ParentDirectory/ChildDirectory"));

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                virtualStorage.RemoveDirectory("ParentDirectory/ChildDirectory");
            });
        }

        [TestMethod]
        public void RemoveDirectory_WhenDirectoryIsNotEmptyAndForceDeleteIsTrue_RemovesDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("ParentDirectory/ChildDirectory", true);
            virtualStorage.MakeDirectory("ParentDirectory/ChildDirectory/GrandChildDirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.DirectoryExists("ParentDirectory/ChildDirectory"));

            // Act
            virtualStorage.RemoveDirectory("ParentDirectory/ChildDirectory", true);

            // Assert
            Assert.IsFalse(virtualStorage.DirectoryExists("ParentDirectory/ChildDirectory"));
        }

        [TestMethod]
        public void AddItem_AddsItemToRootDirectory_WhenPathIsDefault()
        {
            // Arrange
            var storage = new VirtualStorage();
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));

            // Act
            storage.AddItem(item);

            // Assert
            var rootDirectory = storage.GetDirectory(".");
            Assert.IsTrue(rootDirectory.NodeExists("TestItem"));
            Assert.AreEqual(item, rootDirectory["TestItem"]);
        }

        [TestMethod]
        public void AddItem_AddsItemToSpecifiedDirectory_WhenPathIsProvided()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.GetDirectory(".").AddDirectory("TestDirectory");
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));

            // Act
            storage.AddItem(item, "TestDirectory");

            // Assert
            var testDirectory = storage.GetDirectory("TestDirectory");
            Assert.IsTrue(testDirectory.NodeExists("TestItem"));
            Assert.AreEqual(item, testDirectory["TestItem"]);
        }

        [TestMethod]
        public void AddItem_ThrowsException_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var storage = new VirtualStorage();
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(() =>
            {
                storage.AddItem(item, "NonExistentDirectory");
            });
        }

        [TestMethod]
        public void RemoveItem_RemovesItem_WhenItemExists()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string path = "/path/to/existing";
            string itemName = "TestItem";
            string itemFullPath = VirtualPath.Combine(path, itemName);
            virtualStorage.MakeDirectory(path, true);
            var item = new VirtualItem<BinaryData>(itemName, new BinaryData(new byte[] { 1, 2, 3 }));
            virtualStorage.AddItem(item, path);

            // Assert
            Assert.IsTrue(virtualStorage.ItemExists(itemFullPath));

            // Act
            virtualStorage.RemoveItem(itemFullPath);

            // Assert
            Assert.IsFalse(virtualStorage.ItemExists(itemFullPath));
        }

        [TestMethod]
        public void RemoveItem_ThrowsException_WhenItemDoesNotExist()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string path = "/path/to/existing";
            string itemName = "TestItem";
            string itemFullPath = VirtualPath.Combine(path, itemName);
            virtualStorage.MakeDirectory(path, true);

            // Act & Assert
            Assert.ThrowsException<KeyNotFoundException>(() => virtualStorage.RemoveItem(itemFullPath));
        }

        [TestMethod]
        public void EnumerateNodesRecursively_ValidTest()
        {
            var vs = new VirtualStorage();

            vs.MakeDirectory("/Directory1", true);
            vs.MakeDirectory("/Directory1/Directory1_1", true);
            vs.MakeDirectory("/Directory1/Directory1_2", true);
            vs.MakeDirectory("/Directory2", true);
            vs.MakeDirectory("/Directory2/Directory2_1", true);
            vs.MakeDirectory("/Directory2/Directory2_2", true);

            var item_1 = new VirtualItem<BinaryData>("Item_1", new BinaryData(new byte[] { 1, 2, 3 }));
            var item_2 = new VirtualItem<BinaryData>("Item_2", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item_1, "/");
            vs.AddItem(item_2, "/");

            var item1a = new VirtualItem<BinaryData>("Item1a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item1b = new VirtualItem<BinaryData>("Item1b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item1a, "/Directory1");
            vs.AddItem(item1b, "/Directory1");

            var item1_1a = new VirtualItem<BinaryData>("Item1_1a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item1_1b = new VirtualItem<BinaryData>("Item1_1b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item1_1a, "/Directory1/Directory1_1");
            vs.AddItem(item1_1b, "/Directory1/Directory1_1");

            var item1_2a = new VirtualItem<BinaryData>("Item1_2a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item1_2b = new VirtualItem<BinaryData>("Item1_2b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item1_2a, "/Directory1/Directory1_2");
            vs.AddItem(item1_2b, "/Directory1/Directory1_2");

            var item2a = new VirtualItem<BinaryData>("Item2a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2b = new VirtualItem<BinaryData>("Item2b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item2a, "/Directory2");
            vs.AddItem(item2b, "/Directory2");

            var item2_1a = new VirtualItem<BinaryData>("Item2_1a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2_1b = new VirtualItem<BinaryData>("Item2_1b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item2_1a, "/Directory2/Directory2_1");
            vs.AddItem(item2_1b, "/Directory2/Directory2_1");

            var item2_2a = new VirtualItem<BinaryData>("Item2_2a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2_2b = new VirtualItem<BinaryData>("Item2_2b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item2_2a, "/Directory2/Directory2_2");
            vs.AddItem(item2_2b, "/Directory2/Directory2_2");

            Assert.AreEqual(20, vs.EnumerateNodesRecursively("/").Count());            
            foreach (var node in vs.EnumerateNodesRecursively("/"))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }
        }

        [TestMethod]
        public void EnumerateNodeNamesRecursively_ValidTest()
        {
            var vs = new VirtualStorage();

            vs.MakeDirectory("/Directory1", true);
            vs.MakeDirectory("/Directory1/Directory1_1", true);
            vs.MakeDirectory("/Directory1/Directory1_2", true);
            vs.MakeDirectory("/Directory2", true);
            vs.MakeDirectory("/Directory2/Directory2_1", true);
            vs.MakeDirectory("/Directory2/Directory2_2", true);

            var item_1 = new VirtualItem<BinaryData>("Item_1", new BinaryData(new byte[] { 1, 2, 3 }));
            var item_2 = new VirtualItem<BinaryData>("Item_2", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item_1, "/");
            vs.AddItem(item_2, "/");

            var item1a = new VirtualItem<BinaryData>("Item1a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item1b = new VirtualItem<BinaryData>("Item1b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item1a, "/Directory1");
            vs.AddItem(item1b, "/Directory1");

            var item1_1a = new VirtualItem<BinaryData>("Item1_1a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item1_1b = new VirtualItem<BinaryData>("Item1_1b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item1_1a, "/Directory1/Directory1_1");
            vs.AddItem(item1_1b, "/Directory1/Directory1_1");

            var item1_2a = new VirtualItem<BinaryData>("Item1_2a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item1_2b = new VirtualItem<BinaryData>("Item1_2b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item1_2a, "/Directory1/Directory1_2");
            vs.AddItem(item1_2b, "/Directory1/Directory1_2");

            var item2a = new VirtualItem<BinaryData>("Item2a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2b = new VirtualItem<BinaryData>("Item2b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item2a, "/Directory2");
            vs.AddItem(item2b, "/Directory2");

            var item2_1a = new VirtualItem<BinaryData>("Item2_1a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2_1b = new VirtualItem<BinaryData>("Item2_1b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item2_1a, "/Directory2/Directory2_1");
            vs.AddItem(item2_1b, "/Directory2/Directory2_1");

            var item2_2a = new VirtualItem<BinaryData>("Item2_2a", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2_2b = new VirtualItem<BinaryData>("Item2_2b", new BinaryData(new byte[] { 1, 2, 3 }));
            vs.AddItem(item2_2a, "/Directory2/Directory2_2");
            vs.AddItem(item2_2b, "/Directory2/Directory2_2");

            Assert.AreEqual(20, vs.EnumerateNodeNamesRecursively("/").Count());
            foreach (var name in vs.EnumerateNodeNamesRecursively("/"))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }
        }

    }
}
