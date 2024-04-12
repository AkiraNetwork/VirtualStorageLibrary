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
            VirtualNodeNotFoundException exception = new();
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void VirtualItem_Constructor_SetsNameItemAndDates()
        {
            // Arrange
            VirtualNodeName name = "TestItem";
            SimpleData item = new(10);
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;

            // Act
            VirtualItem<SimpleData> virtualItem = new(name, item, createdDate, updatedDate);

            // Assert
            Assert.AreEqual(name, virtualItem.Name);
            Assert.AreEqual(item, virtualItem.ItemData);
            Assert.AreEqual(createdDate, virtualItem.CreatedDate);
            Assert.AreEqual(updatedDate, virtualItem.UpdatedDate);
        }

        [TestMethod]
        public void ConstructorWithMessage_CreatesInstanceWithMessage()
        {
            string message = "Test message";
            VirtualNodeNotFoundException exception = new(message);

            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
        }

        [TestMethod]
        public void ConstructorWithMessageAndInnerException_CreatesInstanceWithMessageAndInnerException()
        {
            string message = "Test message";
            Exception innerException = new("Inner exception");
            VirtualNodeNotFoundException exception = new(message, innerException);

            Assert.IsNotNull(exception);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }
    }

    [TestClass]
    public class VirtualPathTest
    {
        [TestMethod]
        public void ImplicitConversion_FromString_CreatesVirtualPath()
        {
            string path = "/test/path";
            VirtualPath virtualPath = path;

            Assert.AreEqual(path, virtualPath.Path);
        }

        [TestMethod]
        public void ImplicitConversion_ToString_ReturnsPath()
        {
            string path = "/test/path";
            VirtualPath virtualPath = path;

            string result = virtualPath;

            Assert.AreEqual(path, result);
        }
        
        [TestMethod]
        public void NormalizePath_WithAbsolutePath_ReturnsNormalizedPath()
        {
            VirtualPath path = "/path/to/../directory/./";
            VirtualPath expected = "/path/directory";

            VirtualPath result = path.NormalizePath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizePath_WithRelativePath_ReturnsNormalizedPath()
        {
            VirtualPath path = "path/to/../directory/./";
            VirtualPath expected = "path/directory";

            VirtualPath result = path.NormalizePath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizePath_WithEmptyPath()
        {
            VirtualPath path = VirtualPath.Empty;

            Assert.AreEqual(VirtualPath.Empty, path.NormalizePath());
        }

        [TestMethod]
        public void NormalizePath_WithInvalidPath_ThrowsInvalidOperationException()
        {
            VirtualPath path = "/../";

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                path = path.NormalizePath();
            });
        }

        [TestMethod]
        public void NormalizePath_WithPathEndingWithParentDirectory_ReturnsNormalizedPath()
        {
            VirtualPath path = "aaa/../..";
            VirtualPath expected = VirtualPath.DotDot;

            VirtualPath result = path.NormalizePath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DirectoryPath_ReturnsCorrectPath_ForAbsolutePath()
        {
            // テストデータ
            VirtualPath absolutePath = "/directory/subdirectory/file";

            // メソッドを実行(キャッシュなし)
            VirtualPath result = absolutePath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("/directory/subdirectory", result.Path);

            // メソッドを実行(キャッシュあり)
            result = absolutePath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("/directory/subdirectory", result.Path);
        }

        [TestMethod]
        public void DirectoryPath_ReturnsSamePath_ForRelativePath()
        {
            // テストデータ
            VirtualPath relativePath = "file";

            // メソッドを実行
            VirtualPath result = relativePath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("file", result.Path);
        }

        [TestMethod]
        public void NodeName_WithFullPath_ReturnsNodeName()
        {
            // テストデータ
            VirtualPath path = "/path/to/node";
            VirtualNodeName expectedNodeName = "node";

            // メソッドを実行(キャッシュなし)
            VirtualNodeName actualNodeName = path.NodeName;

            // 結果を検証
            Assert.AreEqual(expectedNodeName, actualNodeName);

            // メソッドを実行(キャッシュあり)
            actualNodeName = path.NodeName;

            // 結果を検証
            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithSingleNodeName_ReturnsSameName()
        {
            VirtualPath path = "node";
            VirtualNodeName expectedNodeName = "node";

            VirtualNodeName actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithEmptyString_ReturnsEmptyString()
        {
            VirtualPath path = VirtualPath.Empty;
            VirtualNodeName expectedNodeName = VirtualNodeName.Empty;

            VirtualNodeName actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithDot_ReturnsDot()
        {
            VirtualPath path = VirtualPath.Dot;
            VirtualNodeName expectedNodeName = VirtualNodeName.Dot;

            VirtualNodeName actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithDotDot_ReturnsDotDot()
        {
            VirtualPath path = VirtualPath.DotDot;
            VirtualNodeName expectedNodeName = VirtualNodeName.DotDot;

            VirtualNodeName actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithRelativePathUsingDot_ReturnsNodeName()
        {
            VirtualPath path = "./node";
            VirtualNodeName expectedNodeName = "node";

            VirtualNodeName actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithRelativePathUsingDotDot_ReturnsNodeName()
        {
            VirtualPath path = "../node";
            VirtualNodeName expectedNodeName = "node";

            VirtualNodeName actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void Combine_Path1EndsWithSlash_CombinesCorrectly()
        {
            VirtualPath path1 = "path/to/directory/";
            VirtualPath path2 = "file.txt";
            VirtualPath expected = "path/to/directory/file.txt";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_Path1DoesNotEndWithSlash_CombinesCorrectly()
        {
            VirtualPath path1 = "path/to/directory";
            VirtualPath path2 = "file.txt";
            VirtualPath expected = "path/to/directory/file.txt";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_Path2StartsWithSlash_CombinesCorrectly()
        {
            VirtualPath path1 = "path/to/directory";
            VirtualPath path2 = "/file.txt";
            VirtualPath expected = "path/to/directory/file.txt";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_BothPathsAreEmpty_ReturnsSlash()
        {
            VirtualPath path1 = VirtualPath.Empty;
            VirtualPath path2 = VirtualPath.Empty;
            VirtualPath expected = VirtualPath.Empty;

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath1Empty_ReturnsPath2()
        {
            VirtualPath path1 = VirtualPath.Empty;
            VirtualPath path2 = "/directory/subdirectory";
            VirtualPath expected = path2;

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath2Empty_ReturnsPath1()
        {
            VirtualPath path1 = "/directory/subdirectory";
            VirtualPath path2 = VirtualPath.Empty;
            VirtualPath expected = path1;

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetParentPath_WithSingleLevelPath_ReturnsRoot()
        {
            VirtualPath path = "/level1";
            string expected = VirtualPath.Root;

            VirtualPath actual = path.GetParentPath();

            Assert.AreEqual(expected, (string)actual);
        }

        [TestMethod]
        public void GetParentPath_WithMultiLevelPath_ReturnsParentPath()
        {
            VirtualPath path = "/level1/level2/level3";
            VirtualPath expected = "/level1/level2";

            VirtualPath actual = path.GetParentPath();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithTrailingSlash_ReturnsParentPath()
        {
            VirtualPath path = "/level1/level2/level3/";
            VirtualPath expected = "/level1/level2";

            VirtualPath actual = path.GetParentPath();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithAbsolutePath_ReturnsCorrectParts()
        {
            // Arrange
            VirtualPath virtualPath = "/folder1/folder2/file";
            LinkedList<VirtualNodeName> expected = new(["folder1", "folder2", "file"]);

            // Act
            LinkedList<VirtualNodeName> actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithRelativePath_ReturnsCorrectParts()
        {
            // Arrange
            VirtualPath virtualPath = "folder1/folder2/file";
            LinkedList<VirtualNodeName> expected = new(["folder1", "folder2", "file"]);

            // Act
            LinkedList<VirtualNodeName> actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithEmptySegments_IgnoresEmptySegments()
        {
            // Arrange
            VirtualPath virtualPath = "folder1//folder2///file";
            LinkedList<VirtualNodeName> expected = new(["folder1", "folder2", "file"]);

            // Act
            LinkedList<VirtualNodeName> actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithOnlySlashes_ReturnsEmptyList()
        {
            // Arrange
            VirtualPath virtualPath = "///";
            LinkedList<VirtualPath> expected = new();

            // Act
            LinkedList<VirtualNodeName> actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Equals_WithSamePath_ReturnsTrue()
        {
            // Arrange
            VirtualPath path1 = "/path/to/resource";
            VirtualPath path2 = "/path/to/resource";

            // Act
            bool result = path1.Equals(path2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_WithDifferentPath_ReturnsFalse()
        {
            // Arrange
            VirtualPath path1 = "/path/to/resource";
            VirtualPath path2 = "/path/to/another/resource";

            // Act
            bool result = path1.Equals(path2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            VirtualPath path1 = "/path/to/resource";
            VirtualPath? path2 = null;

            // Act
            bool result = path1.Equals(path2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PartsList_WithAbsolutePath_ReturnsCorrectComponents()
        {
            // テストデータ
            VirtualPath path = "/path/to/directory/file";
            List<VirtualNodeName> expected = (["path", "to", "directory", "file"]);

            // メソッドを実行
            List<VirtualNodeName> result = path.PartsList;

            // 結果を検証
            Assert.AreEqual(expected.Count, result.Count, "The number of parts should match.");
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], result[i], $"The part at index {i} should match.");
            }
        }

        [TestMethod]
        public void PartsList_CachedAfterFirstAccess_DoesNotRegenerateList()
        {
            // テストデータ
            VirtualPath path = "/path/to/directory/file";

            // 最初のアクセスでリストを生成
            List<VirtualNodeName> firstAccessResult = path.PartsList;

            // 2回目のアクセス
            List<VirtualNodeName> secondAccessResult = path.PartsList;

            // キャッシュされたオブジェクトが再利用されているか検証
            Assert.AreSame(firstAccessResult, secondAccessResult, "The PartsList should be cached and reused on subsequent accesses.");
        }

        [TestMethod]
        public void CompareTo_ReturnsNegative_WhenPathIsLexicographicallyBeforeOther()
        {
            VirtualPath path1 = "a/b";
            VirtualPath path2 = "b/a";
            Assert.IsTrue(path1.CompareTo(path2) < 0, "path1 should be lexicographically before path2");
        }

        [TestMethod]
        public void CompareTo_ReturnsZero_WhenPathsAreEqual()
        {
            VirtualPath path1 = "same/path";
            VirtualPath path2 = "same/path";
            Assert.AreEqual(0, path1.CompareTo(path2), "Paths that are equal should return 0");
        }

        [TestMethod]
        public void CompareTo_ReturnsPositive_WhenPathIsLexicographicallyAfterOther()
        {
            VirtualPath path1 = "c/d";
            VirtualPath path2 = "a/b";
            Assert.IsTrue(path1.CompareTo(path2) > 0, "path1 should be lexicographically after path2");
        }

        [TestMethod]
        public void GetRelativePath_BasePathEqualToPath_ReturnsDot()
        {
            // テストデータ
            VirtualPath basePath = "/path/to/directory";
            VirtualPath path = "/path/to/directory";

            // メソッドを実行
            VirtualPath result = path.GetRelativePath(basePath);

            // 結果を検証
            Assert.AreEqual(VirtualPath.Dot, result, "相対パスが'.'であるべきです。");
        }

        [TestMethod]
        public void GetRelativePath_WithPathSubdirectoryOfBasePath_ReturnsRelativePath()
        {
            VirtualPath basePath = "/path/to";
            VirtualPath path = "/path/to/directory/file";

            VirtualPath result = path.GetRelativePath(basePath);

            Assert.AreEqual("directory/file", result.Path);
        }

        [TestMethod]
        public void GetRelativePath_WithBasePathSubdirectoryOfPath_ReturnsRelativePathUsingDotDot()
        {
            VirtualPath basePath = "/path/to/directory/file";
            VirtualPath path = "/path/to";

            VirtualPath result = path.GetRelativePath(basePath);

            Assert.AreEqual("../..", result.Path);
        }

        [TestMethod]
        public void GetRelativePath_WithNonOverlappingPaths_ThrowsInvalidOperationException()
        {
            VirtualPath basePath = "/base/path";
            VirtualPath path = "/different/path";

            VirtualPath result = path.GetRelativePath(basePath);

            Assert.AreEqual(path.Path, result.Path);
        }

        [TestMethod]
        public void GetRelativePath_WithEmptyBasePathAndAbsoluteCurrentPath_ThrowsInvalidOperationException()
        {
            VirtualPath basePath = VirtualPath.Empty;
            VirtualPath path = "/path/to/directory";

            Assert.ThrowsException<InvalidOperationException>(() => path.GetRelativePath(basePath));
        }

        [TestMethod]
        public void GetRelativePath_WithAbsolutePathAndRelativeBasePath_ThrowsInvalidOperationException()
        {
            VirtualPath basePath = "relative/path";
            VirtualPath path = "/absolute/path";

            Assert.ThrowsException<InvalidOperationException>(() => path.GetRelativePath(basePath));
        }
    }

    [TestClass]
    public class VirtualSymbolicLinkTests
    {
        [TestMethod]
        public void VirtualSymbolicLink_Constructor_SetsNameAndTargetPath()
        {
            // Arrange
            VirtualNodeName nodeName = "TestLink";
            VirtualPath targetPath = "/target/path";

            // Act
            VirtualSymbolicLink link = new(nodeName, targetPath);

            // Assert
            Assert.AreEqual(nodeName, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithDates_SetsNameTargetPathAndDates()
        {
            // Arrange
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "/target/path";
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;

            // Act
            VirtualSymbolicLink link = new(name, targetPath, createdDate, updatedDate);

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
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "/target/path";
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;
            VirtualSymbolicLink link = new(name, targetPath, createdDate, updatedDate);

            // Act
            VirtualSymbolicLink? clone = link.DeepClone() as VirtualSymbolicLink;

            // Assert
            Assert.IsNotNull(clone);
            Assert.AreEqual(link.Name, clone.Name);
            Assert.AreEqual(link.TargetPath, clone.TargetPath);
            Assert.AreEqual(link.CreatedDate, clone.CreatedDate);
            Assert.AreEqual(link.UpdatedDate, clone.UpdatedDate);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            VirtualSymbolicLink symbolicLink = new("LinkToRoot", VirtualPath.Root);

            // Act
            string result = symbolicLink.ToString();
            Debug.WriteLine(result);
            Console.WriteLine(result);

            // Assert
            Assert.AreEqual("Symbolic Link: LinkToRoot -> /", result);
        }
    }

    [TestClass]
    public class VirtualItemTest
    {
        [TestMethod]
        public void VirtualItemConstructor_CreatesObjectCorrectly()
        {
            // テストデータ
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new BinaryData(testData);

            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> virtualItem = new(name, binaryData);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(virtualItem);
            Assert.AreEqual(name, virtualItem.Name);
            Assert.AreEqual(binaryData, virtualItem.ItemData);
            CollectionAssert.AreEqual(virtualItem.ItemData.Data, testData);
        }

        [TestMethod]
        public void VirtualItemDeepClone_CreatesDistinctCopyWithSameData()
        {
            // BinaryData オブジェクトを作成
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> originalItem = new("TestItem", new(testData));

            // DeepClone メソッドを使用してクローンを作成
            VirtualItem<BinaryData>? clonedItem = originalItem.DeepClone() as VirtualItem<BinaryData>;

            // クローンが正しく作成されたか検証
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            Assert.AreNotSame(originalItem.ItemData, clonedItem.ItemData);

            // BinaryData の Data プロパティが適切にクローンされていることを検証
            CollectionAssert.AreEqual(originalItem.ItemData.Data, clonedItem.ItemData.Data);
            Assert.AreNotSame(originalItem.ItemData.Data, clonedItem.ItemData.Data);
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualItem()
        {
            // Arrange
            VirtualItem<BinaryData> originalItem = new("item", [1, 2, 3]);

            // Act
            VirtualItem<BinaryData> clonedItem = ((IDeepCloneable<VirtualItem<BinaryData>>)originalItem).DeepClone();

            // Assert
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            CollectionAssert.AreEqual(originalItem.ItemData.Data, clonedItem.ItemData.Data);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            Assert.AreNotSame(originalItem.ItemData, clonedItem.ItemData);
        }

        [TestMethod]
        public void DeepClone_WithNonDeepCloneableItem_ReturnsShallowCopyOfVirtualItem()
        {
            // Arrange
            VirtualItem<SimpleData> originalItem = new("item", new(5));

            // Act
            VirtualItem<SimpleData> clonedItem = ((IDeepCloneable<VirtualItem<SimpleData>>)originalItem).DeepClone();

            // Assert
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.ItemData.Value, clonedItem.ItemData.Value);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);

            // SimpleDataインスタンスがシャローコピーされていることを確認
            Assert.AreSame(originalItem.ItemData, clonedItem.ItemData);
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithInt()
        {
            int value = 10;
            VirtualItem<int> item = new("TestItem", value);

            string result = item.ToString();
            Debug.WriteLine(result);
            
            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithBinaryDataOfLong()
        {
            BinaryData data = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F];
            VirtualItem<BinaryData> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithBinaryDataOfShort()
        {
            BinaryData data = [0x01, 0x02, 0x03];
            VirtualItem<BinaryData> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithSimpleData()
        {
            SimpleData data = new(10);
            VirtualItem<SimpleData> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        struct SimpleStruct
        {
            public int Value { get; set; }
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithStruct()
        {
            SimpleStruct data = new SimpleStruct { Value = 10 };

            VirtualItem<SimpleStruct> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }
    }

    [TestClass]
    public class VirtualDirectoryTests
    {
        [TestMethod]
        public void VirtualDirectoryConstructor_CreatesObjectCorrectly()

        {
            // VirtualDirectory オブジェクトを作成
            VirtualNodeName name = "TestDirectory";
            VirtualDirectory virtualDirectory = new(name);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(virtualDirectory);
            Assert.AreEqual(name, virtualDirectory.Name);
            Assert.AreEqual(0, virtualDirectory.Count);
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
            Assert.AreEqual(originalDirectory.CreatedDate, clonedDirectory.CreatedDate);
            Assert.AreEqual(originalDirectory.UpdatedDate, clonedDirectory.UpdatedDate);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);

            // 各ノードも適切にクローンされていることを検証
            foreach (VirtualNodeName name in originalDirectory.NodeNames)
            {
                Assert.AreNotSame(originalDirectory[name], clonedDirectory[name]);
                Assert.AreEqual(originalDirectory[name].Name, clonedDirectory[name].Name);
            }
        }

        [TestMethod]
        public void DirectoryCount_WithMultipleDirectories_ReturnsCorrectCount()
        {
            VirtualStorage storage = new();
            storage.Root.AddDirectory("dir1");
            storage.Root.AddDirectory("dir2");

            Assert.AreEqual(2, storage.Root.DirectoryCount, "ルートディレクトリ内のディレクトリ数が正しくありません。");
        }

        [TestMethod]
        public void ItemCount_WithMultipleItems_ReturnsCorrectCount()
        {
            VirtualStorage storage = new();
            storage.Root.AddItem("item1", new object());
            storage.Root.AddItem("item2", new object());

            Assert.AreEqual(2, storage.Root.ItemCount, "ルートディレクトリ内のアイテム数が正しくありません。");
        }

        [TestMethod]
        public void SymbolicLinkCount_WithMultipleSymbolicLinks_ReturnsCorrectCount()
        {
            VirtualStorage storage = new();
            storage.Root.Add(new VirtualSymbolicLink("link1", "/path/to/target1"));
            storage.Root.Add(new VirtualSymbolicLink("link2", "/path/to/target2"));

            Assert.AreEqual(2, storage.Root.SymbolicLinkCount, "ルートディレクトリ内のシンボリックリンク数が正しくありません。");
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("original");
            originalDirectory.Add(new VirtualItem<BinaryData>("item", [1, 2, 3]));

            // Act
            VirtualDirectory clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

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
            VirtualDirectory originalDirectory = new("original");
            VirtualDirectory subDirectory = new("sub");
            originalDirectory.Add(subDirectory);

            // Act
            VirtualDirectory clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);
            Assert.AreNotSame(originalDirectory.Nodes.First(), clonedDirectory.Nodes.First());

            VirtualDirectory clonedSubDirectory = (VirtualDirectory)clonedDirectory.Nodes.First();
            Assert.AreEqual(subDirectory.Name, clonedSubDirectory.Name);
            Assert.AreNotSame(subDirectory, clonedSubDirectory);
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory_WithNonDeepCloneableItem()
        {
            // Arrange
            VirtualDirectory originalDirectory = new("original");
            SimpleData nonCloneableItem = new(10);
            VirtualItem<SimpleData> virtualItem = new("item", nonCloneableItem);
            originalDirectory.Add(virtualItem);

            // Act
            VirtualDirectory clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);

            VirtualItem<SimpleData>? originalItem = originalDirectory.Get("item") as VirtualItem<SimpleData>;
            VirtualItem<SimpleData>? clonedItem = clonedDirectory.Get("item") as VirtualItem<SimpleData>;
            Assert.IsNotNull(originalItem);
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            
            // SimpleDataインスタンスがシャローコピーされていることを確認
            Assert.AreSame(originalItem.ItemData, clonedItem.ItemData);
        }
        
        [TestMethod]
        public void Add_NewNode_AddsNodeCorrectly()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> newNode = new("NewItem", new(testData));

            directory.Add(newNode);

            Assert.IsTrue(directory.NodeExists("NewItem"));
            Assert.AreEqual(newNode, directory["NewItem"]);
            CollectionAssert.AreEqual(testData, ((BinaryData)((VirtualItem<BinaryData>)directory["NewItem"]).ItemData).Data);
        }

        [TestMethod]
        public void Add_ExistingNodeWithoutOverwrite_ThrowsInvalidOperationException()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> originalNode = new("OriginalItem", new BinaryData(testData));
            directory.Add(originalNode);

            VirtualItem<BinaryData> newNode = new("OriginalItem", new BinaryData(testData));

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.Add(newNode);
            });
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
            CollectionAssert.AreEqual(newTestData, ((BinaryData)((VirtualItem<BinaryData>)directory["OriginalItem"]).ItemData).Data);
        }

        [TestMethod]
        public void Add_NewDirectory_AddsDirectoryCorrectly()
        {
            VirtualDirectory parentDirectory = new VirtualDirectory("ParentDirectory");
            VirtualDirectory childDirectory = new VirtualDirectory("ChildDirectory");

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

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.Add(newDirectory);
            });
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
        public void AddItem_AddsNewItemSuccessfully()
        {
            VirtualDirectory directory = new("TestDirectory");
            BinaryData itemData = [1, 2, 3];

            directory.AddItem("TestItem", itemData, false);

            Assert.IsTrue(directory.NodeExists("TestItem"));
            VirtualItem<BinaryData>? retrievedItem = directory.Get("TestItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(itemData.Data, retrievedItem.ItemData.Data);
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
            Assert.AreEqual(itemData.Value, retrievedItem.ItemData.Value);
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
            CollectionAssert.AreEqual(newItemData.Data, retrievedItem.ItemData.Data);
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
            VirtualItem<BinaryData> node = new("ItemData", new BinaryData());
            directory.Add(node);

            VirtualNode result = directory["ItemData"];

            Assert.AreEqual(node, result);
        }

        [TestMethod]
        public void Indexer_InvalidKey_ThrowsVirtualNodeNotFoundException()
        {
            VirtualDirectory directory = new("TestDirectory");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                VirtualNode result = directory["InvalidKey"];
            });
        }

        [TestMethod]
        public void Indexer_Setter_UpdatesNode()
        {
            VirtualDirectory directory = new("TestDirectory");
            VirtualItem<BinaryData> newNode = new("NewItem", new BinaryData());

            directory["NewItemKey"] = newNode;
            VirtualNode result = directory["NewItemKey"];

            Assert.AreEqual(newNode, result);
        }

        [TestMethod]
        public void Remove_NonExistentNodeWithoutForce_ThrowsVirtualNodeNotFoundException()
        {
            VirtualDirectory directory = new("TestDirectory");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.Remove("NonExistentNode");
            });
        }

        [TestMethod]
        public void Remove_ExistingNode_RemovesNode()
        {
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualNodeName nodeName = "ExistingNode";
            VirtualItem<BinaryData> node = new(nodeName, new BinaryData(testData));
            directory.Add(node);

            directory.Remove(nodeName);

            Assert.IsFalse(directory.NodeExists(nodeName));
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
            VirtualNode retrievedNode = directory.Get("ExistingNode");

            // 取得したノードが期待通りであることを確認
            Assert.AreEqual(existingNode, retrievedNode);
        }

        [TestMethod]
        public void Get_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            VirtualDirectory directory = new("TestDirectory");

            // 存在しないノード名で Get メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                VirtualNode retrievedNode = directory.Get("NonExistingNode");
            });
        }

        [TestMethod]
        public void Rename_ExistingNode_RenamesNodeCorrectly()
        {
            // VirtualDirectory オブジェクトを作成し、ノードを追加
            VirtualNodeName oldName = "ExistingNode";
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> existingNode = new(oldName, new BinaryData(testData));
            directory.Add(existingNode);

            // Rename メソッドを使用してノードの名前を変更
            VirtualNodeName newName = "RenamedNode";
            directory.Rename(oldName, newName);

            // 名前が変更されたノードが存在し、元のノードが存在しないことを確認
            Assert.IsTrue(directory.NodeExists(newName));
            Assert.IsFalse(directory.NodeExists(oldName));
        }

        [TestMethod]
        public void Rename_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            VirtualDirectory directory = new("TestDirectory");

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
            VirtualDirectory directory = new("TestDirectory");
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> existingNode1 = new("ExistingNode1", new BinaryData(testData));
            VirtualItem<BinaryData> existingNode2 = new("ExistingNode2", new BinaryData(testData));
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
            VirtualDirectory directory = new("TestDirectory");

            IEnumerable<VirtualNodeName> nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(0, nodeNames.Count());
        }

        [TestMethod]
        public void NodeNames_DirectoryWithNodes_ReturnsNodeNames()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");

            IEnumerable<VirtualNodeName> nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(2, nodeNames.Count());
            CollectionAssert.Contains(nodeNames.ToList(), new VirtualNodeName("Node1"));
            CollectionAssert.Contains(nodeNames.ToList(), new VirtualNodeName("Node2"));
        }

        [TestMethod]
        public void NodeNames_DirectoryWithNodesAfterRemovingOne_ReturnsRemainingNodeNames()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");
            directory.Remove("Node1");

            IEnumerable<VirtualNodeName> nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(1, nodeNames.Count());
            CollectionAssert.DoesNotContain(nodeNames.ToList(), new VirtualNodeName("Node1"));
            CollectionAssert.Contains(nodeNames.ToList(), new VirtualNodeName("Node2"));
        }

        [TestMethod]
        public void Nodes_EmptyDirectory_ReturnsEmpty()
        {
            VirtualDirectory directory = new("TestDirectory");

            IEnumerable<VirtualNode> nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(0, nodes.Count());
        }

        [TestMethod]
        public void Nodes_DirectoryWithNodes_ReturnsNodes()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");

            IEnumerable<VirtualNode> nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(2, nodes.Count());
            CollectionAssert.Contains(nodes.ToList(), directory["Node1"]);
            CollectionAssert.Contains(nodes.ToList(), directory["Node2"]);
        }

        [TestMethod]
        public void Nodes_DirectoryWithNodesAfterRemovingOne_ReturnsRemainingNodes()
        {
            VirtualDirectory directory = new("TestDirectory");
            directory.AddDirectory("Node1");
            directory.AddDirectory("Node2");
            directory.Remove("Node1");

            IEnumerable<VirtualNode> nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(1, nodes.Count());
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                // "Node1"が正しく削除されていることを確認
                VirtualNode node = directory["Node1"];
            });
            CollectionAssert.Contains(nodes.ToList(), directory["Node2"]);
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
            VirtualStorage storage = new();

            // 仮想ディレクトリとアイテムを追加
            storage.AddDirectory("/dir2");
            storage.AddItem("/item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddItem("/item1", new BinaryData([1, 2, 3]));

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[1].Name);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[2].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[3].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeAsc()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir2");
            storage.AddItem("/item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddItem("/item1", new BinaryData([1, 2, 3]));

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.All;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, true);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.Name, true)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[1].Name);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[2].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[3].Name);
        }

        [TestMethod]
        public void GetNodeList_CreatedDateAscAndTypeAsc()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualItem<BinaryData> item1 = new("item1", new BinaryData([1, 2, 3]));
            VirtualItem<BinaryData> item2 = new("item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddDirectory("/dir2");
            storage.AddItem("/", item1);
            storage.AddItem("/", item2);

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.All;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, true);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.CreatedDate, true)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[1].Name);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[2].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[3].Name);
        }

        [TestMethod]
        public void GetNodeList_CreatedDateDesAndTypeAsc()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualItem<BinaryData> item1 = new("item1", new BinaryData([1, 2, 3]));
            VirtualItem<BinaryData> item2 = new("item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddDirectory("/dir2");
            storage.AddItem("/", item1);
            storage.AddItem("/", item2);

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.All;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, true);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.CreatedDate, false)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[1].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[2].Name);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[3].Name);
        }

        [TestMethod]
        public void GetNodeList_NameDesAndTypeAsc()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir2");
            storage.AddItem("/item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddItem("/item1", new BinaryData([1, 2, 3]));

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.All;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, true);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.Name, false)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[1].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[2].Name);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[3].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDes()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir2");
            storage.AddItem("/item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddItem("/item1", new BinaryData([1, 2, 3]));

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.All;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, false);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.Name, true)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[1].Name);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[2].Name);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[3].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDesWithOnlyDir()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir2");
            storage.AddItem("/item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddItem("/item1", new BinaryData([1, 2, 3]));

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.Directory;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, true);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.Name, true)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(2, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("dir1"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("dir2"), nodes[1].Name);
        }

        [TestMethod]
        public void GetNodeList_NameAscAndTypeDesWithOnlyItem()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir2");
            storage.AddItem("/item2", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/dir1");
            storage.AddItem("/item1", new BinaryData([1, 2, 3]));

            // 条件を設定
            VirtualStorageSettings.Settings.NodeTypeFilter = VirtualNodeTypeFilter.Item;
            VirtualStorageSettings.Settings.NodeGroupCondition = new(node => node.NodeType, true);
            VirtualStorageSettings.Settings.NodeSortConditions = new()
            {
                new (node => node.Name, true)
            };

            // テスト対象のディレクトリを取得
            VirtualDirectory directory = storage.GetDirectory("/");

            // Act
            List<VirtualNode> nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (VirtualNode node in nodes)
            {
                Debug.WriteLine(node.Name);
            }
            Assert.AreEqual(2, nodes.Count);
            Assert.AreEqual(new VirtualNodeName("item1"), nodes[0].Name);
            Assert.AreEqual(new VirtualNodeName("item2"), nodes[1].Name);
        }
    }

    [TestClass]
    public class VirtualStorageTests
    {
        [TestMethod]
        public void ChangeDirectory_WithExistingDirectory_ChangesCurrentPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            VirtualPath existingDirectory = "/path/to/existing/directory";
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
            VirtualStorage virtualStorage = new();
            VirtualPath nonExistentDirectory = "/path/to/nonexistent/directory";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.ChangeDirectory(nonExistentDirectory));
        }

        [TestMethod]
        public void ChangeDirectory_WithSymbolicLink_ChangesCurrentPathToTargetDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            VirtualPath baseDirectoryPath = "/path/to/";
            VirtualPath targetDirectory = "/real/target/directory";
            VirtualPath symbolicLink = "/path/to/symlink";
            virtualStorage.AddDirectory(baseDirectoryPath, true); // ベースディレクトリを追加
            virtualStorage.AddDirectory(targetDirectory, true); // 実際のターゲットディレクトリを追加
            virtualStorage.AddSymbolicLink(symbolicLink, targetDirectory); // ベースディレクトリ内にシンボリックリンクを追加

            // Act
            virtualStorage.ChangeDirectory(symbolicLink); // シンボリックリンクを通じてディレクトリ変更を試みる

            // Assert
            Assert.AreEqual(symbolicLink, virtualStorage.CurrentPath); // シンボリックリンクのターゲットがカレントディレクトリになっているか検証
        }

        [TestMethod]
        public void ChangeDirectory_WithDotDotInPath_NormalizesPathAndChangesCurrentPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            VirtualPath baseDirectory = "/path/to";
            VirtualPath subDirectory = "/path/to/subdirectory";
            virtualStorage.AddDirectory(baseDirectory, true); // ベースディレクトリを追加
            virtualStorage.AddDirectory(subDirectory, true); // サブディレクトリを追加

            // サブディレクトリに移動し、そこから親ディレクトリに戻るパスを設定
            VirtualPath pathToChange = "/path/to/subdirectory/../";

            // Act
            virtualStorage.ChangeDirectory(pathToChange);

            // Assert
            Assert.AreEqual(baseDirectory, virtualStorage.CurrentPath); // カレントディレクトリがベースディレクトリに正しく変更されたか検証
        }

        [TestMethod]
        public void ChangeDirectory_WithPathIncludingSymbolicLinkAndDotDot_NormalizesAndResolvesPathCorrectly()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            VirtualPath baseDirectory = "/path/to";
            VirtualPath symbolicLinkPath = "/path/to/link";
            VirtualPath targetDirectory = "/real/target/directory";

            // ベースディレクトリとターゲットディレクトリを追加
            virtualStorage.AddDirectory(baseDirectory, true);
            virtualStorage.AddDirectory(targetDirectory, true);

            // サブディレクトリとしてシンボリックリンクを追加
            virtualStorage.AddSymbolicLink(symbolicLinkPath, targetDirectory);

            // シンボリックリンクを経由し、さらに".."を使って親ディレクトリに戻るパスを設定
            VirtualPath pathToChange = "/path/to/link/../..";

            // Act
            virtualStorage.ChangeDirectory(pathToChange);

            // Assert
            // シンボリックリンクを解決後、".."によりさらに上のディレクトリに移動するため、
            // 最終的なカレントディレクトリが/pathになることを期待
            VirtualPath expectedPath = "/path";
            Assert.AreEqual(expectedPath, virtualStorage.CurrentPath);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act and Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.ConvertToAbsolutePath(VirtualPath.Empty));
        }
        
        [TestMethod]
        public void ConvertToAbsolutePath_WhenCurrentPathIsRootAndPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/TestDirectory");
            virtualStorage.ChangeDirectory(VirtualPath.Root);

            VirtualPath result = virtualStorage.ConvertToAbsolutePath("TestDirectory");

            Assert.IsTrue(result == "/TestDirectory");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathStartsWithSlash_ReturnsSamePath()
        {
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            VirtualPath result = virtualStorage.ConvertToAbsolutePath("/test/path");

            Assert.IsTrue(result == "/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            VirtualPath result = virtualStorage.ConvertToAbsolutePath("test/path");

            Assert.IsTrue(result == "/root/subdirectory/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDot_ReturnsAbsolutePathWithoutDot()
        {
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            VirtualPath result = virtualStorage.ConvertToAbsolutePath("./test/path");

            Assert.IsTrue(result == "/root/subdirectory/./test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDoubleDot_ReturnsParentDirectoryPath()
        {
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            VirtualPath result = virtualStorage.ConvertToAbsolutePath("../test/path");

            Assert.IsTrue(result == "/root/subdirectory/../test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsMultipleDoubleDots_ReturnsCorrectPath()
        {
            VirtualStorage virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory("/root/subdirectory", true);
            virtualStorage.ChangeDirectory("/root/subdirectory");

            VirtualPath result = virtualStorage.ConvertToAbsolutePath("../../test/path");

            Assert.IsTrue(result == "/root/subdirectory/../../test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithBasePath_ConvertsRelativePathCorrectly()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/base/path", true); // 必要なディレクトリを作成
            VirtualPath? basePath = "/base/path";
            VirtualPath relativePath = "relative/to/base";

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.IsTrue(result == "/base/path/relative/to/base");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithoutBasePath_UsesCurrentPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/current/path", true); // 必要なディレクトリを作成
            virtualStorage.ChangeDirectory("/current/path");
            VirtualPath relativePath = "relative/path";

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(relativePath, null);

            // Assert
            Assert.IsTrue(result == "/current/path/relative/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithEmptyBasePath_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            VirtualPath relativePath = "some/relative/path";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.ConvertToAbsolutePath(relativePath, VirtualPath.Empty));
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithNullBasePath_UsesCurrentPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/current/path", true); // 必要なディレクトリを作成
            virtualStorage.ChangeDirectory("/current/path");
            VirtualPath relativePath = "relative/path";
            VirtualPath? basePath = null;

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.IsTrue(result == "/current/path/relative/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithAbsolutePath_ReturnsOriginalPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/base/path", true); // 必要なディレクトリを作成
            VirtualPath absolutePath = "/absolute/path";

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(absolutePath, "/base/path");

            // Assert
            Assert.AreEqual(absolutePath, result);
        }

        [TestMethod]
        // ディレクトリの追加が正常に行われることを確認
        public void AddDirectory_WithValidPath_CreatesDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act
            virtualStorage.AddDirectory("/test");
            virtualStorage.AddDirectory("/test/directory");

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory"));
        }

        [TestMethod]
        // ネストされたディレクトリを作成する場合、createSubdirectories が false の場合、例外がスローされることを確認
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                virtualStorage.AddDirectory("/test/directory", false));
        }

        [TestMethod]
        // ネストされたディレクトリを作成する場合、createSubdirectories が true の場合、ディレクトリが作成されることを確認
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesTrue_CreatesDirectories()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act
            virtualStorage.AddDirectory("/test/directory/subdirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory/subdirectory"));
        }

        [TestMethod]
        // 既存のディレクトリに対して createSubdirectories が true の場合でも同じパスにディレクトリを追加しようとすると例外がスローされることを確認
        public void AddDirectory_WithExistingDirectoryAndCreateSubdirectoriesTrue_DoesNotThrowException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test/directory", true);

            // Act & Assert
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                virtualStorage.AddDirectory("/test/directory", true);
            });
            Debug.WriteLine(exception.Message);
        }

        [TestMethod]
        // 既存のディレクトリに対して createSubdirectories が false の場合、例外がスローされることを確認
        public void AddDirectory_WithExistingDirectoryAndCreateSubdirectoriesFalse_ThrowException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test/directory", true);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => 
                virtualStorage.AddDirectory("/test/directory", false));
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddRootDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddDirectory("/"));
        }

        [TestMethod]
        public void AddDirectory_ThroughSymbolicLink_CreatesDirectoryAtResolvedPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test", true);
            virtualStorage.AddSymbolicLink("/link", "/test", true);

            // Act
            virtualStorage.AddDirectory("/link/directory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory"));
        }

        [TestMethod]
        public void AddDirectory_WithRelativePath_CreatesDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test", true);
            virtualStorage.ChangeDirectory("/test");

            // Act
            virtualStorage.AddDirectory("directory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/directory"));
        }

        [TestMethod]
        public void AddDirectory_ExistingSubdirectoriesWithCreateSubdirectoriesTrue_DoesNotAffectExistingSubdirectories()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test/subdirectory", true);

            // Act
            virtualStorage.AddDirectory("/test/newDirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/subdirectory"));
            Assert.IsTrue(virtualStorage.NodeExists("/test/newDirectory"));
        }

        [TestMethod]
        public void AddDirectory_MultipleLevelsOfDirectoriesWithCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddDirectory("/test/subdirectory/anotherDirectory", false));
        }

        [TestMethod]
        public void AddDirectory_WithCurrentDirectoryDot_CreatesDirectoryCorrectly()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test", true); // 初期ディレクトリを作成

            // Act
            // 現在のディレクトリ（"/test"）に対して"."を使用して、さらにサブディレクトリを作成
            virtualStorage.AddDirectory("/test/./subdirectory", true);

            // Assert
            // "/test/subdirectory" が正しく作成されたことを確認
            Assert.IsTrue(virtualStorage.NodeExists("/test/subdirectory"));
        }

        [TestMethod]
        public void AddDirectory_WithPathIncludingDotDot_CreatesDirectoryCorrectly()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test", true);
            virtualStorage.AddDirectory("/test/subdirectory", true);

            // Act
            virtualStorage.AddDirectory("/test/subdirectory/../anotherDirectory", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test/anotherDirectory"));
            Assert.IsTrue(virtualStorage.NodeExists("/test/subdirectory"));
        }

        [TestMethod]
        public void AddDirectory_WithPathNormalization_CreatesDirectoryAtNormalizedPath()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act
            virtualStorage.AddDirectory("/test/../test2", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/test2"));
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddDirectoryUnderNonDirectoryNode_ThrowsException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            // "/dir1"ディレクトリを作成
            virtualStorage.AddDirectory("/dir1", true);
            // "/dir1"内にアイテム（非ディレクトリ）"item"を作成
            virtualStorage.AddItem("/dir1/item", "Dummy content", true);

            // Act & Assert
            // "/dir1/item"がディレクトリではないノードの下に"dir2"ディレクトリを追加しようとすると例外がスローされることを確認
            VirtualNodeNotFoundException exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddDirectory("/dir1/item/dir2", true));
            Debug.WriteLine(exception.Message);
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddDirectoryUnderNonDirectoryNode_ThrowsException2()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            // "/dir1"ディレクトリを作成
            virtualStorage.AddDirectory("/dir1", true);
            virtualStorage.AddDirectory("/dir2", true);
            // "/dir1"内にアイテム（非ディレクトリ）"item"を作成
            virtualStorage.AddSymbolicLink("/dir1/linkToDir2", "/dir2");

            // Act
            // "/dir1/linkToDir2"がリンクの場合、
            virtualStorage.AddDirectory("/dir1/linkToDir2/dir3", true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists("/dir2/dir3"));
        }

        [TestMethod]
        public void AddDirectory_ThrowsException_WhenSymbolicLinkExistsWithSameName()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            // 基本ディレクトリを作成
            virtualStorage.AddDirectory("/base");
            // "/base"ディレクトリにシンボリックリンク"/base/link"を作成
            virtualStorage.AddSymbolicLink("/base/link", "/target");

            // Act & Assert
            // "/base/link"にディレクトリを作成しようとすると、例外が発生することを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddDirectory("/base/link", true));
        }

        [TestMethod]
        public void AddDirectory_ThrowsException_WhenItemExistsWithSameName()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            // 基本ディレクトリを作成
            virtualStorage.AddDirectory("/base", true);
            // "/base"ディレクトリにアイテム"/base/item"を作成（この例では、アイテムを作成するメソッドを仮定）
            virtualStorage.AddItem("/base/item", "Some content", true);

            // Act & Assert
            // "/base/item"にディレクトリを作成しようとすると、例外が発生することを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddDirectory("/base/item", true),
                "Expected VirtualNodeNotFoundException when trying to create a directory where an item exists with the same nodeName.");
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsItem()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/TestDirectory");
            BinaryData item = [1, 2, 3];
            vs.AddItem("/TestDirectory/ItemData", item);

            // メソッドを実行
            VirtualNode node = vs.GetNode("/TestDirectory/ItemData");

            // 結果を検証
            Assert.IsNotNull(node);
            Assert.AreEqual(new VirtualNodeName("ItemData"), node.Name);
            Assert.IsInstanceOfType(node, typeof(VirtualItem<BinaryData>));
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsDirectory()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/TestDirectory/TestSubdirectory", true);

            // メソッドを実行
            VirtualNode node = vs.GetNode("/TestDirectory/TestSubdirectory");

            // 結果を検証
            Assert.IsNotNull(node);
            Assert.AreEqual(new VirtualNodeName("TestSubdirectory"), node.Name);
            Assert.IsInstanceOfType(node, typeof(VirtualDirectory));
        }

        [TestMethod]
        public void GetNode_ThrowsVirtualNodeNotFoundException_WhenDirectoryDoesNotExist()
        {
            // テストデータの設定
            VirtualStorage vs = new();

            // メソッドを実行し、例外を検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetNode("/NonExistentDirectory"));
        }

        [TestMethod]
        public void GetNode_FollowsSymbolicLink_WhenFollowLinksIsTrue()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir");
            storage.AddItem("/dir/item", "TestItem");
            storage.AddSymbolicLink("/link", "/dir/item");

            // Act
            VirtualNode node = storage.GetNode("/link", true);

            // Assert
            Assert.IsInstanceOfType(node, typeof(VirtualItem<string>));
            VirtualItem<string>? item = node as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("TestItem", item.ItemData);
        }

        [TestMethod]
        public void GetNode_ReturnsSymbolicLink_WhenFollowLinksIsFalse()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/dir");
            storage.AddItem("/dir/item", "TestItem");
            storage.AddSymbolicLink("/link", "/dir/item");

            // Act
            VirtualNode node = storage.GetNode("/link", false);

            // Assert
            Assert.IsInstanceOfType(node, typeof(VirtualSymbolicLink));
            VirtualSymbolicLink? link = node as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.IsTrue(link.TargetPath == "/dir/item");
        }

        [TestMethod]
        public void GetNode_ThrowsWhenSymbolicLinkIsBroken()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddSymbolicLink("/brokenLink", "/nonexistent/item");

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.GetNode("/brokenLink", true));
        }

        [TestMethod]
        public void GetNode_StopsAtNonDirectoryNode()
        {
            // Arrange: 仮想ストレージにディレクトリとアイテムをセットアップ
            VirtualStorage storage = new();
            storage.AddDirectory("/dir");
            storage.AddItem("/dir/item", "TestItem");
            // /dir/item はディレクトリではない

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                // Act: ディレクトリではないノードの後ろに更にパスが続く場合の挙動をテスト
                VirtualNode resultNode = storage.GetNode("/dir/item/more", false);
            });
        }

        [TestMethod]
        public void GetNode_FollowsMultipleSymbolicLinksToReachAnItem()
        {
            // Arrange: 仮想ストレージと複数のディレクトリ、シンボリックリンクをセットアップ
            VirtualStorage storage = new();
            storage.AddDirectory("/dir1");
            storage.AddDirectory("/dir1/dir2");
            storage.AddItem("/dir1/dir2/item", "FinalItem");

            // 最初のシンボリックリンクを /dir1 に追加し、/dir1/dir2 を指す
            storage.AddSymbolicLink("/dir1/link1", "/dir1/dir2");

            // 2番目のシンボリックリンクを /dir1/dir2 に追加し、/dir1/dir2/item を指す
            storage.AddSymbolicLink("/dir1/dir2/link2", "/dir1/dir2/item");

            // Act: 複数のシンボリックリンクを透過的に辿る
            VirtualNode resultNode = storage.GetNode("/dir1/link1/link2", true);

            // Assert: 結果が VirtualItem<string> 型で、期待したアイテムを持っていることを確認
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            VirtualItem<string>? item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("FinalItem", item.ItemData);
        }

        [TestMethod]
        public void GetNode_ResolvesRelativePathSymbolicLink()
        {
            // Arrange: 仮想ストレージとディレクトリ、アイテム、相対パスのシンボリックリンクをセットアップ
            VirtualStorage storage = new();
            storage.AddDirectory("/dir1");
            storage.AddDirectory("/dir1/dir2");
            storage.AddItem("/dir1/dir2/item", "RelativeItem");

            // 相対パスでシンボリックリンクを追加。ここでは、/dir1から/dir1/dir2への相対パスリンクを作成
            storage.AddSymbolicLink("/dir1/relativeLink", "dir2/item");

            // Act: 相対パスのシンボリックリンクを透過的に辿る
            VirtualNode resultNode = storage.GetNode("/dir1/relativeLink", true);

            // Assert: 結果が VirtualItem<string> 型で、期待したアイテムを持っていることを確認
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            VirtualItem<string>? item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("RelativeItem", item.ItemData);
        }

        [TestMethod]
        public void GetNode_ResolvesSymbolicLinkWithCurrentDirectoryReference_Correctly()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/Test");
            storage.ChangeDirectory("/Test");
            storage.AddSymbolicLink("LinkToCurrent", VirtualPath.Dot);

            // Act
            VirtualNode node = storage.GetNode("LinkToCurrent", true);

            // Assert
            Assert.IsTrue(node is VirtualDirectory);
            VirtualDirectory? directory = node as VirtualDirectory;
            Assert.AreEqual(new VirtualNodeName("Test"), directory?.Name);
        }

        [TestMethod]
        public void GetNode_ResolvesSymbolicLinkWithParentDirectoryReference_Correctly()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/Parent/Child", true);
            storage.ChangeDirectory("/Parent/Child");
            storage.AddSymbolicLink("LinkToParent", VirtualPath.DotDot);

            // Act
            VirtualNode node = storage.GetNode("LinkToParent", true);

            // Assert
            Assert.IsTrue(node is VirtualDirectory);
            VirtualDirectory? directory = node as VirtualDirectory;
            Assert.AreEqual(new VirtualNodeName("Parent"), directory?.Name);
        }

        [TestMethod]
        public void GetNode_ComplexSymbolicLinkIncludingDotAndDotDot()
        {
            // テスト用の仮想ストレージとディレクトリ構造を準備
            VirtualStorage storage = new();

            // 複数レベルのサブディレクトリを一度に作成
            storage.AddDirectory("/dir/subdir/dir1", true);
            storage.AddDirectory("/dir/subdir/dir2", true); // 隣接するディレクトリを追加
            storage.AddItem("/dir/subdir/dir2/item", "complex item in dir2");

            // シンボリックリンクを作成
            // "/dir/subdir/link" が "./dir1/../dir2/./item" を指し、隣接するディレクトリ内のアイテムへの複合的なパスを使用
            storage.AddSymbolicLink("/dir/subdir/link", "./dir1/../dir2/./item");

            // シンボリックリンクを通じてアイテムにアクセス
            VirtualNode resultNode = storage.GetNode("/dir/subdir/link", true);

            // 検証：リンクを通じて正しいアイテムにアクセスできること
            Assert.IsNotNull(resultNode);
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            VirtualItem<string>? item = resultNode as VirtualItem<string>;
            Assert.AreEqual("complex item in dir2", item?.ItemData);
            Assert.AreEqual(new VirtualNodeName("item"), resultNode.Name);
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathIsSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/Documents");
            vs.AddSymbolicLink("/LinkToDocuments", "/Documents");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("/LinkToDocuments");

            // 結果を検証
            // TODO:これは期待通り。
            Assert.IsTrue(resolvedPath == "/Documents");
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathContainsTwoLinks()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/FinalDestination");
            vs.AddSymbolicLink("/FirstLink", "/SecondLink");
            vs.AddSymbolicLink("/SecondLink", "/FinalDestination");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("/FirstLink");

            // 結果を検証
            // TODO:これはできれば/FinalDestinationになってほしい。
            Assert.IsTrue(resolvedPath == "/FinalDestination");
        }

        [TestMethod]
        public void ResolveLinkTarget_ResolvesPathCorrectly_WhenUsingDotDotInPathWithSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/dir2/dir3/item", "ItemInDir1");
            vs.AddSymbolicLink("/dir1/dir2/dir3/linkToDir2", "/dir1/dir2");
            vs.ChangeDirectory("/dir1/dir2/dir3");

            // メソッドを実行
            // TODO:これはできれば/dir1/dir2/dir3/itemになってほしい。
            VirtualPath resolvedPath = vs.ResolveLinkTarget("linkToDir2/../item");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/dir1/dir2/dir3/item");
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathSpansMultipleLinkLevels()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddDirectory("/dir3");
            vs.AddItem("/dir3/item", "FinalItem");
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddSymbolicLink("/dir2/link2", "/dir3");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("/dir1/link1/link2/item");

            // 結果を検証
            // TODO:これは期待通り。
            Assert.IsTrue(resolvedPath == "/dir3/item");
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathIncludesSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1/dir2", true);
            vs.AddItem("/dir1/dir2/item", "FinalItem");
            vs.AddSymbolicLink("/dir1/linkToDir2", "/dir1/dir2");
            vs.ChangeDirectory("/dir1");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("linkToDir2/item");

            // 結果を検証
            // TODO:これは期待通り。
            Assert.IsTrue(resolvedPath == "/dir1/dir2/item");
        }

        [TestMethod]
        public void ResolveLinkTarget_ResolvesPathCorrectly_WhenUsingDotWithSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1/subdir", true);
            vs.AddItem("/dir1/subdir/item", "SubdirItem");
            vs.AddSymbolicLink("/dir1/link", "/dir1/subdir");
            vs.ChangeDirectory("/dir1");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("./link/item");

            // 結果を検証
            // TODO:これは期待通り。
            Assert.IsTrue(resolvedPath == "/dir1/subdir/item");
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryExists_ReturnsDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test");
            VirtualPath path = "/test";

            // Act
            VirtualDirectory directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual(new VirtualNodeName("test"), directory.Name);
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            VirtualPath path = "/nonexistent";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }

        [TestMethod]
        public void GetDirectory_WhenNodeIsNotDirectory_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            BinaryData file = [1, 2, 3];
            virtualStorage.AddItem("/test-file", file);

            VirtualPath path = "/test-file";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }
        
        [TestMethod]
        public void GetDirectory_WhenPathIsRelative_ReturnsDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test");
            VirtualPath path = "test";

            // Act
            VirtualDirectory directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual(new VirtualNodeName("test"), directory.Name);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();
            BinaryData item = [1, 2, 3];
            VirtualPath path = "/NewItem";

            virtualStorage.AddItem(path, item);

            Assert.IsTrue(virtualStorage.ItemExists(path));
            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();
            BinaryData originalItem = [1, 2, 3];
            BinaryData newItem = [4, 5, 6];
            VirtualPath path = "/ExistingItem";

            virtualStorage.AddItem(path, originalItem);
            virtualStorage.AddItem(path, newItem, true);

            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItem.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsArgumentException_WhenPathIsEmpty_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();
            BinaryData item = [1, 2, 3];

            Assert.ThrowsException<ArgumentException>(() => virtualStorage.AddItem(VirtualPath.Empty, item));
        }

        [TestMethod]
        public void AddItem_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();
            BinaryData item = [1, 2, 3];
            VirtualPath path = "/NonExistentDirectory/ItemData";

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.AddItem(path, item));
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteIsFalseAndItemExists_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();
            BinaryData originalItem = [1, 2, 3];
            VirtualPath path = "/ExistingItem";
            virtualStorage.AddItem(path, originalItem);

            Assert.ThrowsException<InvalidOperationException>(() => virtualStorage.AddItem(path, new BinaryData([4, 5, 6]), false));
        }

        [TestMethod]
        public void AddItem_AddsNewItemToCurrentDirectory_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();
            virtualStorage.ChangeDirectory(VirtualPath.Root);
            BinaryData item = [1, 2, 3];
            VirtualPath itemName = "NewItemInRoot";

            virtualStorage.AddItem(itemName, item); // パスを指定せずにアイテム名のみを渡す

            Assert.IsTrue(virtualStorage.ItemExists(itemName.AddStartSlash())); // カレントディレクトリにアイテムが作成されていることを確認
            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode(itemName.AddStartSlash()) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemUsingRelativePath_WithBinaryData_Corrected()
        {
            VirtualStorage virtualStorage = new();

            // 事前にディレクトリを作成
            virtualStorage.AddDirectory("/existingDirectory", true); // 既存のディレクトリ
            virtualStorage.AddDirectory("/existingDirectory/subDirectory", true); // 新しく作成するサブディレクトリ
            virtualStorage.ChangeDirectory("/existingDirectory"); // カレントディレクトリを変更

            BinaryData item = [4, 5, 6];
            VirtualPath relativePath = "subDirectory/NewItem"; // 相対パスで新しいアイテムを指定

            // 相対パスを使用してアイテムを追加
            virtualStorage.AddItem(relativePath, item, true);

            VirtualPath fullPath = "/existingDirectory/" + relativePath;
            Assert.IsTrue(virtualStorage.ItemExists(fullPath)); // 相対パスで指定された位置にアイテムが作成されていることを確認
            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode(fullPath) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemToSubdirectoryAsCurrentDirectory_WithBinaryData()
        {
            VirtualStorage virtualStorage = new();

            // サブディレクトリを作成し、カレントディレクトリに設定
            virtualStorage.AddDirectory("/subdirectory", true);
            virtualStorage.ChangeDirectory("/subdirectory");
            BinaryData item = [1, 2, 3];
            VirtualPath itemName = "NewItemInSubdirectory";

            // カレントディレクトリにアイテムを追加（パスを指定せずにアイテム名のみを渡す）
            virtualStorage.AddItem(itemName, item);

            // サブディレクトリ内にアイテムが作成されていることを確認
            Assert.IsTrue(virtualStorage.ItemExists("/subdirectory/" + itemName));
            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode("/subdirectory/" + itemName) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteTargetIsNotAnItem()
        {
            VirtualStorage virtualStorage = new();

            // テスト用のディレクトリを作成
            virtualStorage.AddDirectory("/testDirectory", true);

            // 同名のサブディレクトリを追加（アイテムの上書き対象として）
            virtualStorage.AddDirectory("/testDirectory/itemName", true);

            BinaryData item = [1, 2, 3];
            VirtualNodeName itemName = "itemName";

            // アイテム上書きを試みる（ただし、実際にはディレクトリが存在するため、アイテムではない）
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddItem("/testDirectory/" + itemName, item, true),
                "上書き対象がアイテムではなくディレクトリの場合、InvalidOperationExceptionが投げられるべきです。");
        }

        [TestMethod]
        public void AddItem_ThroughSymbolicLink_AddsItemToTargetDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/actualDirectory", true);
            virtualStorage.AddSymbolicLink("/symbolicLink", "/actualDirectory");
            BinaryData binaryData = [1, 2, 3];

            // Act
            virtualStorage.AddItem("/symbolicLink/newItem", binaryData);

            // Assert
            Assert.IsTrue(virtualStorage.ItemExists("/actualDirectory/newItem"));
            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode("/actualDirectory/newItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(binaryData.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ThroughNestedSymbolicLink_AddsItemToFinalTargetDirectory()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/level1/level2/targetDirectory", true);
            virtualStorage.AddSymbolicLink("/linkToLevel1", "/level1");
            virtualStorage.AddSymbolicLink("/level1/linkToLevel2", "/level1/level2");
            BinaryData binaryData = [4, 5, 6];

            // Act
            virtualStorage.AddItem("/linkToLevel1/linkToLevel2/targetDirectory/newItem", binaryData);

            // Assert
            Assert.IsTrue(virtualStorage.ItemExists("/level1/level2/targetDirectory/newItem"));
            VirtualItem<BinaryData>? retrievedItem = virtualStorage.GetNode("/level1/level2/targetDirectory/newItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(binaryData.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ToNonExistentTargetViaSymbolicLink_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddSymbolicLink("/linkToNowhere", "/nonExistentTarget");
            BinaryData binaryData = [7, 8, 9];

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddItem("/linkToNowhere/newItem", binaryData));
        }

        [TestMethod]
        public void AddItem_ThroughSymbolicLinkWithNonExistentIntermediateTarget_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/existingDirectory", true);

            // 中間のターゲットが存在しないシンボリックリンクを作成
            virtualStorage.AddSymbolicLink("/existingDirectory/nonExistentLink", "/nonExistentIntermediateTarget");

            // 最終的なパスの組み立て
            VirtualPath pathToItem = "/existingDirectory/nonExistentLink/finalItem";
            BinaryData binaryData = [10, 11, 12];

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddItem(pathToItem, binaryData),
                "中間のシンボリックリンクのターゲットが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

        [TestMethod]
        public void AddItem_WithVirtualItem_ShouldAddItemToDirectory()
        {
            VirtualStorage storage = new();
            VirtualPath path = "/dir";
            storage.AddDirectory(path);
            VirtualItem<string> item = new("item", "test");

            storage.AddItem(path, item);

            Assert.IsTrue(storage.ItemExists(path + item.Name));
        }

        [TestMethod]
        public void AddItem_WithVirtualItem_WithOverwrite_ShouldThrowExceptionIfItemExists()
        {
            VirtualStorage storage = new();
            VirtualPath directoryPath = "/dir";
            storage.AddDirectory(directoryPath);
            VirtualNodeName existingItemName = "existingItem";
            VirtualItem<string> existingItem = new(existingItemName, "existing value");
            VirtualItem<string> newItem = new(existingItemName, "new value");

            storage.AddItem(directoryPath, existingItem, false);

            Assert.ThrowsException<InvalidOperationException>(() => storage.AddItem(directoryPath, newItem, false));
        }

        [TestMethod]
        public void AddItem_WithOverwrite_ShouldOverwriteExistingItem()
        {
            VirtualStorage storage = new();
            VirtualPath directoryPath = "/dir";
            storage.AddDirectory(directoryPath);
            VirtualNodeName itemName = "item";
            VirtualItem<string> originalItem = new(itemName, "original value");
            VirtualItem<string> newItem = new(itemName, "new value");

            storage.AddItem(directoryPath, originalItem, false);
            storage.AddItem(directoryPath, newItem, true);

            Assert.IsTrue(storage.ItemExists(directoryPath + newItem.Name));
        }

        [TestMethod]
        public void AddItem_ToNonExistingDirectory_ShouldThrowVirtualNodeNotFoundException()
        {
            VirtualStorage storage = new();
            VirtualPath directoryPath = "/nonExistingDir";
            VirtualNodeName itemName = "item";
            VirtualItem<string> item = new(itemName, "value");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.AddItem(directoryPath, item, false));
        }

        [TestMethod]
        public void AddItem_ToLocationPointedBySymbolicLink_ShouldThrowException()
        {
            VirtualStorage storage = new();
            VirtualPath directoryPath = "/dir";
            storage.AddDirectory(directoryPath);
            VirtualPath linkPath = "/link";
            VirtualPath targetPath = "/nonExistingTargetDir";
            storage.AddSymbolicLink(linkPath, targetPath);

            VirtualNodeName itemName = "item";
            VirtualItem<string> item = new(itemName, "value");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.AddItem(linkPath, item, false));
        }

        [TestMethod]
        public void AddItem_ToLocationPointedBySymbolicLink_ShouldAddItemSuccessfully()
        {
            VirtualStorage storage = new();
            VirtualPath actualDirectoryPath = "/actualDir";
            storage.AddDirectory(actualDirectoryPath);
            VirtualPath symbolicLinkPath = "/symbolicLink";
            storage.AddSymbolicLink(symbolicLinkPath, actualDirectoryPath);
            VirtualNodeName itemName = "newItem";
            VirtualItem<string> item = new(itemName, "itemValue");
            storage.AddItem(symbolicLinkPath, item);
            Assert.IsTrue(storage.ItemExists(actualDirectoryPath + itemName));
        }

        [TestMethod]
        public void ItemExists_WhenIntermediateDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act
            bool result = virtualStorage.NodeExists("/nonexistent/test-file");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            BinaryData item = [1, 2, 3];
            virtualStorage.AddItem("/test-file", item);

            // Act
            bool result = virtualStorage.NodeExists("/test-file");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act
            bool result = virtualStorage.NodeExists("/nonexistent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/test-dir");

            // Act
            bool result = virtualStorage.NodeExists("/test-dir");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act
            bool result = virtualStorage.NodeExists("/nonexistent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToItemAndFollowLinksIsTrue_ReturnsTrue()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            BinaryData item = [1, 2, 3];
            virtualStorage.AddItem("/actual-item", item);
            virtualStorage.AddSymbolicLink("/link-to-item", "/actual-item");

            // Act
            bool result = virtualStorage.ItemExists("/link-to-item", true);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToItemAndFollowLinksIsFalse_ReturnsFalse()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            BinaryData item = [1, 2, 3];
            virtualStorage.AddItem("/actual-item", item);
            virtualStorage.AddSymbolicLink("/link-to-item", "/actual-item");

            // Act
            bool result = virtualStorage.ItemExists("/link-to-item", false);

            // Assert
            Assert.IsFalse(result); // シンボリックリンク自体はアイテムとしてカウントしない
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToNonexistentItemAndFollowLinksIsTrue_ReturnsFalse()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddSymbolicLink("/link-to-nonexistent", "/non-existent-item");

            // Act
            bool result = virtualStorage.ItemExists("/link-to-nonexistent", true);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToPointingToDirectoryAndFollowLinksIsTrue_ReturnsFalse()
        {
            // Arrange
            VirtualStorage virtualStorage = new();
            virtualStorage.AddDirectory("/target-dir");
            virtualStorage.AddSymbolicLink("/link-to-dir", "/target-dir");

            // Act
            bool result = virtualStorage.ItemExists("/link-to-dir", true);

            // Assert
            Assert.IsFalse(result); // ディレクトリを指すシンボリックリンクはアイテムとしてカウントしない
        }

        private static void SetTestData(VirtualStorage vs)
        {
            vs.AddDirectory("/Directory1", true);
            vs.AddDirectory("/Directory1/Directory1_1", true);
            vs.AddDirectory("/Directory1/Directory1_2", true);
            vs.AddDirectory("/Directory2", true);
            vs.AddDirectory("/Directory2/Directory2_1", true);
            vs.AddDirectory("/Directory2/Directory2_2", true);

            BinaryData item_1 = [1, 2, 3];
            BinaryData item_2 = [1, 2, 3];
            vs.AddItem("/Item_1", item_1);
            vs.AddItem("/Item_2", item_2);
            vs.AddSymbolicLink("/LinkToItem1a", "/Directory1/Item1a");
            vs.AddSymbolicLink("/LinkToItem2a", "/Directory1/Item1b");

            BinaryData item1a = [1, 2, 3];
            BinaryData item1b = [1, 2, 3];
            vs.AddItem("/Directory1/Item1a", item1a);
            vs.AddItem("/Directory1/Item1b", item1b);

            BinaryData item1_1a = [1, 2, 3];
            BinaryData item1_1b = [1, 2, 3];
            vs.AddItem("/Directory1/Directory1_1/Item1_1a", item1_1a);
            vs.AddItem("/Directory1/Directory1_1/Item1_1b", item1_1b);

            BinaryData item1_2a = [1, 2, 3];
            BinaryData item1_2b = [1, 2, 3];
            vs.AddItem("/Directory1/Directory1_2/Item1_2a", item1_2a);
            vs.AddItem("/Directory1/Directory1_2/Item1_2b", item1_2b);

            BinaryData item2a = [1, 2, 3];
            BinaryData item2b = [1, 2, 3];
            vs.AddItem("/Directory2/Item2a", item2a);
            vs.AddItem("/Directory2/Item2b", item2b);

            BinaryData item2_1a = [1, 2, 3];
            BinaryData item2_1b = [1, 2, 3];
            vs.AddItem("/Directory2/Directory2_1/Item2_1a", item2_1a);
            vs.AddItem("/Directory2/Directory2_1/Item2_1b", item2_1b);

            BinaryData item2_2a = [1, 2, 3];
            BinaryData item2_2b = [1, 2, 3];
            vs.AddItem("/Directory2/Directory2_2/Item2_2a", item2_2a);
            vs.AddItem("/Directory2/Directory2_2/Item2_2b", item2_2b);
        }

        [TestMethod]
        public void GetNodes_ValidTest()
        {
            VirtualStorage vs = new();

            SetTestData(vs);

            Assert.AreEqual(23, vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.All, true).Count());
            Debug.WriteLine("\nAll nodes:");
            foreach (VirtualNode node in vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.All, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(7, vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.Directory, true).Count());
            Debug.WriteLine("\nDirectories:");
            foreach (VirtualNode node in vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.Directory, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(14, vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.Item, true).Count());
            Debug.WriteLine("\nItems:");
            foreach (VirtualNode node in vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.Item, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(2, vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.SymbolicLink, true).Count());
            Debug.WriteLine("\nSymbolicLink:");
            foreach (VirtualNode node in vs.GetNodes(VirtualPath.Root, VirtualNodeTypeFilter.SymbolicLink, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(3, vs.GetNodes(VirtualNodeTypeFilter.Directory, false).Count());
            Debug.WriteLine("\nDirectories in /Directory1:");
            foreach (VirtualNode node in vs.GetNodes(VirtualNodeTypeFilter.Directory, false))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(2, vs.GetNodes(VirtualNodeTypeFilter.Item, false).Count());
            Debug.WriteLine("\nItems in /Directory1:");
            foreach (VirtualNode node in vs.GetNodes(VirtualNodeTypeFilter.Item, false))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

        }

        [TestMethod]
        public void GetNodesWithPaths_ValidTest()
        {
            VirtualStorage vs = new();

            SetTestData(vs);

            Assert.AreEqual(23, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.All, true).Count());
            Debug.WriteLine("\nAll nodes:");
            foreach (VirtualPath name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.All, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(7, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.Directory, true).Count());
            Debug.WriteLine("\nDirectories:");
            foreach (VirtualPath name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.Directory, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(14, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.Item, true).Count());
            Debug.WriteLine("\nItems:");
            foreach (VirtualPath name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.Item, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(2, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.SymbolicLink, true).Count());
            Debug.WriteLine("\nSymbolicLink:");
            foreach (VirtualPath name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.SymbolicLink, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(3, vs.GetNodesWithPaths(VirtualNodeTypeFilter.Directory, false).Count());
            Debug.WriteLine("\nDirectories in /Directory1:");
            foreach (VirtualPath name in vs.GetNodesWithPaths(VirtualNodeTypeFilter.Directory, false))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            vs.ChangeDirectory("/Directory1");
            Assert.AreEqual(2, vs.GetNodesWithPaths(VirtualNodeTypeFilter.Item, false).Count());
            Debug.WriteLine("\nItems in /Directory1:");
            foreach (VirtualPath name in vs.GetNodesWithPaths(VirtualNodeTypeFilter.Item, false))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }
        }

        [TestMethod]
        public void GetNodes_WithEmptyPath_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                virtualStorage.GetNodes(VirtualPath.Empty, VirtualNodeTypeFilter.All, true).ToList());
        }

        [TestMethod]
        public void GetNodes_WithNonAbsolutePath_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage virtualStorage = new();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.GetNodes("relative/path", VirtualNodeTypeFilter.All, true).ToList());
        }

        [TestMethod]
        public void CopyFileToFile_OverwritesWhenAllowed()
        {
            VirtualStorage storage = new();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            storage.CopyNode("/sourceFile", "/destinationFile", false, true);

            VirtualItem<BinaryData> destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData.Data);
        }

        [TestMethod]
        public void CopyFileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            VirtualStorage storage = new();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.CopyNode("/sourceFile", "/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyFileToDirectory_CopiesFileToTargetDirectory()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/destination");
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            storage.CopyNode("/sourceFile", "/destination/", false, false);

            VirtualItem<BinaryData> destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destination/sourceFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData.Data);
        }

        [TestMethod]
        public void CopyEmptyDirectoryToDirectory_CreatesTargetDirectory()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddDirectory("/destinationDir");

            storage.CopyNode("/sourceDir", "/destinationDir/newDir", false, false);

            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithoutRecursive_ThrowsException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/file", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.CopyNode("/sourceDir", "/destinationDir/newDir", false, false));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithRecursive_CopiesAllContents()
        {
            VirtualStorage storage = new();
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
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.CopyNode("/sourceDir", "/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyFileToNonExistentDirectory_ThrowsException()
        {
            VirtualStorage storage = new();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.CopyNode("/sourceFile", "/nonExistentDir/destinationFile", false, false));
        }

        [TestMethod]
        public void CopyDirectoryToNonExistentDirectoryWithRecursive_CreatesAllDirectoriesAndCopiesContents()
        {
            VirtualStorage storage = new();
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
            VirtualStorage storage = new();
            storage.AddDirectory("/source/deep/nested/dir", true);
            BinaryData originalItem = [1, 2, 3];
            storage.AddItem("/source/deep/nested/dir/nestedFile", originalItem);

            storage.AddDirectory("/destination", true);

            storage.CopyNode("/source/deep", "/destination/deepCopy", true, false);

            VirtualItem<BinaryData> copiedItem = (VirtualItem<BinaryData>)storage.GetNode("/destination/deepCopy/nested/dir/nestedFile");

            Assert.IsNotNull(originalItem);
            Assert.IsNotNull(copiedItem);
            Assert.AreNotSame(originalItem, copiedItem.ItemData);
        }

        [TestMethod]
        public void CopyMultipleNestedDirectories_CopiesAllDirectoriesAndContentsAndEnsuresDifferentInstances()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/source/dir1/subdir1", true);
            storage.AddDirectory("/source/dir2/subdir2", true);
            BinaryData originalFile1 = [4, 5, 6];
            BinaryData originalFile2 = [7, 8, 9];
            storage.AddItem("/source/dir1/subdir1/file1", originalFile1);
            storage.AddItem("/source/dir2/subdir2/file2", originalFile2);

            storage.CopyNode("/source", "/destination", true, false);

            VirtualItem<BinaryData> copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode("/destination/dir1/subdir1/file1");
            VirtualItem<BinaryData> copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode("/destination/dir2/subdir2/file2");

            Assert.IsNotNull(originalFile1);
            Assert.IsNotNull(copiedFile1);
            Assert.IsNotNull(originalFile2);
            Assert.IsNotNull(copiedFile2);
            Assert.AreNotSame(originalFile1, copiedFile1.ItemData);
            Assert.AreNotSame(originalFile2, copiedFile2.ItemData);
        }

        [TestMethod]
        public void CopyDirectoryWithComplexStructure_CopiesCorrectlyAndEnsuresDifferentInstances()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/complex/dir1", true);
            storage.AddDirectory("/complex/dir2", true);
            storage.AddDirectory("/complex/dir1/subdir1", true);
            BinaryData originalFile1 = [1, 2, 3];
            BinaryData originalFile2 = [4, 5, 6];
            storage.AddItem("/complex/dir1/file1", originalFile1);
            storage.AddItem("/complex/dir2/file2", originalFile2);

            storage.CopyNode("/complex", "/copiedComplex", true, false);

            VirtualItem<BinaryData> copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode("/copiedComplex/dir1/file1");
            VirtualItem<BinaryData> copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode("/copiedComplex/dir2/file2");

            Assert.IsNotNull(originalFile1);
            Assert.IsNotNull(copiedFile1);
            Assert.IsNotNull(originalFile2);
            Assert.IsNotNull(copiedFile2);
            Assert.AreNotSame(originalFile1, copiedFile1.ItemData);
            Assert.AreNotSame(originalFile2, copiedFile2.ItemData);
        }

        [TestMethod]
        public void CopyNode_EmptySourceAndDestination_ThrowsArgumentException()
        {
            VirtualStorage storage = new();

            // ArgumentException がスローされることを検証
            Assert.ThrowsException<ArgumentException>(() => storage.CopyNode(VirtualPath.Empty, VirtualPath.Empty, false, false));
        }

        [TestMethod]
        public void CopyNode_DestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/parentDir/childDir", true);

            // コピー先がコピー元のサブディレクトリである場合
            VirtualPath sourcePath = "/parentDir";
            VirtualPath destinationPath = "/parentDir/childDir";

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }

        [TestMethod]
        public void CopyNode_SourceIsSubdirectoryOfDestination_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/parentDir/childDir", true);

            // コピー元がコピー先のサブディレクトリである場合
            VirtualPath sourcePath = "/parentDir/childDir";
            VirtualPath destinationPath = "/parentDir";

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }


        [TestMethod]
        public void CopyNode_RootDirectoryCopy_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            VirtualPath sourcePath = VirtualPath.Root;
            VirtualPath destinationPath = "/SomeOtherDirectory";

            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }

        [TestMethod]
        public void CopyNode_NonExistentSource_ThrowsVirtualNodeNotFoundException()
        {
            VirtualStorage storage = new();

            // 存在しないソースパスを指定
            VirtualPath nonExistentSource = "/nonExistentSource";
            VirtualPath destinationPath = "/destination";

            storage.AddDirectory(destinationPath);

            // VirtualNodeNotFoundException がスローされることを検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.CopyNode(nonExistentSource, destinationPath));
        }

        [TestMethod]
        public void CopyNode_SameSourceAndDestination_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddItem("/file", new BinaryData([1, 2, 3]));

            // 同じソースとデスティネーションを指定
            VirtualPath path = "/file";

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(path, path));
        }

        [TestMethod]
        public void CopyNode_ValidTest()
        {
            VirtualStorage vs = new();

            SetTestData(vs);

            Assert.AreEqual(23, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.All, true).Count());

            Debug.WriteLine("コピー前:");
            foreach (VirtualPath nodeName in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.All, true))
            {
                Assert.IsNotNull(nodeName);
                Debug.WriteLine(nodeName);
            }

            vs.AddDirectory("/Destination", true);
            vs.CopyNode("/Directory1", "/Destination", true, false);

            Assert.AreEqual(33, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.All, true).Count());

            Debug.WriteLine("コピー後:");
            foreach (VirtualPath nodeName in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeTypeFilter.All, true))
            {
                Assert.IsNotNull(nodeName);
                Debug.WriteLine(nodeName);
            }
        }

        [TestMethod]
        public void RemoveNode_ExistingItem_RemovesItem()
        {
            VirtualStorage storage = new();
            BinaryData item = [1, 2, 3];
            storage.AddItem("/TestItem", item);

            storage.RemoveNode("/TestItem");

            Assert.IsFalse(storage.NodeExists("/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_NonExistingItem_ThrowsException()
        {
            VirtualStorage storage = new();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.RemoveNode("/NonExistingItem"));
        }

        [TestMethod]
        public void RemoveNode_ExistingEmptyDirectory_RemovesDirectory()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/TestDirectory");

            storage.RemoveNode("/TestDirectory");

            Assert.IsFalse(storage.NodeExists("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_NonExistingDirectory_ThrowsException()
        {
            VirtualStorage storage = new();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.RemoveNode("/NonExistingDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithoutRecursive_ThrowsException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/TestDirectory");
            BinaryData item = [1, 2, 3];
            storage.AddItem("/TestDirectory/TestItem", item);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.RemoveNode("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithRecursive_RemovesDirectoryAndContents()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/TestDirectory");
            BinaryData item = [1, 2, 3];
            storage.AddItem("/TestDirectory/TestItem", item);

            storage.RemoveNode("/TestDirectory", true);

            Assert.IsFalse(storage.NodeExists("/TestDirectory"));
            Assert.IsFalse(storage.NodeExists("/TestDirectory/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithRecursive_RemovesAllNestedContents()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/Level1/Level2/Level3", true);
            BinaryData item1 = [1, 2, 3];
            BinaryData item2 = [4, 5, 6];
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
            VirtualStorage storage = new();
            storage.AddDirectory("/Level1/Level2/Level3", true);
            BinaryData item1 = [1, 2, 3];
            storage.AddItem("/Level1/Level2/Level3/Item1", item1);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.RemoveNode("/Level1"));
        }

        [TestMethod]
        public void RemoveNode_NestedDirectoryWithEmptySubdirectories_RecursiveRemoval()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/Level1/Level2/Level3", true);

            storage.RemoveNode("/Level1", true);

            Assert.IsFalse(storage.NodeExists("/Level1"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2"));
            Assert.IsFalse(storage.NodeExists("/Level1/Level2/Level3"));
        }

        [TestMethod]
        public void RemoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(VirtualPath.Root);
            });
        }

        [TestMethod]
        public void RemoveNode_CurrentDirectoryDot_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(VirtualPath.Dot);
            });
        }

        [TestMethod]
        public void RemoveNode_ParentDirectoryDotDot_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(VirtualPath.DotDot);
            });
        }

        [TestMethod]
        public void TryGetNode_ReturnsNode_WhenNodeExists()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath path = "/existing/path";
            storage.AddDirectory(path, true);

            // Act
            VirtualNode? node = storage.TryGetNode(path);

            // Assert
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void TryGetNode_ReturnsNull_WhenNodeDoesNotExist()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath path = "/non/existing/path";

            // Act
            VirtualNode? node = storage.TryGetNode(path);

            // Assert
            Assert.IsNull(node);
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenNodeExists()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath path = "/existing/path";
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
            VirtualStorage storage = new();
            VirtualPath path = "/non/existing/path";

            // Act
            bool exists = storage.NodeExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenSymbolicLinkExistsAndFollowLinksIsTrue()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath directoryPath = "/existing/directory";
            VirtualPath linkDirectoryPath = "/link/to";
            VirtualPath linkPath = linkDirectoryPath + "directory";
            storage.AddDirectory(directoryPath, true); // 実際のディレクトリを追加
            storage.AddDirectory(linkDirectoryPath, true); // シンボリックリンクの親ディレクトリを追加
            storage.AddSymbolicLink(linkPath, directoryPath); // シンボリックリンクを追加

            // Act
            bool exists = storage.NodeExists(linkPath, true); // シンボリックリンクの追跡を有効に

            // Assert
            Assert.IsTrue(exists); // シンボリックリンクを追跡し、実際のディレクトリが存在することを確認
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenSymbolicLinkExistsAndFollowLinksIsFalse()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath directoryPath = "/existing/directory";
            VirtualPath linkDirectoryPath = "/link/to";
            VirtualPath linkPath = linkDirectoryPath + "directory";
            storage.AddDirectory(directoryPath, true); // 実際のディレクトリを追加
            storage.AddDirectory(linkDirectoryPath, true); // シンボリックリンクの親ディレクトリを追加
            storage.AddSymbolicLink(linkPath, directoryPath); // シンボリックリンクを追加

            // Act
            bool exists = storage.NodeExists(linkPath, false); // シンボリックリンクの追跡を無効に

            // Assert
            Assert.IsTrue(exists); // シンボリックリンク自体の存在を確認
        }

        [TestMethod]
        public void NodeExists_ReturnsFalse_WhenTargetOfSymbolicLinkDoesNotExistAndFollowLinksIsTrue()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath nonExistentTargetPath = "/nonexistent/target";
            VirtualPath linkDirectoryPath = "/link/to";
            VirtualPath linkPath = linkDirectoryPath + "nonexistent";
            storage.AddDirectory(linkDirectoryPath, true); // シンボリックリンクの親ディレクトリを追加
            storage.AddSymbolicLink(linkPath, nonExistentTargetPath); // 存在しないターゲットへのシンボリックリンクを追加

            // Act
            bool exists = storage.NodeExists(linkPath, true); // シンボリックリンクの追跡を有効に

            // Assert
            Assert.IsFalse(exists); // シンボリックリンクのターゲットが存在しないため、falseを返す
        }

        [TestMethod]
        public void DirectoryExists_ReturnsTrue_WhenDirectoryExists()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath path = "/existing/path";
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
            VirtualStorage storage = new();
            VirtualPath path = "/non/existing/path";

            // Act
            bool exists = storage.DirectoryExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsTrue_WhenItemExists()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath path = "/existing/path/item";
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
            VirtualStorage storage = new();
            VirtualPath path = "/non/existing/path/item";

            // Act
            bool exists = storage.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsFalse_WhenPathIsDirectory()
        {
            // Arrange
            VirtualStorage storage = new();
            VirtualPath path = "/existing/path";
            storage.AddDirectory(path, true);

            // Act
            bool exists = storage.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void MoveNode_FileToFile_OverwritesWhenAllowed()
        {
            VirtualStorage storage = new();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            storage.MoveNode("/sourceFile", "/destinationFile", true);

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            VirtualItem<BinaryData> destinationItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData.Data);
        }

        [TestMethod]
        public void MoveNode_FileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            VirtualStorage storage = new();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/sourceFile", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_FileToDirectory_MovesFileToTargetDirectory()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/destination");
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            storage.MoveNode("/sourceFile", "/destination/", false);

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            Assert.IsTrue(storage.NodeExists("/destination/sourceFile"));
        }

        [TestMethod]
        public void MoveNode_DirectoryToDirectory_MovesDirectoryToTargetDirectory()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddDirectory("/destinationDir/newDir", true);

            storage.MoveNode("/sourceDir", "/destinationDir/newDir", false);

            Assert.IsFalse(storage.NodeExists("/sourceDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void MoveNode_WhenSourceDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/destinationDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode("/nonExistentSource", "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_WhenDestinationIsInvalid_ThrowsException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode("/sourceDir", "/nonExistentDestination/newDir", false));
        }

        [TestMethod]
        public void MoveNode_DirectoryWithSameNameExistsAtDestination_ThrowsExceptionRegardlessOfOverwriteFlag()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir", true);
            storage.AddDirectory("/destinationDir/sourceDir", true);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/sourceDir", "/destinationDir", false));
            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/sourceDir", "/destinationDir", true));
        }

        [TestMethod]
        public void MoveNode_DirectoryToFile_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/sourceDir", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(VirtualPath.Root, "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_OverwritesExistingNodeInDestinationWhenAllowed()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/fileName", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");
            storage.AddItem("/destinationDir/fileName", new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            storage.MoveNode("/sourceDir/fileName", "/destinationDir", true); // 上書き許可で移動

            Assert.IsFalse(storage.NodeExists("/sourceDir/fileName")); // 元のファイルが存在しないことを確認
            Assert.IsTrue(storage.NodeExists("/destinationDir/fileName")); // 移動先にファイルが存在することを確認

            VirtualItem<BinaryData> movedItem = (VirtualItem<BinaryData>)storage.GetNode("/destinationDir/fileName");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, movedItem.ItemData.Data); // 移動先のファイルの中身が正しいことを確認
        }

        [TestMethod]
        public void MoveNode_ThrowsWhenDestinationNodeExistsAndOverwriteIsFalse()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/sourceDir/fileName", new BinaryData([1, 2, 3]));
            storage.AddDirectory("/destinationDir");
            storage.AddItem("/destinationDir/fileName", new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/sourceDir/fileName", "/destinationDir", false)); // 上書き禁止で例外を期待
        }

        [TestMethod]
        public void MoveNode_EmptyDirectory_MovesSuccessfully()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/emptyDir");
            storage.AddDirectory("/newDir/emptyDir", true);
            storage.MoveNode("/emptyDir", "/newDir/emptyDir");

            Assert.IsFalse(storage.NodeExists("/emptyDir"));
            Assert.IsTrue(storage.NodeExists("/newDir/emptyDir/emptyDir"));
        }

        [TestMethod]
        public void MoveNode_MultiLevelDirectory_MovesSuccessfully()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir/subDir1/subDir2", true);
            storage.AddDirectory("/destinationDir");
            storage.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            Assert.IsFalse(storage.NodeExists("/sourceDir"));
            Assert.IsTrue(storage.NodeExists("/destinationDir/sourceDir/subDir1/subDir2"));
        }

        [TestMethod]
        public void MoveNode_WithInvalidPath_ThrowsException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/validDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode("/invalid?Path", "/validDir"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MoveNode_DirectoryToFile_ThrowsException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/sourceDir");
            storage.AddItem("/destinationFile", new BinaryData([1, 2, 3]));
            storage.MoveNode("/sourceDir", "/destinationFile");
        }

        [TestMethod]
        public void MoveNode_WithinSameDirectory_RenamesNode()
        {
            VirtualStorage storage = new();
            storage.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            storage.MoveNode("/sourceFile", "/renamedFile");

            Assert.IsFalse(storage.NodeExists("/sourceFile"));
            Assert.IsTrue(storage.NodeExists("/renamedFile"));

            byte[] result = ((VirtualItem<BinaryData>)storage.GetNode("/renamedFile")).ItemData.Data;
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
        }

        // 循環参照チェックテスト
        [TestMethod]
        public void MoveNode_WhenDestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/parentDir/subDir", true);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/parentDir", "/parentDir/subDir"));
        }

        // 移動先と移動元が同じかどうかのチェックテスト
        [TestMethod]
        public void MoveNode_WhenSourceAndDestinationAreSame_ThrowsInvalidOperationException()
        {
            VirtualStorage storage = new();
            storage.AddItem("/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode("/file", "/file"));
        }

        // 移動先の親ディレクトリが存在しない場合のテスト
        [TestMethod]
        public void MoveNode_WhenDestinationParentDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            VirtualStorage storage = new();
            storage.AddDirectory("/existingDir");
            storage.AddItem("/existingDir/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode("/existingDir/file", "/nonExistentDir/file"));
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage storage = new();
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
            VirtualStorage storage = new();
            storage.AddDirectory("/test");

            // Act
            bool exists = storage.SymbolicLinkExists("/test/nonexistentLink");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenParentDirectoryIsALinkAndLinkExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/realParent");
            storage.AddSymbolicLink("/linkedParent", "/realParent");
            storage.AddDirectory("/realParent/testDir");
            storage.AddSymbolicLink("/linkedParent/myLink", "/realParent/testDir");

            // Act
            bool exists = storage.SymbolicLinkExists("/linkedParent/myLink");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void AddSymbolicLink_WhenLinkIsNew_AddsSuccessfully()
        {
            // Arrange
            VirtualStorage storage = new();
            storage.AddDirectory("/test");

            // Act
            storage.AddSymbolicLink("/test/newLink", "/target/path");

            // Assert
            Assert.IsTrue(storage.SymbolicLinkExists("/test/newLink"));
            VirtualSymbolicLink? link = storage.GetNode("/test/newLink") as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.IsTrue(link.TargetPath == "/target/path");
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsFalseAndLinkExists_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage storage = new();
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
            VirtualStorage storage = new();
            storage.AddDirectory("/test");
            storage.AddSymbolicLink("/test/existingLink", "/old/target/path");

            // Act
            storage.AddSymbolicLink("/test/existingLink", "/new/target/path", overwrite: true);

            // Assert
            VirtualSymbolicLink? link = storage.GetNode("/test/existingLink") as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.IsTrue(link.TargetPath == "/new/target/path");
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingItem_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage storage = new();
            BinaryData itemData = [1, 2, 3];

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
            VirtualStorage storage = new();

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
            VirtualStorage virtualStorage = new();
            VirtualPath nonExistentParentDirectory = "/nonexistent/directory";
            VirtualPath symbolicLinkPath = nonExistentParentDirectory + "link";
            VirtualPath targetPath = "/existing/target";

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddSymbolicLink(symbolicLinkPath, targetPath),
                "親ディレクトリが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

        [TestMethod]
        public void WalkPathToTarget_Root()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/";
            VirtualPath targetPath = path;

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Directory1()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/dir1";
            vs.AddDirectory(path, true);
            VirtualPath targetPath = path;

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Directory2()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/dir1/dir2";
            vs.AddDirectory(path, true);
            VirtualPath targetPath = path;

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Item1()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/item";
            vs.AddItem(path, new BinaryData[1, 2, 3]);
            VirtualPath targetPath = path;

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Item2()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/dir1/item";
            vs.AddDirectory(path.DirectoryPath, true);
            vs.AddItem(path, new BinaryData[1, 2, 3]);
            VirtualPath targetPath = path;

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink1()
        {
            VirtualPath targetPath = "/dir1/link1/item";
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddItem("/dir2/item", new BinaryData[1, 2, 3]);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink2()
        {
            VirtualPath targetPath = "/dir1/link1/dir3";
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddDirectory("/dir2/dir3", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink3()
        {
            VirtualPath targetPath = "/dir1/link1";
            VirtualPath linkTargetPath = "/dir2";
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir1/link1", linkTargetPath);

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(linkTargetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath targetPath = "/nonexistent";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathWithExceptionEnabled()
        {
            VirtualStorage vs = new();
            VirtualPath targetPath = "/nonexistent";

            VirtualNodeNotFoundException exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, true);
            });
            Debug.WriteLine($"ExceptionMessage: {exception.Message}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath2()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/dir1/item";
            vs.AddDirectory(path.DirectoryPath, true);
            vs.AddItem(path, new BinaryData[1, 2, 3]);
            VirtualPath targetPath = "/dir1/item/dir2";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath3()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "/dir1/dir2";
            vs.AddDirectory(path.DirectoryPath, true);
            VirtualPath targetPath = "/dir1/dir2/dir3";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath4()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            VirtualPath targetPath = "/dir1/link1/dir3";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_CircularSymbolicLink()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir2/link2", "/dir1");

            NodeInformation? result = vs.WalkPathToTarget("/dir1/link1/link2/link1", notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLinkToNonExistentPath()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/link1", "/nonexistent");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                NodeInformation? result = vs.WalkPathToTarget("/dir1/link1", notifyNode, null, true, true);
            });
        }

        [TestMethod]
        public void WalkPathToTarget_RelativePath()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = "dir2";
            vs.AddDirectory("/dir1/dir2", true);
            VirtualPath targetPath = path;

            vs.ChangeDirectory("/dir1");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_DirAndDotDot()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1/dir2", true);
            vs.AddDirectory("/dir1/dir3", true);
            VirtualPath targetPath = "/dir1/dir2/../dir3";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_LinkAndDotDot()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddDirectory("/dir1/dir3", true);
            VirtualPath targetPath = "/dir1/link1/../dir3";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }


        [TestMethod]
        public void WalkPathToTarget_MultipleLink()
        {
            // テストデータの設定
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddDirectory("/dir3");
            vs.AddItem("/dir3/item", "FinalItem");
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddSymbolicLink("/dir2/link2", "/dir3");

            // メソッドを実行
            NodeInformation? result = vs.WalkPathToTarget("/dir1/link1/link2/item", notifyNode, null, true, false);

            // 結果を検証
            Assert.IsTrue(result?.TraversalPath == "/dir1/link1/link2/item");
            Assert.IsTrue(result?.ResolvedPath == "/dir3/item");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathAndCreatePath()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath targetPath = "/dir1";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, actionNode, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual("dir1", node.Name.ToString());
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathAndCreatePath2()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath targetPath = "/dir1/dir2";

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, actionNode, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual("dir2", node.Name.ToString());
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        private void notifyNode(VirtualPath path, VirtualNode? node, bool isEnd)
        {
            Debug.WriteLine($"Path: {path}, Node: {node}, isEnd: {isEnd}");
        }

        private bool actionNode(VirtualDirectory directory, VirtualNodeName nodeName)
        {
            VirtualDirectory newDirectory = new VirtualDirectory(nodeName);

            directory.Add(newDirectory);

            return true;
        }

        [TestMethod]
        public void WalkPathTree_Test1()
        {
            VirtualStorage vs = new();
            vs.AddItem("/item0", "test");
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/item1", "test");
            vs.AddItem("/dir1/item2", "test");
            vs.AddDirectory("/dir2/sub", true);
            vs.AddItem("/dir2/sub/item3", "test");
            vs.AddItem("/dir2/sub/item4", "test");
            vs.AddItem("/dir2/sub/item5", "test");
            vs.AddItem("/item6", "test");
            vs.AddItem("/item8", "test");
            vs.AddSymbolicLink("/item7", "/dir1");
            vs.AddDirectory("/all-dir", true);
            vs.AddSymbolicLink("/all-dir/item1", "/dir1/item1");
            vs.AddSymbolicLink("/all-dir/item2", "/dir1/item2");


            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            //Assert.AreEqual(4, result.Count());

            string tree = vs.GenerateTextBasedTreeStructure("/", true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void WalkPathTree_EmptyDirectory_ShouldReturnOnlyDirectory()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/emptyDir", true);

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/emptyDir", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(1, result.Count()); // 空のディレクトリ自身のみが結果として返されるべき
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "."));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithItems_ShouldReturnItems()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dirWithItems", true);
            vs.AddItem("/dirWithItems/item1", "content1");
            vs.AddItem("/dirWithItems/item2", "content2");

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/dirWithItems", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(3, result.Count()); // ディレクトリと2つのアイテムが含まれる
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item2"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithSymbolicLink_ShouldFollowLinkIfRequested()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/sourceDir", true);
            vs.AddItem("/sourceDir/item", "content");
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "linkToSourceDir/item"));
        }

        [TestMethod]
        public void WalkPathTree_EmptyDirectory_ReturnsOnlyRoot()
        {
            VirtualStorage vs = new();
            IEnumerable<NodeInformation> result = vs.WalkPathTree(VirtualPath.Root, VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "."));
        }

        [TestMethod]
        public void WalkPathTree_SingleItemDirectory_IncludesItem()
        {
            VirtualStorage vs = new();
            vs.AddItem("/item1", "test");

            IEnumerable<NodeInformation> result = vs.WalkPathTree(VirtualPath.Root, VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(2, result.Count()); // ルートディレクトリとアイテム
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item1"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithSymbolicLink_IncludesLinkAndTarget()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/linkToDir1", "/dir1");

            IEnumerable<NodeInformation> result = vs.WalkPathTree(VirtualPath.Root, VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(3, result.Count()); // ルートディレクトリ、dir1、およびシンボリックリンク
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "dir1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "linkToDir1"));
        }

        [TestMethod]
        public void WalkPathTree_DeepNestedDirectories_ReturnsAllNodes()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/dir2/dir3/item1", "test");

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(5, result.Count()); // ルートディレクトリ、dir1、dir2、dir3、および item1
        }

        [TestMethod]
        public void WalkPathTree_MultipleSymbolicLinks_IncludesLinksAndTargets()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/linkToDir1", "/dir1");
            vs.AddSymbolicLink("/linkToLink1", "/linkToDir1");

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(4, result.Count()); // ルートディレクトリ、dir1、linkToDir1、および linkToLink1
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithManyItems_ReturnsAllItems()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            for (int i = 1; i <= 100; i++)
            {
                vs.AddItem($"/dir1/item{i}", $"test{i}");
            }

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/dir1", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(101, result.Count()); // dir1 および 100個のアイテム
        }

        [TestMethod]
        public void WalkPathTree_WithNonexistentPathSymbolicLink_ThrowsExceptionAndOutputsMessage()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/emptyLink", "/nonexistent");

            VirtualNodeNotFoundException exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
                foreach (NodeInformation item in result)
                {
                    Debug.WriteLine(item.ToString());
                }
            });

            Debug.WriteLine($"例外が捕捉されました: {exception.Message}");
        }

        [TestMethod]
        public void WalkPathTree_ShallowNestedDirectories_ExecutesWithoutErrorAndOutputsTree()
        {
            VirtualStorage vs = new();
            string basePath = "/deep";
            int depth = 100; // 最大1900くらいまでOK
            for (int i = 1; i <= depth; i++)
            {
                basePath = $"{basePath}/dir{i}";
                vs.AddDirectory(basePath, true);
            }

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine($"ノード名: {item.TraversalPath}, 解決済みパス: {item.ResolvedPath}");
            }

            // 期待される結果の数をルートディレクトリ + /deep + depth(3)のディレクトリ = 5に更新
            Assert.AreEqual(depth + 2, result.Count()); // ルートディレクトリ + /deep + 3階層のディレクトリ
            Debug.WriteLine($"深さ{depth}のディレクトリ構造が正常に走査されました。");
        }

        [TestMethod]
        public void WalkPathTree_MultipleEmptyDirectories_ReturnsAllDirectoriesAndOutputsTree()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/empty1/empty2/empty3", true);

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine($"ノード名: {item.TraversalPath}, 解決済みパス: {item.ResolvedPath}");
            }

            Assert.AreEqual(4, result.Count()); // ルートディレクトリ + 各空ディレクトリ
            Debug.WriteLine("複数レベルの空ディレクトリが正常に走査されました。");
        }

        [TestMethod]
        public void WalkPathTree_MultipleSymbolicLinksInSamePath()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/targetDir", true); // ターゲットディレクトリ
            vs.AddDirectory("/dir", true); // シンボリックリンクのための親ディレクトリ
            vs.AddSymbolicLink("/dir/symLink1", "/targetDir");
            vs.AddSymbolicLink("/dir/symLink2", "/targetDir");

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            // `/dir/symLink1` と `/dir/symLink2` の存在を確認
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("dir/symLink1")));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("dir/symLink2")));
        }

        [TestMethod]
        public void WalkPathTree_SymbolicLinkPointsToAnotherSymbolicLink()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/targetDir", true); // ターゲットディレクトリ
            vs.AddSymbolicLink("/symLink", "/targetDir");
            vs.AddSymbolicLink("/linkToLink", "/symLink");

            IEnumerable<NodeInformation> result = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (NodeInformation item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("symLink")));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("linkToLink")));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnOnlyItemsInDir1()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/item1", "content1");
            vs.AddItem("/dir1/item2", "content2");

            List<NodeInformation> result = vs.WalkPathTree("/dir1", VirtualNodeTypeFilter.Item, true, true).ToList();

            // ディレクトリ自体は含まれないため、アイテムの数だけを期待する
            Assert.AreEqual(2, result.Count); // 正しいアイテム数を確認
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item2"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnOnlyDirectoriesInDir2IncludingDirItself()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir2/subdir1", true);
            vs.AddDirectory("/dir2/subdir2", true);

            List<NodeInformation> result = vs.WalkPathTree("/dir2", VirtualNodeTypeFilter.Directory, true, true).ToList();

            // ディレクトリ自体も含む
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "."));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "subdir1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "subdir2"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnDirectoriesAndItemsIncludingDirItself()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir3", true);
            vs.AddItem("/dir3/item1", "content1");
            vs.AddDirectory("/dir3/subdir1", true);

            List<NodeInformation> result = vs.WalkPathTree("/dir3", VirtualNodeTypeFilter.Directory | VirtualNodeTypeFilter.Item, true, false).ToList();

            // ディレクトリ自体を含むため、期待される数はノード数+1
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "."));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "subdir1"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnAllTypesIncludingDirItself()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir4", true);
            vs.AddItem("/dir4/item1", "content1");
            vs.AddDirectory("/dir4/subdir1", true);
            vs.AddSymbolicLink("/dir4/link1", "/dir4/item1");

            List<NodeInformation> result = vs.WalkPathTree("/dir4", VirtualNodeTypeFilter.All, true, true).ToList();

            // ディレクトリ自体も含む
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "."));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "subdir1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "link1"));
        }

        [TestMethod]
        public void WalkPathTree_WithNoFilter_ShouldNotReturnAnyNodes()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir2", true);
            vs.AddItem("/dir2/item1", "content1");
            vs.AddDirectory("/dir2/subdir1", true);

            List<NodeInformation> result = vs.WalkPathTree("/dir2", VirtualNodeTypeFilter.None, true, true).ToList();

            Assert.AreEqual(0, result.Count); // フィルター未適用の場合、ノードは返されない
        }

        [TestMethod]
        public void WalkPathTree_WithDirectoryFilterButNoDirectories_ShouldReturnEmptyList()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir3", true);
            vs.AddItem("/dir3/item1", "content1");
            vs.AddItem("/dir3/item2", "content2");

            List<NodeInformation> result = vs.WalkPathTree("/dir3", VirtualNodeTypeFilter.Directory, true, true).ToList();

            Assert.AreEqual(1, result.Count); // ディレクトリが存在しない場合でも、指定したディレクトリ自体は結果に含まれる
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "."));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnOnlySymbolicLinksInDir1()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/link1", "/item1");
            vs.AddSymbolicLink("/dir1/link2", "/item2");

            List<NodeInformation> result = vs.WalkPathTree("/dir1", VirtualNodeTypeFilter.SymbolicLink, true, false).ToList();

            // シンボリックリンクのみをフィルタリングするため、シンボリックリンクの数だけを期待する
            Assert.AreEqual(2, result.Count); // シンボリックリンクの正しい数を確認
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "link1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "link2"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnNoNodesWhenFilterDoesNotMatch()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir3", true);
            vs.AddItem("/dir3/item1", "content1");

            List<NodeInformation> result = vs.WalkPathTree("/dir3", VirtualNodeTypeFilter.SymbolicLink, true, true).ToList();

            // シンボリックリンクのみをフィルタリングし、ディレクトリにシンボリックリンクがない場合、結果は空であることを期待する
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithDeepNestingAndRecursiveFalse_ShouldReturnImmediateChildrenOnly()
        {
            VirtualStorage vs = new VirtualStorage();
            // 複数階層のディレクトリとアイテムを追加
            vs.AddDirectory("/testDir", true);
            vs.AddItem("/testDir/item1", "content1");
            vs.AddDirectory("/testDir/subDir1", true);
            vs.AddItem("/testDir/subDir1/item2", "content2");
            vs.AddDirectory("/testDir/subDir1/subSubDir1", true);
            vs.AddItem("/testDir/subDir1/subSubDir1/item3", "content3");

            // 再帰なしで走査
            List<NodeInformation> result = vs.WalkPathTree("/testDir", VirtualNodeTypeFilter.All, false, false).ToList();

            // 直下のアイテムとサブディレクトリのみが返されることを確認
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result[0].TraversalPath == ".");
            Assert.IsTrue(result[1].TraversalPath == "subDir1");
            Assert.IsTrue(result[2].TraversalPath == "item1");
        }

        [TestMethod]
        public void WalkPathTree_StartFromSubdirectoryWithRecursiveFalse_ShouldReturnOrderedImmediateChildrenOnly()
        {
            VirtualStorage vs = new VirtualStorage();
            // 深い階層のディレクトリとアイテムを追加
            vs.AddDirectory("/testDir", true);
            vs.AddItem("/testDir/item1", "content1");
            vs.AddDirectory("/testDir/subDir1", true);
            vs.AddItem("/testDir/subDir1/item2", "content2");
            vs.AddDirectory("/testDir/subDir1/subSubDir1", true);
            vs.AddItem("/testDir/subDir1/subSubDir1/item3", "content3");
            vs.AddDirectory("/testDir/subDir1/subSubDir1/subSubSubDir1", true);
            vs.AddItem("/testDir/subDir1/subSubDir1/subSubSubDir1/item4", "content4");

            // 再帰なしで/testDir/subDir1から走査を開始
            List<NodeInformation> result = vs.WalkPathTree("/testDir/subDir1", VirtualNodeTypeFilter.All, false, false).ToList();

            // 結果の順番と内容を検証
            Assert.AreEqual(3, result.Count, "Should return the starting directory and its immediate children only.");
            Assert.IsTrue(result[0].TraversalPath.ToString() == ".");
            Assert.IsTrue(result[1].TraversalPath.ToString() == "subSubDir1");
            Assert.IsTrue(result[2].TraversalPath.ToString() == "item2");
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnDirectoryForDirectoryPath()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);

            VirtualNodeType result = vs.GetNodeType("/dir1");

            Assert.AreEqual(VirtualNodeType.Directory, result);
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnItemForItemPath()
        {
            VirtualStorage vs = new();
            vs.AddItem("/item1", "content1");

            VirtualNodeType result = vs.GetNodeType("/item1");

            Assert.AreEqual(VirtualNodeType.Item, result);
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnSymbolicLinkForLinkPathWithoutFollowing()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/link1", "/dir1");

            VirtualNodeType result = vs.GetNodeType("/link1", false);

            Assert.AreEqual(VirtualNodeType.SymbolicLink, result);
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnDirectoryForLinkPathWhenFollowing()
        {
            VirtualStorage vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/link1", "/dir1");

            VirtualNodeType result = vs.GetNodeType("/link1", true);

            Assert.AreEqual(VirtualNodeType.Directory, result);
        }
    }
}
