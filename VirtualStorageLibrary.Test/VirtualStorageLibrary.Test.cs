using System.Diagnostics;

namespace VirtualStorageLibrary.Test
{
    public class SimpleData
    {
        public int Value { get; set; }

        public SimpleData(int value)
        {
            Value = value;
        }
    }

    [TestClass]
    public class VirtualNodeTests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var exception = new VirtualNodeNotFoundException();
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void VirtualItem_Constructor_SetsNameItemAndDates()
        {
            // Arrange
            string name = "TestItem";
            var item = new SimpleData(10);
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;

            // Act
            var virtualItem = new VirtualItem<SimpleData>(name, item, createdDate, updatedDate);

            // Assert
            Assert.AreEqual(name, virtualItem.Name);
            Assert.AreEqual(item, virtualItem.Item);
            Assert.AreEqual(createdDate, virtualItem.CreatedDate);
            Assert.AreEqual(updatedDate, virtualItem.UpdatedDate);
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
    public class VirtualSymbolicLinkTests
    {
        [TestMethod]
        public void VirtualSymbolicLink_Constructor_SetsNameAndTargetPath()
        {
            // Arrange
            string name = "TestLink";
            string targetPath = "/target/path";

            // Act
            var link = new VirtualSymbolicLink(name, targetPath);

            // Assert
            Assert.AreEqual(name, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithDates_SetsNameTargetPathAndDates()
        {
            // Arrange
            string name = "TestLink";
            string targetPath = "/target/path";
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;

            // Act
            var link = new VirtualSymbolicLink(name, targetPath, createdDate, updatedDate);

            // Assert
            Assert.AreEqual(name, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
            Assert.AreEqual(createdDate, link.CreatedDate);
            Assert.AreEqual(updatedDate, link.UpdatedDate);
        }

        [TestMethod]
        public void VirtualSymbolicLink_DeepClone_CreatesExactCopy()
        {
            // Arrange
            string name = "TestLink";
            string targetPath = "/target/path";
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;
            var link = new VirtualSymbolicLink(name, targetPath, createdDate, updatedDate);

            // Act
            var clone = link.DeepClone() as VirtualSymbolicLink;

            // Assert
            Assert.IsNotNull(clone);
            Assert.AreEqual(link.Name, clone.Name);
            Assert.AreEqual(link.TargetPath, clone.TargetPath);
            Assert.AreEqual(link.CreatedDate, clone.CreatedDate);
            Assert.AreEqual(link.UpdatedDate, clone.UpdatedDate);
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
            Assert.AreNotSame(originalItem.Item, clonedItem.Item);
        }

        [TestMethod]
        public void DeepClone_WithNonDeepCloneableItem_ReturnsShallowCopyOfVirtualItem()
        {
            // Arrange
            var originalItem = new VirtualItem<SimpleData>("item", new SimpleData(5));

            // Act
            var clonedItem = ((IDeepCloneable<VirtualItem<SimpleData>>)originalItem).DeepClone();

            // Assert
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.Item.Value, clonedItem.Item.Value);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);

            // SimpleDataインスタンスがシャローコピーされていることを確認
            Assert.AreSame(originalItem.Item, clonedItem.Item);
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
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory_WithNonDeepCloneableItem()
        {
            // Arrange
            var originalDirectory = new VirtualDirectory("original");
            var nonCloneableItem = new SimpleData(10); // SimpleDataはIDeepCloneableを実装していない
            var virtualItem = new VirtualItem<SimpleData>("item", nonCloneableItem);
            originalDirectory.Add(virtualItem);

            // Act
            var clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);

            var originalItem = originalDirectory.Get("item") as VirtualItem<SimpleData>;
            var clonedItem = clonedDirectory.Get("item") as VirtualItem<SimpleData>;
            Assert.IsNotNull(originalItem);
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            
            // SimpleDataインスタンスがシャローコピーされていることを確認
            Assert.AreSame(originalItem.Item, clonedItem.Item);
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
        public void AddItem_AddsNewItemSuccessfully()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });

            directory.AddItem("TestItem", itemData, false);

            Assert.IsTrue(directory.NodeExists("TestItem"));
            var retrievedItem = directory.Get("TestItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(itemData.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_BySimpleData()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var itemData = new SimpleData(5);

            directory.AddItem("TestItem", itemData, false);

            Assert.IsTrue(directory.NodeExists("TestItem"));
            var retrievedItem = directory.Get("TestItem") as VirtualItem<SimpleData>;
            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(itemData.Value, retrievedItem.Item.Value);
        }

        [TestMethod]
        public void AddItem_ThrowsWhenItemAlreadyExistsAndOverwriteIsFalse()
        {
            var directory = new VirtualDirectory("TestDirectory");
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });
            directory.AddItem("TestItem", itemData, false);

            Assert.ThrowsException<InvalidOperationException>(() =>
                directory.AddItem("TestItem", new BinaryData(new byte[] { 4, 5, 6 }), false));
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddItem("TestItem", new BinaryData(new byte[] { 1, 2, 3 }), false);

            var newItemData = new BinaryData(new byte[] { 4, 5, 6 });
            directory.AddItem("TestItem", newItemData, true);

            var retrievedItem = directory.Get("TestItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItemData.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddSymbolicLink_AddsNewLinkSuccessfully()
        {
            var directory = new VirtualDirectory("TestDirectory");

            directory.AddSymbolicLink("TestLink", "/path/to/target", false);

            Assert.IsTrue(directory.NodeExists("TestLink"));
            var retrievedLink = directory.Get("TestLink") as VirtualSymbolicLink;
            Assert.IsNotNull(retrievedLink);
            Assert.AreEqual("/path/to/target", retrievedLink.TargetPath);
        }

        [TestMethod]
        public void AddSymbolicLink_ThrowsWhenLinkAlreadyExistsAndOverwriteIsFalse()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddSymbolicLink("TestLink", "/path/to/oldtarget", false);

            Assert.ThrowsException<InvalidOperationException>(() =>
                directory.AddSymbolicLink("TestLink", "/path/to/newtarget", false));
        }

        [TestMethod]
        public void AddSymbolicLink_OverwritesExistingLinkWhenAllowed()
        {
            var directory = new VirtualDirectory("TestDirectory");
            directory.AddSymbolicLink("TestLink", "/path/to/oldtarget", false);

            directory.AddSymbolicLink("TestLink", "/path/to/newtarget", true);

            var retrievedLink = directory.Get("TestLink") as VirtualSymbolicLink;
            Assert.IsNotNull(retrievedLink);
            Assert.AreEqual("/path/to/newtarget", retrievedLink.TargetPath);
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
            virtualStorage.AddDirectory(existingDirectory, true);

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
            virtualStorage.AddDirectory("/TestDirectory");
            virtualStorage.ChangeDirectory("/");

            var result = virtualStorage.ConvertToAbsolutePath("TestDirectory");

            Assert.AreEqual("/TestDirectory", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathStartsWithSlash_ReturnsSamePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("/test/path");

            Assert.AreEqual("/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("test/path");

            Assert.AreEqual("/root/subdirectory/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDot_ReturnsAbsolutePathWithoutDot()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("./test/path");

            Assert.AreEqual("/root/subdirectory/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDoubleDot_ReturnsParentDirectoryPath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("../test/path");

            Assert.AreEqual("/root/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsMultipleDoubleDots_ReturnsCorrectPath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            var result = virtualStorage.ConvertToAbsolutePath("../../test/path");

            Assert.AreEqual("/test/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithBasePath_ConvertsRelativePathCorrectly()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/base/path", true); // 必要なディレクトリを作成
            string? basePath = "/base/path";
            string relativePath = "relative/to/base";

            // Act
            string result = virtualStorage.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.AreEqual("/base/path/relative/to/base", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithoutBasePath_UsesCurrentPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/current/path", true); // 必要なディレクトリを作成
            virtualStorage.ChangeDirectory("/current/path");
            string relativePath = "relative/path";

            // Act
            string result = virtualStorage.ConvertToAbsolutePath(relativePath, null);

            // Assert
            Assert.AreEqual("/current/path/relative/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithEmptyBasePath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string relativePath = "some/relative/path";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.ConvertToAbsolutePath(relativePath, ""));
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithNullBasePath_UsesCurrentPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/current/path", true); // 必要なディレクトリを作成
            virtualStorage.ChangeDirectory("/current/path");
            string relativePath = "relative/path";
            string? basePath = null;

            // Act
            string result = virtualStorage.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.AreEqual("/current/path/relative/path", result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithAbsolutePath_ReturnsOriginalPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/base/path", true); // 必要なディレクトリを作成
            string absolutePath = "/absolute/path";

            // Act
            string result = virtualStorage.ConvertToAbsolutePath(absolutePath, "/base/path");

            // Assert
            Assert.AreEqual(absolutePath, result);
        }

        [TestMethod]
        public void AddDirectory_WithValidPath_CreatesDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.AddDirectory("/test/directory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory"));
        }

        [TestMethod]
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<Exception>(() => virtualStorage.AddDirectory("/test/directory", false));
        }

        [TestMethod]
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesTrue_CreatesDirectories()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.AddDirectory("/test/directory/subdirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory/subdirectory"));
        }

        [TestMethod]
        public void AddDirectory_WithExistingDirectory_DoesNotThrowException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/test/directory", true);

            // Act & Assert
            virtualStorage.AddDirectory("/test/directory", true);
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsItem()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory("/TestDirectory");
            var item = new BinaryData([1, 2, 3]);
            vs.AddItem("/TestDirectory/Item", item);

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
            vs.AddDirectory("/TestDirectory/TestSubdirectory", true);

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
        public void GetNode_FollowsSymbolicLink_WhenFollowLinksIsTrue()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/dir");
            storage.AddItem("/dir/item", "TestItem");
            storage.AddSymbolicLink("/link", "/dir/item");

            // Act
            var node = storage.GetNode("/link", true);

            // Assert
            Assert.IsInstanceOfType(node, typeof(VirtualItem<string>));
            var item = node as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("TestItem", item.Item);
        }

        [TestMethod]
        public void GetNode_ReturnsSymbolicLink_WhenFollowLinksIsFalse()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/dir");
            storage.AddItem("/dir/item", "TestItem");
            storage.AddSymbolicLink("/link", "/dir/item");

            // Act
            var node = storage.GetNode("/link", false);

            // Assert
            Assert.IsInstanceOfType(node, typeof(VirtualSymbolicLink));
            var link = node as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.AreEqual("/dir/item", link.TargetPath);
        }

        [TestMethod]
        public void GetNode_ThrowsWhenSymbolicLinkIsBroken()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddSymbolicLink("/brokenLink", "/nonexistent/item");

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.GetNode("/brokenLink", true));
        }

