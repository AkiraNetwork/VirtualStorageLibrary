using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;
using System.Diagnostics;
using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualPathTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            VirtualStorageSettings.Initialize();
        }

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
            VirtualPath path = string.Empty;

            Assert.AreEqual(string.Empty, path.NormalizePath().ToString());
        }

        [TestMethod]
        public void NormalizePath_WithInvalidPath_ThrowsInvalidOperationException()
        {
            VirtualPath path = "/../";

            Exception err = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                path = path.NormalizePath();
            });

            Assert.AreEqual("Due to path normalization, it is above the root directory. [/../]", err.Message);

            Debug.WriteLine(err.Message);
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
            VirtualPath path = string.Empty;
            VirtualNodeName expectedNodeName = string.Empty;

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
            VirtualPath path1 = string.Empty;
            VirtualPath path2 = string.Empty;
            VirtualPath expected = string.Empty;

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath1Empty_ReturnsPath2()
        {
            VirtualPath path1 = string.Empty;
            VirtualPath path2 = "/directory/subdirectory";
            VirtualPath expected = path2;

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_WithPath2Empty_ReturnsPath1()
        {
            VirtualPath path1 = "/directory/subdirectory";
            VirtualPath path2 = string.Empty;
            VirtualPath expected = path1;

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_RootPathWithDot_ReturnsRootPath()
        {
            VirtualPath path1 = "/";
            VirtualPath path2 = ".";
            VirtualPath expected = "/.";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_RootPathWithSingleDirectory_ReturnsCorrectPath()
        {
            VirtualPath path1 = "/";
            VirtualPath path2 = "directory";
            VirtualPath expected = "/directory";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_DirectoryWithDot_ReturnsSameDirectory()
        {
            VirtualPath path1 = "/directory";
            VirtualPath path2 = ".";
            VirtualPath expected = "/directory/.";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_DirectoryWithDoubleDot_ReturnsParentDirectory()
        {
            VirtualPath path1 = "/directory/subdirectory";
            VirtualPath path2 = "..";
            VirtualPath expected = "/directory/subdirectory/..";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_MultipleDirectoriesWithRelativePath_ReturnsCorrectPath()
        {
            VirtualPath path1 = "/directory/subdirectory";
            VirtualPath path2 = "../anotherDirectory";
            VirtualPath expected = "/directory/subdirectory/../anotherDirectory";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Combine_PathsWithDuplicatedSeparators_ReturnsCorrectPath()
        {
            VirtualPath path1 = "/directory//subdirectory/";
            VirtualPath path2 = "//file.txt";
            VirtualPath expected = "/directory//subdirectory/file.txt";

            VirtualPath result = path1.Combine(path2);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_VirtualPathCombinesCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to";
            VirtualPath path2 = "directory/file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithRelativePath_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to";
            VirtualPath path2 = "./directory/file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithParentDirectory_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to/directory";
            VirtualPath path2 = "../file.txt";
            VirtualPath expected = "/path/to/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithMultipleParentDirectories_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to/directory/subdirectory";
            VirtualPath path2 = "../../file.txt";
            VirtualPath expected = "/path/to/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithTrailingSlash_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to/directory/";
            VirtualPath path2 = "file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithRootPath_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/";
            VirtualPath path2 = "path/to/directory";
            VirtualPath expected = "/path/to/directory";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithEmptyPaths_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "";
            VirtualPath path2 = "";
            VirtualPath expected = "";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithOneEmptyPath_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to/directory";
            VirtualPath path2 = "";
            VirtualPath expected = "/path/to/directory";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithOneEmptyPathReversed_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "";
            VirtualPath path2 = "/path/to/directory";
            VirtualPath expected = "/path/to/directory";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_WithMultipleSlashes_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path//to///directory/";
            VirtualPath path2 = "//file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_RootPathWithDot_ReturnsRoot()
        {
            // Arrange
            VirtualPath path1 = "/";
            VirtualPath path2 = ".";
            VirtualPath expected = "/";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_VirtualPathWithNodeNameCombinesCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to";
            VirtualNodeName nodeName = new("directory");
            VirtualPath expected = "/path/to/directory";

            // Act
            VirtualPath result = path1 + nodeName;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_VirtualPathWithNodeNameWithRelativePath_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to";
            VirtualNodeName nodeName = new("./directory");
            VirtualPath expected = "/path/to/directory";

            // Act
            VirtualPath result = path1 + nodeName;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_VirtualPathWithStringCombinesCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to";
            string path2 = "directory/file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_VirtualPathWithStringWithRelativePath_ReturnsCorrectly()
        {
            // Arrange
            VirtualPath path1 = "/path/to";
            string path2 = "./directory/file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_StringWithVirtualPathCombinesCorrectly()
        {
            // Arrange
            string path1 = "/path/to";
            VirtualPath path2 = "directory/file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_StringWithVirtualPathWithRelativePath_ReturnsCorrectly()
        {
            // Arrange
            string path1 = "/path/to";
            VirtualPath path2 = "./directory/file.txt";
            VirtualPath expected = "/path/to/directory/file.txt";

            // Act
            VirtualPath result = path1 + path2;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_VirtualPathWithCharAddsSeparatorAtEnd()
        {
            // Arrange
            VirtualPath path = "/path/to/directory";
            char separator = '/';
            VirtualPath expected = "/path/to/directory/";

            // Act
            VirtualPath result = path + separator;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void OperatorPlus_CharWithVirtualPathAddsSeparatorAtStart()
        {
            // Arrange
            char separator = '/';
            VirtualPath path = "path/to/directory";
            VirtualPath expected = "/path/to/directory";

            // Act
            VirtualPath result = separator + path;

            // Assert
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
            Assert.AreEqual(VirtualPath.Dot, result.ToString(), "相対パスが'.'であるべきです。");
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
            VirtualPath basePath = string.Empty;
            VirtualPath path = "/path/to/directory";

            Assert.ThrowsException<InvalidOperationException>(() => path.GetRelativePath(basePath));
        }

        [TestMethod]
        public void GetRelativePath_WithAbsolutePathAndRelativeBasePath_ThrowsInvalidOperationException()
        {
            VirtualPath basePath = "relative/basePath";
            VirtualPath path = "/absolute/path";

            Exception err = Assert.ThrowsException<InvalidOperationException>(() => path.GetRelativePath(basePath));

            Assert.AreEqual("The specified base path is not an absolute path. [relative/basePath]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void GetRelativePath_WithRelativePathAndRelativeBasePath_ThrowsInvalidOperationException()
        {
            VirtualPath basePath = "relative/basePath";
            VirtualPath path = "relative/path";

            Exception err = Assert.ThrowsException<InvalidOperationException>(() => path.GetRelativePath(basePath));

            Assert.AreEqual("This path is not an absolute path. [relative/path]", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void Depth_WithRootPath_ReturnsZero()
        {
            VirtualPath path = "/";

            Assert.AreEqual(0, path.Depth);
        }

        [TestMethod]
        public void Depth_WithSingleLevelPath_ReturnsOne()
        {
            VirtualPath path = "/folder";

            Assert.AreEqual(1, path.Depth);
        }

        [TestMethod]
        public void Depth_WithMultiLevelPath_ReturnsCorrectDepth()
        {
            VirtualPath path = "/folder/subfolder/file";

            Assert.AreEqual(3, path.Depth);
        }

        [TestMethod]
        public void Depth_WithRelativePath_ReturnsCorrectDepth()
        {
            VirtualPath path = "folder/subfolder/file";

            Assert.AreEqual(3, path.Depth);
        }

        [TestMethod]
        public void Depth_WithEmptyPath_ReturnsZero()
        {
            VirtualPath path = "";

            Assert.AreEqual(0, path.Depth);
        }

        [TestMethod]
        public void Depth_WithNormalizedPath_ReturnsCorrectDepth()
        {
            VirtualPath path = "/path/./to/../directory/";

            VirtualPath normalizedPath = path.NormalizePath();

            Assert.AreEqual(2, normalizedPath.Depth);
        }

        [TestMethod]
        public void CompareTo_NullObject_ReturnsOne()
        {
            // Arrange
            VirtualPath path = new("/dir1");

            // Act
            int result = path.CompareTo(null);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CompareTo_NonVirtualPathObject_ThrowsArgumentException()
        {
            // Arrange
            VirtualPath path = new("/dir1");
            object nonVirtualPathObject = "NonVirtualPath";

            // Act and Assert
            Exception err = Assert.ThrowsException<ArgumentException>(() => path.CompareTo(nonVirtualPathObject));

            Assert.AreEqual("The object specified by the parameter is not of type VirtualPath. (Parameter 'obj')", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void CompareTo_SamePath_ReturnsZero()
        {
            // Arrange
            VirtualPath path1 = new("/dir1");
            VirtualPath path2 = new("/dir1");

            // Act
            int result = path1.CompareTo(path2);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CompareTo_DifferentPath_ReturnsNonZero()
        {
            // Arrange
            VirtualPath path1 = new("/dir1");
            VirtualPath path2 = new("/dir2");

            // Act
            int result = path1.CompareTo(path2);

            // Assert
            Assert.AreNotEqual(0, result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_SourcePathIsSubdirectoryOfDestinationPath_ReturnsTrue()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = "/dir1/dir2";

            bool result = VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_DestinationPathIsSubdirectoryOfSourcePath_ReturnsTrue()
        {
            VirtualPath sourcePath = "/dir1/dir2";
            VirtualPath destinationPath = "/dir1";

            bool result = VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_PathsAreIdentical_ReturnsTrue()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = "/dir1";

            bool result = VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_PathsAreNotRelated_ReturnsFalse()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = "/dir3";

            bool result = VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_SourcePathIsRoot_ReturnsTrue()
        {
            VirtualPath sourcePath = "/";
            VirtualPath destinationPath = "/dir1/dir2";

            bool result = VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_DestinationPathIsRoot_ReturnsTrue()
        {
            VirtualPath sourcePath = "/dir1/dir2";
            VirtualPath destinationPath = "/";

            bool result = VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ArePathsSubdirectories_SourcePathIsEmpty_ThrowsArgumentException()
        {
            VirtualPath sourcePath = string.Empty;
            VirtualPath destinationPath = "/dir1";

            Exception err = Assert.ThrowsException<ArgumentException>(() =>
            {
                VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);
            });

            Assert.AreEqual("An empty string cannot be specified as a path. (Parameter 'path1')", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void ArePathsSubdirectories_DestinationPathIsEmpty_ThrowsArgumentException()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = string.Empty;

            Exception err = Assert.ThrowsException<ArgumentException>(() =>
            {
                VirtualPath.ArePathsSubdirectories(sourcePath, destinationPath);
            });

            Assert.AreEqual("An empty string cannot be specified as a path. (Parameter 'path2')", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void IsSubdirectory_SourcePathIsSubdirectoryOfDestinationPath_ReturnsTrue()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = "/dir1/dir2";

            bool result = destinationPath.IsSubdirectory(sourcePath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsSubdirectory_DestinationPathIsSubdirectoryOfSourcePath_ReturnsFalse()
        {
            VirtualPath sourcePath = "/dir1/dir2";
            VirtualPath destinationPath = "/dir1";

            bool result = destinationPath.IsSubdirectory(sourcePath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsSubdirectory_PathsAreIdentical_ReturnsFalse()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = "/dir1";

            bool result = destinationPath.IsSubdirectory(sourcePath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsSubdirectory_PathsAreNotRelated_ReturnsFalse()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = "/dir3";

            bool result = destinationPath.IsSubdirectory(sourcePath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsSubdirectory_SourcePathIsRoot_ReturnsFalse()
        {
            VirtualPath sourcePath = "/";
            VirtualPath destinationPath = "/dir1/dir2";

            bool result = destinationPath.IsSubdirectory(sourcePath);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsSubdirectory_DestinationPathIsRoot_ReturnsTrue()
        {
            VirtualPath sourcePath = "/dir1/dir2";
            VirtualPath destinationPath = "/";

            bool result = destinationPath.IsSubdirectory(sourcePath);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsSubdirectory_SourcePathIsEmpty_ThrowsArgumentException()
        {
            VirtualPath sourcePath = string.Empty;
            VirtualPath destinationPath = "/dir1";

            Exception err = Assert.ThrowsException<ArgumentException>(() =>
            {
                destinationPath.IsSubdirectory(sourcePath);
            });

            Assert.AreEqual("An empty string cannot be specified as a path. (Parameter 'parentPath')", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void IsSubdirectory_DestinationPathIsEmpty_ThrowsArgumentException()
        {
            VirtualPath sourcePath = "/dir1";
            VirtualPath destinationPath = string.Empty;

            Exception err = Assert.ThrowsException<ArgumentException>(() =>
            {
                destinationPath.IsSubdirectory(sourcePath);
            });

            Assert.AreEqual("An empty string cannot be specified as a path.", err.Message);

            Debug.WriteLine(err.Message);
        }

        [TestMethod]
        public void FixedPath_NoWildcardMatcher_ReturnsPathAndDepth()
        {
            // Arrange
            VirtualPath dir1 = "/dir1/file.txt";
            VirtualPath expectedPath = "/dir1/file.txt";
            int expectedDepth = 2;
            VirtualStorageState.State.WildcardMatcher = null;

            // Act
            VirtualPath resultPath = dir1.FixedPath;
            int resultDepth = dir1.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_WithWildcards_ReturnsTrimmedPathAndDepth()
        {
            // Arrange
            VirtualPath path = "/dir1/dir*/file.txt";
            VirtualPath expectedPath = "/dir1";
            int expectedDepth = 1;

            // Act
            VirtualPath resultPath = path.FixedPath;
            int resultDepth = path.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_NoWildcardsInParts_ReturnsPathAndDepth()
        {
            // Arrange
            VirtualPath path = "/dir1/dir2/file.txt";
            VirtualPath expectedPath = "/dir1/dir2";
            int expectedDepth = 2;

            // Act
            VirtualPath resultPath = path.FixedPath;
            int resultDepth = path.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_WithMultipleWildcards_ReturnsTrimmedPathAndDepth()
        {
            // Arrange
            VirtualPath path = "/dir1/dir*/dir2/dir*/file.txt";
            VirtualPath expectedPath = "/dir1";
            int expectedDepth = 1;

            // Act
            VirtualPath resultPath = path.FixedPath;
            int resultDepth = path.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_RootPath_ReturnsRootPathAndZeroDepth()
        {
            // Arrange
            VirtualPath rootPath = "/";
            VirtualPath expectedPath = "/";
            int expectedDepth = 0;
            VirtualStorageState.State.WildcardMatcher = null;

            // Act
            VirtualPath resultPath = rootPath.FixedPath;
            int resultDepth = rootPath.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_RootPath_ReturnsRootPathAndZeroDepth2()
        {
            // Arrange
            VirtualPath rootPath = "/";
            VirtualPath expectedPath = "/";
            int expectedDepth = 0;

            // Act
            VirtualPath resultPath = rootPath.FixedPath;
            int resultDepth = rootPath.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_PathWithoutWildcards_ReturnsPathAndDepth()
        {
            // Arrange
            VirtualPath path = "/dir1/dir2/dir3";
            VirtualPath expectedPath = "/dir1/dir2";
            int expectedDepth = 2;

            // Act
            VirtualPath resultPath = path.FixedPath;
            int resultDepth = path.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }

        [TestMethod]
        public void FixedPath_PathWithEndingWildcard_ReturnsTrimmedPathAndDepth()
        {
            // Arrange
            VirtualPath path = "/dir1/dir2/dir*";
            VirtualPath expectedPath = "/dir1/dir2";
            int expectedDepth = 2;

            // Act
            VirtualPath resultPath = path.FixedPath;
            int resultDepth = path.BaseDepth;

            // Assert
            Assert.AreEqual(expectedPath, resultPath);
            Assert.AreEqual(expectedDepth, resultDepth);
        }
    }
}
