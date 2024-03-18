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
            VirtualPath name = new VirtualPath("TestItem");
            var item = new SimpleData(10);
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;

            // Act
            var virtualItem = new VirtualItem<SimpleData>(name, item, createdDate, updatedDate);

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
            var path = new VirtualPath("/path/to/../directory/./");
            var expected = new VirtualPath("/path/directory");

            var result = path.NormalizePath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizePath_WithRelativePath_ReturnsNormalizedPath()
        {
            var path = new VirtualPath("path/to/../directory/./");
            var expected = new VirtualPath("path/directory");

            var result = path.NormalizePath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NormalizePath_WithEmptyPath()
        {
            var path = VirtualPath.Empty;

            Assert.AreEqual(VirtualPath.Empty, path.NormalizePath());
        }

        [TestMethod]
        public void NormalizePath_WithInvalidPath_ThrowsInvalidOperationException()
        {
            var path = new VirtualPath("/../");

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                path = path.NormalizePath();
            });
        }

        [TestMethod]
        public void NormalizePath_WithPathEndingWithParentDirectory_ReturnsNormalizedPath()
        {
            var path = new VirtualPath("aaa/../..");
            var expected = VirtualPath.DotDot;

            var result = path.NormalizePath();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DirectoryPath_ReturnsCorrectPath_ForAbsolutePath()
        {
            // テストデータ
            var absolutePath = new VirtualPath("/directory/subdirectory/file");

            // メソッドを実行(キャッシュなし)
            var result = absolutePath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("/directory/subdirectory", result.Path);

            // メソッドを実行(キャッシュあり)
            result = absolutePath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("/directory/subdirectory", result.Path);
        }

        [TestMethod]
        public void DirectoryPath_ReturnsRoot_ForRootPath()
        {
            // テストデータ
            var rootPath = VirtualPath.Root;

            // メソッドを実行
            var result = rootPath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("/", result.Path);
        }

        [TestMethod]
        public void DirectoryPath_ReturnsSamePath_ForRelativePath()
        {
            // テストデータ
            var relativePath = new VirtualPath("file");

            // メソッドを実行
            var result = relativePath.DirectoryPath;

            // 結果を検証
            Assert.AreEqual("file", result.Path);
        }

        [TestMethod]
        public void NodeName_WithFullPath_ReturnsNodeName()
        {
            // テストデータ
            var path = new VirtualPath("/path/to/node");
            var expectedNodeName = new VirtualPath("node");

            // メソッドを実行(キャッシュなし)
            var actualNodeName = path.NodeName;

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
            var path = new VirtualPath("node");
            var expectedNodeName = new VirtualPath("node");

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithEmptyString_ReturnsEmptyString()
        {
            var path = VirtualPath.Empty;
            var expectedNodeName = VirtualPath.Empty;

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithRootPath_ReturnsRootPath()
        {
            var path = VirtualPath.Root;
            var expectedNodeName = VirtualPath.Root;

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithDot_ReturnsDot()
        {
            var path = VirtualPath.Dot;
            var expectedNodeName = VirtualPath.Dot;

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithDotDot_ReturnsDotDot()
        {
            var path = VirtualPath.DotDot;
            var expectedNodeName = VirtualPath.DotDot;

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithRelativePathUsingDot_ReturnsNodeName()
        {
            var path = new VirtualPath("./node");
            var expectedNodeName = new VirtualPath("node");

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void NodeName_WithRelativePathUsingDotDot_ReturnsNodeName()
        {
            var path = new VirtualPath("../node");
            var expectedNodeName = new VirtualPath("node");

            var actualNodeName = path.NodeName;

            Assert.AreEqual(expectedNodeName, actualNodeName);
        }

        [TestMethod]
        public void Combine_Path1EndsWithSlash_CombinesCorrectly()
        {
            var path1 = new VirtualPath("path/to/directory/");
            var path2 = new VirtualPath("file.txt");
            var expected = new VirtualPath("path/to/directory/file.txt");

            var result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_Path1DoesNotEndWithSlash_CombinesCorrectly()
        {
            var path1 = new VirtualPath("path/to/directory");
            var path2 = new VirtualPath("file.txt");
            var expected = new VirtualPath("path/to/directory/file.txt");

            var result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_Path2StartsWithSlash_CombinesCorrectly()
        {
            var path1 = new VirtualPath("path/to/directory");
            var path2 = new VirtualPath("/file.txt");
            var expected = new VirtualPath("path/to/directory/file.txt");

            var result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_BothPathsAreEmpty_ReturnsSlash()
        {
            var path1 = VirtualPath.Empty;
            var path2 = VirtualPath.Empty;
            var expected = VirtualPath.Empty;

            var result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath1Empty_ReturnsPath2()
        {
            var path1 = VirtualPath.Empty;
            var path2 = new VirtualPath("/directory/subdirectory");
            var expected = path2;

            var result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath2Empty_ReturnsPath1()
        {
            var path1 = new VirtualPath("/directory/subdirectory");
            var path2 = VirtualPath.Empty;
            var expected = path1;

            var result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetParentPath_WithRootPath_ReturnsEmpty()
        {
            var path = VirtualPath.Root;
            var expected = VirtualPath.Root;

            var actual = path.GetParentPath();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithSingleLevelPath_ReturnsRoot()
        {
            var path = new VirtualPath("/level1");
            var expected = VirtualPath.Root;

            var actual = path.GetParentPath();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithMultiLevelPath_ReturnsParentPath()
        {
            var path = new VirtualPath("/level1/level2/level3");
            var expected = new VirtualPath("/level1/level2");

            var actual = path.GetParentPath();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetParentPath_WithTrailingSlash_ReturnsParentPath()
        {
            var path = new VirtualPath("/level1/level2/level3/");
            var expected = new VirtualPath("/level1/level2");

            var actual = path.GetParentPath();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithAbsolutePath_ReturnsCorrectParts()
        {
            // Arrange
            var virtualPath = new VirtualPath("/folder1/folder2/file");
            var expected = new LinkedList<VirtualPath>(new[] {
                new VirtualPath("folder1"),
                new VirtualPath("folder2"),
                new VirtualPath("file")
            });

            // Act
            var actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithRelativePath_ReturnsCorrectParts()
        {
            // Arrange
            var virtualPath = new VirtualPath("folder1/folder2/file");
            var expected = new LinkedList<VirtualPath>(new[] {
                new VirtualPath("folder1"),
                new VirtualPath("folder2"),
                new VirtualPath("file")
            });

            // Act
            var actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithEmptySegments_IgnoresEmptySegments()
        {
            // Arrange
            var virtualPath = new VirtualPath("folder1//folder2///file");
            var expected = new LinkedList<VirtualPath>(new[] {
                new VirtualPath("folder1"),
                new VirtualPath("folder2"),
                new VirtualPath("file")
            });

            // Act
            var actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPartsLinkedList_WithOnlySlashes_ReturnsEmptyList()
        {
            // Arrange
            var virtualPath = new VirtualPath("///");
            var expected = new LinkedList<VirtualPath>();

            // Act
            var actual = virtualPath.GetPartsLinkedList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Equals_WithSamePath_ReturnsTrue()
        {
            // Arrange
            var path1 = new VirtualPath("/path/to/resource");
            var path2 = new VirtualPath("/path/to/resource");

            // Act
            bool result = path1.Equals(path2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_WithDifferentPath_ReturnsFalse()
        {
            // Arrange
            var path1 = new VirtualPath("/path/to/resource");
            var path2 = new VirtualPath("/path/to/another/resource");

            // Act
            bool result = path1.Equals(path2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var path1 = new VirtualPath("/path/to/resource");
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
            var path = new VirtualPath("/path/to/directory/file");
            var expected = new List<VirtualPath>
        {
            new VirtualPath("path"),
            new VirtualPath("to"),
            new VirtualPath("directory"),
            new VirtualPath("file")
        };

            // メソッドを実行
            var result = path.PartsList;

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
            var path = new VirtualPath("/path/to/directory/file");

            // 最初のアクセスでリストを生成
            var firstAccessResult = path.PartsList;
            // 2回目のアクセス
            var secondAccessResult = path.PartsList;

            // キャッシュされたオブジェクトが再利用されているか検証
            Assert.AreSame(firstAccessResult, secondAccessResult, "The PartsList should be cached and reused on subsequent accesses.");
        }

        [TestMethod]
        public void CompareTo_ReturnsNegative_WhenPathIsLexicographicallyBeforeOther()
        {
            var path1 = new VirtualPath("a/b");
            var path2 = new VirtualPath("b/a");
            Assert.IsTrue(path1.CompareTo(path2) < 0, "path1 should be lexicographically before path2");
        }

        [TestMethod]
        public void CompareTo_ReturnsZero_WhenPathsAreEqual()
        {
            var path1 = new VirtualPath("same/path");
            var path2 = new VirtualPath("same/path");
            Assert.AreEqual(0, path1.CompareTo(path2), "Paths that are equal should return 0");
        }

        [TestMethod]
        public void CompareTo_ReturnsPositive_WhenPathIsLexicographicallyAfterOther()
        {
            var path1 = new VirtualPath("c/d");
            var path2 = new VirtualPath("a/b");
            Assert.IsTrue(path1.CompareTo(path2) > 0, "path1 should be lexicographically after path2");
        }
    }

    [TestClass]
    public class VirtualSymbolicLinkTests
    {
        [TestMethod]
        public void VirtualSymbolicLink_Constructor_SetsNameAndTargetPath()
        {
            // Arrange
            VirtualPath name = new VirtualPath("TestLink");
            VirtualPath targetPath = new VirtualPath("/target/path");

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
            VirtualPath name = new VirtualPath("TestLink");
            VirtualPath targetPath = new VirtualPath("/target/path");
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
            VirtualPath name = new VirtualPath("TestLink");
            VirtualPath targetPath = new VirtualPath("/target/path");
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

        [TestMethod]
        public void VirtualSymbolicLink_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var symbolicLink = new VirtualSymbolicLink(new VirtualPath("LinkToRoot"), VirtualPath.Root);

            // Act
            var result = symbolicLink.ToString();
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
            var testData = new byte[] { 1, 2, 3 };

            // BinaryData オブジェクトを作成
            var binaryData = new BinaryData(testData);

            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualPath name = new VirtualPath("TestBinaryItem");
            var virtualItem = new VirtualItem<BinaryData>(name, binaryData);

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
            byte[] testData = new byte[] { 1, 2, 3 };
            var originalItem = new VirtualItem<BinaryData>(new VirtualPath("TestItem"), new BinaryData(testData));

            // DeepClone メソッドを使用してクローンを作成
            var clonedItem = originalItem.DeepClone() as VirtualItem<BinaryData>;

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
            var originalItem = new VirtualItem<BinaryData>(new VirtualPath("item"), new BinaryData(new byte[] { 1, 2, 3 }));

            // Act
            var clonedItem = ((IDeepCloneable<VirtualItem<BinaryData>>)originalItem).DeepClone();

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
            var originalItem = new VirtualItem<SimpleData>(new VirtualPath("item"), new SimpleData(5));

            // Act
            var clonedItem = ((IDeepCloneable<VirtualItem<SimpleData>>)originalItem).DeepClone();

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
            VirtualItem<int> item = new VirtualItem<int>(new VirtualPath("TestItem"), value);

            string result = item.ToString();
            Debug.WriteLine(result);
            
            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithBinaryDataOfLong()
        {
            BinaryData data = new BinaryData([0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F]);
            VirtualItem<BinaryData> item = new VirtualItem<BinaryData>(new VirtualPath("TestItem"), data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithBinaryDataOfShort()
        {
            BinaryData data = new BinaryData([0x01, 0x02, 0x03]);
            VirtualItem<BinaryData> item = new VirtualItem<BinaryData>(new VirtualPath("TestItem"), data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithSimpleData()
        {
            SimpleData data = new SimpleData(10);
            VirtualItem<SimpleData> item = new VirtualItem<SimpleData>(new VirtualPath("TestItem"), data);

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

            VirtualItem<SimpleStruct> item = new VirtualItem<SimpleStruct>(new VirtualPath("TestItem"), data);

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
            VirtualPath name = new VirtualPath("TestDirectory");
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
            var originalDirectory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            originalDirectory.AddDirectory(new VirtualPath("Node1"));
            originalDirectory.AddDirectory(new VirtualPath("Node2"));

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
        public void DirectoryCount_WithMultipleDirectories_ReturnsCorrectCount()
        {
            var storage = new VirtualStorage();
            storage.Root.AddDirectory(new VirtualPath("dir1"));
            storage.Root.AddDirectory(new VirtualPath("dir2"));

            Assert.AreEqual(2, storage.Root.DirectoryCount, "ルートディレクトリ内のディレクトリ数が正しくありません。");
        }

        [TestMethod]
        public void ItemCount_WithMultipleItems_ReturnsCorrectCount()
        {
            var storage = new VirtualStorage();
            storage.Root.AddItem(new VirtualPath("item1"), new object());
            storage.Root.AddItem(new VirtualPath("item2"), new object());

            Assert.AreEqual(2, storage.Root.ItemCount, "ルートディレクトリ内のアイテム数が正しくありません。");
        }

        [TestMethod]
        public void SymbolicLinkCount_WithMultipleSymbolicLinks_ReturnsCorrectCount()
        {
            var storage = new VirtualStorage();
            storage.Root.Add(new VirtualSymbolicLink(new VirtualPath("link1"), new VirtualPath("/path/to/target1")));
            storage.Root.Add(new VirtualSymbolicLink(new VirtualPath("link2"), new VirtualPath("/path/to/target2")));

            Assert.AreEqual(2, storage.Root.SymbolicLinkCount, "ルートディレクトリ内のシンボリックリンク数が正しくありません。");
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualDirectory()
        {
            // Arrange
            var originalDirectory = new VirtualDirectory(new VirtualPath("original"));
            originalDirectory.Add(new VirtualItem<BinaryData>(new VirtualPath("item"), new BinaryData(new byte[] { 1, 2, 3 })));

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
            var originalDirectory = new VirtualDirectory(new VirtualPath("original"));
            var subDirectory = new VirtualDirectory(new VirtualPath("sub"));
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
            var originalDirectory = new VirtualDirectory(new VirtualPath("original"));
            var nonCloneableItem = new SimpleData(10); // SimpleDataはIDeepCloneableを実装していない
            var virtualItem = new VirtualItem<SimpleData>(new VirtualPath("item"), nonCloneableItem);
            originalDirectory.Add(virtualItem);

            // Act
            var clonedDirectory = ((IDeepCloneable<VirtualDirectory>)originalDirectory).DeepClone();

            // Assert
            Assert.IsNotNull(clonedDirectory);
            Assert.AreNotSame(originalDirectory, clonedDirectory);
            Assert.AreEqual(originalDirectory.Name, clonedDirectory.Name);
            Assert.AreEqual(originalDirectory.Count, clonedDirectory.Count);

            var originalItem = originalDirectory.Get(new VirtualPath("item")) as VirtualItem<SimpleData>;
            var clonedItem = clonedDirectory.Get(new VirtualPath("item")) as VirtualItem<SimpleData>;
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
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            var newNode = new VirtualItem<BinaryData>(new VirtualPath("NewItem"), new BinaryData(testData));

            directory.Add(newNode);

            Assert.IsTrue(directory.NodeExists(new VirtualPath("NewItem")));
            Assert.AreEqual(newNode, directory[new VirtualPath("NewItem")]);
            CollectionAssert.AreEqual(testData, ((BinaryData)((VirtualItem<BinaryData>)directory[new VirtualPath("NewItem")]).ItemData).Data);
        }

        [TestMethod]
        public void Add_ExistingNodeWithoutOverwrite_ThrowsInvalidOperationException()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            var originalNode = new VirtualItem<BinaryData>(new VirtualPath("OriginalItem"), new BinaryData(testData));
            directory.Add(originalNode);

            var newNode = new VirtualItem<BinaryData>(new VirtualPath("OriginalItem"), new BinaryData(testData));

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.Add(newNode);
            });
        }

        [TestMethod]
        public void Add_ExistingNodeWithOverwrite_OverwritesNode()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            var originalNode = new VirtualItem<BinaryData>(new VirtualPath("OriginalItem"), new BinaryData(testData));
            directory.Add(originalNode);

            var newTestData = new byte[] { 4, 5, 6 };
            var newNode = new VirtualItem<BinaryData>(new VirtualPath("OriginalItem"), new BinaryData(newTestData));

            directory.Add(newNode, allowOverwrite: true);

            Assert.AreEqual(newNode, directory[new VirtualPath("OriginalItem")]);
            CollectionAssert.AreEqual(newTestData, ((BinaryData)((VirtualItem<BinaryData>)directory[new VirtualPath("OriginalItem")]).ItemData).Data);
        }

        [TestMethod]
        public void Add_NewDirectory_AddsDirectoryCorrectly()
        {
            var parentDirectory = new VirtualDirectory(new VirtualPath("ParentDirectory"));
            var childDirectory = new VirtualDirectory(new VirtualPath("ChildDirectory"));

            parentDirectory.Add(childDirectory);

            Assert.IsTrue(parentDirectory.NodeExists(new VirtualPath("ChildDirectory")));
            Assert.AreEqual(childDirectory, parentDirectory[new VirtualPath("ChildDirectory")]);
        }

        [TestMethod]
        public void Add_ExistingDirectoryWithoutOverwrite_ThrowsInvalidOperationException()
        {
            var parentDirectory = new VirtualDirectory(new VirtualPath("ParentDirectory"));
            var originalDirectory = new VirtualDirectory(new VirtualPath("OriginalDirectory"));
            parentDirectory.Add(originalDirectory);

            var newDirectory = new VirtualDirectory(new VirtualPath("OriginalDirectory"));

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.Add(newDirectory);
            });
        }

        [TestMethod]
        public void Add_ExistingDirectoryWithOverwrite_OverwritesDirectory()
        {
            var parentDirectory = new VirtualDirectory(new VirtualPath("ParentDirectory"));
            var originalDirectory = new VirtualDirectory(new VirtualPath("OriginalDirectory"));
            parentDirectory.Add(originalDirectory);

            var newDirectory = new VirtualDirectory(new VirtualPath("OriginalDirectory"));

            parentDirectory.Add(newDirectory, allowOverwrite: true);

            Assert.AreEqual(newDirectory, parentDirectory[new VirtualPath("OriginalDirectory")]);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });

            directory.AddItem(new VirtualPath("TestItem"), itemData, false);

            Assert.IsTrue(directory.NodeExists(new VirtualPath("TestItem")));
            var retrievedItem = directory.Get(new VirtualPath("TestItem")) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(itemData.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_BySimpleData()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var itemData = new SimpleData(5);

            directory.AddItem(new VirtualPath("TestItem"), itemData, false);

            Assert.IsTrue(directory.NodeExists(new VirtualPath("TestItem")));
            var retrievedItem = directory.Get(new VirtualPath("TestItem")) as VirtualItem<SimpleData>;
            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(itemData.Value, retrievedItem.ItemData.Value);
        }

        [TestMethod]
        public void AddItem_ThrowsWhenItemAlreadyExistsAndOverwriteIsFalse()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });
            directory.AddItem(new VirtualPath("TestItem"), itemData, false);

            Assert.ThrowsException<InvalidOperationException>(() =>
                directory.AddItem(new VirtualPath("TestItem"), new BinaryData(new byte[] { 4, 5, 6 }), false));
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddItem(new VirtualPath("TestItem"), new BinaryData(new byte[] { 1, 2, 3 }), false);

            var newItemData = new BinaryData(new byte[] { 4, 5, 6 });
            directory.AddItem(new VirtualPath("TestItem"), newItemData, true);

            var retrievedItem = directory.Get(new VirtualPath("TestItem")) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItemData.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddSymbolicLink_AddsNewLinkSuccessfully()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            directory.AddSymbolicLink(new VirtualPath("TestLink"), new VirtualPath("/path/to/target"), false);

            Assert.IsTrue(directory.NodeExists(new VirtualPath("TestLink")));
            var retrievedLink = directory.Get(new VirtualPath("TestLink")) as VirtualSymbolicLink;
            Assert.IsNotNull(retrievedLink);
            Assert.AreEqual(new VirtualPath("/path/to/target"), retrievedLink.TargetPath);
        }

        [TestMethod]
        public void AddSymbolicLink_ThrowsWhenLinkAlreadyExistsAndOverwriteIsFalse()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddSymbolicLink(new VirtualPath("TestLink"), new VirtualPath("/path/to/oldtarget"), false);

            Assert.ThrowsException<InvalidOperationException>(() =>
                directory.AddSymbolicLink(new VirtualPath("TestLink"), new VirtualPath("/path/to/newtarget"), false));
        }

        [TestMethod]
        public void AddSymbolicLink_OverwritesExistingLinkWhenAllowed()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddSymbolicLink(new VirtualPath("TestLink"), new VirtualPath("/path/to/oldtarget"), false);

            directory.AddSymbolicLink(new VirtualPath("TestLink"), new VirtualPath("/path/to/newtarget"), true);

            var retrievedLink = directory.Get(new VirtualPath("TestLink")) as VirtualSymbolicLink;
            Assert.IsNotNull(retrievedLink);
            Assert.AreEqual(new VirtualPath("/path/to/newtarget"), retrievedLink.TargetPath);
        }

        [TestMethod]
        public void AddDirectory_NewDirectory_AddsDirectoryCorrectly()
        {
            var parentDirectory = new VirtualDirectory(new VirtualPath("ParentDirectory"));

            parentDirectory.AddDirectory(new VirtualPath("ChildDirectory"));

            Assert.IsTrue(parentDirectory.NodeExists(new VirtualPath("ChildDirectory")));
            Assert.IsInstanceOfType(parentDirectory[new VirtualPath("ChildDirectory")], typeof(VirtualDirectory));
        }

        [TestMethod]
        public void AddDirectory_ExistingDirectoryWithoutOverwrite_ThrowsInvalidOperationException()
        {
            var parentDirectory = new VirtualDirectory(new VirtualPath("ParentDirectory"));
            parentDirectory.AddDirectory(new VirtualPath("OriginalDirectory"));

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                parentDirectory.AddDirectory(new VirtualPath("OriginalDirectory"));
            });
        }

        [TestMethod]
        public void AddDirectory_ExistingDirectoryWithOverwrite_OverwritesDirectory()
        {
            var parentDirectory = new VirtualDirectory(new VirtualPath("ParentDirectory"));
            parentDirectory.AddDirectory(new VirtualPath("OriginalDirectory"));

            parentDirectory.AddDirectory(new VirtualPath("OriginalDirectory"), allowOverwrite: true);

            Assert.IsTrue(parentDirectory.NodeExists(new VirtualPath("OriginalDirectory")));
            Assert.IsInstanceOfType(parentDirectory[new VirtualPath("OriginalDirectory")], typeof(VirtualDirectory));
        }
        
        [TestMethod]
        public void Indexer_ValidKey_ReturnsNode()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var node = new VirtualItem<BinaryData>(new VirtualPath("ItemData"), new BinaryData());
            directory.Add(node);

            var result = directory[new VirtualPath("ItemData")];

            Assert.AreEqual(node, result);
        }

        [TestMethod]
        public void Indexer_InvalidKey_ThrowsVirtualNodeNotFoundException()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                var result = directory[new VirtualPath("InvalidKey")];
            });
        }

        [TestMethod]
        public void Indexer_Setter_UpdatesNode()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var newNode = new VirtualItem<BinaryData>(new VirtualPath("NewItem"), new BinaryData());

            directory[new VirtualPath("NewItemKey")] = newNode;
            var result = directory[new VirtualPath("NewItemKey")];

            Assert.AreEqual(newNode, result);
        }

        [TestMethod]
        public void Remove_NonExistentNodeWithoutForce_ThrowsVirtualNodeNotFoundException()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.Remove(new VirtualPath("NonExistentNode"));
            });
        }

        [TestMethod]
        public void Remove_ExistingNode_RemovesNode()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            VirtualPath nodeName = new VirtualPath("ExistingNode");
            var node = new VirtualItem<BinaryData>(nodeName, new BinaryData(testData));
            directory.Add(node);

            directory.Remove(nodeName);

            Assert.IsFalse(directory.NodeExists(nodeName));
        }

        [TestMethod]
        public void Get_ExistingNode_ReturnsNode()
        {
            // VirtualDirectory オブジェクトを作成し、ノードを追加
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            var existingNode = new VirtualItem<BinaryData>(new VirtualPath("ExistingNode"), new BinaryData(testData));
            directory.Add(existingNode);

            // Get メソッドを使用してノードを取得
            var retrievedNode = directory.Get(new VirtualPath("ExistingNode"));

            // 取得したノードが期待通りであることを確認
            Assert.AreEqual(existingNode, retrievedNode);
        }

        [TestMethod]
        public void Get_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            // 存在しないノード名で Get メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                var retrievedNode = directory.Get(new VirtualPath("NonExistingNode"));
            });
        }

        [TestMethod]
        public void Rename_ExistingNode_RenamesNodeCorrectly()
        {
            // VirtualDirectory オブジェクトを作成し、ノードを追加
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            var existingNode = new VirtualItem<BinaryData>(new VirtualPath("ExistingNode"), new BinaryData(testData));
            directory.Add(existingNode);

            // Rename メソッドを使用してノードの名前を変更
            var newName = new VirtualPath("RenamedNode");
            directory.Rename(new VirtualPath("ExistingNode"), newName);

            // 名前が変更されたノードが存在し、元のノードが存在しないことを確認
            Assert.IsTrue(directory.NodeExists(newName));
            Assert.IsFalse(directory.NodeExists(new VirtualPath("ExistingNode")));
        }

        [TestMethod]
        public void Rename_NonExistingNode_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualDirectory オブジェクトを作成
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            // 存在しないノード名で Rename メソッドを呼び出すと例外がスローされることを確認
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                directory.Rename(new VirtualPath("NonExistingNode"), new VirtualPath("NewName"));
            });
        }

        [TestMethod]
        public void Rename_ToExistingNodeName_ThrowsInvalidOperationException()
        {
            // VirtualDirectory オブジェクトを作成し、2つのノードを追加
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var testData = new byte[] { 1, 2, 3 };
            var existingNode1 = new VirtualItem<BinaryData>(new VirtualPath("ExistingNode1"), new BinaryData(testData));
            var existingNode2 = new VirtualItem<BinaryData>(new VirtualPath("ExistingNode2"), new BinaryData(testData));
            directory.Add(existingNode1);
            directory.Add(existingNode2);

            // 既に存在するノード名に Rename メソッドを使用してノードの名前を変更しようとすると例外がスローされることを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                directory.Rename(new VirtualPath("ExistingNode1"), new VirtualPath("ExistingNode2"));
            });
        }

        [TestMethod]
        public void NodeNames_EmptyDirectory_ReturnsEmpty()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            var nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(0, nodeNames.Count());
        }

        [TestMethod]
        public void NodeNames_DirectoryWithNodes_ReturnsNodeNames()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddDirectory(new VirtualPath("Node1"));
            directory.AddDirectory(new VirtualPath("Node2"));

            var nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(2, nodeNames.Count());
            CollectionAssert.Contains(nodeNames.ToList(), new VirtualPath("Node1"));
            CollectionAssert.Contains(nodeNames.ToList(), new VirtualPath("Node2"));
        }

        [TestMethod]
        public void NodeNames_DirectoryWithNodesAfterRemovingOne_ReturnsRemainingNodeNames()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddDirectory(new VirtualPath("Node1"));
            directory.AddDirectory(new VirtualPath("Node2"));
            directory.Remove(new VirtualPath("Node1"));

            var nodeNames = directory.NodeNames;

            Assert.IsNotNull(nodeNames);
            Assert.AreEqual(1, nodeNames.Count());
            CollectionAssert.DoesNotContain(nodeNames.ToList(), new VirtualPath("Node1"));
            CollectionAssert.Contains(nodeNames.ToList(), new VirtualPath("Node2"));
        }

        [TestMethod]
        public void Nodes_EmptyDirectory_ReturnsEmpty()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            var nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(0, nodes.Count());
        }

        [TestMethod]
        public void Nodes_DirectoryWithNodes_ReturnsNodes()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddDirectory(new VirtualPath("Node1"));
            directory.AddDirectory(new VirtualPath("Node2"));

            var nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(2, nodes.Count());
            CollectionAssert.Contains(nodes.ToList(), directory[new VirtualPath("Node1")]);
            CollectionAssert.Contains(nodes.ToList(), directory[new VirtualPath("Node2")]);
        }

        [TestMethod]
        public void Nodes_DirectoryWithNodesAfterRemovingOne_ReturnsRemainingNodes()
        {
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            directory.AddDirectory(new VirtualPath("Node1"));
            directory.AddDirectory(new VirtualPath("Node2"));
            directory.Remove(new VirtualPath("Node1"));

            var nodes = directory.Nodes;

            Assert.IsNotNull(nodes);
            Assert.AreEqual(1, nodes.Count());
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                // "Node1"が正しく削除されていることを確認
                var node = directory[new VirtualPath("Node1")];
            });
            CollectionAssert.Contains(nodes.ToList(), directory[new VirtualPath("Node2")]);
        }

        [TestMethod]
        public void VirtualDirectory_ToString_ReturnsCorrectFormat()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));

            // Act
            var result = directory.ToString();
            Debug.WriteLine(result);

            // Assert
            Assert.IsTrue(result.Contains("TestDirectory"));
        }

        [TestMethod]
        public void ItemExists_WithExistingItem_ReturnsTrue()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var itemPath = new VirtualPath("TestItem");
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });
            directory.AddItem(itemPath, itemData, false);

            // Act
            var exists = directory.ItemExists(itemPath);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void ItemExists_WithNonExistingItem_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var nonExistingItemPath = new VirtualPath("NonExistingItem");

            // Act
            var exists = directory.ItemExists(nonExistingItemPath);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_WithExistingDirectory_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var subDirectoryPath = new VirtualPath("SubDirectory");
            directory.AddDirectory(subDirectoryPath);

            // Act
            var exists = directory.ItemExists(subDirectoryPath);

            // Assert
            Assert.IsFalse(exists); // ディレクトリはアイテムではないため、falseを返すべき
        }

        [TestMethod]
        public void ItemExists_WithExistingSymbolicLink_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var linkPath = new VirtualPath("TestLink");
            directory.AddSymbolicLink(linkPath, new VirtualPath("/path/to/target"), false);

            // Act
            var exists = directory.ItemExists(linkPath);

            // Assert
            Assert.IsFalse(exists); // シンボリックリンクはアイテムとして扱わず、falseを返す
        }

        [TestMethod]
        public void DirectoryExists_WithExistingDirectory_ReturnsTrue()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var subDirectoryPath = new VirtualPath("SubDirectory");
            directory.AddDirectory(subDirectoryPath);

            // Act
            var exists = directory.DirectoryExists(subDirectoryPath);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DirectoryExists_WithNonExistingDirectory_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var nonExistingDirectoryPath = new VirtualPath("NonExistingDirectory");

            // Act
            var exists = directory.DirectoryExists(nonExistingDirectoryPath);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void DirectoryExists_WithExistingItem_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var itemPath = new VirtualPath("TestItem");
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });
            directory.AddItem(itemPath, itemData, false);

            // Act
            var exists = directory.DirectoryExists(itemPath);

            // Assert
            Assert.IsFalse(exists); // アイテムはディレクトリではないため、falseを返すべき
        }

        [TestMethod]
        public void DirectoryExists_WithExistingSymbolicLink_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var linkPath = new VirtualPath("TestLink");
            directory.AddSymbolicLink(linkPath, new VirtualPath("/path/to/target"), false);

            // Act
            var exists = directory.DirectoryExists(linkPath);

            // Assert
            Assert.IsFalse(exists); // シンボリックリンクはディレクトリとして扱わず、falseを返す
        }

        [TestMethod]
        public void SymbolicLinkExists_WithExistingSymbolicLink_ReturnsTrue()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var linkPath = new VirtualPath("TestLink");
            directory.AddSymbolicLink(linkPath, new VirtualPath("/path/to/target"), false);

            // Act
            var exists = directory.SymbolicLinkExists(linkPath);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WithNonExistingSymbolicLink_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var nonExistingLinkPath = new VirtualPath("NonExistingLink");

            // Act
            var exists = directory.SymbolicLinkExists(nonExistingLinkPath);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WithExistingDirectory_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var subDirectoryPath = new VirtualPath("SubDirectory");
            directory.AddDirectory(subDirectoryPath);

            // Act
            var exists = directory.SymbolicLinkExists(subDirectoryPath);

            // Assert
            Assert.IsFalse(exists); // ディレクトリはシンボリックリンクではないため、falseを返すべき
        }

        [TestMethod]
        public void SymbolicLinkExists_WithExistingItem_ReturnsFalse()
        {
            // Arrange
            var directory = new VirtualDirectory(new VirtualPath("TestDirectory"));
            var itemPath = new VirtualPath("TestItem");
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });
            directory.AddItem(itemPath, itemData, false);

            // Act
            var exists = directory.SymbolicLinkExists(itemPath);

            // Assert
            Assert.IsFalse(exists); // アイテムはシンボリックリンクではないため、falseを返すべき
        }
    }

    [TestClass]
    public class VirtualNodeDictionaryTests
    {
        [TestMethod]
        public void GetNodeList_DefaultOption()
        {
            // Arrange
            var storage = new VirtualStorage();
            // 仮想ディレクトリとアイテムを追加
            storage.AddDirectory(new VirtualPath("/dir2"));
            storage.AddItem(new VirtualPath("/item2"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddItem(new VirtualPath("/item1"), new BinaryData([1, 2, 3]));

            // テスト対象のディレクトリを取得
            var directory = storage.GetDirectory(new VirtualPath("/"));

            // Act
            var nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (var node in nodes)
            {
                Debug.WriteLine(node.Name.NodeName);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualPath("dir1"), nodes[0].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir2"), nodes[1].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item1"), nodes[2].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item2"), nodes[3].Name.NodeName);
        }

        [TestMethod]
        public void GetNodeList_NameAscDir()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir2"));
            storage.AddItem(new VirtualPath("/item2"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddItem(new VirtualPath("/item1"), new BinaryData([1, 2, 3]));

            var options = new VirtualNodeDisplayOptions
            {
                SortBy = VirtualSortProperty.Name,
                Order = VirtualSortOrder.Ascending,
                GroupBy = VirtualGroupOrder.DirectoryFirst
            };

            // テスト対象のディレクトリを取得
            var directory = storage.GetDirectory(new VirtualPath("/"));
            directory.Options = options;

            // Act
            var nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (var node in nodes)
            {
                Debug.WriteLine(node.Name.NodeName);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualPath("dir1"), nodes[0].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir2"), nodes[1].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item1"), nodes[2].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item2"), nodes[3].Name.NodeName);
        }

        [TestMethod]
        public void GetNodeList_CreatedDateAscDir()
        {
            // Arrange
            var storage = new VirtualStorage();
            var item1 = new VirtualItem<BinaryData>(new VirtualPath("item1"), new BinaryData([1, 2, 3]));
            var item2 = new VirtualItem<BinaryData>(new VirtualPath("item2"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddDirectory(new VirtualPath("/dir2"));
            storage.AddItem(new VirtualPath("/"), item1);
            storage.AddItem(new VirtualPath("/"), item2);

            var options = new VirtualNodeDisplayOptions
            {
                SortBy = VirtualSortProperty.CreatedDate,
                Order = VirtualSortOrder.Ascending,
                GroupBy = VirtualGroupOrder.DirectoryFirst
            };

            // テスト対象のディレクトリを取得
            var directory = storage.GetDirectory(new VirtualPath("/"));
            directory.Options = options;

            // Act
            var nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (var node in nodes)
            {
                Debug.WriteLine(node);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualPath("dir1"), nodes[0].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir2"), nodes[1].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item1"), nodes[2].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item2"), nodes[3].Name.NodeName);
        }

        [TestMethod]
        public void GetNodeList_CreatedDateDesDir()
        {
            // Arrange
            var storage = new VirtualStorage();
            var item1 = new VirtualItem<BinaryData>(new VirtualPath("item1"), new BinaryData([1, 2, 3]));
            var item2 = new VirtualItem<BinaryData>(new VirtualPath("item2"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddDirectory(new VirtualPath("/dir2"));
            storage.AddItem(new VirtualPath("/"), item1);
            storage.AddItem(new VirtualPath("/"), item2);

            var options = new VirtualNodeDisplayOptions
            {
                SortBy = VirtualSortProperty.CreatedDate,
                Order = VirtualSortOrder.Descending,
                GroupBy = VirtualGroupOrder.DirectoryFirst
            };

            // テスト対象のディレクトリを取得
            var directory = storage.GetDirectory(new VirtualPath("/"));
            directory.Options = options;

            // Act
            var nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (var node in nodes)
            {
                Debug.WriteLine(node);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualPath("dir2"), nodes[0].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir1"), nodes[1].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item2"), nodes[2].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item1"), nodes[3].Name.NodeName);
        }

        [TestMethod]
        public void GetNodeList_NameDesDir()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir2"));
            storage.AddItem(new VirtualPath("/item2"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddItem(new VirtualPath("/item1"), new BinaryData([1, 2, 3]));

            var options = new VirtualNodeDisplayOptions
            {
                SortBy = VirtualSortProperty.Name,
                Order = VirtualSortOrder.Descending,
                GroupBy = VirtualGroupOrder.DirectoryFirst
            };

            // テスト対象のディレクトリを取得
            var directory = storage.GetDirectory(new VirtualPath("/"));
            directory.Options = options;

            // Act
            var nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (var node in nodes)
            {
                Debug.WriteLine(node.Name.NodeName);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualPath("dir2"), nodes[0].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir1"), nodes[1].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item2"), nodes[2].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item1"), nodes[3].Name.NodeName);
        }

        [TestMethod]
        public void GetNodeList_NameAscItem()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir2"));
            storage.AddItem(new VirtualPath("/item2"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddItem(new VirtualPath("/item1"), new BinaryData([1, 2, 3]));

            var options = new VirtualNodeDisplayOptions
            {
                SortBy = VirtualSortProperty.Name,
                Order = VirtualSortOrder.Ascending,
                GroupBy = VirtualGroupOrder.ItemFirst
            };

            // テスト対象のディレクトリを取得
            var directory = storage.GetDirectory(new VirtualPath("/"));
            directory.Options = options;

            // Act
            var nodes = directory.GetNodeList().ToList();

            // Assert
            foreach (var node in nodes)
            {
                Debug.WriteLine(node.Name.NodeName);
            }
            Assert.AreEqual(4, nodes.Count);
            Assert.AreEqual(new VirtualPath("item1"), nodes[0].Name.NodeName);
            Assert.AreEqual(new VirtualPath("item2"), nodes[1].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir1"), nodes[2].Name.NodeName);
            Assert.AreEqual(new VirtualPath("dir2"), nodes[3].Name.NodeName);
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
            VirtualPath existingDirectory = new VirtualPath("/path/to/existing/directory");
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
            VirtualPath nonExistentDirectory = new VirtualPath("/path/to/nonexistent/directory");

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.ChangeDirectory(nonExistentDirectory));
        }

        [TestMethod]
        public void ChangeDirectory_WithSymbolicLink_ChangesCurrentPathToTargetDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            VirtualPath baseDirectoryPath = new VirtualPath("/path/to/");
            VirtualPath targetDirectory = new VirtualPath("/real/target/directory");
            VirtualPath symbolicLink = new VirtualPath("/path/to/symlink");
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
            var virtualStorage = new VirtualStorage();
            VirtualPath baseDirectory = new VirtualPath("/path/to");
            VirtualPath subDirectory = new VirtualPath("/path/to/subdirectory");
            virtualStorage.AddDirectory(baseDirectory, true); // ベースディレクトリを追加
            virtualStorage.AddDirectory(subDirectory, true); // サブディレクトリを追加

            // サブディレクトリに移動し、そこから親ディレクトリに戻るパスを設定
            VirtualPath pathToChange = new VirtualPath("/path/to/subdirectory/../");

            // Act
            virtualStorage.ChangeDirectory(pathToChange);

            // Assert
            Assert.AreEqual(baseDirectory, virtualStorage.CurrentPath); // カレントディレクトリがベースディレクトリに正しく変更されたか検証
        }

        [TestMethod]
        public void ChangeDirectory_WithPathIncludingSymbolicLinkAndDotDot_NormalizesAndResolvesPathCorrectly()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            VirtualPath baseDirectory = new VirtualPath("/path/to");
            VirtualPath symbolicLinkPath = new VirtualPath("/path/to/link");
            VirtualPath targetDirectory = new VirtualPath("/real/target/directory");
            // ベースディレクトリとターゲットディレクトリを追加
            virtualStorage.AddDirectory(baseDirectory, true);
            virtualStorage.AddDirectory(targetDirectory, true);
            // サブディレクトリとしてシンボリックリンクを追加
            virtualStorage.AddSymbolicLink(symbolicLinkPath, targetDirectory);

            // シンボリックリンクを経由し、さらに".."を使って親ディレクトリに戻るパスを設定
            VirtualPath pathToChange = new VirtualPath("/path/to/link/../..");

            // Act
            virtualStorage.ChangeDirectory(pathToChange);

            // Assert
            // シンボリックリンクを解決後、".."によりさらに上のディレクトリに移動するため、
            // 最終的なカレントディレクトリが/pathになることを期待
            VirtualPath expectedPath = new VirtualPath("/path");
            Assert.AreEqual(expectedPath, virtualStorage.CurrentPath);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act and Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.ConvertToAbsolutePath(VirtualPath.Empty));
        }
        
        [TestMethod]
        public void ConvertToAbsolutePath_WhenCurrentPathIsRootAndPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/TestDirectory"));
            virtualStorage.ChangeDirectory(VirtualPath.Root);

            var result = virtualStorage.ConvertToAbsolutePath(new VirtualPath("TestDirectory"));

            Assert.AreEqual(new VirtualPath("/TestDirectory"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathStartsWithSlash_ReturnsSamePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/root/subdirectory"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/root/subdirectory"));

            var result = virtualStorage.ConvertToAbsolutePath(new VirtualPath("/test/path"));

            Assert.AreEqual(new VirtualPath("/test/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/root/subdirectory"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/root/subdirectory"));

            var result = virtualStorage.ConvertToAbsolutePath(new VirtualPath("test/path"));

            Assert.AreEqual(new VirtualPath("/root/subdirectory/test/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDot_ReturnsAbsolutePathWithoutDot()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/root/subdirectory"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/root/subdirectory"));

            var result = virtualStorage.ConvertToAbsolutePath(new VirtualPath("./test/path"));

            Assert.AreEqual(new VirtualPath("/root/subdirectory/./test/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDoubleDot_ReturnsParentDirectoryPath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/root/subdirectory"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/root/subdirectory"));

            var result = virtualStorage.ConvertToAbsolutePath(new VirtualPath("../test/path"));

            Assert.AreEqual(new VirtualPath("/root/subdirectory/../test/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsMultipleDoubleDots_ReturnsCorrectPath()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/root/subdirectory"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/root/subdirectory"));

            var result = virtualStorage.ConvertToAbsolutePath(new VirtualPath("../../test/path"));

            Assert.AreEqual(new VirtualPath("/root/subdirectory/../../test/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithBasePath_ConvertsRelativePathCorrectly()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/base/path"), true); // 必要なディレクトリを作成
            VirtualPath? basePath = new VirtualPath("/base/path");
            VirtualPath relativePath = new VirtualPath("relative/to/base");

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.AreEqual(new VirtualPath("/base/path/relative/to/base"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithoutBasePath_UsesCurrentPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/current/path"), true); // 必要なディレクトリを作成
            virtualStorage.ChangeDirectory(new VirtualPath("/current/path"));
            VirtualPath relativePath = new VirtualPath("relative/path");

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(relativePath, null);

            // Assert
            Assert.AreEqual(new VirtualPath("/current/path/relative/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithEmptyBasePath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            VirtualPath relativePath = new VirtualPath("some/relative/path");

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => virtualStorage.ConvertToAbsolutePath(relativePath, VirtualPath.Empty));
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithNullBasePath_UsesCurrentPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/current/path"), true); // 必要なディレクトリを作成
            virtualStorage.ChangeDirectory(new VirtualPath("/current/path"));
            VirtualPath relativePath = new VirtualPath("relative/path");
            VirtualPath? basePath = null;

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.AreEqual(new VirtualPath("/current/path/relative/path"), result);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithAbsolutePath_ReturnsOriginalPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/base/path"), true); // 必要なディレクトリを作成
            VirtualPath absolutePath = new VirtualPath("/absolute/path");

            // Act
            VirtualPath result = virtualStorage.ConvertToAbsolutePath(absolutePath, new VirtualPath("/base/path"));

            // Assert
            Assert.AreEqual(absolutePath, result);
        }

        [TestMethod]
        // ディレクトリの追加が正常に行われることを確認
        public void AddDirectory_WithValidPath_CreatesDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.AddDirectory(new VirtualPath("/test"));
            virtualStorage.AddDirectory(new VirtualPath("/test/directory"));

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/directory")));
        }

        [TestMethod]
        // ネストされたディレクトリを作成する場合、createSubdirectories が false の場合、例外がスローされることを確認
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                virtualStorage.AddDirectory(new VirtualPath("/test/directory"), false));
        }

        [TestMethod]
        // ネストされたディレクトリを作成する場合、createSubdirectories が true の場合、ディレクトリが作成されることを確認
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesTrue_CreatesDirectories()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.AddDirectory(new VirtualPath("/test/directory/subdirectory"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/directory/subdirectory")));
        }

        [TestMethod]
        // 既存のディレクトリに対して createSubdirectories が true の場合でも同じパスにディレクトリを追加しようとすると例外がスローされることを確認
        public void AddDirectory_WithExistingDirectoryAndCreateSubdirectoriesTrue_DoesNotThrowException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test/directory"), true);

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                virtualStorage.AddDirectory(new VirtualPath("/test/directory"), true);
            });
            Debug.WriteLine(exception.Message);
        }

        [TestMethod]
        // 既存のディレクトリに対して createSubdirectories が false の場合、例外がスローされることを確認
        public void AddDirectory_WithExistingDirectoryAndCreateSubdirectoriesFalse_ThrowException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test/directory"), true);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => 
                virtualStorage.AddDirectory(new VirtualPath("/test/directory"), false));
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddRootDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddDirectory(new VirtualPath("/")));
        }

        [TestMethod]
        public void AddDirectory_ThroughSymbolicLink_CreatesDirectoryAtResolvedPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test"), true);
            virtualStorage.AddSymbolicLink(new VirtualPath("/link"), new VirtualPath("/test"), true);

            // Act
            virtualStorage.AddDirectory(new VirtualPath("/link/directory"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/directory")));
        }

        [TestMethod]
        public void AddDirectory_WithRelativePath_CreatesDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/test"));

            // Act
            virtualStorage.AddDirectory(new VirtualPath("directory"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/directory")));
        }

        [TestMethod]
        public void AddDirectory_ExistingSubdirectoriesWithCreateSubdirectoriesTrue_DoesNotAffectExistingSubdirectories()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test/subdirectory"), true);

            // Act
            virtualStorage.AddDirectory(new VirtualPath("/test/newDirectory"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/subdirectory")));
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/newDirectory")));
        }

        [TestMethod]
        public void AddDirectory_MultipleLevelsOfDirectoriesWithCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddDirectory(new VirtualPath("/test/subdirectory/anotherDirectory"), false));
        }

        [TestMethod]
        public void AddDirectory_WithCurrentDirectoryDot_CreatesDirectoryCorrectly()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test"), true); // 初期ディレクトリを作成

            // Act
            // 現在のディレクトリ（"/test"）に対して"."を使用して、さらにサブディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/test/./subdirectory"), true);

            // Assert
            // "/test/subdirectory" が正しく作成されたことを確認
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/subdirectory")));
        }

        [TestMethod]
        public void AddDirectory_WithPathIncludingDotDot_CreatesDirectoryCorrectly()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test"), true);
            virtualStorage.AddDirectory(new VirtualPath("/test/subdirectory"), true);

            // Act
            virtualStorage.AddDirectory(new VirtualPath("/test/subdirectory/../anotherDirectory"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/anotherDirectory")));
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test/subdirectory")));
        }

        [TestMethod]
        public void AddDirectory_WithPathNormalization_CreatesDirectoryAtNormalizedPath()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            virtualStorage.AddDirectory(new VirtualPath("/test/../test2"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/test2")));
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddDirectoryUnderNonDirectoryNode_ThrowsException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            // "/dir1"ディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/dir1"), true);
            // "/dir1"内にアイテム（非ディレクトリ）"item"を作成
            virtualStorage.AddItem(new VirtualPath("/dir1/item"), "Dummy content", true);

            // Act & Assert
            // "/dir1/item"がディレクトリではないノードの下に"dir2"ディレクトリを追加しようとすると例外がスローされることを確認
            var exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddDirectory(new VirtualPath("/dir1/item/dir2"), true));
            Debug.WriteLine(exception.Message);
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddDirectoryUnderNonDirectoryNode_ThrowsException2()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            // "/dir1"ディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/dir1"), true);
            virtualStorage.AddDirectory(new VirtualPath("/dir2"), true);
            // "/dir1"内にアイテム（非ディレクトリ）"item"を作成
            virtualStorage.AddSymbolicLink(new VirtualPath("/dir1/linkToDir2"), new VirtualPath("/dir2"));

            // Act
            // "/dir1/linkToDir2"がリンクの場合、
            virtualStorage.AddDirectory(new VirtualPath("/dir1/linkToDir2/dir3"), true);

            // Assert
            Assert.IsTrue(virtualStorage.NodeExists(new VirtualPath("/dir2/dir3")));
        }

        [TestMethod]
        public void AddDirectory_ThrowsException_WhenSymbolicLinkExistsWithSameName()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            // 基本ディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/base"));
            // "/base"ディレクトリにシンボリックリンク"/base/link"を作成
            virtualStorage.AddSymbolicLink(new VirtualPath("/base/link"), new VirtualPath("/target"));

            // Act & Assert
            // "/base/link"にディレクトリを作成しようとすると、例外が発生することを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddDirectory(new VirtualPath("/base/link"), true));
        }

        [TestMethod]
        public void AddDirectory_ThrowsException_WhenItemExistsWithSameName()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            // 基本ディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/base"), true);
            // "/base"ディレクトリにアイテム"/base/item"を作成（この例では、アイテムを作成するメソッドを仮定）
            virtualStorage.AddItem(new VirtualPath("/base/item"), "Some content", true);

            // Act & Assert
            // "/base/item"にディレクトリを作成しようとすると、例外が発生することを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddDirectory(new VirtualPath("/base/item"), true), "Expected VirtualNodeNotFoundException when trying to create a directory where an item exists with the same name.");
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsItem()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/TestDirectory"));
            var item = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/TestDirectory/ItemData"), item);

            // メソッドを実行
            var node = vs.GetNode(new VirtualPath("/TestDirectory/ItemData"));

            // 結果を検証
            Assert.IsNotNull(node);
            Assert.AreEqual(new VirtualPath("ItemData"), node.Name);
            Assert.IsInstanceOfType(node, typeof(VirtualItem<BinaryData>));
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsDirectory()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/TestDirectory/TestSubdirectory"), true);

            // メソッドを実行
            var node = vs.GetNode(new VirtualPath("/TestDirectory/TestSubdirectory"));

            // 結果を検証
            Assert.IsNotNull(node);
            Assert.AreEqual(new VirtualPath("TestSubdirectory"), node.Name);
            Assert.IsInstanceOfType(node, typeof(VirtualDirectory));
        }

        [TestMethod]
        public void GetNode_ThrowsVirtualNodeNotFoundException_WhenDirectoryDoesNotExist()
        {
            // テストデータの設定
            var vs = new VirtualStorage();

            // メソッドを実行し、例外を検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetNode(new VirtualPath("/NonExistentDirectory")));
        }

        [TestMethod]
        public void GetNode_FollowsSymbolicLink_WhenFollowLinksIsTrue()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir"));
            storage.AddItem(new VirtualPath("/dir/item"), "TestItem");
            storage.AddSymbolicLink(new VirtualPath("/link"), new VirtualPath("/dir/item"));

            // Act
            var node = storage.GetNode(new VirtualPath("/link"), true);

            // Assert
            Assert.IsInstanceOfType(node, typeof(VirtualItem<string>));
            var item = node as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("TestItem", item.ItemData);
        }

        [TestMethod]
        public void GetNode_ReturnsSymbolicLink_WhenFollowLinksIsFalse()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir"));
            storage.AddItem(new VirtualPath("/dir/item"), "TestItem");
            storage.AddSymbolicLink(new VirtualPath("/link"), new VirtualPath("/dir/item"));

            // Act
            var node = storage.GetNode(new VirtualPath("/link"), false);

            // Assert
            Assert.IsInstanceOfType(node, typeof(VirtualSymbolicLink));
            var link = node as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.AreEqual(new VirtualPath("/dir/item"), link.TargetPath);
        }

        [TestMethod]
        public void GetNode_ThrowsWhenSymbolicLinkIsBroken()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddSymbolicLink(new VirtualPath("/brokenLink"), new VirtualPath("/nonexistent/item"));

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.GetNode(new VirtualPath("/brokenLink"), true));
        }

        [TestMethod]
        public void GetNode_StopsAtNonDirectoryNode()
        {
            // Arrange: 仮想ストレージにディレクトリとアイテムをセットアップ
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir"));
            storage.AddItem(new VirtualPath("/dir/item"), "TestItem");
            // /dir/item はディレクトリではない

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                // Act: ディレクトリではないノードの後ろに更にパスが続く場合の挙動をテスト
                var resultNode = storage.GetNode(new VirtualPath("/dir/item/more"), false);
            });
        }

        [TestMethod]
        public void GetNode_FollowsMultipleSymbolicLinksToReachAnItem()
        {
            // Arrange: 仮想ストレージと複数のディレクトリ、シンボリックリンクをセットアップ
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddDirectory(new VirtualPath("/dir1/dir2"));
            storage.AddItem(new VirtualPath("/dir1/dir2/item"), "FinalItem");

            // 最初のシンボリックリンクを /dir1 に追加し、/dir1/dir2 を指す
            storage.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir1/dir2"));

            // 2番目のシンボリックリンクを /dir1/dir2 に追加し、/dir1/dir2/item を指す
            storage.AddSymbolicLink(new VirtualPath("/dir1/dir2/link2"), new VirtualPath("/dir1/dir2/item"));

            // Act: 複数のシンボリックリンクを透過的に辿る
            var resultNode = storage.GetNode(new VirtualPath("/dir1/link1/link2"), true);

            // Assert: 結果が VirtualItem<string> 型で、期待したアイテムを持っていることを確認
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            var item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("FinalItem", item.ItemData);
        }

        [TestMethod]
        public void GetNode_ResolvesRelativePathSymbolicLink()
        {
            // Arrange: 仮想ストレージとディレクトリ、アイテム、相対パスのシンボリックリンクをセットアップ
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/dir1"));
            storage.AddDirectory(new VirtualPath("/dir1/dir2"));
            storage.AddItem(new VirtualPath("/dir1/dir2/item"), "RelativeItem");

            // 相対パスでシンボリックリンクを追加。ここでは、/dir1から/dir1/dir2への相対パスリンクを作成
            storage.AddSymbolicLink(new VirtualPath("/dir1/relativeLink"), new VirtualPath("dir2/item"));

            // Act: 相対パスのシンボリックリンクを透過的に辿る
            var resultNode = storage.GetNode(new VirtualPath("/dir1/relativeLink"), true);

            // Assert: 結果が VirtualItem<string> 型で、期待したアイテムを持っていることを確認
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            var item = resultNode as VirtualItem<string>;
            Assert.IsNotNull(item);
            Assert.AreEqual("RelativeItem", item.ItemData);
        }

        [TestMethod]
        public void GetNode_ResolvesSymbolicLinkWithCurrentDirectoryReference_Correctly()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/Test"));
            storage.ChangeDirectory(new VirtualPath("/Test"));
            storage.AddSymbolicLink(new VirtualPath("LinkToCurrent"), VirtualPath.Dot);

            // Act
            var node = storage.GetNode(new VirtualPath("LinkToCurrent"), true);

            // Assert
            Assert.IsTrue(node is VirtualDirectory);
            var directory = node as VirtualDirectory;
            Assert.AreEqual(new VirtualPath("Test"), directory?.Name);
        }

        [TestMethod]
        public void GetNode_ResolvesSymbolicLinkWithParentDirectoryReference_Correctly()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/Parent/Child"), true);
            storage.ChangeDirectory(new VirtualPath("/Parent/Child"));
            storage.AddSymbolicLink(new VirtualPath("LinkToParent"), VirtualPath.DotDot);

            // Act
            var node = storage.GetNode(new VirtualPath("LinkToParent"), true);

            // Assert
            Assert.IsTrue(node is VirtualDirectory);
            var directory = node as VirtualDirectory;
            Assert.AreEqual(new VirtualPath("Parent"), directory?.Name);
        }

        [TestMethod]
        public void GetNode_ComplexSymbolicLinkIncludingDotAndDotDot()
        {
            // テスト用の仮想ストレージとディレクトリ構造を準備
            var storage = new VirtualStorage();
            // 複数レベルのサブディレクトリを一度に作成
            storage.AddDirectory(new VirtualPath("/dir/subdir/dir1"), true);
            storage.AddDirectory(new VirtualPath("/dir/subdir/dir2"), true); // 隣接するディレクトリを追加
            storage.AddItem(new VirtualPath("/dir/subdir/dir2/item"), "complex item in dir2");

            // シンボリックリンクを作成
            // "/dir/subdir/link" が "./dir1/../dir2/./item" を指し、隣接するディレクトリ内のアイテムへの複合的なパスを使用
            storage.AddSymbolicLink(new VirtualPath("/dir/subdir/link"), new VirtualPath("./dir1/../dir2/./item"));

            // シンボリックリンクを通じてアイテムにアクセス
            var resultNode = storage.GetNode(new VirtualPath("/dir/subdir/link"), true);

            // 検証：リンクを通じて正しいアイテムにアクセスできること
            Assert.IsNotNull(resultNode);
            Assert.IsInstanceOfType(resultNode, typeof(VirtualItem<string>));
            var item = resultNode as VirtualItem<string>;
            Assert.AreEqual("complex item in dir2", item?.ItemData);
            Assert.AreEqual(resultNode.Name, new VirtualPath("item"));
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathIsSymbolicLink()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/Documents"));
            vs.AddSymbolicLink(new VirtualPath("/LinkToDocuments"), new VirtualPath("/Documents"));

            // メソッドを実行
            var resolvedPath = vs.ResolveLinkTarget(new VirtualPath("/LinkToDocuments"));

            // 結果を検証
            // TODO:これは期待通り。
            Assert.AreEqual(new VirtualPath("/Documents"), resolvedPath);
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathContainsTwoLinks()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/FinalDestination"));
            vs.AddSymbolicLink(new VirtualPath("/FirstLink"), new VirtualPath("/SecondLink"));
            vs.AddSymbolicLink(new VirtualPath("/SecondLink"), new VirtualPath("/FinalDestination"));

            // メソッドを実行
            var resolvedPath = vs.ResolveLinkTarget(new VirtualPath("/FirstLink"));

            // 結果を検証
            // TODO:これはできれば/FinalDestinationになってほしい。
            Assert.AreEqual(new VirtualPath("/FinalDestination"), resolvedPath);
        }

        [TestMethod]
        public void ResolveLinkTarget_ResolvesPathCorrectly_WhenUsingDotDotInPathWithSymbolicLink()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1/dir2/dir3"), true);
            vs.AddItem(new VirtualPath("/dir1/dir2/dir3/item"), "ItemInDir1");
            vs.AddSymbolicLink(new VirtualPath("/dir1/dir2/dir3/linkToDir2"), new VirtualPath("/dir1/dir2"));
            vs.ChangeDirectory(new VirtualPath("/dir1/dir2/dir3"));

            // メソッドを実行
            // TODO:これはできれば/dir1/dir2/dir3/itemになってほしい。
            var resolvedPath = vs.ResolveLinkTarget(new VirtualPath("linkToDir2/../item"));

            // 結果を検証
            Assert.AreEqual(new VirtualPath("/dir1/dir2/dir3/item"), resolvedPath);
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathSpansMultipleLinkLevels()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"));
            vs.AddDirectory(new VirtualPath("/dir2"));
            vs.AddDirectory(new VirtualPath("/dir3"));
            vs.AddItem(new VirtualPath("/dir3/item"), "FinalItem");
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));
            vs.AddSymbolicLink(new VirtualPath("/dir2/link2"), new VirtualPath("/dir3"));

            // メソッドを実行
            var resolvedPath = vs.ResolveLinkTarget(new VirtualPath("/dir1/link1/link2/item"));

            // 結果を検証
            // TODO:これは期待通り。
            Assert.AreEqual(new VirtualPath("/dir3/item"), resolvedPath);
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathIncludesSymbolicLink()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1/dir2"), true);
            vs.AddItem(new VirtualPath("/dir1/dir2/item"), "FinalItem");
            vs.AddSymbolicLink(new VirtualPath("/dir1/linkToDir2"), new VirtualPath("/dir1/dir2"));
            vs.ChangeDirectory(new VirtualPath("/dir1"));

            // メソッドを実行
            var resolvedPath = vs.ResolveLinkTarget(new VirtualPath("linkToDir2/item"));

            // 結果を検証
            // TODO:これは期待通り。
            Assert.AreEqual(new VirtualPath("/dir1/dir2/item"), resolvedPath);
        }

        [TestMethod]
        public void ResolveLinkTarget_ResolvesPathCorrectly_WhenUsingDotWithSymbolicLink()
        {
            // テストデータの設定
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1/subdir"), true);
            vs.AddItem(new VirtualPath("/dir1/subdir/item"), "SubdirItem");
            vs.AddSymbolicLink(new VirtualPath("/dir1/link"), new VirtualPath("/dir1/subdir"));
            vs.ChangeDirectory(new VirtualPath("/dir1"));

            // メソッドを実行
            var resolvedPath = vs.ResolveLinkTarget(new VirtualPath("./link/item"));

            // 結果を検証
            // TODO:これは期待通り。
            Assert.AreEqual(new VirtualPath("/dir1/subdir/item"), resolvedPath);
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryExists_ReturnsDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test"));
            VirtualPath path = new VirtualPath("/test");

            // Act
            var directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual(new VirtualPath("test"), directory.Name);
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            VirtualPath path = new VirtualPath("/nonexistent");

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }

        [TestMethod]
        public void GetDirectory_WhenNodeIsNotDirectory_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            var file = new BinaryData([1, 2, 3]);
            virtualStorage.AddItem(new VirtualPath("/testfile"), file);

            VirtualPath path = new VirtualPath("/testfile");

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.GetDirectory(path));
        }
        
        [TestMethod]
        public void GetDirectory_WhenPathIsRelative_ReturnsDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/test"));
            VirtualPath path = new VirtualPath("test");

            // Act
            var directory = virtualStorage.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual(new VirtualPath("test"), directory.Name);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            VirtualPath path = new VirtualPath("/NewItem");

            virtualStorage.AddItem(path, item);

            Assert.IsTrue(virtualStorage.ItemExists(path));
            var retrievedItem = virtualStorage.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var originalItem = new BinaryData(new byte[] { 1, 2, 3 });
            var newItem = new BinaryData(new byte[] { 4, 5, 6 });
            VirtualPath path = new VirtualPath("/ExistingItem");
            virtualStorage.AddItem(path, originalItem);

            virtualStorage.AddItem(path, newItem, true);

            var retrievedItem = virtualStorage.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItem.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsArgumentException_WhenPathIsEmpty_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });

            Assert.ThrowsException<ArgumentException>(() => virtualStorage.AddItem(VirtualPath.Empty, item));
        }

        [TestMethod]
        public void AddItem_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            VirtualPath path = new VirtualPath("/NonExistentDirectory/ItemData");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => virtualStorage.AddItem(path, item));
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteIsFalseAndItemExists_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            var originalItem = new BinaryData(new byte[] { 1, 2, 3 });
            VirtualPath path = new VirtualPath("/ExistingItem");
            virtualStorage.AddItem(path, originalItem);

            Assert.ThrowsException<InvalidOperationException>(() => virtualStorage.AddItem(path, new BinaryData(new byte[] { 4, 5, 6 }), false));
        }

        [TestMethod]
        public void AddItem_AddsNewItemToCurrentDirectory_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            virtualStorage.ChangeDirectory(VirtualPath.Root); // カレントディレクトリをルートに設定
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            VirtualPath itemName = new VirtualPath("NewItemInRoot");

            virtualStorage.AddItem(itemName, item); // パスを指定せずにアイテム名のみを渡す

            Assert.IsTrue(virtualStorage.ItemExists(itemName.AddStartSlash())); // カレントディレクトリにアイテムが作成されていることを確認
            var retrievedItem = virtualStorage.GetNode(itemName.AddStartSlash()) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemUsingRelativePath_WithBinaryData_Corrected()
        {
            var virtualStorage = new VirtualStorage();
            // 事前にディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/existingDirectory"), true); // 既存のディレクトリ
            virtualStorage.AddDirectory(new VirtualPath("/existingDirectory/subDirectory"), true); // 新しく作成するサブディレクトリ
            virtualStorage.ChangeDirectory(new VirtualPath("/existingDirectory")); // カレントディレクトリを変更

            var item = new BinaryData(new byte[] { 4, 5, 6 });
            VirtualPath relativePath = new VirtualPath("subDirectory/NewItem"); // 相対パスで新しいアイテムを指定

            // 相対パスを使用してアイテムを追加
            virtualStorage.AddItem(relativePath, item, true);

            VirtualPath fullPath = new VirtualPath("/existingDirectory/") + relativePath;
            Assert.IsTrue(virtualStorage.ItemExists(fullPath)); // 相対パスで指定された位置にアイテムが作成されていることを確認
            var retrievedItem = virtualStorage.GetNode(fullPath) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemToSubdirectoryAsCurrentDirectory_WithBinaryData()
        {
            var virtualStorage = new VirtualStorage();
            // サブディレクトリを作成し、カレントディレクトリに設定
            virtualStorage.AddDirectory(new VirtualPath("/subdirectory"), true);
            virtualStorage.ChangeDirectory(new VirtualPath("/subdirectory"));
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            VirtualPath itemName = new VirtualPath("NewItemInSubdirectory");

            // カレントディレクトリにアイテムを追加（パスを指定せずにアイテム名のみを渡す）
            virtualStorage.AddItem(itemName, item);

            // サブディレクトリ内にアイテムが作成されていることを確認
            Assert.IsTrue(virtualStorage.ItemExists(new VirtualPath("/subdirectory/") + itemName));
            var retrievedItem = virtualStorage.GetNode(new VirtualPath("/subdirectory/") + itemName) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteTargetIsNotAnItem()
        {
            var virtualStorage = new VirtualStorage();
            // テスト用のディレクトリを作成
            virtualStorage.AddDirectory(new VirtualPath("/testDirectory"), true);
            // 同名のサブディレクトリを追加（アイテムの上書き対象として）
            virtualStorage.AddDirectory(new VirtualPath("/testDirectory/itemName"), true);

            var item = new BinaryData(new byte[] { 1, 2, 3 });
            VirtualPath itemName = new VirtualPath("itemName");

            // アイテム上書きを試みる（ただし、実際にはディレクトリが存在するため、アイテムではない）
            Assert.ThrowsException<InvalidOperationException>(() =>
                virtualStorage.AddItem(new VirtualPath("/testDirectory/") + itemName, item, true),
                "上書き対象がアイテムではなくディレクトリの場合、InvalidOperationExceptionが投げられるべきです。");
        }

        [TestMethod]
        public void AddItem_ThroughSymbolicLink_AddsItemToTargetDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/actualDirectory"), true);
            virtualStorage.AddSymbolicLink(new VirtualPath("/symbolicLink"), new VirtualPath("/actualDirectory"));
            var binaryData = new BinaryData(new byte[] { 1, 2, 3 });

            // Act
            virtualStorage.AddItem(new VirtualPath("/symbolicLink/newItem"), binaryData);

            // Assert
            Assert.IsTrue(virtualStorage.ItemExists(new VirtualPath("/actualDirectory/newItem")));
            var retrievedItem = virtualStorage.GetNode(new VirtualPath("/actualDirectory/newItem")) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(binaryData.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ThroughNestedSymbolicLink_AddsItemToFinalTargetDirectory()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/level1/level2/targetDirectory"), true);
            virtualStorage.AddSymbolicLink(new VirtualPath("/linkToLevel1"), new VirtualPath("/level1"));
            virtualStorage.AddSymbolicLink(new VirtualPath("/level1/linkToLevel2"), new VirtualPath("/level1/level2"));
            var binaryData = new BinaryData(new byte[] { 4, 5, 6 });

            // Act
            virtualStorage.AddItem(new VirtualPath("/linkToLevel1/linkToLevel2/targetDirectory/newItem"), binaryData);

            // Assert
            Assert.IsTrue(virtualStorage.ItemExists(new VirtualPath("/level1/level2/targetDirectory/newItem")));
            var retrievedItem = virtualStorage.GetNode(new VirtualPath("/level1/level2/targetDirectory/newItem")) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(binaryData.Data, retrievedItem.ItemData.Data);
        }

        [TestMethod]
        public void AddItem_ToNonExistentTargetViaSymbolicLink_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddSymbolicLink(new VirtualPath("/linkToNowhere"), new VirtualPath("/nonExistentTarget"));
            var binaryData = new BinaryData(new byte[] { 7, 8, 9 });

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddItem(new VirtualPath("/linkToNowhere/newItem"), binaryData));
        }

        [TestMethod]
        public void AddItem_ThroughSymbolicLinkWithNonExistentIntermediateTarget_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/existingDirectory"), true);
            // 中間のターゲットが存在しないシンボリックリンクを作成
            virtualStorage.AddSymbolicLink(new VirtualPath("/existingDirectory/nonExistentLink"), new VirtualPath("/nonExistentIntermediateTarget"));
            // 最終的なパスの組み立て
            VirtualPath pathToItem = new VirtualPath("/existingDirectory/nonExistentLink/finalItem");
            var binaryData = new BinaryData(new byte[] { 10, 11, 12 });

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddItem(pathToItem, binaryData), "中間のシンボリックリンクのターゲットが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

        [TestMethod]
        public void AddItem_WithVirtualItem_ShouldAddItemToDirectory()
        {
            var storage = new VirtualStorage();
            var path = new VirtualPath("/dir");
            storage.AddDirectory(path);
            var item = new VirtualItem<string>(new VirtualPath("item"), "test");

            storage.AddItem(path, item);

            Assert.IsTrue(storage.ItemExists(path + item.Name));
        }

        [TestMethod]
        public void AddItem_WithVirtualItem_WithOverwrite_ShouldThrowExceptionIfItemExists()
        {
            var storage = new VirtualStorage();
            var directoryPath = new VirtualPath("/dir");
            storage.AddDirectory(directoryPath);
            var existingItemPath = new VirtualPath("existingItem");
            var existingItem = new VirtualItem<string>(existingItemPath, "existing value");
            var newItem = new VirtualItem<string>(existingItemPath, "new value");

            storage.AddItem(directoryPath, existingItem, false);

            Assert.ThrowsException<InvalidOperationException>(() => storage.AddItem(directoryPath, newItem, false));
        }

        [TestMethod]
        public void AddItem_WithOverwrite_ShouldOverwriteExistingItem()
        {
            var storage = new VirtualStorage();
            var directoryPath = new VirtualPath("/dir");
            storage.AddDirectory(directoryPath);
            var itemPath = new VirtualPath("item");
            var originalItem = new VirtualItem<string>(itemPath, "original value");
            var newItem = new VirtualItem<string>(itemPath, "new value");

            storage.AddItem(directoryPath, originalItem, false);
            storage.AddItem(directoryPath, newItem, true);

            Assert.IsTrue(storage.ItemExists(directoryPath + newItem.Name));
        }

        [TestMethod]
        public void AddItem_ToNonExistingDirectory_ShouldThrowVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();
            var directoryPath = new VirtualPath("/nonExistingDir");
            var itemPath = new VirtualPath("item");
            var item = new VirtualItem<string>(itemPath, "value");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.AddItem(directoryPath, item, false));
        }

        [TestMethod]
        public void AddItem_ToLocationPointedBySymbolicLink_ShouldThrowException()
        {
            var storage = new VirtualStorage();
            var directoryPath = new VirtualPath("/dir");
            storage.AddDirectory(directoryPath);
            var linkPath = new VirtualPath("/link");
            var targetPath = new VirtualPath("/nonExistingTargetDir");
            storage.AddSymbolicLink(linkPath, targetPath);

            var itemPath = new VirtualPath("item");
            var item = new VirtualItem<string>(itemPath, "value");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.AddItem(linkPath, item, false));
        }

        [TestMethod]
        public void AddItem_ToLocationPointedBySymbolicLink_ShouldAddItemSuccessfully()
        {
            var storage = new VirtualStorage();
            var actualDirectoryPath = new VirtualPath("/actualDir");
            storage.AddDirectory(actualDirectoryPath);
            var symbolicLinkPath = new VirtualPath("/symbolicLink");
            storage.AddSymbolicLink(symbolicLinkPath, actualDirectoryPath);
            var itemName = new VirtualPath("newItem");
            var item = new VirtualItem<string>(itemName, "itemValue");
            storage.AddItem(symbolicLinkPath, item);
            Assert.IsTrue(storage.ItemExists(actualDirectoryPath + itemName));
        }

        [TestMethod]
        public void ItemExists_WhenIntermediateDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.NodeExists(new VirtualPath("/nonexistent/testfile"));

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemExists_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData([1, 2, 3]);
            virtualStorage.AddItem(new VirtualPath("/testfile"), item);

            // Act
            bool result = virtualStorage.NodeExists(new VirtualPath("/testfile"));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.NodeExists(new VirtualPath("/nonexistent"));

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryExists_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/testdir"));

            // Act
            bool result = virtualStorage.NodeExists(new VirtualPath("/testdir"));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act
            bool result = virtualStorage.NodeExists(new VirtualPath("/nonexistent"));

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToItemAndFollowLinksIsTrue_ReturnsTrue()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            virtualStorage.AddItem(new VirtualPath("/actualitem"), item);
            virtualStorage.AddSymbolicLink(new VirtualPath("/linktoitem"), new VirtualPath("/actualitem"));

            // Act
            bool result = virtualStorage.ItemExists(new VirtualPath("/linktoitem"), true);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToItemAndFollowLinksIsFalse_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            var item = new BinaryData(new byte[] { 1, 2, 3 });
            virtualStorage.AddItem(new VirtualPath("/actualitem"), item);
            virtualStorage.AddSymbolicLink(new VirtualPath("/linktoitem"), new VirtualPath("/actualitem"));

            // Act
            bool result = virtualStorage.ItemExists(new VirtualPath("/linktoitem"), false);

            // Assert
            Assert.IsFalse(result); // シンボリックリンク自体はアイテムとしてカウントしない
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToNonexistentItemAndFollowLinksIsTrue_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddSymbolicLink(new VirtualPath("/linktononexistent"), new VirtualPath("/nonexistentitem"));

            // Act
            bool result = virtualStorage.ItemExists(new VirtualPath("/linktononexistent"), true);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToPointingToDirectoryAndFollowLinksIsTrue_ReturnsFalse()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            virtualStorage.AddDirectory(new VirtualPath("/targetdir"));
            virtualStorage.AddSymbolicLink(new VirtualPath("/linktodir"), new VirtualPath("/targetdir"));

            // Act
            bool result = virtualStorage.ItemExists(new VirtualPath("/linktodir"), true);

            // Assert
            Assert.IsFalse(result); // ディレクトリを指すシンボリックリンクはアイテムとしてカウントしない
        }

        [TestMethod]
        public void GetNodes_WithEmptyPath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => 
                virtualStorage.GetNodes(VirtualPath.Empty, VirtualNodeType.All, true).ToList());
        }

        [TestMethod]
        public void GetNodes_WithNonAbsolutePath_ThrowsArgumentException()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => 
                virtualStorage.GetNodes(new VirtualPath("relative/path"), VirtualNodeType.All, true).ToList());
        }

        private static void SetTestData(VirtualStorage vs)
        {
            vs.AddDirectory(new VirtualPath("/Directory1"), true);
            vs.AddDirectory(new VirtualPath("/Directory1/Directory1_1"), true);
            vs.AddDirectory(new VirtualPath("/Directory1/Directory1_2"), true);
            vs.AddDirectory(new VirtualPath("/Directory2"), true);
            vs.AddDirectory(new VirtualPath("/Directory2/Directory2_1"), true);
            vs.AddDirectory(new VirtualPath("/Directory2/Directory2_2"), true);

            var item_1 = new BinaryData([1, 2, 3]);
            var item_2 = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Item_1"), item_1);
            vs.AddItem(new VirtualPath("/Item_2"), item_2);
            vs.AddSymbolicLink(new VirtualPath("/LinkToItem1a"), new VirtualPath("/Directory1/Item1a"));
            vs.AddSymbolicLink(new VirtualPath("/LinkToItem2a"), new VirtualPath("/Directory1/Item1b"));

            var item1a = new BinaryData([1, 2, 3]);
            var item1b = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Directory1/Item1a"), item1a);
            vs.AddItem(new VirtualPath("/Directory1/Item1b"), item1b);

            var item1_1a = new BinaryData([1, 2, 3]);
            var item1_1b = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Directory1/Directory1_1/Item1_1a"), item1_1a);
            vs.AddItem(new VirtualPath("/Directory1/Directory1_1/Item1_1b"), item1_1b);

            var item1_2a = new BinaryData([1, 2, 3]);
            var item1_2b = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Directory1/Directory1_2/Item1_2a"), item1_2a);
            vs.AddItem(new VirtualPath("/Directory1/Directory1_2/Item1_2b"), item1_2b);

            var item2a = new BinaryData([1, 2, 3]);
            var item2b = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Directory2/Item2a"), item2a);
            vs.AddItem(new VirtualPath("/Directory2/Item2b"), item2b);

            var item2_1a = new BinaryData([1, 2, 3]);
            var item2_1b = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Directory2/Directory2_1/Item2_1a"), item2_1a);
            vs.AddItem(new VirtualPath("/Directory2/Directory2_1/Item2_1b"), item2_1b);

            var item2_2a = new BinaryData([1, 2, 3]);
            var item2_2b = new BinaryData([1, 2, 3]);
            vs.AddItem(new VirtualPath("/Directory2/Directory2_2/Item2_2a"), item2_2a);
            vs.AddItem(new VirtualPath("/Directory2/Directory2_2/Item2_2b"), item2_2b);
        }

        [TestMethod]
        public void GetNodes_ValidTest()
        {
            var vs = new VirtualStorage();

            SetTestData(vs);

            Assert.AreEqual(22, vs.GetNodes(VirtualPath.Root, VirtualNodeType.All, true).Count());
            Debug.WriteLine("\nAll nodes:");
            foreach (var node in vs.GetNodes(VirtualPath.Root, VirtualNodeType.All, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(6, vs.GetNodes(VirtualPath.Root, VirtualNodeType.Directory, true).Count());
            Debug.WriteLine("\nDirectories:");
            foreach (var node in vs.GetNodes(VirtualPath.Root, VirtualNodeType.Directory, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(14, vs.GetNodes(VirtualPath.Root, VirtualNodeType.Item, true).Count());
            Debug.WriteLine("\nItems:");
            foreach (var node in vs.GetNodes(VirtualPath.Root, VirtualNodeType.Item, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            Assert.AreEqual(2, vs.GetNodes(VirtualPath.Root, VirtualNodeType.SymbolicLink, true).Count());
            Debug.WriteLine("\nSymbolicLink:");
            foreach (var node in vs.GetNodes(VirtualPath.Root, VirtualNodeType.SymbolicLink, true))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            vs.ChangeDirectory(new VirtualPath("/Directory1"));
            Assert.AreEqual(2, vs.GetNodes(VirtualNodeType.Directory, false).Count());
            Debug.WriteLine("\nDirectories in /Directory1:");
            foreach (var node in vs.GetNodes(VirtualNodeType.Directory, false))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

            vs.ChangeDirectory(new VirtualPath("/Directory1"));
            Assert.AreEqual(2, vs.GetNodes(VirtualNodeType.Item, false).Count());
            Debug.WriteLine("\nItems in /Directory1:");
            foreach (var node in vs.GetNodes(VirtualNodeType.Item, false))
            {
                Assert.IsNotNull(node);
                Debug.WriteLine(node.Name);
            }

        }

        //[TestMethod]
        //public void GetNodes_ValidTestWithSymbolicLink()
        //{
        //    var vs = new VirtualStorage();

        //    SetTestData(vs);

        //    Assert.AreEqual(4, vs.GetNodes(VirtualPath.Root, VirtualNodeType.ItemData, false, true).Count());
        //    Debug.WriteLine("\nItems:");
        //    foreach (var node in vs.GetNodes(VirtualPath.Root, VirtualNodeType.ItemData, false, true))
        //    {
        //        Assert.IsNotNull(node);
        //        Debug.WriteLine(node.Name);
        //    }

        //}

        [TestMethod]
        public void GetNodesWithPaths_ValidTest()
        {
            var vs = new VirtualStorage();

            SetTestData(vs);

            Assert.AreEqual(22, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.All, true).Count());
            Debug.WriteLine("\nAll nodes:");
            foreach (var name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.All, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(6, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.Directory, true).Count());
            Debug.WriteLine("\nDirectories:");
            foreach (var name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.Directory, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(14, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.Item, true).Count());
            Debug.WriteLine("\nItems:");
            foreach (var name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.Item, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            Assert.AreEqual(2, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.SymbolicLink, true).Count());
            Debug.WriteLine("\nSymbolicLink:");
            foreach (var name in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.SymbolicLink, true))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            vs.ChangeDirectory(new VirtualPath("/Directory1"));
            Assert.AreEqual(2, vs.GetNodesWithPaths(VirtualNodeType.Directory, false).Count());
            Debug.WriteLine("\nDirectories in /Directory1:");
            foreach (var name in vs.GetNodesWithPaths(VirtualNodeType.Directory, false))
            {
                Assert.IsNotNull(name);
                Debug.WriteLine(name);
            }

            vs.ChangeDirectory(new VirtualPath("/Directory1"));
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
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData([4, 5, 6]));

            storage.CopyNode(new VirtualPath("/sourceFile"), new VirtualPath("/destinationFile"), false, true);

            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destinationFile"));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData.Data);
        }

        [TestMethod]
        public void CopyFileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData(new byte[] { 1, 2, 3 }));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData(new byte[] { 4, 5, 6 }));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.CopyNode(new VirtualPath("/sourceFile"), new VirtualPath("/destinationFile"), false, false));
        }

        [TestMethod]
        public void CopyFileToDirectory_CopiesFileToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/destination"));
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));

            storage.CopyNode(new VirtualPath("/sourceFile"), new VirtualPath("/destination/"), false, false);

            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destination/sourceFile"));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData.Data);
        }

        [TestMethod]
        public void CopyEmptyDirectoryToDirectory_CreatesTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddDirectory(new VirtualPath("/destinationDir"));

            storage.CopyNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir/newDir"), false, false);

            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destinationDir/newDir")));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/sourceDir/file"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/destinationDir"));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.CopyNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir/newDir"), false, false));
        }

        [TestMethod]
        public void CopyNonEmptyDirectoryToDirectoryWithRecursive_CopiesAllContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/sourceDir/file"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/destinationDir"));

            storage.CopyNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir/newDir"), true, false);

            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destinationDir/newDir")));
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destinationDir/newDir/file")));
        }

        [TestMethod]
        public void CopyDirectoryToFile_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.CopyNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationFile"), false, false));
        }

        [TestMethod]
        public void CopyFileToNonExistentDirectory_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.CopyNode(new VirtualPath("/sourceFile"), new VirtualPath("/nonExistentDir/destinationFile"), false, false));
        }

        [TestMethod]
        public void CopyDirectoryToNonExistentDirectoryWithRecursive_CreatesAllDirectoriesAndCopiesContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/sourceDir/file"), new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                storage.CopyNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir/newDir"), true, false);
            });
        }

        [TestMethod]
        public void CopyDeepNestedDirectoryToNewLocation_CopiesAllNestedContentsAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/source/deep/nested/dir"), true);
            var originalItem = new BinaryData([1, 2, 3]);
            storage.AddItem(new VirtualPath("/source/deep/nested/dir/nestedFile"), originalItem);

            storage.AddDirectory(new VirtualPath("/destination"), true);

            storage.CopyNode(new VirtualPath("/source/deep"), new VirtualPath("/destination/deepCopy"), true, false);

            var copiedItem = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destination/deepCopy/nested/dir/nestedFile"));

            Assert.IsNotNull(originalItem);
            Assert.IsNotNull(copiedItem);
            Assert.AreNotSame(originalItem, copiedItem.ItemData);
        }

        [TestMethod]
        public void CopyMultipleNestedDirectories_CopiesAllDirectoriesAndContentsAndEnsuresDifferentInstances()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/source/dir1/subdir1"), true);
            storage.AddDirectory(new VirtualPath("/source/dir2/subdir2"), true);
            var originalFile1 = new BinaryData([4, 5, 6]);
            var originalFile2 = new BinaryData([7, 8, 9]);
            storage.AddItem(new VirtualPath("/source/dir1/subdir1/file1"), originalFile1);
            storage.AddItem(new VirtualPath("/source/dir2/subdir2/file2"), originalFile2);

            storage.CopyNode(new VirtualPath("/source"), new VirtualPath("/destination"), true, false);

            var copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destination/dir1/subdir1/file1"));
            var copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destination/dir2/subdir2/file2"));

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
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/complex/dir1"), true);
            storage.AddDirectory(new VirtualPath("/complex/dir2"), true);
            storage.AddDirectory(new VirtualPath("/complex/dir1/subdir1"), true);
            var originalFile1 = new BinaryData([1, 2, 3]);
            var originalFile2 = new BinaryData([4, 5, 6]);
            storage.AddItem(new VirtualPath("/complex/dir1/file1"), originalFile1);
            storage.AddItem(new VirtualPath("/complex/dir2/file2"), originalFile2);

            storage.CopyNode(new VirtualPath("/complex"), new VirtualPath("/copiedComplex"), true, false);

            var copiedFile1 = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/copiedComplex/dir1/file1"));
            var copiedFile2 = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/copiedComplex/dir2/file2"));

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
            var storage = new VirtualStorage();

            // ArgumentException がスローされることを検証
            Assert.ThrowsException<ArgumentException>(() => storage.CopyNode(VirtualPath.Empty, VirtualPath.Empty, false, false));
        }

        [TestMethod]
        public void CopyNode_DestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/parentDir/childDir"), true);

            // コピー先がコピー元のサブディレクトリである場合
            VirtualPath sourcePath = new VirtualPath("/parentDir");
            VirtualPath destinationPath = new VirtualPath("/parentDir/childDir");

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }

        [TestMethod]
        public void CopyNode_SourceIsSubdirectoryOfDestination_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/parentDir/childDir"), true);

            // コピー元がコピー先のサブディレクトリである場合
            VirtualPath sourcePath = new VirtualPath("/parentDir/childDir");
            VirtualPath destinationPath = new VirtualPath("/parentDir");

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(sourcePath, destinationPath));
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CopyNode_RootDirectoryCopy_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            VirtualPath sourcePath = VirtualPath.Root;
            VirtualPath destinationPath = new VirtualPath("/SomeOtherDirectory");

            storage.CopyNode(sourcePath, destinationPath);
        }

        [TestMethod]
        public void CopyNode_NonExistentSource_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();

            // 存在しないソースパスを指定
            VirtualPath nonExistentSource = new VirtualPath("/nonExistentSource");
            VirtualPath destinationPath = new VirtualPath("/destination");

            storage.AddDirectory(destinationPath);

            // VirtualNodeNotFoundException がスローされることを検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.CopyNode(nonExistentSource, destinationPath));
        }

        [TestMethod]
        public void CopyNode_SameSourceAndDestination_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualPath("/file"), new BinaryData(new byte[] { 1, 2, 3 }));

            // 同じソースとデスティネーションを指定
            VirtualPath path = new VirtualPath("/file");

            // InvalidOperationException がスローされることを検証
            Assert.ThrowsException<InvalidOperationException>(() => storage.CopyNode(path, path));
        }

        [TestMethod]
        public void CopyNode_ValidTest()
        {
            var vs = new VirtualStorage();

            SetTestData(vs);

            Assert.AreEqual(22, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.All, true).Count());

            Debug.WriteLine("コピー前:");
            foreach (var nodeName in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.All, true))
            {
                Assert.IsNotNull(nodeName);
                Debug.WriteLine(nodeName);
            }

            vs.AddDirectory(new VirtualPath("/Destination"), true);
            vs.CopyNode(new VirtualPath("/Directory1"), new VirtualPath("/Destination"), true, false);

            Assert.AreEqual(32, vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.All, true).Count());

            Debug.WriteLine("コピー後:");
            foreach (var nodeName in vs.GetNodesWithPaths(VirtualPath.Root, VirtualNodeType.All, true))
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
            storage.AddItem(new VirtualPath("/TestItem"), item);

            storage.RemoveNode(new VirtualPath("/TestItem"));

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/TestItem")));
        }

        [TestMethod]
        public void RemoveNode_NonExistingItem_ThrowsException()
        {
            var storage = new VirtualStorage();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => storage.RemoveNode(new VirtualPath("/NonExistingItem")));
        }

        [TestMethod]
        public void RemoveNode_ExistingEmptyDirectory_RemovesDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/TestDirectory"));

            storage.RemoveNode(new VirtualPath("/TestDirectory"));

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/TestDirectory")));
        }

        [TestMethod]
        public void RemoveNode_NonExistingDirectory_ThrowsException()
        {
            var storage = new VirtualStorage();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.RemoveNode(new VirtualPath("/NonExistingDirectory")));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/TestDirectory"));
            var item = new BinaryData([1, 2, 3]);
            storage.AddItem(new VirtualPath("/TestDirectory/TestItem"), item);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.RemoveNode(new VirtualPath("/TestDirectory")));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithRecursive_RemovesDirectoryAndContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/TestDirectory"));
            var item = new BinaryData([1, 2, 3]);
            storage.AddItem(new VirtualPath("/TestDirectory/TestItem"), item);

            storage.RemoveNode(new VirtualPath("/TestDirectory"), true);

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/TestDirectory")));
            Assert.IsFalse(storage.NodeExists(new VirtualPath("/TestDirectory/TestItem")));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithRecursive_RemovesAllNestedContents()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/Level1/Level2/Level3"), true);
            var item1 = new BinaryData([1, 2, 3]);
            var item2 = new BinaryData([4, 5, 6]);
            storage.AddItem(new VirtualPath("/Level1/Level2/Level3/Item1"), item1);
            storage.AddItem(new VirtualPath("/Level1/Level2/Item2"), item2);

            storage.RemoveNode(new VirtualPath("/Level1"), true);

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/Level1")));
            Assert.IsFalse(storage.NodeExists(new VirtualPath("/Level1/Level2/Level3/Item1")));
            Assert.IsFalse(storage.NodeExists(new VirtualPath("/Level1/Level2/Item2")));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithoutRecursive_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/Level1/Level2/Level3"), true);
            var item1 = new BinaryData([1, 2, 3]);
            storage.AddItem(new VirtualPath("/Level1/Level2/Level3/Item1"), item1);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.RemoveNode(new VirtualPath("/Level1")));
        }

        [TestMethod]
        public void RemoveNode_NestedDirectoryWithEmptySubdirectories_RecursiveRemoval()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/Level1/Level2/Level3"), true);

            storage.RemoveNode(new VirtualPath("/Level1"), true);

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/Level1")));
            Assert.IsFalse(storage.NodeExists(new VirtualPath("/Level1/Level2")));
            Assert.IsFalse(storage.NodeExists(new VirtualPath("/Level1/Level2/Level3")));
        }

        [TestMethod]
        public void RemoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(VirtualPath.Root);
            });
        }

        [TestMethod]
        public void RemoveNode_CurrentDirectoryDot_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(VirtualPath.Dot);
            });
        }

        [TestMethod]
        public void RemoveNode_ParentDirectoryDotDot_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                storage.RemoveNode(VirtualPath.DotDot);
            });
        }

        [TestMethod]
        public void TryGetNode_ReturnsNode_WhenNodeExists()
        {
            // Arrange
            var storage = new VirtualStorage();
            VirtualPath path = new VirtualPath("/existing/path");
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
            VirtualPath path = new VirtualPath("/non/existing/path");

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
            VirtualPath path = new VirtualPath("/existing/path");
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
            VirtualPath path = new VirtualPath("/non/existing/path");

            // Act
            bool exists = storage.NodeExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenSymbolicLinkExistsAndFollowLinksIsTrue()
        {
            // Arrange
            var storage = new VirtualStorage();
            VirtualPath directoryPath = new VirtualPath("/existing/directory");
            VirtualPath linkDirectoryPath = new VirtualPath("/link/to");
            VirtualPath linkPath = linkDirectoryPath + new VirtualPath("directory");
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
            var storage = new VirtualStorage();
            VirtualPath directoryPath = new VirtualPath("/existing/directory");
            VirtualPath linkDirectoryPath = new VirtualPath("/link/to");
            VirtualPath linkPath = linkDirectoryPath + new VirtualPath("directory");
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
            var storage = new VirtualStorage();
            VirtualPath nonExistentTargetPath = new VirtualPath("/nonexistent/target");
            VirtualPath linkDirectoryPath = new VirtualPath("/link/to");
            VirtualPath linkPath = linkDirectoryPath + new VirtualPath("nonexistent");
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
            var storage = new VirtualStorage();
            VirtualPath path = new VirtualPath("/existing/path");
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
            VirtualPath path = new VirtualPath("/non/existing/path");

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
            VirtualPath path = new VirtualPath("/existing/path/item");
            storage.AddDirectory(new VirtualPath("/existing/path"), true);
            storage.AddItem(new VirtualPath("/existing/path/item"), new BinaryData([1, 2, 3]));

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
            VirtualPath path = new VirtualPath("/non/existing/path/item");

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
            VirtualPath path = new VirtualPath("/existing/path");
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
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData([4, 5, 6]));

            storage.MoveNode(new VirtualPath("/sourceFile"), new VirtualPath("/destinationFile"), true);

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/sourceFile")));
            var destinationItem = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destinationFile"));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData.Data);
        }

        [TestMethod]
        public void MoveNode_FileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/sourceFile"), new VirtualPath("/destinationFile"), false));
        }

        [TestMethod]
        public void MoveNode_FileToDirectory_MovesFileToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/destination"));
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));

            storage.MoveNode(new VirtualPath("/sourceFile"), new VirtualPath("/destination/"), false);

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/sourceFile")));
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destination/sourceFile")));
        }

        [TestMethod]
        public void MoveNode_DirectoryToDirectory_MovesDirectoryToTargetDirectory()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddDirectory(new VirtualPath("/destinationDir/newDir"), true);

            storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir/newDir"), false);

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/sourceDir")));
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destinationDir/newDir")));
        }

        [TestMethod]
        public void MoveNode_WhenSourceDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/destinationDir"));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode(new VirtualPath("/nonExistentSource"), new VirtualPath("/destinationDir"), false));
        }

        [TestMethod]
        public void MoveNode_WhenDestinationIsInvalid_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/nonExistentDestination/newDir"), false));
        }

        [TestMethod]
        public void MoveNode_DirectoryWithSameNameExistsAtDestination_ThrowsExceptionRegardlessOfOverwriteFlag()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"), true);
            storage.AddDirectory(new VirtualPath("/destinationDir/sourceDir"), true);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir"), false));
            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir"), true));
        }

        [TestMethod]
        public void MoveNode_DirectoryToFile_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationFile"), false));
        }

        [TestMethod]
        public void MoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/destinationDir"));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(VirtualPath.Root, new VirtualPath("/destinationDir"), false));
        }

        [TestMethod]
        public void MoveNode_OverwritesExistingNodeInDestinationWhenAllowed()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/sourceDir/fileName"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/destinationDir"));
            storage.AddItem(new VirtualPath("/destinationDir/fileName"), new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            storage.MoveNode(new VirtualPath("/sourceDir/fileName"), new VirtualPath("/destinationDir"), true); // 上書き許可で移動

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/sourceDir/fileName"))); // 元のファイルが存在しないことを確認
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destinationDir/fileName"))); // 移動先にファイルが存在することを確認

            var movedItem = (VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/destinationDir/fileName"));
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, movedItem.ItemData.Data); // 移動先のファイルの中身が正しいことを確認
        }

        [TestMethod]
        public void MoveNode_ThrowsWhenDestinationNodeExistsAndOverwriteIsFalse()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/sourceDir/fileName"), new BinaryData([1, 2, 3]));
            storage.AddDirectory(new VirtualPath("/destinationDir"));
            storage.AddItem(new VirtualPath("/destinationDir/fileName"), new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/sourceDir/fileName"), new VirtualPath("/destinationDir"), false)); // 上書き禁止で例外を期待
        }

        [TestMethod]
        public void MoveNode_EmptyDirectory_MovesSuccessfully()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/emptyDir"));
            storage.AddDirectory(new VirtualPath("/newDir/emptyDir"), true);
            storage.MoveNode(new VirtualPath("/emptyDir"), new VirtualPath("/newDir/emptyDir"));

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/emptyDir")));
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/newDir/emptyDir/emptyDir")));
        }

        [TestMethod]
        public void MoveNode_MultiLevelDirectory_MovesSuccessfully()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir/subDir1/subDir2"), true);
            storage.AddDirectory(new VirtualPath("/destinationDir"));
            storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationDir/sourceDir"));

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/sourceDir")));
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/destinationDir/sourceDir/subDir1/subDir2")));
        }

        [TestMethod]
        public void MoveNode_WithInvalidPath_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/validDir"));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode(new VirtualPath("/invalid?Path"), new VirtualPath("/validDir")));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MoveNode_DirectoryToFile_ThrowsException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/sourceDir"));
            storage.AddItem(new VirtualPath("/destinationFile"), new BinaryData([1, 2, 3]));
            storage.MoveNode(new VirtualPath("/sourceDir"), new VirtualPath("/destinationFile"));
        }

        [TestMethod]
        public void MoveNode_WithinSameDirectory_RenamesNode()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualPath("/sourceFile"), new BinaryData([1, 2, 3]));
            storage.MoveNode(new VirtualPath("/sourceFile"), new VirtualPath("/renamedFile"));

            Assert.IsFalse(storage.NodeExists(new VirtualPath("/sourceFile")));
            Assert.IsTrue(storage.NodeExists(new VirtualPath("/renamedFile")));

            var result = ((VirtualItem<BinaryData>)storage.GetNode(new VirtualPath("/renamedFile"))).ItemData.Data;
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
        }

        // 循環参照チェックテスト
        [TestMethod]
        public void MoveNode_WhenDestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/parentDir/subDir"), true);

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/parentDir"), new VirtualPath("/parentDir/subDir")));
        }

        // 移動先と移動元が同じかどうかのチェックテスト
        [TestMethod]
        public void MoveNode_WhenSourceAndDestinationAreSame_ThrowsInvalidOperationException()
        {
            var storage = new VirtualStorage();
            storage.AddItem(new VirtualPath("/file"), new BinaryData([1, 2, 3]));

            Assert.ThrowsException<InvalidOperationException>(() => 
                storage.MoveNode(new VirtualPath("/file"), new VirtualPath("/file")));
        }

        // 移動先の親ディレクトリが存在しない場合のテスト
        [TestMethod]
        public void MoveNode_WhenDestinationParentDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/existingDir"));
            storage.AddItem(new VirtualPath("/existingDir/file"), new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => 
                storage.MoveNode(new VirtualPath("/existingDir/file"), new VirtualPath("/nonExistentDir/file")));
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkExists_ReturnsTrue()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/test"));
            storage.AddSymbolicLink(new VirtualPath("/test/link"), new VirtualPath("/target/path"));

            // Act
            bool exists = storage.SymbolicLinkExists(new VirtualPath("/test/link"));

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/test"));

            // Act
            bool exists = storage.SymbolicLinkExists(new VirtualPath("/test/nonexistentLink"));

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenParentDirectoryIsALinkAndLinkExists_ReturnsTrue()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/realParent"));
            storage.AddSymbolicLink(new VirtualPath("/linkedParent"), new VirtualPath("/realParent"));
            storage.AddDirectory(new VirtualPath("/realParent/testDir"));
            storage.AddSymbolicLink(new VirtualPath("/linkedParent/myLink"), new VirtualPath("/realParent/testDir"));

            // Act
            bool exists = storage.SymbolicLinkExists(new VirtualPath("/linkedParent/myLink"));

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void AddSymbolicLink_WhenLinkIsNew_AddsSuccessfully()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/test"));

            // Act
            storage.AddSymbolicLink(new VirtualPath("/test/newLink"), new VirtualPath("/target/path"));

            // Assert
            Assert.IsTrue(storage.SymbolicLinkExists(new VirtualPath("/test/newLink")));
            var link = storage.GetNode(new VirtualPath("/test/newLink")) as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.AreEqual(new VirtualPath("/target/path"), link.TargetPath);
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsFalseAndLinkExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/test"));
            storage.AddSymbolicLink(new VirtualPath("/test/existingLink"), new VirtualPath("/old/target/path"));

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.AddSymbolicLink(new VirtualPath("/test/existingLink"), new VirtualPath("/new/target/path"), overwrite: false));
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsTrueAndLinkExists_OverwritesLink()
        {
            // Arrange
            var storage = new VirtualStorage();
            storage.AddDirectory(new VirtualPath("/test"));
            storage.AddSymbolicLink(new VirtualPath("/test/existingLink"), new VirtualPath("/old/target/path"));

            // Act
            storage.AddSymbolicLink(new VirtualPath("/test/existingLink"), new VirtualPath("/new/target/path"), overwrite: true);

            // Assert
            var link = storage.GetNode(new VirtualPath("/test/existingLink")) as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.AreEqual(new VirtualPath("/new/target/path"), link.TargetPath);
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingItem_ThrowsInvalidOperationException()
        {
            // Arrange
            var storage = new VirtualStorage();
            var itemData = new BinaryData(new byte[] { 1, 2, 3 });

            storage.AddDirectory(new VirtualPath("/test"));
            storage.AddItem(new VirtualPath("/test/existingItem"), itemData); // 既存のアイテムを追加

            storage.AddDirectory(new VirtualPath("/new/target/path"), true); // シンボリックリンクのターゲットとなるディレクトリを追加

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.AddSymbolicLink(new VirtualPath("/test/existingItem"), new VirtualPath("/new/target/path"), true),
                "既存のアイテム上にシンボリックリンクを追加しようとすると、上書きがtrueでもInvalidOperationExceptionが発生するべきです。");
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            var storage = new VirtualStorage();

            storage.AddDirectory(new VirtualPath("/test/existingDirectory"), true); // 既存のディレクトリを追加

            storage.AddDirectory(new VirtualPath("/new/target/path"), true); // シンボリックリンクのターゲットとなるディレクトリを追加

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                storage.AddSymbolicLink(new VirtualPath("/test/existingDirectory"), new VirtualPath("/new/target/path"), true),
                "既存のディレクトリ上にシンボリックリンクを追加しようとすると、上書きがtrueでもInvalidOperationExceptionが発生するべきです。");
        }

        [TestMethod]
        public void AddSymbolicLink_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist()
        {
            // Arrange
            var virtualStorage = new VirtualStorage();
            VirtualPath nonExistentParentDirectory = new VirtualPath("/nonexistent/directory");
            VirtualPath symbolicLinkPath = nonExistentParentDirectory + new VirtualPath("link");
            VirtualPath targetPath = new VirtualPath("/existing/target");

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                virtualStorage.AddSymbolicLink(symbolicLinkPath, targetPath),
                "親ディレクトリが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

        [TestMethod]
        public void WalkPathToTarget_Root()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = new VirtualPath("/");
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
            VirtualPath path = new VirtualPath("/dir1");
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
            VirtualPath path = new VirtualPath("/dir1/dir2");
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
            VirtualPath path = new VirtualPath("/item");
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
            VirtualPath path = new VirtualPath("/dir1/item");
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
            VirtualPath targetPath = new VirtualPath("/dir1/link1/item");
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddItem(new VirtualPath("/dir2/item"), new BinaryData[1, 2, 3]);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink2()
        {
            VirtualPath targetPath = new VirtualPath("/dir1/link1/dir3");
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddDirectory(new VirtualPath("/dir2/dir3"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink3()
        {
            VirtualPath targetPath = new VirtualPath("/dir1/link1");
            VirtualPath linkTargetPath = new VirtualPath("/dir2");
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), linkTargetPath);

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
            VirtualPath targetPath = new VirtualPath("/nonexistent");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathWithExceptionEnabled()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath targetPath = new VirtualPath("/nonexistent");

            var exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, true);
            });
            Debug.WriteLine($"ExceptionMessage: {exception.Message}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath2()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = new VirtualPath("/dir1/item");
            vs.AddDirectory(path.DirectoryPath, true);
            vs.AddItem(path, new BinaryData[1, 2, 3]);
            VirtualPath targetPath = new VirtualPath("/dir1/item/dir2");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath3()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = new VirtualPath("/dir1/dir2");
            vs.AddDirectory(path.DirectoryPath, true);
            VirtualPath targetPath = new VirtualPath("/dir1/dir2/dir3");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath4()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));
            VirtualPath targetPath = new VirtualPath("/dir1/link1/dir3");

            NodeInformation? result = vs.WalkPathToTarget(targetPath, notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_CircularSymbolicLink()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir2/link2"), new VirtualPath("/dir1"));

            NodeInformation? result = vs.WalkPathToTarget(new VirtualPath("/dir1/link1/link2/link1"), notifyNode, null, true, false);
            VirtualNode? node = result?.Node;

            Assert.IsNotNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLinkToNonExistentPath()
        {
            VirtualStorage vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/nonexistent"));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                NodeInformation? result = vs.WalkPathToTarget(new VirtualPath("/dir1/link1"), notifyNode, null, true, true);
            });
        }

        [TestMethod]
        public void WalkPathToTarget_RelativePath()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath path = new VirtualPath("dir2");
            vs.AddDirectory(new VirtualPath("/dir1/dir2"), true);
            VirtualPath targetPath = path;

            vs.ChangeDirectory(new VirtualPath("/dir1"));

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
            vs.AddDirectory(new VirtualPath("/dir1/dir2"), true);
            vs.AddDirectory(new VirtualPath("/dir1/dir3"), true);
            VirtualPath targetPath = new VirtualPath("/dir1/dir2/../dir3");

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
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));
            vs.AddDirectory(new VirtualPath("/dir1/dir3"), true);
            VirtualPath targetPath = new VirtualPath("/dir1/link1/../dir3");

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
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"));
            vs.AddDirectory(new VirtualPath("/dir2"));
            vs.AddDirectory(new VirtualPath("/dir3"));
            vs.AddItem(new VirtualPath("/dir3/item"), "FinalItem");
            vs.AddSymbolicLink(new VirtualPath("/dir1/link1"), new VirtualPath("/dir2"));
            vs.AddSymbolicLink(new VirtualPath("/dir2/link2"), new VirtualPath("/dir3"));

            // メソッドを実行
            var result = vs.WalkPathToTarget(new VirtualPath("/dir1/link1/link2/item"), notifyNode, null, true, false);

            // 結果を検証
            Assert.AreEqual(new VirtualPath("/dir1/link1/link2/item"), result?.TraversalPath);
            Assert.AreEqual(new VirtualPath("/dir3/item"), result?.ResolvedPath);
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathAndCreatePath()
        {
            VirtualStorage vs = new VirtualStorage();
            VirtualPath targetPath = new VirtualPath("/dir1");

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
            VirtualPath targetPath = new VirtualPath("/dir1/dir2");

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

        private bool actionNode(VirtualDirectory directory, VirtualPath nodeName)
        {
            VirtualDirectory newDirectory = new VirtualDirectory(nodeName);

            directory.Add(newDirectory);

            return true;
        }

        [TestMethod]
        public void WalkPathTree_Test1()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddItem(new VirtualPath("/dir1/item1"), "test");
            vs.AddItem(new VirtualPath("/dir1/item2"), "test");
            vs.AddDirectory(new VirtualPath("/dir2"), true);
            vs.AddItem(new VirtualPath("/dir2/item3"), "test");
            vs.AddItem(new VirtualPath("/dir2/item4"), "test");
            vs.AddItem(new VirtualPath("/item5"), "test");
            vs.AddItem(new VirtualPath("/item6"), "test");

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            //Assert.AreEqual(4, result.Count());

            string tree = vs.GenerateTextBasedTreeStructure(new VirtualPath("/"), true);
            Debug.WriteLine(tree);

            Debug.WriteLine("------");
            Debug.WriteLine("/");
            Debug.WriteLine("├dir1");
            Debug.WriteLine("│├item1");
            Debug.WriteLine("│└item2");
            Debug.WriteLine("└dir2");
            Debug.WriteLine("  ├item3");
            Debug.WriteLine("  └item4");
        }

        [TestMethod]
        public void WalkPathTree_EmptyDirectory_ShouldReturnOnlyDirectory()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/emptyDir"), true);

            var result = vs.WalkPathTree(new VirtualPath("/emptyDir"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(1, result.Count()); // 空のディレクトリ自身のみが結果として返されるべき
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/emptyDir"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithItems_ShouldReturnItems()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dirWithItems"), true);
            vs.AddItem(new VirtualPath("/dirWithItems/item1"), "content1");
            vs.AddItem(new VirtualPath("/dirWithItems/item2"), "content2");

            var result = vs.WalkPathTree(new VirtualPath("/dirWithItems"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(3, result.Count()); // ディレクトリと2つのアイテムが含まれる
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/dirWithItems/item1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/dirWithItems/item2"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithSymbolicLink_ShouldFollowLinkIfRequested()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/sourceDir"), true);
            vs.AddItem(new VirtualPath("/sourceDir/item"), "content");
            vs.AddSymbolicLink(new VirtualPath("/linkToSourceDir"), new VirtualPath("/sourceDir"));

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/linkToSourceDir/item"));
        }

        [TestMethod]
        public void WalkPathTree_EmptyDirectory_ReturnsOnlyRoot()
        {
            var vs = new VirtualStorage();
            var result = vs.WalkPathTree(VirtualPath.Root, true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/"));
        }

        [TestMethod]
        public void WalkPathTree_SingleItemDirectory_IncludesItem()
        {
            var vs = new VirtualStorage();
            vs.AddItem(new VirtualPath("/item1"), "test");

            var result = vs.WalkPathTree(VirtualPath.Root, true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(2, result.Count()); // ルートディレクトリとアイテム
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/item1"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithSymbolicLink_IncludesLinkAndTarget()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddSymbolicLink(new VirtualPath("/linkToDir1"), new VirtualPath("/dir1"));

            var result = vs.WalkPathTree(VirtualPath.Root, true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(3, result.Count()); // ルートディレクトリ、dir1、およびシンボリックリンク
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/dir1"));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString() == "/linkToDir1"));
        }

        [TestMethod]
        public void WalkPathTree_DeepNestedDirectories_ReturnsAllNodes()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1/dir2/dir3"), true);
            vs.AddItem(new VirtualPath("/dir1/dir2/dir3/item1"), "test");

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(5, result.Count()); // ルートディレクトリ、dir1、dir2、dir3、およびitem1
        }

        [TestMethod]
        public void WalkPathTree_MultipleSymbolicLinks_IncludesLinksAndTargets()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddSymbolicLink(new VirtualPath("/linkToDir1"), new VirtualPath("/dir1"));
            vs.AddSymbolicLink(new VirtualPath("/linkToLink1"), new VirtualPath("/linkToDir1"));

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(4, result.Count()); // ルートディレクトリ、dir1、linkToDir1、およびlinkToLink1
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithManyItems_ReturnsAllItems()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            for (int i = 1; i <= 100; i++)
            {
                vs.AddItem(new VirtualPath($"/dir1/item{i}"), $"test{i}");
            }

            var result = vs.WalkPathTree(new VirtualPath("/dir1"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.AreEqual(101, result.Count()); // dir1 および 100個のアイテム
        }

        [TestMethod]
        public void WalkPathTree_WithNonexistentPathSymbolicLink_ThrowsExceptionAndOutputsMessage()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/dir1"), true);
            vs.AddSymbolicLink(new VirtualPath("/dir1/emptyLink"), new VirtualPath("/nonexistent"));

            var exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                var result = vs.WalkPathTree(new VirtualPath("/"), true);
                foreach (var item in result)
                {
                    Debug.WriteLine(item.ToString());
                }
            });

            Debug.WriteLine($"例外が捕捉されました: {exception.Message}");
        }

        [TestMethod]
        public void WalkPathTree_ShallowNestedDirectories_ExecutesWithoutErrorAndOutputsTree()
        {
            var vs = new VirtualStorage();
            var basePath = "/deep";
            var depth = 100; // 最大1900くらいまでOK
            for (int i = 1; i <= depth; i++)
            {
                basePath = $"{basePath}/dir{i}";
                vs.AddDirectory(new VirtualPath(basePath), true);
            }

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
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
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/empty1/empty2/empty3"), true);

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine($"ノード名: {item.TraversalPath}, 解決済みパス: {item.ResolvedPath}");
            }

            Assert.AreEqual(4, result.Count()); // ルートディレクトリ + 各空ディレクトリ
            Debug.WriteLine("複数レベルの空ディレクトリが正常に走査されました。");
        }

        [TestMethod]
        public void WalkPathTree_MultipleSymbolicLinksInSamePath()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/targetDir"), true); // ターゲットディレクトリ
            vs.AddDirectory(new VirtualPath("/dir"), true); // シンボリックリンクのための親ディレクトリ
            vs.AddSymbolicLink(new VirtualPath("/dir/symLink1"), new VirtualPath("/targetDir"));
            vs.AddSymbolicLink(new VirtualPath("/dir/symLink2"), new VirtualPath("/targetDir"));

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            // `/dir/symLink1` と `/dir/symLink2` の存在を確認
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("/dir/symLink1")));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("/dir/symLink2")));
        }

        [TestMethod]
        public void WalkPathTree_SymbolicLinkPointsToAnotherSymbolicLink()
        {
            var vs = new VirtualStorage();
            vs.AddDirectory(new VirtualPath("/targetDir"), true); // ターゲットディレクトリ
            vs.AddSymbolicLink(new VirtualPath("/symLink"), new VirtualPath("/targetDir"));
            vs.AddSymbolicLink(new VirtualPath("/linkToLink"), new VirtualPath("/symLink"));

            var result = vs.WalkPathTree(new VirtualPath("/"), true);
            foreach (var item in result)
            {
                Debug.WriteLine(item);
            }

            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("/symLink")));
            Assert.IsTrue(result.Any(r => r.TraversalPath.ToString().Equals("/linkToLink")));
        }
    }
}