        [TestMethod]
        public void GetNode_StopsAtNonDirectoryNode()
        {
            // Arrange: 仮想ストレージにディレクトリとアイテムをセットアップ
            var storage = new VirtualStorage();
            storage.AddDirectory("/dir");
            storage.AddItem("/dir/item", "TestItem");
            // /dir/item はディレクトリではない

            // Act: ディレクトリではないノードの後ろに更にパスが続く場合の挙動をテスト
            var resultNode = storage.GetNode("/dir/item/more", false);

            // Assert: パスの解析は /dir/item で停止し、それ以降は無視されるべき
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            var item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("TestItem", item.Item);
        }

        [TestMethod]
        public void GetNode_FollowsMultipleSymbolicLinksToReachAnItem()
        {
            // Arrange: 仮想ストレージと複数のディレクトリ、シンボリックリンクをセットアップ
            var storage = new VirtualStorage();
            storage.AddDirectory("/dir1");
            storage.AddDirectory("/dir1/dir2");
            storage.AddItem("/dir1/dir2/item", "FinalItem");

            // 最初のシンボリックリンクを /dir1 に追加し、/dir1/dir2 を指す
            storage.AddSymbolicLink("/dir1/link1", "/dir1/dir2");

            // 2番目のシンボリックリンクを /dir1/dir2 に追加し、/dir1/dir2/item を指す
            storage.AddSymbolicLink("/dir1/dir2/link2", "/dir1/dir2/item");

            // Act: 複数のシンボリックリンクを透過的に辿る
            var resultNode = storage.GetNode("/dir1/link1/link2", true);

            // Assert: 結果が VirtualItem<string> 型で、期待したアイテムを持っていることを確認
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            var item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("FinalItem", item.Item);
        }

