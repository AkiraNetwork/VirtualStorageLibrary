using System.Diagnostics;

namespace VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualNodeNotFoundExceptionTest
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var exception = new VirtualNodeNotFoundException();
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void ConstructorWithMessage_CreatesInstanceWithMessage()
        {
            string message = "Test message";
            var exception = new VirtualNodeNotFoundException(message);

            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
        }

        [TestMethod]
        public void ConstructorWithMessageAndInnerException_CreatesInstanceWithMessageAndInnerException()
        {
            string message = "Test message";
            var innerException = new Exception("Inner exception");
            var exception = new VirtualNodeNotFoundException(message, innerException);

            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }
    }

    [TestClass]
    public class VirtualPathTest
    {
        [TestMethod]
        public void NormalizePath_WithAbsolutePath_ReturnsNormalizedPath()
        {
            string path = "/path/to/../directory/./";
            string expected = "/path/directory";

            string result = VirtualPath.NormalizePath(path);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizePath_WithRelativePath_ReturnsNormalizedPath()
        {
            string path = "path/to/../directory/./";
            string expected = "path/directory";

            string result = VirtualPath.NormalizePath(path);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizePath_WithEmptyPath_ThrowsArgumentException()
        {
            string path = "";

            Assert.ThrowsException<ArgumentException>(() => VirtualPath.NormalizePath(path));
        }

        [TestMethod]
        public void NormalizePath_WithInvalidPath_ThrowsInvalidOperationException()
        {
            string path = "/../";

            Assert.ThrowsException<InvalidOperationException>(() => VirtualPath.NormalizePath(path));
        }

        [TestMethod]
        public void NormalizePath_WithPathEndingWithParentDirectory_ReturnsNormalizedPath()
        {
            string path = "aaa/../..";
            string expected = "..";

            string result = VirtualPath.NormalizePath(path);

            Assert.AreEqual(expected, result);
        }

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
        public void Combine_WithPath1Empty_ReturnsPath2()
        {
            string path1 = "";
            string path2 = "/directory/subdirectory";
            string expected = path2;

            string result = VirtualPath.Combine(path1, path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath2Empty_ReturnsPath1()
        {
            string path1 = "/directory/subdirectory";
            string path2 = "";
            string expected = path1;

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

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualItem()
        {
            // Arrange
            var originalItem = new VirtualItem<BinaryData>("item", new BinaryData(new byte[] { 1, 2, 3 }));

            // Act
            var clonedItem = ((IDeepCloneable<VirtualItem<BinaryData>>)originalItem).DeepClone();

            // Assert
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            CollectionAssert.AreEqual(originalItem.Item.Data, clonedItem.Item.Data);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
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
            Assert.AreEqual(originalDirectory.CreatedDate, clonedDirectory.CreatedDate);
            Assert.AreEqual(originalDirectory.UpdatedDate, clonedDirectory.UpdatedDate);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);

            // 各ノードも適切にクローンされていることを検証
            foreach (var name in originalDirectory.NodeNames)
            {
                Assert.AreNotSame(originalDirectory[name], clonedDirectory[name]);
                Assert.AreEqual(originalDirectory[name].Name, clonedDirectory[name].Name);
            }
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory()
        {
            // Arrange
            var originalDirectory = new VirtualDirectory("original");
            originalDirectory.Add(new VirtualItem<BinaryData>("item", new BinaryData(new byte[] { 1, 2, 3 })));

            // Act
            var clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);
            Assert.AreNotSame(originalDirectory.Nodes.First(), clonedDirectory.Nodes.First());
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory_WithSubdirectories()
        {
            // Arrange
            var originalDirectory = new VirtualDirectory("original");
            var subDirectory = new VirtualDirectory("sub");
            originalDirectory.Add(subDirectory);

            // Act
            var clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);
            Assert.AreNotSame(originalDirectory.Nodes.First(), clonedDirectory.Nodes.First());

            var clonedSubDirectory = (VirtualDirectory)clonedDirectory.Nodes.First();
            Assert.AreEqual(subDirectory.Name, clonedSubDirectory.Name);
            Assert.AreNotSame(subDirectory, clonedSubDirectory);
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
        public void Indexer_InvalidKey_ThrowsVirtualNodeNotFoundException()
        {
            var directory = new VirtualDirectory("TestDirectory");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
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
        public void Remove_NonExistentNodeWithoutForce_ThrowsVirtualNodeNotFoundException()
        {
            var directory = new VirtualDirectory("TestDirectory");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
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
        public void Get_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            var directory = new VirtualDirectory("TestDirectory");

            // 存在しないノード名で Get メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
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
        public void Rename_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            var directory = new VirtualDirectory("TestDirectory");

            // 存在しないノード名で Rename メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
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
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
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
        public void ChangeDirectory_WithExistingDirectory_ChangesCurrentPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string existingDirectory = "/path/to/existing/directory";
            virtualStorage.MakeDirectory(existingDirectory, true);

            // Act
            virtualStorage.ChangeDirectory(existingDirectory);

            // Assert
            Assert.AreEqual(existingDirectory, virtualStorage.CurrentPath);
        }

        [TestMethod]
        public void ChangeDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string nonExistentDirectory = "/path/to/nonexistent/directory";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.ChangeDirectory(nonExistentDirectory));
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act and Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.ConvertToAbsolutePath(""));
        }
        
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
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory"));
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
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory/subdirectory"));
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
        public void GetNode_ReturnsCorrectNode_WhenNodeIsItem()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.MakeDirectory("/TestDirectory");
            var item = new VirtualItem<BinaryData>("Item", new BinaryData([1, 2, 3]));
            vs.AddItem(item, "/TestDirectory");

            // メソッドを実行
            var node = vs.GetNode("/TestDirectory/Item");

            // 結果を検証
            Assert.IsNotNull(node);
            Assert.AreEqual("Item", node.Name);
            Assert.IsInstanceOfType(node, typeof(VirtualItem<BinaryData>));
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsDirectory()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.MakeDirectory("/TestDirectory/TestSubdirectory", true);

            // メソッドを実行
            var node = vs.GetNode("/TestDirectory/TestSubdirectory");

            // 結果を検証
            Assert.IsNotNull(node);
            Assert.AreEqual("TestSubdirectory", node.Name);
            Assert.IsInstanceOfType(node, typeof(VirtualDirectory));
        }

        [TestMethod]
        public void GetNode_ThrowsVirtualNodeNotFoundException_WhenDirectoryDoesNotExist()
        {
            // テストデータの設定
            var vs = new VirtualStorage();

            // メソッドを実行し、例外を検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetNode("/NonExistentDirectory"));
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryExists_ReturnsDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/test");
            string path = "/test";

            // Act
            var directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual("test", directory.Name);
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string path = "/nonexistent";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }

        [TestMethod]
        public void GetDirectory_WhenNodeIsNotDirectory_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            var file = new VirtualItem<BinaryData>("testfile", new BinaryData(new byte[] { 1, 2, 3 }));
            virtualStorage.AddItem(file, "/");

            string path = "/testfile";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }
        
        [TestMethod]
        public void GetDirectory_WhenPathIsRelative_ReturnsDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/test");
            string path = "test";

            // Act
            var directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual("test", directory.Name);
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
            var rootDirectory = (VirtualDirectory)storage.GetNode(".");
            Assert.IsTrue(rootDirectory.NodeExists("TestItem"));
            Assert.AreEqual(item, rootDirectory["TestItem"]);
        }

        [TestMethod]
        public void AddItem_AddsItemToSpecifiedDirectory_WhenPathIsProvided()
        {
            // Arrange
            var storage = new VirtualStorage();
            var node = (VirtualDirectory)storage.GetNode(".");
            node.AddDirectory("TestDirectory");
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));

            // Act
            storage.AddItem(item, "TestDirectory");

            // Assert
            var testDirectory = (VirtualDirectory)storage.GetNode("TestDirectory");
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
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                storage.AddItem(item, "NonExistentDirectory");
            });
        }

        [TestMethod]
        public void ItemExists_WhenIntermediateDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.NodeExists("/nonexistent/testfile");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemExists_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            var item = new VirtualItem<BinaryData>("testfile", new BinaryData(new byte[] { 1, 2, 3 }));
            virtualStorage.AddItem(item, "/");

            // Act
            bool result = virtualStorage.NodeExists("/testfile");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.NodeExists("/nonexistent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryExists_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.MakeDirectory("/testdir");

            // Act
            bool result = virtualStorage.NodeExists("/testdir");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.NodeExists("/nonexistent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EnumerateNodesRecursively_WithEmptyPath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.EnumerateNodesRecursively("", true).ToList());
        }

        [TestMethod]
        public void EnumerateNodesRecursively_WithNonAbsolutePath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.EnumerateNodesRecursively("relative/path", true).ToList());
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

        [TestMethod]
        public void CopyFileToFile_OverwritesWhenAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");
            storage.AddItem(new VirtualItem<BinaryData>("destinationFile", new BinaryData(new byte[] { 4, 5, 6 })), "/");

            storage.CopyNode("/sourceFile", "/destinationFile", false, true);

            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.Item.Data);
        }

        [TestMethod]
        public void CopyFileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");
            storage.AddItem(new VirtualItem<BinaryData>("destinationFile", new BinaryData(new byte[] { 4, 5, 6 })), "/");

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode("/sourceFile", "/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyFileToDirectory_CopiesFileToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/destination");
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");

            storage.CopyNode("/sourceFile", "/destination/", false, false);

            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destination/sourceFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.Item.Data);
        }

        [TestMethod]
        public void CopyEmptyDirectoryToDirectory_CreatesTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.MakeDirectory("/destinationDir");

            storage.CopyNode("/sourceDir", "/destinationDir/newDir", false, false);

            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.AddItem(new VirtualItem<BinaryData>("file", new BinaryData(new byte[] { 1, 2, 3 })), "/sourceDir");
            storage.MakeDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode("/sourceDir", "/destinationDir/newDir", false, false));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithRecursive_CopiesAllContents()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.AddItem(new VirtualItem<BinaryData>("file", new BinaryData(new byte[] { 1, 2, 3 })), "/sourceDir");
            storage.MakeDirectory("/destinationDir");

            storage.CopyNode("/sourceDir", "/destinationDir/newDir", true, false);

            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir/file"));
        }

        [TestMethod]
        public void CopyDirectoryToFile_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.AddItem(new VirtualItem<BinaryData>("destinationFile", new BinaryData(new byte[] { 4, 5, 6 })), "/");

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode("/sourceDir", "/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyFileToNonExistentDirectory_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.CopyNode("/sourceFile", "/nonExistentDir/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyDirectoryToNonExistentDirectoryWithRecursive_CreatesAllDirectoriesAndCopiesContents()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.AddItem(new VirtualItem<BinaryData>("file", new BinaryData(new byte[] { 1, 2, 3 })), "/sourceDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                storage.CopyNode("/sourceDir", "/destinationDir/newDir", true, false);
            });
        }

        [TestMethod]
        public void CopyDeepNestedDirectoryToNewLocation_CopiesAllNestedContentsAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/source/deep/nested/dir", true);
            var originalItem = new VirtualItem<BinaryData>("nestedFile", new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem(originalItem, "/source/deep/nested/dir");

            storage.MakeDirectory("/destination", true);

            storage.CopyNode("/source/deep", "/destination/deepCopy", true, false);

            var copiedItem = (VirtualItem<BinaryData>)storage.GetNode("/destination/deepCopy/nested/dir/nestedFile");

            Assert.IsNotNull(originalItem);
            Assert.IsNotNull(copiedItem);
            Assert.AreNotSame(originalItem, copiedItem);
            Assert.AreNotSame(originalItem.Item, copiedItem.Item);
        }

        [TestMethod]
        public void CopyMultipleNestedDirectories_CopiesAllDirectoriesAndContentsAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/source/dir1/subdir1", true);
            storage.MakeDirectory("/source/dir2/subdir2", true);
            var originalFile1 = new VirtualItem<BinaryData>("file1", new BinaryData(new byte[] { 4, 5, 6 }));
            var originalFile2 = new VirtualItem<BinaryData>("file2", new BinaryData(new byte[] { 7, 8, 9 }));
            storage.AddItem(originalFile1, "/source/dir1/subdir1");
            storage.AddItem(originalFile2, "/source/dir2/subdir2");

            storage.CopyNode("/source", "/destination", true, false);

            var copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode("/destination/dir1/subdir1/file1");
            var copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode("/destination/dir2/subdir2/file2");

            Assert.IsNotNull(originalFile1);
            Assert.IsNotNull(copiedFile1);
            Assert.IsNotNull(originalFile2);
            Assert.IsNotNull(copiedFile2);
            Assert.AreNotSame(originalFile1, copiedFile1);
            Assert.AreNotSame(originalFile2, copiedFile2);
            Assert.AreNotSame(originalFile1.Item, copiedFile1.Item);
            Assert.AreNotSame(originalFile2.Item, copiedFile2.Item);
        }

        [TestMethod]
        public void CopyDirectoryWithComplexStructure_CopiesCorrectlyAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/complex/dir1", true);
            storage.MakeDirectory("/complex/dir2", true);
            storage.MakeDirectory("/complex/dir1/subdir1", true);
            var originalFile1 = new VirtualItem<BinaryData>("file1", new BinaryData(new byte[] { 1, 2, 3 }));
            var originalFile2 = new VirtualItem<BinaryData>("file2", new BinaryData(new byte[] { 4, 5, 6 }));
            storage.AddItem(originalFile1, "/complex/dir1");
            storage.AddItem(originalFile2, "/complex/dir2");

            storage.CopyNode("/complex", "/copiedComplex", true, false);

            var copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode("/copiedComplex/dir1/file1");
            var copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode("/copiedComplex/dir2/file2");

            Assert.IsNotNull(originalFile1);
            Assert.IsNotNull(copiedFile1);
            Assert.IsNotNull(originalFile2);
            Assert.IsNotNull(copiedFile2);
            Assert.AreNotSame(originalFile1, copiedFile1);
            Assert.AreNotSame(originalFile2, copiedFile2);
            Assert.AreNotSame(originalFile1.Item, copiedFile1.Item);
            Assert.AreNotSame(originalFile2.Item, copiedFile2.Item);
        }

        [TestMethod]
        public void RemoveNode_ExistingItem_RemovesItem()
        {
            var storage = new VirtualStorage();
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem(item, "/");

            storage.RemoveNode("/TestItem");

            Assert.IsFalse(storage.NodeExists("/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_NonExistingItem_ThrowsException()
        {
            var storage = new VirtualStorage();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.RemoveNode("/NonExistingItem"));
        }

        [TestMethod]
        public void RemoveNode_ExistingEmptyDirectory_RemovesDirectory()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/TestDirectory");

            storage.RemoveNode("/TestDirectory");

            Assert.IsFalse(storage.NodeExists("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_NonExistingDirectory_ThrowsException()
        {
            var storage = new VirtualStorage();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.RemoveNode("/NonExistingDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/TestDirectory");
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem(item, "/TestDirectory");

            Assert.ThrowsException<InvalidOperationException>(() => storage.RemoveNode("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithRecursive_RemovesDirectoryAndContents()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/TestDirectory");
            var item = new VirtualItem<BinaryData>("TestItem", new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem(item, "/TestDirectory");

            storage.RemoveNode("/TestDirectory", true);

            Assert.IsFalse(storage.NodeExists("/TestDirectory"));
            Assert.IsFalse(storage.NodeExists("/TestDirectory/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithRecursive_RemovesAllNestedContents()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/Level1/Level2/Level3", true);
            var item1 = new VirtualItem<BinaryData>("Item1", new BinaryData(new byte[] { 1, 2, 3 }));
            var item2 = new VirtualItem<BinaryData>("Item2", new BinaryData(new byte[] { 4, 5, 6 }));
            storage.AddItem(item1, "/Level1/Level2/Level3");
            storage.AddItem(item2, "/Level1/Level2");

            storage.RemoveNode("/Level1", true);

            Assert.IsFalse(storage.NodeExists("/Level1"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2/Level3/Item1"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2/Item2"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/Level1/Level2/Level3", true);
            var item1 = new VirtualItem<BinaryData>("Item1", new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem(item1, "/Level1/Level2/Level3");

            Assert.ThrowsException<InvalidOperationException>(() => storage.RemoveNode("/Level1"));
        }

        [TestMethod]
        public void RemoveNode_NestedDirectoryWithEmptySubdirectories_RecursiveRemoval()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/Level1/Level2/Level3", true);

            storage.RemoveNode("/Level1", true);

            Assert.IsFalse(storage.NodeExists("/Level1"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2/Level3"));
        }

        [TestMethod]
        public void RemoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode("/");
            });
        }

        [TestMethod]
        public void RemoveNode_CurrentDirectoryDot_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(".");
            });
        }

        [TestMethod]
        public void RemoveNode_ParentDirectoryDotDot_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode("..");
            });
        }

        [TestMethod]
        public void TryGetNode_ReturnsNode_WhenNodeExists()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/existing/path";
            storage.MakeDirectory(path, true);

            // Act
            var node = storage.TryGetNode(path);

            // Assert
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void TryGetNode_ReturnsNull_WhenNodeDoesNotExist()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/non/existing/path";

            // Act
            var node = storage.TryGetNode(path);

            // Assert
            Assert.IsNull(node);
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenNodeExists()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/existing/path";
            storage.MakeDirectory(path, true);

            // Act
            bool exists = storage.NodeExists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void NodeExists_ReturnsFalse_WhenNodeDoesNotExist()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/non/existing/path";

            // Act
            bool exists = storage.NodeExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void DirectoryExists_ReturnsTrue_WhenDirectoryExists()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/existing/path";
            storage.MakeDirectory(path, true);

            // Act
            bool exists = storage.DirectoryExists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DirectoryExists_ReturnsFalse_WhenDirectoryDoesNotExist()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/non/existing/path";

            // Act
            bool exists = storage.DirectoryExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsTrue_WhenItemExists()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/existing/path/item";
            storage.MakeDirectory("/existing/path", true);
            storage.AddItem(new VirtualItem<BinaryData>("item", new BinaryData() {1, 2, 3}), "/existing/path");

            // Act
            bool exists = storage.ItemExists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsFalse_WhenItemDoesNotExist()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/non/existing/path/item";

            // Act
            bool exists = storage.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsFalse_WhenPathIsDirectory()
        {
            // Arrange
            var storage = new VirtualStorage();
            string path = "/existing/path";
            storage.MakeDirectory(path, true);

            // Act
            bool exists = storage.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void MoveNode_FileToFile_OverwritesWhenAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");
            storage.AddItem(new VirtualItem<BinaryData>("destinationFile", new BinaryData(new byte[] { 4, 5, 6 })), "/");

            storage.MoveNode("/sourceFile", "/destinationFile", true);

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.Item.Data);
        }

        [TestMethod]
        public void MoveNode_FileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");
            storage.AddItem(new VirtualItem<BinaryData>("destinationFile", new BinaryData(new byte[] { 4, 5, 6 })), "/");

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceFile", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_FileToDirectory_MovesFileToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/destination");
            storage.AddItem(new VirtualItem<BinaryData>("sourceFile", new BinaryData(new byte[] { 1, 2, 3 })), "/");

            storage.MoveNode("/sourceFile", "/destination/", false);

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            Assert.IsTrue(storage.NodeExists("/destination/sourceFile"));
        }

        [TestMethod]
        public void MoveNode_DirectoryToDirectory_MovesDirectoryToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.MakeDirectory("/destinationDir/newDir", true);

            storage.MoveNode("/sourceDir", "/destinationDir/newDir", false);

            Assert.IsFalse(storage.NodeExists("/sourceDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void MoveNode_WhenSourceDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/destinationDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.MoveNode("/nonExistentSource", "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_WhenDestinationIsInvalid_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.MoveNode("/sourceDir", "/nonExistentDestination/newDir", false));
        }

        [TestMethod]
        public void MoveNode_DirectoryWithSameNameExistsAtDestination_ThrowsExceptionRegardlessOfOverwriteFlag()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir", true);
            storage.MakeDirectory("/destinationDir/sourceDir", true);

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir", "/destinationDir", false));
            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir", "/destinationDir", true));
        }

        [TestMethod]
        public void MoveNode_DirectoryToFile_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/sourceDir");
            storage.AddItem(new VirtualItem<BinaryData>("destinationFile", new BinaryData(new byte[] { 4, 5, 6 })), "/");

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.MakeDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/", "/destinationDir", false));
        }


    }
}