        [TestMethod]
        public void GetNode_ResolvesRelativePathSymbolicLink()
        {
            // Arrange: 仮想ストレージとディレクトリ、アイテム、相対パスのシンボリックリンクをセットアップ
            var storage = new VirtualStorage();
            storage.AddDirectory("/dir1");
            storage.AddDirectory("/dir1/dir2");
            storage.AddItem("/dir1/dir2/item", "RelativeItem");

            // 相対パスでシンボリックリンクを追加。ここでは、/dir1から/dir1/dir2への相対パスリンクを作成
            storage.AddSymbolicLink("/dir1/relativeLink", "dir2/item");

            // Act: 相対パスのシンボリックリンクを透過的に辿る
            var resultNode = storage.GetNode("/dir1/relativeLink", true);

            // Assert: 結果が VirtualItem<string> 型で、期待したアイテムを持っていることを確認
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            var item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("RelativeItem", item.Item);
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryExists_ReturnsDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/test");
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
            var file = new BinaryData([1, 2, 3]);
            virtualStorage.AddItem("/testfile", file);

            string path = "/testfile";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }
        
        [TestMethod]
        public void GetDirectory_WhenPathIsRelative_ReturnsDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/test");
            string path = "test";

            // Act
            var directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual("test", directory.Name);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            string path = "/NewItem";

            virtualStorage.AddItem(path, item);

            Assert.IsTrue(virtualStorage.ItemExists(path));
            var retrievedItem = virtualStorage.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var originalItem = new BinaryData(new byte[] { 1, 2, 3 });
            var newItem = new BinaryData(new byte[] { 4, 5, 6 });
            string path = "/ExistingItem";
            virtualStorage.AddItem(path, originalItem);

            virtualStorage.AddItem(path, newItem, true);

            var retrievedItem = virtualStorage.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItem.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsArgumentException_WhenPathIsEmpty_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });

            Assert.ThrowsException<ArgumentException>(() => virtualStorage.AddItem("", item));
        }

        [TestMethod]
        public void AddItem_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            string path = "/NonExistentDirectory/Item";

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.AddItem(path, item));
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteIsFalseAndItemExists_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var originalItem = new BinaryData(new byte[] { 1, 2, 3 });
            string path = "/ExistingItem";
            virtualStorage.AddItem(path, originalItem);

            Assert.ThrowsException<InvalidOperationException>(() => virtualStorage.AddItem(path, new BinaryData(new byte[] { 4, 5, 6 }), false));
        }

        [TestMethod]
        public void AddItem_AddsNewItemToCurrentDirectory_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.ChangeDirectory("/"); // カレントディレクトリをルートに設定
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            string itemName = "NewItemInRoot";

            virtualStorage.AddItem(itemName, item); // パスを指定せずにアイテム名のみを渡す

            Assert.IsTrue(virtualStorage.ItemExists("/" + itemName)); // カレントディレクトリにアイテムが作成されていることを確認
            var retrievedItem = virtualStorage.GetNode("/" + itemName) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemUsingRelativePath_WithBinaryData_Corrected()
        {
            var virtualStorage = new VirtualStorage();
            // 事前にディレクトリを作成
            virtualStorage.AddDirectory("/existingDirectory", true); // 既存のディレクトリ
            virtualStorage.AddDirectory("/existingDirectory/subDirectory", true); // 新しく作成するサブディレクトリ
            virtualStorage.ChangeDirectory("/existingDirectory"); // カレントディレクトリを変更

            var item = new BinaryData(new byte[] { 4, 5, 6 });
            string relativePath = "subDirectory/NewItem"; // 相対パスで新しいアイテムを指定

            // 相対パスを使用してアイテムを追加
            virtualStorage.AddItem(relativePath, item, true);

            string fullPath = "/existingDirectory/" + relativePath;
            Assert.IsTrue(virtualStorage.ItemExists(fullPath)); // 相対パスで指定された位置にアイテムが作成されていることを確認
            var retrievedItem = virtualStorage.GetNode(fullPath) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemToSubdirectoryAsCurrentDirectory_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            // サブディレクトリを作成し、カレントディレクトリに設定
            virtualStorage.AddDirectory("/subdirectory", true);
            virtualStorage.ChangeDirectory("/subdirectory");
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            string itemName = "NewItemInSubdirectory";

            // カレントディレクトリにアイテムを追加（パスを指定せずにアイテム名のみを渡す）
            virtualStorage.AddItem(itemName, item);

            // サブディレクトリ内にアイテムが作成されていることを確認
            Assert.IsTrue(virtualStorage.ItemExists("/subdirectory/" + itemName));
            var retrievedItem = virtualStorage.GetNode("/subdirectory/" + itemName) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.Item.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteTargetIsNotAnItem()
        {
            var virtualStorage = new VirtualStorage();
            // テスト用のディレクトリを作成
            virtualStorage.AddDirectory("/testDirectory", true);
            // 同名のサブディレクトリを追加（アイテムの上書き対象として）
            virtualStorage.AddDirectory("/testDirectory/itemName", true);

            var item = new BinaryData(new byte[] { 1, 2, 3 });
            string itemName = "itemName";

            // アイテム上書きを試みる（ただし、実際にはディレクトリが存在するため、アイテムではない）
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddItem("/testDirectory/" + itemName, item, true),
                "上書き対象がアイテムではなくディレクトリの場合、InvalidOperationExceptionが投げられるべきです。");
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
            var item = new BinaryData([1, 2, 3]);
            virtualStorage.AddItem("/testfile", item);

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
            virtualStorage.AddDirectory("/testdir");

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
        public void GetNodes_WithEmptyPath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.GetNodes("", VirtualNodeType.All, true).ToList());
        }

        [TestMethod]
        public void GetNodes_WithNonAbsolutePath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.GetNodes("relative/path", VirtualNodeType.All, true).ToList());
        }
        
        [TestMethod]
        public void GetNodes_ValidTest()
        {
            var vs = new VirtualStorage();

            vs.AddDirectory("/Directory1", true);
            vs.AddDirectory("/Directory1/Directory1_1", true);
            vs.AddDirectory("/Directory1/Directory1_2", true);
            vs.AddDirectory("/Directory2", true);
            vs.AddDirectory("/Directory2/Directory2_1", true);
            vs.AddDirectory("/Directory2/Directory2_2", true);

            var item_1 = new BinaryData([1, 2, 3]);
            var item_2 = new BinaryData([1, 2, 3]);
            vs.AddItem("/Item_1", item_1);
            vs.AddItem("/Item_2", item_2);

            var item1a = new BinaryData([1, 2, 3]);
            var item1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Item1a", item1a);
            vs.AddItem("/Directory1/Item1b", item1b);

            var item1_1a = new BinaryData([1, 2, 3]);
            var item1_1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Directory1_1/Item1_1a", item1_1a);
            vs.AddItem("/Directory1/Directory1_1/Item1_1b", item1_1b);

            var item1_2a = new BinaryData([1, 2, 3]);
            var item1_2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Directory1_2/Item1_2a", item1_2a);
            vs.AddItem("/Directory1/Directory1_2/Item1_2b", item1_2b);

            var item2a = new BinaryData([1, 2, 3]);
            var item2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Item2a", item2a);
            vs.AddItem("/Directory2/Item2b", item2b);

            var item2_1a = new BinaryData([1, 2, 3]);
            var item2_1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Directory2_1/Item2_1a", item2_1a);
            vs.AddItem("/Directory2/Directory2_1/Item2_1b", item2_1b);

            var item2_2a = new BinaryData([1, 2, 3]);
            var item2_2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Directory2_2/Item2_2a", item2_2a);
            vs.AddItem("/Directory2/Directory2_2/Item2_2b", item2_2b);

            Assert.AreEqual(20, vs.GetNodes("/", VirtualNodeType.All, true).Count());
            Debug.WriteLine("\nAll nodes:");
            foreach (var node in vs.GetNodes("/", VirtualNodeType.All, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(6, vs.GetNodes("/", VirtualNodeType.Directory, true).Count());
            Debug.WriteLine("\nDirectories:");
            foreach (var node in vs.GetNodes("/", VirtualNodeType.Directory, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(14, vs.GetNodes("/", VirtualNodeType.Item, true).Count());
            Debug.WriteLine("\nItems:");
            foreach (var node in vs.GetNodes("/", VirtualNodeType.Item, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(2, vs.GetNodes(VirtualNodeType.Directory, false).Count());
            Debug.WriteLine("\nDirectories in /Directory1:");
            foreach (var node in vs.GetNodes(VirtualNodeType.Directory, false))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(2, vs.GetNodes(VirtualNodeType.Item, false).Count());
            Debug.WriteLine("\nItems in /Directory1:");
            foreach (var node in vs.GetNodes(VirtualNodeType.Item, false))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

        }

        [TestMethod]
        public void GetNodesWithPaths_ValidTest()
        {
            var vs = new VirtualStorage();

            vs.AddDirectory("/Directory1", true);
            vs.AddDirectory("/Directory1/Directory1_1", true);
            vs.AddDirectory("/Directory1/Directory1_2", true);
            vs.AddDirectory("/Directory2", true);
            vs.AddDirectory("/Directory2/Directory2_1", true);
            vs.AddDirectory("/Directory2/Directory2_2", true);

            var item_1 = new BinaryData([1, 2, 3]);
            var item_2 = new BinaryData([1, 2, 3]);
            vs.AddItem("/Item_1", item_1);
            vs.AddItem("/Item_2", item_2);

            var item1a = new BinaryData([1, 2, 3]);
            var item1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Item1a", item1a);
            vs.AddItem("/Directory1/Item1b", item1b);

            var item1_1a = new BinaryData([1, 2, 3]);
            var item1_1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Directory1_1/Item1_1a", item1_1a);
            vs.AddItem("/Directory1/Directory1_1/Item1_1b", item1_1b);

            var item1_2a = new BinaryData([1, 2, 3]);
            var item1_2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Directory1_2/Item1_2a", item1_2a);
            vs.AddItem("/Directory1/Directory1_2/Item1_2b", item1_2b);

            var item2a = new BinaryData([1, 2, 3]);
            var item2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Item2a", item2a);
            vs.AddItem("/Directory2/Item2b", item2b);

            var item2_1a = new BinaryData([1, 2, 3]);
            var item2_1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Directory2_1/Item2_1a", item2_1a);
            vs.AddItem("/Directory2/Directory2_1/Item2_1b", item2_1b);

            var item2_2a = new BinaryData([1, 2, 3]);
            var item2_2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Directory2_2/Item2_2a", item2_2a);
            vs.AddItem("/Directory2/Directory2_2/Item2_2b", item2_2b);

            Assert.AreEqual(20, vs.GetNodesWithPaths("/", VirtualNodeType.All, true).Count());
            Debug.WriteLine("\nAll nodes:");
            foreach (var name in vs.GetNodesWithPaths("/", VirtualNodeType.All, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(6, vs.GetNodesWithPaths("/", VirtualNodeType.Directory, true).Count());
            Debug.WriteLine("\nDirectories:");
            foreach (var name in vs.GetNodesWithPaths("/", VirtualNodeType.Directory, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(14, vs.GetNodesWithPaths("/", VirtualNodeType.Item, true).Count());
            Debug.WriteLine("\nItems:");
            foreach (var name in vs.GetNodesWithPaths("/", VirtualNodeType.Item, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(2, vs.GetNodesWithPaths(VirtualNodeType.Directory, false).Count());
            Debug.WriteLine("\nDirectories in /Directory1:");
            foreach (var name in vs.GetNodesWithPaths(VirtualNodeType.Directory, false))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(2, vs.GetNodesWithPaths(VirtualNodeType.Item, false).Count());
            Debug.WriteLine("\nItems in /Directory1:");
            foreach (var name in vs.GetNodesWithPaths(VirtualNodeType.Item, false))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }
        }

        [TestMethod]
        public void CopyFileToFile_OverwritesWhenAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            storage.CopyNode("/sourceFile", "/destinationFile", false, true);

            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.Item.Data);
        }

        [TestMethod]
        public void CopyFileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/sourceFile", new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem("/destinationFile", new BinaryData(new byte[] { 4, 5, 6 }));

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode("/sourceFile", "/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyFileToDirectory_CopiesFileToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/destination");
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            storage.CopyNode("/sourceFile", "/destination/", false, false);

            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destination/sourceFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.Item.Data);
        }

        [TestMethod]
        public void CopyEmptyDirectoryToDirectory_CreatesTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddDirectory("/destinationDir");

            storage.CopyNode("/sourceDir", "/destinationDir/newDir", false, false);

            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/file", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode("/sourceDir", "/destinationDir/newDir", false, false));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithRecursive_CopiesAllContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/file", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");

            storage.CopyNode("/sourceDir", "/destinationDir/newDir", true, false);

            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir/file"));
        }

        [TestMethod]
        public void CopyDirectoryToFile_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode("/sourceDir", "/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyFileToNonExistentDirectory_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.CopyNode("/sourceFile", "/nonExistentDir/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyDirectoryToNonExistentDirectoryWithRecursive_CreatesAllDirectoriesAndCopiesContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                storage.CopyNode("/sourceDir", "/destinationDir/newDir", true, false);
            });
        }

        [TestMethod]
        public void CopyDeepNestedDirectoryToNewLocation_CopiesAllNestedContentsAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/source/deep/nested/dir", true);
            var originalItem = new BinaryData([1, 2, 3]);
            storage.AddItem("/source/deep/nested/dir/nestedFile", originalItem);

            storage.AddDirectory("/destination", true);

            storage.CopyNode("/source/deep", "/destination/deepCopy", true, false);

            var copiedItem = (VirtualItem<BinaryData>)storage.GetNode("/destination/deepCopy/nested/dir/nestedFile");

            Assert.IsNotNull(originalItem);
            Assert.IsNotNull(copiedItem);
            Assert.AreNotSame(originalItem, copiedItem.Item);
        }

        [TestMethod]
        public void CopyMultipleNestedDirectories_CopiesAllDirectoriesAndContentsAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/source/dir1/subdir1", true);
            storage.AddDirectory("/source/dir2/subdir2", true);
            var originalFile1 = new BinaryData([4, 5, 6]);
            var originalFile2 = new BinaryData([7, 8, 9]);
            storage.AddItem("/source/dir1/subdir1/file1", originalFile1);
            storage.AddItem("/source/dir2/subdir2/file2", originalFile2);

            storage.CopyNode("/source", "/destination", true, false);

            var copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode("/destination/dir1/subdir1/file1");
            var copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode("/destination/dir2/subdir2/file2");

            Assert.IsNotNull(originalFile1);
            Assert.IsNotNull(copiedFile1);
            Assert.IsNotNull(originalFile2);
            Assert.IsNotNull(copiedFile2);
            Assert.AreNotSame(originalFile1, copiedFile1.Item);
            Assert.AreNotSame(originalFile2, copiedFile2.Item);
        }

        [TestMethod]
        public void CopyDirectoryWithComplexStructure_CopiesCorrectlyAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/complex/dir1", true);
            storage.AddDirectory("/complex/dir2", true);
            storage.AddDirectory("/complex/dir1/subdir1", true);
            var originalFile1 = new BinaryData([1, 2, 3]);
            var originalFile2 = new BinaryData([4, 5, 6]);
            storage.AddItem("/complex/dir1/file1", originalFile1);
            storage.AddItem("/complex/dir2/file2", originalFile2);

            storage.CopyNode("/complex", "/copiedComplex", true, false);

            var copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode("/copiedComplex/dir1/file1");
            var copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode("/copiedComplex/dir2/file2");

            Assert.IsNotNull(originalFile1);
            Assert.IsNotNull(copiedFile1);
            Assert.IsNotNull(originalFile2);
            Assert.IsNotNull(copiedFile2);
            Assert.AreNotSame(originalFile1, copiedFile1.Item);
            Assert.AreNotSame(originalFile2, copiedFile2.Item);
        }

        [TestMethod]
        public void CopyNode_EmptySourceAndDestination_ThrowsArgumentException()
        {
            var storage = new VirtualStorage();

            // ArgumentException がスローされることを検証
            Assert.ThrowsException<ArgumentException>(() => storage.CopyNode("", "", false, false));
        }

        [TestMethod]
        public void CopyNode_DestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/parentDir/childDir", true);

            // コピー先がコピー元のサブディレクトリである場合
            string sourcePath = "/parentDir";
            string destinationPath = "/parentDir/childDir";

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }

        [TestMethod]
        public void CopyNode_SourceIsSubdirectoryOfDestination_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/parentDir/childDir", true);

            // コピー元がコピー先のサブディレクトリである場合
            string sourcePath = "/parentDir/childDir";
            string destinationPath = "/parentDir";

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CopyNode_RootDirectoryCopy_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            string sourcePath = "/";
            string destinationPath = "/SomeOtherDirectory";

            storage.CopyNode(sourcePath, destinationPath);
        }

        [TestMethod]
        public void CopyNode_NonExistentSource_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();

            // 存在しないソースパスを指定
            string nonExistentSource = "/nonExistentSource";
            string destinationPath = "/destination";

            storage.AddDirectory(destinationPath);

            // VirtualNodeNotFoundException がスローされることを検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.CopyNode(nonExistentSource, destinationPath));
        }

        [TestMethod]
        public void CopyNode_SameSourceAndDestination_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/file", new BinaryData(new byte[] { 1, 2, 3 }));

            // 同じソースとデスティネーションを指定
            string path = "/file";

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(path, path));
        }

        [TestMethod]
        public void CopyNode_ValidTest()
        {
            var vs = new VirtualStorage();

            vs.AddDirectory("/Directory1", true);
            vs.AddDirectory("/Directory1/Directory1_1", true);
            vs.AddDirectory("/Directory1/Directory1_2", true);
            vs.AddDirectory("/Directory2", true);
            vs.AddDirectory("/Directory2/Directory2_1", true);
            vs.AddDirectory("/Directory2/Directory2_2", true);

            var item_1 = new BinaryData([1, 2, 3]);
            var item_2 = new BinaryData([1, 2, 3]);
            vs.AddItem("/Item_1", item_1);
            vs.AddItem("/Item_2", item_2);

            var item1a = new BinaryData([1, 2, 3]);
            var item1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Item1a", item1a);
            vs.AddItem("/Directory1/Item1b", item1b);

            var item1_1a = new BinaryData([1, 2, 3]);
            var item1_1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Directory1_1/Item1_1a", item1_1a);
            vs.AddItem("/Directory1/Directory1_1/Item1_1b", item1_1b);

            var item1_2a = new BinaryData([1, 2, 3]);
            var item1_2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory1/Directory1_2/Item1_2a", item1_2a);
            vs.AddItem("/Directory1/Directory1_2/Item1_2b", item1_2b);

            var item2a = new BinaryData([1, 2, 3]);
            var item2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Item2a", item2a);
            vs.AddItem("/Directory2/Item2b", item2b);

            var item2_1a = new BinaryData([1, 2, 3]);
            var item2_1b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Directory2_1/Item2_1a", item2_1a);
            vs.AddItem("/Directory2/Directory2_1/Item2_1b", item2_1b);

            var item2_2a = new BinaryData([1, 2, 3]);
            var item2_2b = new BinaryData([1, 2, 3]);
            vs.AddItem("/Directory2/Directory2_2/Item2_2a", item2_2a);
            vs.AddItem("/Directory2/Directory2_2/Item2_2b", item2_2b);

            Assert.AreEqual(20, vs.GetNodesWithPaths("/", VirtualNodeType.All, true).Count());

            Debug.WriteLine("コピー前:");
            foreach (var nodeName in vs.GetNodesWithPaths("/", VirtualNodeType.All, true))
            {
                Assert.IsNotNull(nodeName);
                Debug.WriteLine(nodeName);
            }

            vs.AddDirectory("/Destination", true);
            vs.CopyNode("/Directory1", "/Destination", true, false);

            Assert.AreEqual(30, vs.GetNodesWithPaths("/", VirtualNodeType.All, true).Count());

            Debug.WriteLine("コピー後:");
            foreach (var nodeName in vs.GetNodesWithPaths("/", VirtualNodeType.All, true))
            {
                Assert.IsNotNull(nodeName);
                Debug.WriteLine(nodeName);
            }
        }

        [TestMethod]
        public void RemoveNode_ExistingItem_RemovesItem()
        {
            var storage = new VirtualStorage();
            var item = new BinaryData([1, 2, 3]);
            storage.AddItem("/TestItem", item);

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
            storage.AddDirectory("/TestDirectory");

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
            storage.AddDirectory("/TestDirectory");
            var item = new BinaryData([1, 2, 3]);
            storage.AddItem("/TestDirectory/TestItem", item);

            Assert.ThrowsException<InvalidOperationException>(() => storage.RemoveNode("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithRecursive_RemovesDirectoryAndContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/TestDirectory");
            var item = new BinaryData([1, 2, 3]);
            storage.AddItem("/TestDirectory/TestItem", item);

            storage.RemoveNode("/TestDirectory", true);

            Assert.IsFalse(storage.NodeExists("/TestDirectory"));
            Assert.IsFalse(storage.NodeExists("/TestDirectory/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithRecursive_RemovesAllNestedContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/Level1/Level2/Level3", true);
            var item1 = new BinaryData([1, 2, 3]);
            var item2 = new BinaryData([4, 5, 6]);
            storage.AddItem("/Level1/Level2/Level3/Item1", item1);
            storage.AddItem("/Level1/Level2/Item2", item2);

            storage.RemoveNode("/Level1", true);

            Assert.IsFalse(storage.NodeExists("/Level1"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2/Level3/Item1"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2/Item2"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/Level1/Level2/Level3", true);
            var item1 = new BinaryData([1, 2, 3]);
            storage.AddItem("/Level1/Level2/Level3/Item1", item1);

            Assert.ThrowsException<InvalidOperationException>(() => storage.RemoveNode("/Level1"));
        }

        [TestMethod]
        public void RemoveNode_NestedDirectoryWithEmptySubdirectories_RecursiveRemoval()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/Level1/Level2/Level3", true);

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
            storage.AddDirectory(path, true);

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
            storage.AddDirectory(path, true);

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
            storage.AddDirectory(path, true);

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
            storage.AddDirectory("/existing/path", true);
            storage.AddItem("/existing/path/item", new BinaryData([1, 2, 3]));

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
            storage.AddDirectory(path, true);

            // Act
            bool exists = storage.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void MoveNode_FileToFile_OverwritesWhenAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            storage.MoveNode("/sourceFile", "/destinationFile", true);

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.Item.Data);
        }

        [TestMethod]
        public void MoveNode_FileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceFile", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_FileToDirectory_MovesFileToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/destination");
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            storage.MoveNode("/sourceFile", "/destination/", false);

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            Assert.IsTrue(storage.NodeExists("/destination/sourceFile"));
        }

        [TestMethod]
        public void MoveNode_DirectoryToDirectory_MovesDirectoryToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddDirectory("/destinationDir/newDir", true);

            storage.MoveNode("/sourceDir", "/destinationDir/newDir", false);

            Assert.IsFalse(storage.NodeExists("/sourceDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void MoveNode_WhenSourceDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/destinationDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.MoveNode("/nonExistentSource", "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_WhenDestinationIsInvalid_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.MoveNode("/sourceDir", "/nonExistentDestination/newDir", false));
        }

        [TestMethod]
        public void MoveNode_DirectoryWithSameNameExistsAtDestination_ThrowsExceptionRegardlessOfOverwriteFlag()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir", true);
            storage.AddDirectory("/destinationDir/sourceDir", true);

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir", "/destinationDir", false));
            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir", "/destinationDir", true));
        }

        [TestMethod]
        public void MoveNode_DirectoryToFile_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/", "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_OverwritesExistingNodeInDestinationWhenAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/fileName", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");
            storage.AddItem("/destinationDir/fileName", new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            storage.MoveNode("/sourceDir/fileName", "/destinationDir", true); // 上書き許可で移動

            Assert.IsFalse(storage.NodeExists("/sourceDir/fileName")); // 元のファイルが存在しないことを確認
            Assert.IsTrue(storage.NodeExists("/destinationDir/fileName")); // 移動先にファイルが存在することを確認

            var movedItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationDir/fileName");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, movedItem.Item.Data); // 移動先のファイルの中身が正しいことを確認
        }

        [TestMethod]
        public void MoveNode_ThrowsWhenDestinationNodeExistsAndOverwriteIsFalse()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/fileName", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");
            storage.AddItem("/destinationDir/fileName", new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/sourceDir/fileName", "/destinationDir", false)); // 上書き禁止で例外を期待
        }

        [TestMethod]
        public void MoveNode_EmptyDirectory_MovesSuccessfully()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/emptyDir");
            storage.AddDirectory("/newDir/emptyDir", true);
            storage.MoveNode("/emptyDir", "/newDir/emptyDir");

            Assert.IsFalse(storage.NodeExists("/emptyDir"));
            Assert.IsTrue(storage.NodeExists("/newDir/emptyDir/emptyDir"));
        }

        [TestMethod]
        public void MoveNode_MultiLevelDirectory_MovesSuccessfully()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir/subDir1/subDir2", true);
            storage.AddDirectory("/destinationDir");
            storage.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            Assert.IsFalse(storage.NodeExists("/sourceDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/sourceDir/subDir1/subDir2"));
        }

        [TestMethod]
        public void MoveNode_WithInvalidPath_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/validDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.MoveNode("/invalid?Path", "/validDir"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MoveNode_DirectoryToFile_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/destinationFile", new BinaryData([1, 2, 3]));
            storage.MoveNode("/sourceDir", "/destinationFile");
        }

        [TestMethod]
        public void MoveNode_WithinSameDirectory_RenamesNode()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.MoveNode("/sourceFile", "/renamedFile");

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            Assert.IsTrue(storage.NodeExists("/renamedFile"));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, ((VirtualItem<BinaryData>)storage.GetNode("/renamedFile")).Item.Data);
        }

        // 循環参照チェックテスト
        [TestMethod]
        public void MoveNode_WhenDestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/parentDir/subDir", true);

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/parentDir", "/parentDir/subDir"));
        }

        // 移動先と移動元が同じかどうかのチェックテスト
        [TestMethod]
        public void MoveNode_WhenSourceAndDestinationAreSame_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddItem("/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<InvalidOperationException>(() => storage.MoveNode("/file", "/file"));
        }

        // 移動先の親ディレクトリが存在しない場合のテスト
        [TestMethod]
        public void MoveNode_WhenDestinationParentDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory("/existingDir");
            storage.AddItem("/existingDir/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.MoveNode("/existingDir/file", "/nonExistentDir/file"));
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkExists_ReturnsTrue()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/test");
            storage.AddSymbolicLink("/test/link", "/target/path");

            // Act
            bool exists = storage.SymbolicLinkExists("/test/link");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/test");

            // Act
            bool exists = storage.SymbolicLinkExists("/test/nonexistentLink");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void AddSymbolicLink_WhenLinkIsNew_AddsSuccessfully()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/test");

            // Act
            storage.AddSymbolicLink("/test/newLink", "/target/path");

            // Assert
            Assert.IsTrue(storage.SymbolicLinkExists("/test/newLink"));
            var link = storage.GetNode("/test/newLink") as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.AreEqual("/target/path", link.TargetPath);
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsFalseAndLinkExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/test");
            storage.AddSymbolicLink("/test/existingLink", "/old/target/path");

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.AddSymbolicLink("/test/existingLink", "/new/target/path", overwrite: false));
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsTrueAndLinkExists_OverwritesLink()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory("/test");
            storage.AddSymbolicLink("/test/existingLink", "/old/target/path");

            // Act
            storage.AddSymbolicLink("/test/existingLink", "/new/target/path", overwrite: true);

            // Assert
            var link = storage.GetNode("/test/existingLink") as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.AreEqual("/new/target/path", link.TargetPath);
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingItem_ThrowsInvalidOperationException()
        {
            // Arrange
            var storage = new VirtualStorage();
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });

            storage.AddDirectory("/test");
            storage.AddItem("/test/existingItem", itemData); // 既存のアイテムを追加

            storage.AddDirectory("/new/target/path", true); // シンボリックリンクのターゲットとなるディレクトリを追加

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.AddSymbolicLink("/test/existingItem", "/new/target/path", true),
                "既存のアイテム上にシンボリックリンクを追加しようとすると、上書きがtrueでもInvalidOperationExceptionが発生するべきです。");
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            var storage = new VirtualStorage();

            storage.AddDirectory("/test/existingDirectory", true); // 既存のディレクトリを追加

            storage.AddDirectory("/new/target/path", true); // シンボリックリンクのターゲットとなるディレクトリを追加

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.AddSymbolicLink("/test/existingDirectory", "/new/target/path", true),
                "既存のディレクトリ上にシンボリックリンクを追加しようとすると、上書きがtrueでもInvalidOperationExceptionが発生するべきです。");
        }

        [TestMethod]
        public void AddSymbolicLink_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            string nonExistentParentDirectory = "/nonexistent/directory";
            string symbolicLinkPath = $"{nonExistentParentDirectory}/link";
            string targetPath = "/existing/target";

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddSymbolicLink(symbolicLinkPath, targetPath),
                "親ディレクトリが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

    }
}
