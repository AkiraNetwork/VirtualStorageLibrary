using System.Diagnostics;

namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualStorageTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            VirtualStorageSettings.Initialize();
        }

        [TestMethod]
        public void ChangeDirectory_WithExistingDirectory_ChangesCurrentPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath existingDirectory = "/path/to/existing/directory";
            vs.AddDirectory(existingDirectory, true);

            // Act
            vs.ChangeDirectory(existingDirectory);

            // Assert
            Assert.AreEqual(existingDirectory, vs.CurrentPath);
        }

        [TestMethod]
        public void ChangeDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath nonExistentDirectory = "/path/to/nonexistent/directory";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.ChangeDirectory(nonExistentDirectory));
        }

        [TestMethod]
        public void ChangeDirectory_WithSymbolicLink_ChangesCurrentPathToTargetDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath baseDirectoryPath = "/path/to/";
            VirtualPath targetDirectory = "/real/target/directory";
            VirtualPath symbolicLink = "/path/to/link";
            vs.AddDirectory(baseDirectoryPath, true); // ベースディレクトリを追加
            vs.AddDirectory(targetDirectory, true); // 実際のターゲットディレクトリを追加
            vs.AddSymbolicLink(symbolicLink, targetDirectory); // ベースディレクトリ内にシンボリックリンクを追加

            // Act
            vs.ChangeDirectory(symbolicLink); // シンボリックリンクを通じてディレクトリ変更を試みる

            // Assert
            Assert.AreEqual(symbolicLink, vs.CurrentPath); // シンボリックリンクのターゲットがカレントディレクトリになっているか検証
        }

        [TestMethod]
        public void ChangeDirectory_WithDotDotInPath_NormalizesPathAndChangesCurrentPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath baseDirectory = "/path/to";
            VirtualPath subDirectory = "/path/to/subdirectory";
            vs.AddDirectory(baseDirectory, true); // ベースディレクトリを追加
            vs.AddDirectory(subDirectory, true); // サブディレクトリを追加

            // サブディレクトリに移動し、そこから親ディレクトリに戻るパスを設定
            VirtualPath pathToChange = "/path/to/subdirectory/../";

            // Act
            vs.ChangeDirectory(pathToChange);

            // Assert
            Assert.AreEqual(baseDirectory, vs.CurrentPath); // カレントディレクトリがベースディレクトリに正しく変更されたか検証
        }

        [TestMethod]
        public void ChangeDirectory_WithPathIncludingSymbolicLinkAndDotDot_NormalizesAndResolvesPathCorrectly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath baseDirectory = "/path/to";
            VirtualPath symbolicLinkPath = "/path/to/link";
            VirtualPath targetDirectory = "/real/target/directory";

            // ベースディレクトリとターゲットディレクトリを追加
            vs.AddDirectory(baseDirectory, true);
            vs.AddDirectory(targetDirectory, true);

            // サブディレクトリとしてシンボリックリンクを追加
            vs.AddSymbolicLink(symbolicLinkPath, targetDirectory);

            // シンボリックリンクを経由し、さらに".."を使って親ディレクトリに戻るパスを設定
            VirtualPath pathToChange = "/path/to/link/../..";

            // Act
            vs.ChangeDirectory(pathToChange);

            // Assert
            // シンボリックリンクを解決後、".."によりさらに上のディレクトリに移動するため、
            // 最終的なカレントディレクトリが/pathになることを期待
            VirtualPath expectedPath = "/path";
            Assert.AreEqual(expectedPath, vs.CurrentPath);
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act and Assert
            Assert.ThrowsException<ArgumentException>(() => vs.ConvertToAbsolutePath(string.Empty));
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenCurrentPathIsRootAndPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/TestDirectory");
            vs.ChangeDirectory(VirtualPath.Root);

            VirtualPath result = vs.ConvertToAbsolutePath("TestDirectory");

            Assert.IsTrue(result == "/TestDirectory");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathStartsWithSlash_ReturnsSamePath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/root/subdirectory", true);
            vs.ChangeDirectory("/root/subdirectory");

            VirtualPath result = vs.ConvertToAbsolutePath("/test/path");

            Assert.IsTrue(result == "/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathDoesNotStartWithSlash_ReturnsAbsolutePath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/root/subdirectory", true);
            vs.ChangeDirectory("/root/subdirectory");

            VirtualPath result = vs.ConvertToAbsolutePath("test/path");

            Assert.IsTrue(result == "/root/subdirectory/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDot_ReturnsAbsolutePathWithoutDot()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/root/subdirectory", true);
            vs.ChangeDirectory("/root/subdirectory");

            VirtualPath result = vs.ConvertToAbsolutePath("./test/path");

            Assert.IsTrue(result == "/root/subdirectory/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsDoubleDot_ReturnsParentDirectoryPath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/root/subdirectory", true);
            vs.ChangeDirectory("/root/subdirectory");

            VirtualPath result = vs.ConvertToAbsolutePath("../test/path");

            Assert.IsTrue(result == "/root/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WhenPathContainsMultipleDoubleDots_ReturnsCorrectPath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/root/subdirectory", true);
            vs.ChangeDirectory("/root/subdirectory");

            VirtualPath result = vs.ConvertToAbsolutePath("../../test/path");

            Assert.IsTrue(result == "/test/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithBasePath_ConvertsRelativePathCorrectly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/base/path", true); // 必要なディレクトリを作成
            VirtualPath? basePath = "/base/path";
            VirtualPath relativePath = "relative/to/base";

            // Act
            VirtualPath result = vs.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.IsTrue(result == "/base/path/relative/to/base");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithoutBasePath_UsesCurrentPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/current/path", true); // 必要なディレクトリを作成
            vs.ChangeDirectory("/current/path");
            VirtualPath relativePath = "relative/path";

            // Act
            VirtualPath result = vs.ConvertToAbsolutePath(relativePath, null);

            // Assert
            Assert.IsTrue(result == "/current/path/relative/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithEmptyBasePath_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath relativePath = "some/relative/path";

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => vs.ConvertToAbsolutePath(relativePath, string.Empty));
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithNullBasePath_UsesCurrentPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/current/path", true); // 必要なディレクトリを作成
            vs.ChangeDirectory("/current/path");
            VirtualPath relativePath = "relative/path";
            VirtualPath? basePath = null;

            // Act
            VirtualPath result = vs.ConvertToAbsolutePath(relativePath, basePath);

            // Assert
            Assert.IsTrue(result == "/current/path/relative/path");
        }

        [TestMethod]
        public void ConvertToAbsolutePath_WithAbsolutePath_ReturnsOriginalPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/base/path", true); // 必要なディレクトリを作成
            VirtualPath absolutePath = "/absolute/path";

            // Act
            VirtualPath result = vs.ConvertToAbsolutePath(absolutePath, "/base/path");

            // Assert
            Assert.AreEqual(absolutePath, result);
        }

        [TestMethod]
        // ディレクトリの追加が正常に行われることを確認
        public void AddDirectory_WithValidPath_CreatesDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act
            vs.AddDirectory("/test");
            vs.AddDirectory("/test/directory");

            // Assert
            Assert.IsTrue(vs.NodeExists("/test/directory"));
        }

        [TestMethod]
        // ネストされたディレクトリを作成する場合、createSubdirectories が false の場合、例外がスローされることを確認
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.AddDirectory("/test/directory", false));
        }

        [TestMethod]
        // ネストされたディレクトリを作成する場合、createSubdirectories が true の場合、ディレクトリが作成されることを確認
        public void AddDirectory_WithNestedPathAndCreateSubdirectoriesTrue_CreatesDirectories()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act
            vs.AddDirectory("/test/directory/subdirectory", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/test/directory/subdirectory"));
        }

        [TestMethod]
        // 既存のディレクトリに対して createSubdirectories が true の場合でも同じパスにディレクトリを追加しようとすると例外がスローされることを確認
        public void AddDirectory_WithExistingDirectoryAndCreateSubdirectoriesTrue_DoesNotThrowException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/directory", true);

            // Act & Assert
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.AddDirectory("/test/directory", true);
            });
            Debug.WriteLine(exception.Message);
        }

        [TestMethod]
        // 既存のディレクトリに対して createSubdirectories が false の場合、例外がスローされることを確認
        public void AddDirectory_WithExistingDirectoryAndCreateSubdirectoriesFalse_ThrowException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/directory", true);

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddDirectory("/test/directory", false));
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddRootDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddDirectory("/"));
        }

        [TestMethod]
        public void AddDirectory_ThroughSymbolicLink_CreatesDirectoryAtResolvedPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test", true);
            vs.AddSymbolicLink("/link", "/test", true);

            // Act
            vs.AddDirectory("/link/directory", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/test/directory"));
        }

        [TestMethod]
        public void AddDirectory_WithRelativePath_CreatesDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test", true);
            vs.ChangeDirectory("/test");

            // Act
            vs.AddDirectory("directory", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/test/directory"));
        }

        [TestMethod]
        public void AddDirectory_ExistingSubdirectoriesWithCreateSubdirectoriesTrue_DoesNotAffectExistingSubdirectories()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/subdirectory", true);

            // Act
            vs.AddDirectory("/test/newDirectory", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/test/subdirectory"));
            Assert.IsTrue(vs.NodeExists("/test/newDirectory"));
        }

        [TestMethod]
        public void AddDirectory_MultipleLevelsOfDirectoriesWithCreateSubdirectoriesFalse_ThrowsException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.AddDirectory("/test/subdirectory/anotherDirectory", false));
        }

        [TestMethod]
        public void AddDirectory_WithCurrentDirectoryDot_CreatesDirectoryCorrectly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test", true); // 初期ディレクトリを作成

            // Act
            // 現在のディレクトリ（"/test"）に対して"."を使用して、さらにサブディレクトリを作成
            vs.AddDirectory("/test/./subdirectory", true);

            // Assert
            // "/test/subdirectory" が正しく作成されたことを確認
            Assert.IsTrue(vs.NodeExists("/test/subdirectory"));
        }

        [TestMethod]
        public void AddDirectory_WithPathIncludingDotDot_CreatesDirectoryCorrectly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test", true);
            vs.AddDirectory("/test/subdirectory", true);

            // Act
            vs.AddDirectory("/test/subdirectory/../anotherDirectory", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/test/anotherDirectory"));
            Assert.IsTrue(vs.NodeExists("/test/subdirectory"));
        }

        [TestMethod]
        public void AddDirectory_WithPathNormalization_CreatesDirectoryAtNormalizedPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act
            vs.AddDirectory("/test/../test2", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/test2"));
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddDirectoryUnderNonDirectoryNode_ThrowsException()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            // "/dir1"ディレクトリを作成
            vs.AddDirectory("/dir1", true);
            // "/dir1"内にアイテム（非ディレクトリ）"item"を作成
            vs.AddItem("/dir1/item", "Dummy content", true);

            // Act & Assert
            // "/dir1/item"がディレクトリではないノードの下に"dir2"ディレクトリを追加しようとすると例外がスローされることを確認
            VirtualNodeNotFoundException exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.AddDirectory("/dir1/item/dir2", true));
            Debug.WriteLine(exception.Message);
        }

        [TestMethod]
        public void AddDirectory_AttemptToAddDirectoryUnderNonDirectoryNode_ThrowsException2()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            // "/dir1"ディレクトリを作成
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            // "/dir1"内にアイテム（非ディレクトリ）"item"を作成
            vs.AddSymbolicLink("/dir1/linkToDir2", "/dir2");

            // Act
            // "/dir1/linkToDir2"がリンクの場合、
            vs.AddDirectory("/dir1/linkToDir2/dir3", true);

            // Assert
            Assert.IsTrue(vs.NodeExists("/dir2/dir3"));
        }

        [TestMethod]
        public void AddDirectory_ThrowsException_WhenSymbolicLinkExistsWithSameName()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            // 基本ディレクトリを作成
            vs.AddDirectory("/base");
            // "/base"ディレクトリにシンボリックリンク"/base/link"を作成
            vs.AddSymbolicLink("/base/link", "/target");

            // Act & Assert
            // "/base/link"にディレクトリを作成しようとすると、例外が発生することを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddDirectory("/base/link", true));
        }

        [TestMethod]
        public void AddDirectory_ThrowsException_WhenItemExistsWithSameName()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            // 基本ディレクトリを作成
            vs.AddDirectory("/base", true);
            // "/base"ディレクトリにアイテム"/base/item"を作成（この例では、アイテムを作成するメソッドを仮定）
            vs.AddItem("/base/item", "Some content", true);

            // Act & Assert
            // "/base/item"にディレクトリを作成しようとすると、例外が発生することを確認
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddDirectory("/base/item", true),
                "Expected VirtualNodeNotFoundException when trying to create a directory where an item exists with the same nodeName.");
        }

        [TestMethod]
        public void AddDirectory_Success()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            var directoryPath = new VirtualPath("/parentDirectory");
            var newDirectory = new VirtualDirectory(new VirtualNodeName("newDirectory"));

            // Act
            vs.AddDirectory(directoryPath);
            vs.AddDirectory(directoryPath, newDirectory);

            // Assert
            var parentDirectory = vs.GetNode(directoryPath) as VirtualDirectory;
            Assert.IsNotNull(parentDirectory);
            Assert.IsTrue(parentDirectory.NodeExists(newDirectory.Name));
        }

        [TestMethod]
        public void AddDirectory_WithSubdirectories_Success()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            var directoryPath = new VirtualPath("/parentDirectory/subDirectory");
            var newDirectory = new VirtualDirectory(new VirtualNodeName("newDirectory"));

            // Act
            vs.AddDirectory(directoryPath, newDirectory, true);

            // Assert
            var subDirectoryPath = new VirtualPath("/parentDirectory/subDirectory");
            var parentDirectory = vs.GetNode(subDirectoryPath) as VirtualDirectory;
            Assert.IsNotNull(parentDirectory);
            Assert.IsTrue(parentDirectory.NodeExists(newDirectory.Name));
        }

        [TestMethod]
        public void AddDirectory_DirectoryAlreadyExists_ThrowsException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            var directoryPath = new VirtualPath("/parentDirectory");
            var existingDirectory = new VirtualDirectory(new VirtualNodeName("existingDirectory"));
            vs.AddDirectory(directoryPath);
            vs.AddDirectory(directoryPath, existingDirectory);

            var newDirectory = new VirtualDirectory(new VirtualNodeName("existingDirectory"));

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() => vs.AddDirectory(directoryPath, newDirectory));
        }

        [TestMethod]
        public void AddDirectory_InvalidPath_ThrowsException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            var invalidPath = new VirtualPath("/invalid/path");
            var newDirectory = new VirtualDirectory(new VirtualNodeName("newDirectory"));

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.AddDirectory(invalidPath, newDirectory));
        }

        [TestMethod]
        public void AddNode_AddDirectory_CreatesDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            var directoryPath = new VirtualPath("/parentDirectory");
            var newDirectory = new VirtualDirectory(new VirtualNodeName("newDirectory"));
            vs.AddDirectory(directoryPath);

            // Act
            vs.AddNode(directoryPath, newDirectory);

            // Assert
            var parentDirectory = vs.GetNode(directoryPath) as VirtualDirectory;
            Assert.IsNotNull(parentDirectory);
            Assert.IsTrue(parentDirectory.NodeExists(newDirectory.Name));
        }

        [TestMethod]
        public void AddNode_AddSymbolicLink_CreatesLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            var linkDirectoryPath = new VirtualPath("/linkDirectory");
            var linkName = new VirtualNodeName("link");
            var targetPath = new VirtualPath("/targetDirectory");
            var newLink = new VirtualSymbolicLink(linkName, targetPath);
            vs.AddDirectory(linkDirectoryPath);

            // Act
            vs.AddNode(linkDirectoryPath, newLink);

            // Assert
            var linkDirectory = vs.GetNode(linkDirectoryPath) as VirtualDirectory;
            Assert.IsNotNull(linkDirectory);
            Assert.IsTrue(linkDirectory.NodeExists(linkName));
        }

        [TestMethod]
        public void AddNode_AddItem_CreatesItem()
        {
            // Arrange
            VirtualStorage<object> vs = new();
            var itemDirectoryPath = new VirtualPath("/itemDirectory");
            var itemName = new VirtualNodeName("item");
            var data = new object();
            var newItem = new VirtualItem<object>(itemName, data);
            vs.AddDirectory(itemDirectoryPath);

            // Act
            vs.AddNode(itemDirectoryPath, newItem);

            // Assert
            var itemDirectory = vs.GetNode(itemDirectoryPath) as VirtualDirectory;
            Assert.IsNotNull(itemDirectory);
            Assert.IsTrue(itemDirectory.NodeExists(itemName));
        }

        [TestMethod]
        public void GetNode_ReturnsCorrectNode_WhenNodeIsItem()
        {
            // テストデータの設定
            VirtualStorage<BinaryData> vs = new();
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
            VirtualStorage<BinaryData> vs = new();
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
            VirtualStorage<BinaryData> vs = new();

            // メソッドを実行し、例外を検証
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetNode("/NonExistentDirectory"));
        }

        [TestMethod]
        public void GetNode_FollowsSymbolicLink_WhenFollowLinksIsTrue()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir");
            vs.AddItem("/dir/item", "TestItem");
            vs.AddSymbolicLink("/link", "/dir/item");

            // Act
            VirtualNode node = vs.GetNode("/link", true);

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
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir");
            vs.AddItem("/dir/item", "TestItem");
            vs.AddSymbolicLink("/link", "/dir/item");

            // Act
            VirtualNode node = vs.GetNode("/link", false);

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
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/brokenLink", "/nonexistent/item");

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetNode("/brokenLink", true));
        }

        [TestMethod]
        public void GetNode_StopsAtNonDirectoryNode()
        {
            // Arrange: 仮想ストレージにディレクトリとアイテムをセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir");
            vs.AddItem("/dir/item", "TestItem");
            // /dir/item はディレクトリではない

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                // Act: ディレクトリではないノードの後ろに更にパスが続く場合の挙動をテスト
                VirtualNode resultNode = vs.GetNode("/dir/item/more", false);
            });
        }

        [TestMethod]
        public void GetNode_FollowsMultipleSymbolicLinksToReachAnItem()
        {
            // Arrange: 仮想ストレージと複数のディレクトリ、シンボリックリンクをセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir1/dir2");
            vs.AddItem("/dir1/dir2/item", "FinalItem");

            // 最初のシンボリックリンクを /dir1 に追加し、/dir1/dir2 を指す
            vs.AddSymbolicLink("/dir1/link1", "/dir1/dir2");

            // 2番目のシンボリックリンクを /dir1/dir2 に追加し、/dir1/dir2/item を指す
            vs.AddSymbolicLink("/dir1/dir2/link2", "/dir1/dir2/item");

            // Act: 複数のシンボリックリンクを透過的に辿る
            VirtualNode resultNode = vs.GetNode("/dir1/link1/link2", true);

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
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir1/dir2");
            vs.AddItem("/dir1/dir2/item", "RelativeItem");

            // 相対パスでシンボリックリンクを追加。ここでは、/dir1から/dir1/dir2への相対パスリンクを作成
            vs.AddSymbolicLink("/dir1/relativeLink", "dir2/item");

            // Act: 相対パスのシンボリックリンクを透過的に辿る
            VirtualNode resultNode = vs.GetNode("/dir1/relativeLink", true);

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
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/Test");
            vs.ChangeDirectory("/Test");
            vs.AddSymbolicLink("LinkToCurrent", VirtualPath.Dot);

            // Act
            VirtualNode node = vs.GetNode("LinkToCurrent", true);

            // Assert
            Assert.IsTrue(node is VirtualDirectory);
            VirtualDirectory? directory = node as VirtualDirectory;
            Assert.AreEqual(new VirtualNodeName("Test"), directory?.Name);
        }

        [TestMethod]
        public void GetNode_ResolvesSymbolicLinkWithParentDirectoryReference_Correctly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/Parent/Child", true);
            vs.ChangeDirectory("/Parent/Child");
            vs.AddSymbolicLink("LinkToParent", VirtualPath.DotDot);

            // Act
            VirtualNode node = vs.GetNode("LinkToParent", true);

            // Assert
            Assert.IsTrue(node is VirtualDirectory);
            VirtualDirectory? directory = node as VirtualDirectory;
            Assert.AreEqual(new VirtualNodeName("Parent"), directory?.Name);
        }

        [TestMethod]
        public void GetNode_ComplexSymbolicLinkIncludingDotAndDotDot()
        {
            // テスト用の仮想ストレージとディレクトリ構造を準備
            VirtualStorage<string> vs = new();

            // 複数レベルのサブディレクトリを一度に作成
            vs.AddDirectory("/dir/subdir/dir1", true);
            vs.AddDirectory("/dir/subdir/dir2", true); // 隣接するディレクトリを追加
            vs.AddItem("/dir/subdir/dir2/item", "complex item in dir2");

            // シンボリックリンクを作成
            // "/dir/subdir/link" が "./dir1/../dir2/./item" を指し、隣接するディレクトリ内のアイテムへの複合的なパスを使用
            vs.AddSymbolicLink("/dir/subdir/link", "./dir1/../dir2/./item");

            // シンボリックリンクを通じてアイテムにアクセス
            VirtualNode resultNode = vs.GetNode("/dir/subdir/link", true);

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
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/Documents");
            vs.AddSymbolicLink("/LinkToDocuments", "/Documents");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("/LinkToDocuments");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/Documents");
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathContainsTwoLinks()
        {
            // テストデータの設定
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/FinalDestination");
            vs.AddSymbolicLink("/FirstLink", "/SecondLink");
            vs.AddSymbolicLink("/SecondLink", "/FinalDestination");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("/FirstLink");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/FinalDestination");
        }

        [TestMethod]
        public void ResolveLinkTarget_ResolvesPathCorrectly_WhenUsingDotDotInPathWithSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/dir2/dir3/item", "ItemInDir1");
            vs.AddSymbolicLink("/dir1/dir2/dir3/linkToDir2", "/dir1/dir2");
            vs.ChangeDirectory("/dir1/dir2/dir3");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("linkToDir2/../item");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/dir1/dir2/dir3/item");
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathSpansMultipleLinkLevels()
        {
            // テストデータの設定
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddDirectory("/dir3");
            vs.AddItem("/dir3/item", "FinalItem");
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddSymbolicLink("/dir2/link2", "/dir3");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("/dir1/link1/link2/item");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/dir3/item");
        }

        [TestMethod]
        public void ResolveLinkTarget_ReturnsResolvedPath_WhenPathIncludesSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/dir2", true);
            vs.AddItem("/dir1/dir2/item", "FinalItem");
            vs.AddSymbolicLink("/dir1/linkToDir2", "/dir1/dir2");
            vs.ChangeDirectory("/dir1");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("linkToDir2/item");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/dir1/dir2/item");
        }

        [TestMethod]
        public void ResolveLinkTarget_ResolvesPathCorrectly_WhenUsingDotWithSymbolicLink()
        {
            // テストデータの設定
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/subdir", true);
            vs.AddItem("/dir1/subdir/item", "SubdirItem");
            vs.AddSymbolicLink("/dir1/link", "/dir1/subdir");
            vs.ChangeDirectory("/dir1");

            // メソッドを実行
            VirtualPath resolvedPath = vs.ResolveLinkTarget("./link/item");

            // 結果を検証
            Assert.IsTrue(resolvedPath == "/dir1/subdir/item");
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryExists_ReturnsDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            VirtualPath path = "/test";

            // Act
            VirtualDirectory directory = vs.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual(new VirtualNodeName("test"), directory.Name);
        }

        [TestMethod]
        public void GetDirectory_WhenDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/nonexistent";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetDirectory(path));
        }

        [TestMethod]
        public void GetDirectory_WhenNodeIsNotDirectory_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData file = [1, 2, 3];
            vs.AddItem("/test-file", file);

            VirtualPath path = "/test-file";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetDirectory(path));
        }

        [TestMethod]
        public void GetDirectory_WhenPathIsRelative_ReturnsDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            VirtualPath path = "test";

            // Act
            VirtualDirectory directory = vs.GetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
            Assert.AreEqual(new VirtualNodeName("test"), directory.Name);
        }

        [TestMethod]
        public void GetItem_WhenItemExists_ReturnsItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = new([1, 2, 3]);
            vs.AddItem("/test-item", data);
            VirtualPath path = "/test-item";

            // Act
            VirtualItem<BinaryData> item = vs.GetItem(path);

            // Assert
            Assert.IsNotNull(item);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, item.ItemData!.Data);
        }

        [TestMethod]
        public void GetItem_WhenItemDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/nonexistent";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetItem(path));
        }

        [TestMethod]
        public void GetItem_WhenNodeIsNotItemType_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test-directory");
            VirtualPath path = "/test-directory";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetItem(path));
        }

        [TestMethod]
        public void GetItem_WhenPathIsRelative_ReturnsItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = new([1, 2, 3]);
            vs.AddItem("/test-item", data);
            VirtualPath path = "test-item";  // Relative path

            // Act
            VirtualItem<BinaryData> item = vs.GetItem(path);

            // Assert
            Assert.IsNotNull(item);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, item.ItemData!.Data);
        }

        [TestMethod]
        public void GetSymbolicLink_WhenLinkExists_ReturnsLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/target";
            vs.AddDirectory(targetPath);
            VirtualPath linkPath = "/link";
            vs.AddSymbolicLink(linkPath, targetPath);

            // Act
            VirtualSymbolicLink link = vs.GetSymbolicLink(linkPath);

            // Assert
            Assert.IsNotNull(link);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void GetSymbolicLink_WhenLinkDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/nonexistent-link";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetSymbolicLink(path));
        }

        [TestMethod]
        public void GetSymbolicLink_WhenNodeIsNotLink_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData file = new([1, 2, 3]);
            vs.AddItem("/test-file", file);

            VirtualPath path = "/test-file";

            // Act and Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.GetSymbolicLink(path));
        }

        [TestMethod]
        public void GetSymbolicLink_WhenPathIsRelative_ReturnsLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/target";
            vs.AddDirectory(targetPath);
            VirtualPath linkPath = "/link";
            vs.AddSymbolicLink(linkPath, targetPath);
            VirtualPath relativePath = "link";

            // Act
            VirtualSymbolicLink link = vs.GetSymbolicLink(relativePath);

            // Assert
            Assert.IsNotNull(link);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void AddItem_AddsNewItemSuccessfully_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];
            VirtualPath path = "/NewItem";

            vs.AddItem(path, item);

            Assert.IsTrue(vs.ItemExists(path));
            VirtualItem<BinaryData>? retrievedItem = vs.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_OverwritesExistingItemWhenAllowed_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalItem = [1, 2, 3];
            BinaryData newItem = [4, 5, 6];
            VirtualPath path = "/ExistingItem";

            vs.AddItem(path, originalItem);
            vs.AddItem(path, newItem, true);

            VirtualItem<BinaryData>? retrievedItem = vs.GetNode(path) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(newItem.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsArgumentException_WhenPathIsEmpty_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];

            Assert.ThrowsException<ArgumentException>(() => vs.AddItem(string.Empty, item));
        }

        [TestMethod]
        public void AddItem_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];
            VirtualPath path = "/NonExistentDirectory/ItemData";

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.AddItem(path, item));
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteIsFalseAndItemExists_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalItem = [1, 2, 3];
            VirtualPath path = "/ExistingItem";
            vs.AddItem(path, originalItem);

            Assert.ThrowsException<InvalidOperationException>(() => vs.AddItem(path, new BinaryData([4, 5, 6]), false));
        }

        [TestMethod]
        public void AddItem_AddsNewItemToCurrentDirectory_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.ChangeDirectory(VirtualPath.Root);
            BinaryData item = [1, 2, 3];
            VirtualPath itemName = "NewItemInRoot";

            vs.AddItem(itemName, item); // パスを指定せずにアイテム名のみを渡す

            Assert.IsTrue(vs.ItemExists(itemName.AddStartSlash())); // カレントディレクトリにアイテムが作成されていることを確認
            VirtualItem<BinaryData>? retrievedItem = vs.GetNode(itemName.AddStartSlash()) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemUsingRelativePath_WithBinaryData_Corrected()
        {
            VirtualStorage<BinaryData> vs = new();

            // 事前にディレクトリを作成
            vs.AddDirectory("/existingDirectory", true); // 既存のディレクトリ
            vs.AddDirectory("/existingDirectory/subDirectory", true); // 新しく作成するサブディレクトリ
            vs.ChangeDirectory("/existingDirectory"); // カレントディレクトリを変更

            BinaryData item = [4, 5, 6];
            VirtualPath relativePath = "subDirectory/NewItem"; // 相対パスで新しいアイテムを指定

            // 相対パスを使用してアイテムを追加
            vs.AddItem(relativePath, item, true);

            VirtualPath fullPath = "/existingDirectory/" + relativePath;
            Assert.IsTrue(vs.ItemExists(fullPath)); // 相対パスで指定された位置にアイテムが作成されていることを確認
            VirtualItem<BinaryData>? retrievedItem = vs.GetNode(fullPath) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_AddsNewItemToSubdirectoryAsCurrentDirectory_WithBinaryData()
        {
            VirtualStorage<BinaryData> vs = new();

            // サブディレクトリを作成し、カレントディレクトリに設定
            vs.AddDirectory("/subdirectory", true);
            vs.ChangeDirectory("/subdirectory");
            BinaryData item = [1, 2, 3];
            VirtualPath itemName = "NewItemInSubdirectory";

            // カレントディレクトリにアイテムを追加（パスを指定せずにアイテム名のみを渡す）
            vs.AddItem(itemName, item);

            // サブディレクトリ内にアイテムが作成されていることを確認
            Assert.IsTrue(vs.ItemExists("/subdirectory/" + itemName));
            VirtualItem<BinaryData>? retrievedItem = vs.GetNode("/subdirectory/" + itemName) as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(item.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_ThrowsInvalidOperationException_WhenOverwriteTargetIsNotAnItem()
        {
            VirtualStorage<BinaryData> vs = new();

            // テスト用のディレクトリを作成
            vs.AddDirectory("/testDirectory", true);

            // 同名のサブディレクトリを追加（アイテムの上書き対象として）
            vs.AddDirectory("/testDirectory/itemName", true);

            BinaryData item = [1, 2, 3];
            VirtualNodeName itemName = "itemName";

            // アイテム上書きを試みる（ただし、実際にはディレクトリが存在するため、アイテムではない）
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddItem("/testDirectory/" + itemName, item, true),
                "上書き対象がアイテムではなくディレクトリの場合、InvalidOperationExceptionが投げられるべきです。");
        }

        [TestMethod]
        public void AddItem_ThroughSymbolicLink_AddsItemToTargetDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/actualDirectory", true);
            vs.AddSymbolicLink("/symbolicLink", "/actualDirectory");
            BinaryData binaryData = [1, 2, 3];

            // Act
            vs.AddItem("/symbolicLink/newItem", binaryData);

            // Assert
            Assert.IsTrue(vs.ItemExists("/actualDirectory/newItem"));
            VirtualItem<BinaryData>? retrievedItem = vs.GetNode("/actualDirectory/newItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(binaryData.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_ThroughNestedSymbolicLink_AddsItemToFinalTargetDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/level1/level2/targetDirectory", true);
            vs.AddSymbolicLink("/linkToLevel1", "/level1");
            vs.AddSymbolicLink("/level1/linkToLevel2", "/level1/level2");
            BinaryData binaryData = [4, 5, 6];

            // Act
            vs.AddItem("/linkToLevel1/linkToLevel2/targetDirectory/newItem", binaryData);

            // Assert
            Assert.IsTrue(vs.ItemExists("/level1/level2/targetDirectory/newItem"));
            VirtualItem<BinaryData>? retrievedItem = vs.GetNode("/level1/level2/targetDirectory/newItem") as VirtualItem<BinaryData>;
            Assert.IsNotNull(retrievedItem);
            CollectionAssert.AreEqual(binaryData.Data, retrievedItem.ItemData!.Data);
        }

        [TestMethod]
        public void AddItem_ToNonExistentTargetViaSymbolicLink_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/linkToNowhere", "/nonExistentTarget");
            BinaryData binaryData = [7, 8, 9];

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.AddItem("/linkToNowhere/newItem", binaryData));
        }

        [TestMethod]
        public void AddItem_ThroughSymbolicLinkWithNonExistentIntermediateTarget_ThrowsVirtualNodeNotFoundException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/existingDirectory", true);

            // 中間のターゲットが存在しないシンボリックリンクを作成
            vs.AddSymbolicLink("/existingDirectory/nonExistentLink", "/nonExistentIntermediateTarget");

            // 最終的なパスの組み立て
            VirtualPath pathToItem = "/existingDirectory/nonExistentLink/finalItem";
            BinaryData binaryData = [10, 11, 12];

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.AddItem(pathToItem, binaryData),
                "中間のシンボリックリンクのターゲットが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

        [TestMethod]
        public void AddItem_WithVirtualItem_ShouldAddItemToDirectory()
        {
            VirtualStorage<string> vs = new();
            VirtualPath path = "/dir";
            vs.AddDirectory(path);
            VirtualItem<string> item = new("item", "test");

            vs.AddItem(path, item);

            Assert.IsTrue(vs.ItemExists(path + item.Name));
        }

        [TestMethod]
        public void AddItem_WithVirtualItem_WithOverwrite_ShouldThrowExceptionIfItemExists()
        {
            VirtualStorage<string> vs = new();
            VirtualPath directoryPath = "/dir";
            vs.AddDirectory(directoryPath);
            VirtualNodeName existingItemName = "existingItem";
            VirtualItem<string> existingItem = new(existingItemName, "existing value");
            VirtualItem<string> newItem = new(existingItemName, "new value");

            vs.AddItem(directoryPath, existingItem, false);

            Assert.ThrowsException<InvalidOperationException>(() => vs.AddItem(directoryPath, newItem, false));
        }

        [TestMethod]
        public void AddItem_WithOverwrite_ShouldOverwriteExistingItem()
        {
            VirtualStorage<string> vs = new();
            VirtualPath directoryPath = "/dir";
            vs.AddDirectory(directoryPath);
            VirtualNodeName itemName = "item";
            VirtualItem<string> originalItem = new(itemName, "original value");
            VirtualItem<string> newItem = new(itemName, "new value");

            vs.AddItem(directoryPath, originalItem, false);
            vs.AddItem(directoryPath, newItem, true);

            Assert.IsTrue(vs.ItemExists(directoryPath + newItem.Name));
        }

        [TestMethod]
        public void AddItem_ToNonExistingDirectory_ShouldThrowVirtualNodeNotFoundException()
        {
            VirtualStorage<string> vs = new();
            VirtualPath directoryPath = "/nonExistingDir";
            VirtualNodeName itemName = "item";
            VirtualItem<string> item = new(itemName, "value");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.AddItem(directoryPath, item, false));
        }

        [TestMethod]
        public void AddItem_ToLocationPointedBySymbolicLink_ShouldThrowException()
        {
            VirtualStorage<string> vs = new();
            VirtualPath directoryPath = "/dir";
            vs.AddDirectory(directoryPath);
            VirtualPath linkPath = "/link";
            VirtualPath targetPath = "/nonExistingTargetDir";
            vs.AddSymbolicLink(linkPath, targetPath);

            VirtualNodeName itemName = "item";
            VirtualItem<string> item = new(itemName, "value");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.AddItem(linkPath, item, false));
        }

        [TestMethod]
        public void AddItem_ToLocationPointedBySymbolicLink_ShouldAddItemSuccessfully()
        {
            VirtualStorage<string> vs = new();
            VirtualPath actualDirectoryPath = "/actualDir";
            vs.AddDirectory(actualDirectoryPath);
            VirtualPath symbolicLinkPath = "/symbolicLink";
            vs.AddSymbolicLink(symbolicLinkPath, actualDirectoryPath);
            VirtualNodeName itemName = "newItem";
            VirtualItem<string> item = new(itemName, "itemValue");
            vs.AddItem(symbolicLinkPath, item);
            Assert.IsTrue(vs.ItemExists(actualDirectoryPath + itemName));
        }

        [TestMethod]
        public void ItemExists_WhenIntermediateDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act
            bool result = vs.NodeExists("/nonexistent/test-file");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];
            vs.AddItem("/test-file", item);

            // Act
            bool result = vs.NodeExists("/test-file");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act
            bool result = vs.NodeExists("/nonexistent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test-dir");

            // Act
            bool result = vs.NodeExists("/test-dir");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act
            bool result = vs.NodeExists("/nonexistent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToItemAndFollowLinksIsTrue_ReturnsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];
            vs.AddItem("/actual-item", item);
            vs.AddSymbolicLink("/link-to-item", "/actual-item");

            // Act
            bool result = vs.ItemExists("/link-to-item", true);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToItemAndFollowLinksIsFalse_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];
            vs.AddItem("/actual-item", item);
            vs.AddSymbolicLink("/link-to-item", "/actual-item");

            // Act
            bool result = vs.ItemExists("/link-to-item", false);

            // Assert
            Assert.IsFalse(result); // シンボリックリンク自体はアイテムとしてカウントしない
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToNonexistentItemAndFollowLinksIsTrue_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/link-to-nonexistent", "/non-existent-item");

            // Act
            bool result = vs.ItemExists("/link-to-nonexistent", true);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ItemExists_WithSymbolicLinkToPointingToDirectoryAndFollowLinksIsTrue_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/target-dir");
            vs.AddSymbolicLink("/link-to-dir", "/target-dir");

            // Act
            bool result = vs.ItemExists("/link-to-dir", true);

            // Assert
            Assert.IsFalse(result); // ディレクトリを指すシンボリックリンクはアイテムとしてカウントしない
        }

        private static void SetTestData(VirtualStorage<BinaryData> vs)
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
            VirtualStorage<BinaryData> vs = new();

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
            VirtualStorage<BinaryData> vs = new();

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
            VirtualStorage<BinaryData> vs = new();

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() =>
                vs.GetNodes(string.Empty, VirtualNodeTypeFilter.All, true).ToList());
        }

        [TestMethod]
        public void GetNodes_WithNonAbsolutePath_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.GetNodes("relative/path", VirtualNodeTypeFilter.All, true).ToList());
        }

        [TestMethod]
        public void RemoveNode_ExistingItem_RemovesItem()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData item = [1, 2, 3];
            vs.AddItem("/TestItem", item);

            vs.RemoveNode("/TestItem");

            Assert.IsFalse(vs.NodeExists("/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_NonExistingItem_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.RemoveNode("/NonExistingItem"));
        }

        [TestMethod]
        public void RemoveNode_ExistingEmptyDirectory_RemovesDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/TestDirectory");

            vs.RemoveNode("/TestDirectory");

            Assert.IsFalse(vs.NodeExists("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_NonExistingDirectory_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.RemoveNode("/NonExistingDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithoutRecursive_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/TestDirectory");
            BinaryData item = [1, 2, 3];
            vs.AddItem("/TestDirectory/TestItem", item);

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.RemoveNode("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_ExistingNonEmptyDirectoryWithRecursive_RemovesDirectoryAndContents()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/TestDirectory");
            BinaryData item = [1, 2, 3];
            vs.AddItem("/TestDirectory/TestItem", item);

            vs.RemoveNode("/TestDirectory", true);

            Assert.IsFalse(vs.NodeExists("/TestDirectory"));
            Assert.IsFalse(vs.NodeExists("/TestDirectory/TestItem"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithRecursive_RemovesAllNestedContents()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/Level1/Level2/Level3", true);
            BinaryData item1 = [1, 2, 3];
            BinaryData item2 = [4, 5, 6];
            vs.AddItem("/Level1/Level2/Level3/Item1", item1);
            vs.AddItem("/Level1/Level2/Item2", item2);

            vs.RemoveNode("/Level1", true);

            Assert.IsFalse(vs.NodeExists("/Level1"));
            Assert.IsFalse(vs.NodeExists("/Level1/Level2/Level3/Item1"));
            Assert.IsFalse(vs.NodeExists("/Level1/Level2/Item2"));
        }

        [TestMethod]
        public void RemoveNode_DeepNestedDirectoryWithoutRecursive_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/Level1/Level2/Level3", true);
            BinaryData item1 = [1, 2, 3];
            vs.AddItem("/Level1/Level2/Level3/Item1", item1);

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.RemoveNode("/Level1"));
        }

        [TestMethod]
        public void RemoveNode_NestedDirectoryWithEmptySubdirectories_RecursiveRemoval()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/Level1/Level2/Level3", true);

            vs.RemoveNode("/Level1", true);

            Assert.IsFalse(vs.NodeExists("/Level1"));
            Assert.IsFalse(vs.NodeExists("/Level1/Level2"));
            Assert.IsFalse(vs.NodeExists("/Level1/Level2/Level3"));
        }

        [TestMethod]
        public void RemoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.RemoveNode(VirtualPath.Root);
            });
        }

        [TestMethod]
        public void RemoveNode_CurrentDirectoryDot_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.RemoveNode(VirtualPath.Dot);
            });
        }

        [TestMethod]
        public void RemoveNode_ParentDirectoryDotDot_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.RemoveNode(VirtualPath.DotDot);
            });
        }

        [TestMethod]
        public void RemoveNode_SymbolicLink_RemovesLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            vs.AddSymbolicLink("/test/link", "/target/path");

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                vs.RemoveNode("/test/link");
            });
        }

        [TestMethod]
        public void RemoveNode_RecursiveDeletionWithSymbolicLink_RemovesDirectoryAndLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/nested", true);
            vs.AddSymbolicLink("/test/nested/link", "/target/path");

            // Act
            vs.RemoveNode("/test", true);

            // Assert
            Assert.IsFalse(vs.NodeExists("/test"));
            Assert.IsFalse(vs.NodeExists("/test/nested"));
            Assert.IsFalse(vs.NodeExists("/test/nested/link"));
        }

        [TestMethod]
        public void RemoveNode_SymbolicLinkTargetDeletion_RemovesTargetButNotLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/target/path", true);
            vs.AddDirectory("/test");
            vs.AddSymbolicLink("/test/link", "/target/path");

            // Act
            vs.RemoveNode("/target/path");

            // Assert
            Assert.IsFalse(vs.NodeExists("/target/path"));
            Assert.IsTrue(vs.SymbolicLinkExists("/test/link"));
        }

        [TestMethod]
        public void RemoveNode_NonRecursiveDeletionWithSymbolicLink_ThrowsException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            vs.AddSymbolicLink("/test/link", "/target/path");

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.RemoveNode("/test"));
        }

        [TestMethod]
        public void RemoveNode_RecursiveDeletionWithSymbolicLinkFollowLinksFalse_RemovesDirectoryAndLinkOnly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/nested", true);
            vs.AddDirectory("/target");
            BinaryData targetItem = [ 1, 2, 3 ];
            vs.AddItem("/target/targetItem", targetItem);
            vs.AddSymbolicLink("/test/nested/link", "/target");

            // Act
            vs.RemoveNode("/test", true, false);

            // Assert
            Assert.IsFalse(vs.NodeExists("/test"));
            Assert.IsFalse(vs.NodeExists("/test/nested"));
            Assert.IsFalse(vs.NodeExists("/test/nested/link"));
            Assert.IsTrue(vs.NodeExists("/target")); // ターゲットディレクトリは削除されない
            Assert.IsTrue(vs.NodeExists("/target/targetItem")); // ターゲットディレクトリ内のアイテムも削除されない
        }

        [TestMethod]
        public void RemoveNode_RecursiveDeletionWithSymbolicLinkFollowLinksTrue_RemovesDirectoryAndTarget()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/nested", true);
            vs.AddDirectory("/target");
            BinaryData targetItem = [ 1, 2, 3 ];
            vs.AddItem("/target/targetItem", targetItem);
            vs.AddSymbolicLink("/test/nested/link", "/target");

            // Act
            vs.RemoveNode("/test", true, true);

            // Assert
            Assert.IsFalse(vs.NodeExists("/test"));
            Assert.IsFalse(vs.NodeExists("/test/nested"));
            Assert.IsFalse(vs.NodeExists("/test/nested/link"));
            Assert.IsFalse(vs.NodeExists("/target")); // ターゲットディレクトリも削除される
            Assert.IsFalse(vs.NodeExists("/target/targetItem")); // ターゲットディレクトリ内のアイテムも削除される
        }

        [TestMethod]
        public void RemoveNode_RecursiveDeletionWithSymbolicLinkToFileFollowLinksFalse_RemovesDirectoryAndLinkOnly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/nested", true);
            BinaryData targetItem = [ 1, 2, 3 ];
            vs.AddItem("/targetItem", targetItem);
            vs.AddSymbolicLink("/test/nested/linkToFile", "/targetItem");

            // Act
            vs.RemoveNode("/test", true, false);

            // Assert
            Assert.IsFalse(vs.NodeExists("/test"));
            Assert.IsFalse(vs.NodeExists("/test/nested"));
            Assert.IsFalse(vs.NodeExists("/test/nested/linkToFile"));
            Assert.IsTrue(vs.NodeExists("/targetItem")); // ターゲットアイテムは削除されない
        }

        [TestMethod]
        public void RemoveNode_RecursiveDeletionWithSymbolicLinkToFileFollowLinksTrue_RemovesDirectoryAndTargetItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/nested", true);
            BinaryData targetItem = [1, 2, 3];
            vs.AddItem("/targetItem", targetItem);
            vs.AddSymbolicLink("/test/nested/linkToFile", "/targetItem");

            // Act
            vs.RemoveNode("/test", true, true);

            // Assert
            Assert.IsFalse(vs.NodeExists("/test"));
            Assert.IsFalse(vs.NodeExists("/test/nested"));
            Assert.IsFalse(vs.NodeExists("/test/nested/linkToFile"));
            Assert.IsFalse(vs.NodeExists("/targetItem")); // ターゲットアイテムも削除される
        }

        [TestMethod]
        public void RemoveNode_RecursiveDeletionWithSymbolicLinkToFileFollowLinksTrue_RemovesDirectoryAndTargetItem_Part2()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test/nested", true);
            BinaryData targetItem = [1, 2, 3];
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/targetItem", targetItem);
            vs.AddSymbolicLink("/test/nested/linkToFile", "/dir1/targetItem");

            // Act
            vs.RemoveNode("/test", true, true);

            // Assert
            Assert.IsFalse(vs.NodeExists("/test"));
            Assert.IsFalse(vs.NodeExists("/test/nested"));
            Assert.IsFalse(vs.NodeExists("/test/nested/linkToFile"));
            Assert.IsFalse(vs.NodeExists("/dir1/targetItem")); // ターゲットアイテムも削除される
            Assert.IsTrue(vs.NodeExists("/dir1"));
        }

        [TestMethod]
        public void RemoveNode_ExistingItem_DisposesItem()
        {
            // Arrange
            string itemName = "TestItem";
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];
            VirtualItem<BinaryData> item = new(itemName, data);
            vs.AddItem("/", item);

            // Act
            vs.RemoveNode("/" + itemName);

            // Assert
            // BinaryDataのDisposeメソッドが呼び出されたかどうかを確認
            Assert.IsTrue(item.ItemData!.Count == 0, "Item data should be cleared on Dispose.");
            Assert.IsFalse(vs.ItemExists("/" + itemName));
        }

        [TestMethod]
        public void RemoveNode_ExistingDirectoryWithDisposableItems_DisposesItems()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/TestDirectory");
            BinaryData data1 = [1, 2, 3];
            BinaryData data2 = [4, 5, 6];
            VirtualItem<BinaryData> item1 = new("Item1", data1);
            VirtualItem<BinaryData> item2 = new("Item2", data2);
            vs.AddItem("/TestDirectory", item1);
            vs.AddItem("/TestDirectory", item2);

            // Act
            vs.RemoveNode("/TestDirectory", true);

            // Assert
            Assert.IsTrue(item1.ItemData!.Count == 0, "Item1 data should be cleared on Dispose.");
            Assert.IsTrue(item2.ItemData!.Count == 0, "Item2 data should be cleared on Dispose.");
            Assert.IsFalse(vs.DirectoryExists("/TestDirectory"));
        }

        [TestMethod]
        public void RemoveNode_SymbolicLink_DisposesLinkTarget()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            BinaryData targetData = [1, 2, 3];
            VirtualItem<BinaryData> targetItem = new("TargetItem", targetData);
            vs.AddDirectory("/target/path", true);
            vs.AddItem("/target/path", targetItem);
            vs.AddSymbolicLink("/test/link", "/target/path");

            // Act
            vs.RemoveNode("/test/link", true, true);

            // Assert
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure(VirtualPath.Root, true, false));
            Assert.IsTrue(targetItem.ItemData!.Count == 0, "Target item data should be cleared on Dispose.");
            Assert.IsFalse(vs.NodeExists("/test/link"));
            Assert.IsFalse(vs.NodeExists("/target/path"));
            Assert.IsTrue(vs.NodeExists("/test"));
            Assert.IsTrue(vs.NodeExists("/target"));
        }

        [TestMethod]
        public void TryGetNode_ReturnsNode_WhenNodeExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/path";
            vs.AddDirectory(path, true);

            // Act
            VirtualNode? node = vs.TryGetNode(path);

            // Assert
            Assert.IsNotNull(node);
        }

        [TestMethod]
        public void TryGetNode_ReturnsNull_WhenNodeDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/non/existing/path";

            // Act
            VirtualNode? node = vs.TryGetNode(path);

            // Assert
            Assert.IsNull(node);
        }

        [TestMethod]
        public void TryGetDirectory_ReturnsDirectory_WhenDirectoryExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/directory";
            vs.AddDirectory(path, true);

            // Act
            VirtualDirectory? directory = vs.TryGetDirectory(path);

            // Assert
            Assert.IsNotNull(directory);
        }

        [TestMethod]
        public void TryGetDirectory_ReturnsNull_WhenDirectoryDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/non/existing/directory";

            // Act
            VirtualDirectory? directory = vs.TryGetDirectory(path);

            // Assert
            Assert.IsNull(directory);
        }

        [TestMethod]
        public void TryGetItem_ReturnsItem_WhenItemExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/item";
            BinaryData data = new([1, 2, 3]);
            vs.AddDirectory(path.DirectoryPath, true);
            vs.AddItem(path, data);

            // Act
            VirtualItem<BinaryData>? item = vs.TryGetItem(path);

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(data, item.ItemData);
        }

        [TestMethod]
        public void TryGetItem_ReturnsNull_WhenItemDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/non/existing/item";

            // Act
            VirtualItem<BinaryData>? item = vs.TryGetItem(path);

            // Assert
            Assert.IsNull(item);
        }

        [TestMethod]
        public void TryGetSymbolicLink_ReturnsLink_WhenLinkExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/target/path";
            VirtualPath linkPath = "/existing/link";
            vs.AddDirectory(targetPath, true);
            vs.AddDirectory(linkPath.DirectoryPath, true);
            vs.AddSymbolicLink(linkPath, targetPath);

            // Act
            VirtualSymbolicLink? link = vs.TryGetSymbolicLink(linkPath);

            // Assert
            Assert.IsNotNull(link);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void TryGetSymbolicLink_ReturnsNull_WhenLinkDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath linkPath = "/non/existing/link";

            // Act
            VirtualSymbolicLink? link = vs.TryGetSymbolicLink(linkPath);

            // Assert
            Assert.IsNull(link);
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenNodeExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/path";
            vs.AddDirectory(path, true);

            // Act
            bool exists = vs.NodeExists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void NodeExists_ReturnsFalse_WhenNodeDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/non/existing/path";

            // Act
            bool exists = vs.NodeExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenSymbolicLinkExistsAndFollowLinksIsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/existing/directory";
            VirtualPath linkDirectoryPath = "/link/to";
            VirtualPath linkPath = linkDirectoryPath + "directory";
            vs.AddDirectory(directoryPath, true); // 実際のディレクトリを追加
            vs.AddDirectory(linkDirectoryPath, true); // シンボリックリンクの親ディレクトリを追加
            vs.AddSymbolicLink(linkPath, directoryPath); // シンボリックリンクを追加

            // Act
            bool exists = vs.NodeExists(linkPath, true); // シンボリックリンクの追跡を有効に

            // Assert
            Assert.IsTrue(exists); // シンボリックリンクを追跡し、実際のディレクトリが存在することを確認
        }

        [TestMethod]
        public void NodeExists_ReturnsTrue_WhenSymbolicLinkExistsAndFollowLinksIsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/existing/directory";
            VirtualPath linkDirectoryPath = "/link/to";
            VirtualPath linkPath = linkDirectoryPath + "directory";
            vs.AddDirectory(directoryPath, true); // 実際のディレクトリを追加
            vs.AddDirectory(linkDirectoryPath, true); // シンボリックリンクの親ディレクトリを追加
            vs.AddSymbolicLink(linkPath, directoryPath); // シンボリックリンクを追加

            // Act
            bool exists = vs.NodeExists(linkPath, false); // シンボリックリンクの追跡を無効に

            // Assert
            Assert.IsTrue(exists); // シンボリックリンク自体の存在を確認
        }

        [TestMethod]
        public void NodeExists_ReturnsFalse_WhenTargetOfSymbolicLinkDoesNotExistAndFollowLinksIsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath nonExistentTargetPath = "/nonexistent/target";
            VirtualPath linkDirectoryPath = "/link/to";
            VirtualPath linkPath = linkDirectoryPath + "nonexistent";
            vs.AddDirectory(linkDirectoryPath, true); // シンボリックリンクの親ディレクトリを追加
            vs.AddSymbolicLink(linkPath, nonExistentTargetPath); // 存在しないターゲットへのシンボリックリンクを追加

            // Act
            bool exists = vs.NodeExists(linkPath, true); // シンボリックリンクの追跡を有効に

            // Assert
            Assert.IsFalse(exists); // シンボリックリンクのターゲットが存在しないため、falseを返す
        }

        [TestMethod]
        public void DirectoryExists_ReturnsTrue_WhenDirectoryExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/path";
            vs.AddDirectory(path, true);

            // Act
            bool exists = vs.DirectoryExists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DirectoryExists_ReturnsFalse_WhenDirectoryDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/non/existing/path";

            // Act
            bool exists = vs.DirectoryExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsTrue_WhenItemExists()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/path/item";
            vs.AddDirectory("/existing/path", true);
            vs.AddItem("/existing/path/item", new BinaryData([1, 2, 3]));

            // Act
            bool exists = vs.ItemExists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsFalse_WhenItemDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/non/existing/path/item";

            // Act
            bool exists = vs.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ItemExists_ReturnsFalse_WhenPathIsDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/existing/path";
            vs.AddDirectory(path, true);

            // Act
            bool exists = vs.ItemExists(path);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void SetNodeName_ChangesItemNameSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testFile", new BinaryData([1, 2, 3]));

            vs.SetNodeName("/testFile", "newTestFile");

            Assert.IsFalse(vs.NodeExists("/testFile"));
            Assert.IsTrue(vs.NodeExists("/newTestFile"));
        }

        [TestMethod]
        public void SetNodeName_ChangesDirectoryNameSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/testDir");
            vs.AddItem("/testDir/item1", new BinaryData([1, 2, 3]));

            vs.SetNodeName("/testDir", "newTestDir");

            Assert.IsFalse(vs.NodeExists("/testDir"));
            Assert.IsTrue(vs.NodeExists("/newTestDir"));
            Assert.IsTrue(vs.NodeExists("/newTestDir/item1"));
        }

        [TestMethod]
        public void SetNodeName_ChangesSymbolicLinkNameSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/target");
            vs.AddSymbolicLink("/link", "/target");

            vs.SetNodeName("/link", "newLink");

            Assert.IsFalse(vs.NodeExists("/link"));
            Assert.IsTrue(vs.NodeExists("/newLink"));

            // リンク辞書の確認
            var links = vs.GetLinksFromDictionary("/target");
            Assert.IsTrue(links.Contains("/newLink"));
            Assert.IsFalse(links.Contains("/link"));
        }

        [TestMethod]
        public void SetNodeName_ThrowsWhenNodeDoesNotExist()
        {
            VirtualStorage<BinaryData> vs = new();

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.SetNodeName("/nonExistentFile", "newName"));
        }

        [TestMethod]
        public void SetNodeName_ThrowsWhenNewNameAlreadyExists()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testFile", new BinaryData([1, 2, 3]));
            vs.AddItem("/newTestFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.SetNodeName("/testFile", "newTestFile"));
        }

        [TestMethod]
        public void SetNodeName_ThrowsWhenNewNameIsSameAsOldName()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testFile", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.SetNodeName("/testFile", "testFile"));
        }

        [TestMethod]
        public void SetNodeName_ThrowsWhenNewNameIsDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testFile", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/newTestFile");

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.SetNodeName("/testFile", "newTestFile"));
        }

        [TestMethod]
        public void SetNodeName_UpdatesSymbolicLinkWhenTargetIsMissing()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/link", "/missingTarget");

            vs.SetNodeName("/link", "newLink");

            Assert.IsFalse(vs.NodeExists("/link"));
            Assert.IsTrue(vs.NodeExists("/newLink"));

            // リンク辞書の確認
            var links = vs.GetLinksFromDictionary("/missingTarget");
            Assert.IsTrue(links.Contains("/newLink"));
            Assert.IsFalse(links.Contains("/link"));
        }

        [TestMethod]
        public void SetNodeName_UpdatesLinksToTargetForItem()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath itemPath = "/dir1/item1";
            VirtualPath newItemPath = "/dir1/newItem1";
            VirtualPath linkPath = "/dir1/linkToItem1";

            // アイテムとシンボリックリンクの追加
            vs.AddDirectory("/dir1", true);
            BinaryData data = [1, 2, 3];
            vs.AddItem(itemPath, data);
            vs.AddSymbolicLink(linkPath, itemPath);

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary(itemPath).Contains(linkPath));

            // デバッグ出力
            DebugPrintLinkDictionary(vs);

            // アイテムの名前を変更
            vs.SetNodeName(itemPath, new VirtualNodeName("newItem1"));

            // リンク辞書の更新を確認
            Assert.IsFalse(vs.GetLinksFromDictionary(itemPath).Contains(linkPath));
            Assert.IsTrue(vs.GetLinksFromDictionary(newItemPath).Contains(linkPath));

            // デバッグ出力
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        public void SetNodeName_UpdatesLinksToTargetForDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath dirPath = "/dir1";
            VirtualPath newDirPath = "/newDir1";
            VirtualPath linkPath = "/linkToDir1";
            VirtualPath itemPath = "/dir1/item1";

            // ディレクトリとシンボリックリンク、アイテムの追加
            vs.AddDirectory(dirPath, true);
            BinaryData data = [1, 2, 3];
            vs.AddItem(itemPath, data);
            vs.AddSymbolicLink(linkPath, dirPath);

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary(dirPath).Contains(linkPath));

            // デバッグ出力
            DebugPrintLinkDictionary(vs);

            // ディレクトリの名前を変更
            vs.SetNodeName(dirPath, new VirtualNodeName("newDir1"));

            // リンク辞書の更新を確認
            Assert.IsFalse(vs.GetLinksFromDictionary(dirPath).Contains(linkPath));
            Assert.IsTrue(vs.GetLinksFromDictionary(newDirPath).Contains(linkPath));

            // 配下のアイテムが存在することを確認
            Assert.IsTrue(vs.NodeExists(newDirPath + "/item1"));

            // デバッグ出力
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        public void SetNodeName_UpdatesLinkNameForSymbolicLink()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetLinkPath = "/dir1/targetLink";
            VirtualPath linkPath = "/dir1/linkToTargetLink";
            VirtualPath newLinkPath = "/dir1/newLinkToTargetLink";

            // ディレクトリとシンボリックリンクの追加
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink(targetLinkPath, "/dir1"); // targetLinkは dir1 を指す
            vs.AddSymbolicLink(linkPath, targetLinkPath); // linkToTargetLinkは targetLink を指す

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary(targetLinkPath).Contains(linkPath));

            // デバッグ出力
            DebugPrintLinkDictionary(vs);

            // シンボリックリンクの名前を変更
            vs.SetNodeName(linkPath, new VirtualNodeName("newLinkToTargetLink"));

            // リンク辞書の更新を確認
            Assert.IsFalse(vs.GetLinksFromDictionary(targetLinkPath).Contains(linkPath));
            Assert.IsTrue(vs.GetLinksFromDictionary(targetLinkPath).Contains(newLinkPath));

            // デバッグ出力
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        public void MoveNode_FileToFile_OverwritesWhenAllowed()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            vs.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            vs.MoveNode("/sourceFile", "/destinationFile", true);

            Assert.IsFalse(vs.NodeExists("/sourceFile"));
            VirtualItem<BinaryData> destinationItem = (VirtualItem<BinaryData>)vs.GetNode("/destinationFile");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, destinationItem.ItemData!.Data);
        }

        [TestMethod]
        public void MoveNode_FileToFile_ThrowsWhenOverwriteNotAllowed()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            vs.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/sourceFile", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_FileToDirectory_MovesFileToTargetDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/destination");
            vs.AddItem("/sourceFile", new BinaryData([1, 2, 3]));

            vs.MoveNode("/sourceFile", "/destination/", false);

            Assert.IsFalse(vs.NodeExists("/sourceFile"));
            Assert.IsTrue(vs.NodeExists("/destination/sourceFile"));
        }

        [TestMethod]
        public void MoveNode_DirectoryToDirectory_MovesDirectoryToTargetDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir");
            vs.AddDirectory("/destinationDir/newDir", true);

            vs.MoveNode("/sourceDir", "/destinationDir/newDir", false);

            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/newDir"));
        }

        [TestMethod]
        public void MoveNode_WhenSourceDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/destinationDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.MoveNode("/nonExistentSource", "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_WhenDestinationIsInvalid_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.MoveNode("/sourceDir", "/nonExistentDestination/newDir", false));
        }

        [TestMethod]
        public void MoveNode_DirectoryWithSameNameExistsAtDestination_ThrowsExceptionRegardlessOfOverwriteFlag()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/destinationDir/sourceDir", true);

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/sourceDir", "/destinationDir", false));
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/sourceDir", "/destinationDir", true));
        }

        [TestMethod]
        public void MoveNode_DirectoryToFile_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir");
            vs.AddItem("/destinationFile", new BinaryData([4, 5, 6]));

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/sourceDir", "/destinationFile", false));
        }

        [TestMethod]
        public void MoveNode_RootDirectory_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/destinationDir");

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode(VirtualPath.Root, "/destinationDir", false));
        }

        [TestMethod]
        public void MoveNode_OverwritesExistingNodeInDestinationWhenAllowed()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir");
            vs.AddItem("/sourceDir/fileName", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/destinationDir");
            vs.AddItem("/destinationDir/fileName", new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            vs.MoveNode("/sourceDir/fileName", "/destinationDir", true); // 上書き許可で移動

            Assert.IsFalse(vs.NodeExists("/sourceDir/fileName")); // 元のファイルが存在しないことを確認
            Assert.IsTrue(vs.NodeExists("/destinationDir/fileName")); // 移動先にファイルが存在することを確認

            VirtualItem<BinaryData> movedItem = (VirtualItem<BinaryData>)vs.GetNode("/destinationDir/fileName");
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, movedItem.ItemData!.Data); // 移動先のファイルの中身が正しいことを確認
        }

        [TestMethod]
        public void MoveNode_ThrowsWhenDestinationNodeExistsAndOverwriteIsFalse()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir");
            vs.AddItem("/sourceDir/fileName", new BinaryData([1, 2, 3]));
            vs.AddDirectory("/destinationDir");
            vs.AddItem("/destinationDir/fileName", new BinaryData([4, 5, 6])); // 移動先に同名のファイルが存在

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/sourceDir/fileName", "/destinationDir", false)); // 上書き禁止で例外を期待
        }

        [TestMethod]
        public void MoveNode_EmptyDirectory_MovesSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/emptyDir");
            vs.AddDirectory("/newDir/emptyDir", true);
            vs.MoveNode("/emptyDir", "/newDir/emptyDir");

            Assert.IsFalse(vs.NodeExists("/emptyDir"));
            Assert.IsTrue(vs.NodeExists("/newDir/emptyDir/emptyDir"));
        }

        [TestMethod]
        public void MoveNode_MultiLevelDirectory_MovesSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir/subDir1/subDir2", true);
            vs.AddDirectory("/destinationDir");
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/subDir2"));
        }

        [TestMethod]
        public void MoveNode_WithInvalidPath_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/validDir");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.MoveNode("/invalid?Path", "/validDir"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MoveNode_DirectoryToFile_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/sourceDir");
            vs.AddItem("/destinationFile", new BinaryData([1, 2, 3]));
            vs.MoveNode("/sourceDir", "/destinationFile");
        }

        [TestMethod]
        public void MoveNode_WithinSameDirectory_RenamesNode()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/sourceFile", new BinaryData([1, 2, 3]));
            vs.MoveNode("/sourceFile", "/renamedFile");

            Assert.IsFalse(vs.NodeExists("/sourceFile"));
            Assert.IsTrue(vs.NodeExists("/renamedFile"));

            byte[] result = ((VirtualItem<BinaryData>)vs.GetNode("/renamedFile")).ItemData!.Data;
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result);
        }

        // 循環参照チェックテスト
        [TestMethod]
        public void MoveNode_WhenDestinationIsSubdirectoryOfSource_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/parentDir/subDir", true);

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/parentDir", "/parentDir/subDir"));
        }

        // 移動先と移動元が同じかどうかのチェックテスト
        [TestMethod]
        public void MoveNode_WhenSourceAndDestinationAreSame_ThrowsInvalidOperationException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/file", "/file"));
        }

        // 移動先の親ディレクトリが存在しない場合のテスト
        [TestMethod]
        public void MoveNode_WhenDestinationParentDirectoryDoesNotExist_ThrowsVirtualNodeNotFoundException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/existingDir");
            vs.AddItem("/existingDir/file", new BinaryData([1, 2, 3]));

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.MoveNode("/existingDir/file", "/nonExistentDir/file"));
        }

        [TestMethod]
        public void MoveNode_SymbolicLinkToDirectory_MovesLinkToTargetDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/destination");
            vs.AddSymbolicLink("/sourceLink", "/target/path");

            vs.MoveNode("/sourceLink", "/destination/", false);

            Assert.IsFalse(vs.NodeExists("/sourceLink"));
            Assert.IsTrue(vs.NodeExists("/destination/sourceLink"));
            VirtualSymbolicLink movedLink = vs.GetSymbolicLink("/destination/sourceLink");
            Assert.AreEqual("/target/path", (string)movedLink.TargetPath);
        }

        [TestMethod]
        public void MoveNode_SymbolicLinkToExistingLink_OverwritesWhenAllowed()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/sourceLink", "/target/path");
            vs.AddSymbolicLink("/destinationLink", "/other/target");

            vs.MoveNode("/sourceLink", "/destinationLink", true);

            Assert.IsFalse(vs.NodeExists("/sourceLink"));
            VirtualSymbolicLink destinationLink = (VirtualSymbolicLink)vs.GetNode("/destinationLink");
            Assert.AreEqual("/target/path", (string)destinationLink.TargetPath);
        }

        [TestMethod]
        public void MoveNode_SymbolicLinkToExistingLink_ThrowsWhenOverwriteNotAllowed()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/sourceLink", "/target/path");
            vs.AddSymbolicLink("/destinationLink", "/other/target");

            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.MoveNode("/sourceLink", "/destinationLink", false));
        }

        [TestMethod]
        public void MoveNode_SymbolicLinkWithNonExistentTarget_MovesLink()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddSymbolicLink("/sourceLink", "/nonExistent/target");

            vs.MoveNode("/sourceLink", "/destinationLink", false);

            Assert.IsFalse(vs.NodeExists("/sourceLink"));
            Assert.IsTrue(vs.NodeExists("/destinationLink"));
            VirtualSymbolicLink movedLink = (VirtualSymbolicLink)vs.GetNode("/destinationLink");
            Assert.AreEqual("/nonExistent/target", (string)movedLink.TargetPath);
        }

        // アイテムのパス変更時のリンク辞書の更新を確認
        [TestMethod]
        public void MoveNode_ChangesItemPathSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testItem", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/linkToItem", "/testItem");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);

            vs.MoveNode("/testItem", "/newTestItem");

            Assert.IsFalse(vs.NodeExists("/testItem"));
            Assert.IsTrue(vs.NodeExists("/newTestItem"));

            // リンク辞書の更新を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/newTestItem").Contains("/linkToItem"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);
        }

        // シンボリックリンクのパス変更時のリンク辞書の更新を確認
        [TestMethod]
        public void MoveNode_ChangesSymbolicLinkPathSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/target");
            vs.AddSymbolicLink("/link", "/target");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/target").Contains("/link"));
            DebugPrintLinkDictionary(vs);

            vs.MoveNode("/link", "/newLink");

            Assert.IsFalse(vs.NodeExists("/link"));
            Assert.IsTrue(vs.NodeExists("/newLink"));

            // リンク辞書の更新を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/target").Contains("/newLink"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/target").Contains("/link"));
            DebugPrintLinkDictionary(vs);
        }

        // 存在しないノードの移動時に例外をスロー
        [TestMethod]
        public void MoveNode_ThrowsWhenNodeDoesNotExist()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/existingItem", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/linkToItem", "/existingItem");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/existingItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.MoveNode("/nonExistentItem", "/newName"));

            // リンク辞書が変更されていないことを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/existingItem").Contains("/linkToItem"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/newName").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);
        }

        // 移動先に既にノードが存在する場合に例外をスロー
        [TestMethod]
        public void MoveNode_ThrowsWhenDestinationAlreadyExists()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testItem", new BinaryData([1, 2, 3]));
            vs.AddItem("/newTestItem", new BinaryData([4, 5, 6]));
            vs.AddSymbolicLink("/linkToItem", "/testItem");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);

            Assert.ThrowsException<InvalidOperationException>(() => vs.MoveNode("/testItem", "/newTestItem"));

            // リンク辞書が変更されていないことを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/newTestItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);
        }

        // 移動元と移動先が同じ場合に例外をスロー
        [TestMethod]
        public void MoveNode_ThrowsWhenDestinationIsSameAsSource()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddItem("/testItem", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/linkToItem", "/testItem");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);

            Assert.ThrowsException<InvalidOperationException>(() => vs.MoveNode("/testItem", "/testItem"));

            // リンク辞書が変更されていないことを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);
        }

        // アイテムをディレクトリに移動した際のリンク辞書の更新を確認
        [TestMethod]
        public void MoveNode_MovesItemToDirectorySuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/destinationDir");
            vs.AddItem("/testItem", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/linkToItem", "/testItem");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);

            vs.MoveNode("/testItem", "/destinationDir/testItem");

            Assert.IsFalse(vs.NodeExists("/testItem"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/testItem"));

            // リンク辞書の更新を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/testItem").Contains("/linkToItem"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/testItem").Contains("/linkToItem"));
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        // 単純なディレクトリの移動
        public void MoveDirectoryInternal_SimpleMoveUpdatesLinksCorrectly()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリを作成
            vs.AddDirectory("/sourceDir", true);

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir", true);

            // ソースディレクトリに対してシンボリックリンクを追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリをディスティネーションディレクトリに移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        // 多階層ディレクトリの移動
        public void MoveDirectoryInternal_MultiLevelMoveUpdatesLinksCorrectly()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリを作成
            vs.AddDirectory("/sourceDir/subDir1/subDir2", true);

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir", true);

            // ソースディレクトリとそのサブディレクトリに対してシンボリックリンクを追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");
            vs.AddSymbolicLink("/linkToSubDir1", "/sourceDir/subDir1");
            vs.AddSymbolicLink("/linkToSubDir2", "/sourceDir/subDir1/subDir2");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/subDir2").Contains("/linkToSubDir2"));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリをディスティネーションディレクトリに移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/subDir2"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1/subDir2").Contains("/linkToSubDir2"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1/subDir2").Contains("/linkToSubDir2"));
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        // 移動先が既存のディレクトリ
        public void MoveDirectoryInternal_MoveToExistingDirectoryUpdatesLinksCorrectly()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddDirectory("/sourceDir/subDir2", true);

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir/existingDir", true);

            // ソースディレクトリとそのサブディレクトリに対してシンボリックリンクを追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");
            vs.AddSymbolicLink("/linkToSubDir1", "/sourceDir/subDir1");
            vs.AddSymbolicLink("/linkToSubDir2", "/sourceDir/subDir2");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir2").Contains("/linkToSubDir2"));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリをディスティネーションディレクトリ内の既存のディレクトリに移動
            vs.MoveNode("/sourceDir", "/destinationDir/existingDir");

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/existingDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/existingDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/existingDir/sourceDir/subDir2"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/existingDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/existingDir/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/existingDir/sourceDir/subDir2").Contains("/linkToSubDir2"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir2").Contains("/linkToSubDir2"));
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        // 移動先に同名のディレクトリが存在する場合
        public void MoveDirectoryInternal_MovesToSubdirectoryWhenDestinationDirectoryAlreadyExists()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddDirectory("/sourceDir/subDir2", true);

            // ディスティネーションディレクトリと同名のディレクトリを作成
            vs.AddDirectory("/destinationDir/sourceDir", true);

            // ソースディレクトリとそのサブディレクトリに対してシンボリックリンクをルートに追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");
            vs.AddSymbolicLink("/linkToSubDir1", "/sourceDir/subDir1");
            vs.AddSymbolicLink("/linkToSubDir2", "/sourceDir/subDir2");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir2").Contains("/linkToSubDir2"));

            // デバッグ出力
            Debug.WriteLine("MoveNode前:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリを移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // デバッグ出力
            Debug.WriteLine("\nMoveNode後:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/sourceDir/subDir2"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/sourceDir/subDir2").Contains("/linkToSubDir2"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir2").Contains("/linkToSubDir2"));
        }

        [TestMethod]
        // シンボリックリンクを含むディレクトリの移動
        public void MoveDirectoryInternal_UpdatesLinksCorrectlyWhenSymbolicLinkIncludedInDirectory()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリおよびアイテムを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddItem("/sourceDir/subDir1/item1", new BinaryData([1, 2, 3]));
            vs.AddSymbolicLink("/sourceDir/subDir1/linkToItem1", "/sourceDir/subDir1/item1");

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir", true);

            // ソースディレクトリおよびそのサブディレクトリに対してシンボリックリンクをルートに追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");
            vs.AddSymbolicLink("/LinkToLink", "/linkToSourceDir");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/sourceDir/subDir1/linkToItem1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/linkToSourceDir").Contains("/LinkToLink"));

            // デバッグ出力
            Debug.WriteLine("MoveNode前:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリを移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // デバッグ出力
            Debug.WriteLine("\nMoveNode後:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/item1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/linkToItem1"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1/item1").Contains("/destinationDir/sourceDir/subDir1/linkToItem1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/linkToSourceDir").Contains("/LinkToLink"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/sourceDir/subDir1/linkToItem1"));
        }

        [TestMethod]
        // ターゲットディレクトリのシンボリックリンク更新
        public void MoveDirectoryInternal_UpdatesLinksCorrectlyWhenTargetDirectoryIsSymbolicLink()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリおよびアイテムを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddItem("/sourceDir/subDir1/item1", new BinaryData([1, 2, 3]));

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir", true);

            // /sourceDirをターゲットとするシンボリックリンクをルートに追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");

            // /sourceDir/subDir1/item1をターゲットとするシンボリックリンクを追加
            vs.AddSymbolicLink("/linkToItem1", "/sourceDir/subDir1/item1");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/linkToItem1"));

            // デバッグ出力
            Debug.WriteLine("MoveNode前:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリを移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // デバッグ出力
            Debug.WriteLine("\nMoveNode後:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/item1"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1/item1").Contains("/linkToItem1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/linkToItem1"));
        }

        [TestMethod]
        // 親ディレクトリが存在しない場合の例外
        public void MoveDirectoryInternal_ThrowsExceptionWhenParentDirectoryDoesNotExist()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリおよびアイテムを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddItem("/sourceDir/subDir1/item1", new BinaryData([1, 2, 3]));

            // ソースディレクトリおよびそのサブディレクトリに対してシンボリックリンクをルートに追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");
            vs.AddSymbolicLink("/linkToSubDir1", "/sourceDir/subDir1");
            vs.AddSymbolicLink("/linkToItem1", "/sourceDir/subDir1/item1");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/linkToItem1"));

            // デバッグ出力
            Debug.WriteLine("MoveNode前:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // 親ディレクトリが存在しないため例外がスローされることを確認
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.MoveNode("/sourceDir", "/nonExistentDir/newDir"));

            // デバッグ出力
            Debug.WriteLine("\nMoveNode後:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ノードが移動されていないことを確認
            Assert.IsTrue(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/sourceDir/subDir1/item1"));

            // リンク辞書が変更されていないことを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/linkToItem1"));
        }


        [TestMethod]
        // ディレクトリとアイテムが混在するディレクトリの移動
        public void MoveDirectoryInternal_MovesDirectoryWithMixedContentCorrectly()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリおよびアイテムを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddItem("/sourceDir/item1", new BinaryData([1, 2, 3]));
            vs.AddItem("/sourceDir/subDir1/item2", new BinaryData([4, 5, 6]));

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir", true);

            // ソースディレクトリおよびそのサブディレクトリ、アイテムに対してシンボリックリンクをルートに追加
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");
            vs.AddSymbolicLink("/linkToSubDir1", "/sourceDir/subDir1");
            vs.AddSymbolicLink("/linkToItem1", "/sourceDir/item1");
            vs.AddSymbolicLink("/linkToItem2", "/sourceDir/subDir1/item2");

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/item1").Contains("/linkToItem1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/item2").Contains("/linkToItem2"));

            // デバッグ出力
            Debug.WriteLine("MoveNode前:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリを移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // デバッグ出力
            Debug.WriteLine("\nMoveNode後:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/item1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/item2"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/item1").Contains("/linkToItem1"));
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1/item2").Contains("/linkToItem2"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir").Contains("/linkToSourceDir"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1").Contains("/linkToSubDir1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/item1").Contains("/linkToItem1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1/item2").Contains("/linkToItem2"));
        }

        [TestMethod]
        // ネストされたシンボリックリンクを含むディレクトリの移動
        public void MoveDirectoryInternal_MovesDirectoryWithNestedSymbolicLinksCorrectly()
        {
            // 仮想ストレージを初期化
            VirtualStorage<BinaryData> vs = new();

            // ソースディレクトリとそのサブディレクトリおよびアイテムを作成
            vs.AddDirectory("/sourceDir", true);
            vs.AddDirectory("/sourceDir/subDir1", true);
            vs.AddItem("/sourceDir/subDir1/item1", new BinaryData([1, 2, 3]));

            // サブディレクトリ内にシンボリックリンクを作成
            vs.AddSymbolicLink("/sourceDir/subDir1/linkToItem1", "/sourceDir/subDir1/item1");

            // ディスティネーションディレクトリを作成
            vs.AddDirectory("/destinationDir", true);

            // リンク辞書の初期状態を確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/sourceDir/subDir1/linkToItem1"));

            // デバッグ出力
            Debug.WriteLine("MoveNode前:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ソースディレクトリを移動
            vs.MoveNode("/sourceDir", "/destinationDir/sourceDir");

            // デバッグ出力
            Debug.WriteLine("\nMoveNode後:");
            Debug.WriteLine(vs.GenerateTextBasedTreeStructure("/", true, false));
            DebugPrintLinkDictionary(vs);

            // ノードが適切に移動されたことを確認
            Assert.IsFalse(vs.NodeExists("/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/item1"));
            Assert.IsTrue(vs.NodeExists("/destinationDir/sourceDir/subDir1/linkToItem1"));

            // リンク辞書が更新されていることを確認
            Assert.IsTrue(vs.GetLinksFromDictionary("/destinationDir/sourceDir/subDir1/item1").Contains("/destinationDir/sourceDir/subDir1/linkToItem1"));
            Assert.IsFalse(vs.GetLinksFromDictionary("/sourceDir/subDir1/item1").Contains("/sourceDir/subDir1/linkToItem1"));
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            vs.AddSymbolicLink("/test/link", "/target/path");

            // Act
            bool exists = vs.SymbolicLinkExists("/test/link");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenLinkDoesNotExist_ReturnsFalse()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");

            // Act
            bool exists = vs.SymbolicLinkExists("/test/nonexistentLink");

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void SymbolicLinkExists_WhenParentDirectoryIsALinkAndLinkExists_ReturnsTrue()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/realParent");
            vs.AddSymbolicLink("/linkedParent", "/realParent");
            vs.AddDirectory("/realParent/testDir");
            vs.AddSymbolicLink("/linkedParent/myLink", "/realParent/testDir");

            // Act
            bool exists = vs.SymbolicLinkExists("/linkedParent/myLink");

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void AddSymbolicLink_WhenLinkIsNew_AddsSuccessfully()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");

            // Act
            vs.AddSymbolicLink("/test/newLink", "/target/path");

            // Assert
            Assert.IsTrue(vs.SymbolicLinkExists("/test/newLink"));
            VirtualSymbolicLink? link = vs.GetNode("/test/newLink") as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.IsTrue(link.TargetPath == "/target/path");
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsFalseAndLinkExists_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            vs.AddSymbolicLink("/test/existingLink", "/old/target/path");

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddSymbolicLink("/test/existingLink", "/new/target/path", overwrite: false));
        }

        [TestMethod]
        public void AddSymbolicLink_WhenOverwriteIsTrueAndLinkExists_OverwritesLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/test");
            vs.AddSymbolicLink("/test/existingLink", "/old/target/path");

            // Act
            vs.AddSymbolicLink("/test/existingLink", "/new/target/path", overwrite: true);

            // Assert
            VirtualSymbolicLink? link = vs.GetNode("/test/existingLink") as VirtualSymbolicLink;
            Assert.IsNotNull(link);
            Assert.IsTrue(link.TargetPath == "/new/target/path");
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingItem_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData itemData = [1, 2, 3];

            vs.AddDirectory("/test");
            vs.AddItem("/test/existingItem", itemData); // 既存のアイテムを追加

            vs.AddDirectory("/new/target/path", true); // シンボリックリンクのターゲットとなるディレクトリを追加

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddSymbolicLink("/test/existingItem", "/new/target/path", true),
                "既存のアイテム上にシンボリックリンクを追加しようとすると、上書きが true でもInvalidOperationExceptionが発生するべきです。");
        }

        [TestMethod]
        public void AddSymbolicLink_OverwriteTrue_LinkOverExistingDirectory_ThrowsInvalidOperationException()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            vs.AddDirectory("/test/existingDirectory", true); // 既存のディレクトリを追加

            vs.AddDirectory("/new/target/path", true); // シンボリックリンクのターゲットとなるディレクトリを追加

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
                vs.AddSymbolicLink("/test/existingDirectory", "/new/target/path", true),
                "既存のディレクトリ上にシンボリックリンクを追加しようとすると、上書きが true でもInvalidOperationExceptionが発生するべきです。");
        }

        [TestMethod]
        public void AddSymbolicLink_ThrowsVirtualNodeNotFoundException_WhenParentDirectoryDoesNotExist()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath nonExistentParentDirectory = "/nonexistent/directory";
            VirtualPath symbolicLinkPath = nonExistentParentDirectory + "link";
            VirtualPath targetPath = "/existing/target";

            // Act & Assert
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
                vs.AddSymbolicLink(symbolicLinkPath, targetPath),
                "親ディレクトリが存在しない場合、VirtualNodeNotFoundExceptionがスローされるべきです。");
        }

        // シンボリックリンクをディレクトリに追加できるか確認する。
        [TestMethod]
        public void AddSymbolicLink_WithVirtualSymbolicLink_ShouldAddLinkToDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/dir";
            vs.AddDirectory(path);
            VirtualSymbolicLink link = new("link", "/target");

            vs.AddSymbolicLink(path, link);

            Assert.IsTrue(vs.SymbolicLinkExists(path + link.Name));
        }

        // 既存のリンクがある場合、上書きフラグが false のときに例外がスローされるか確認する。
        [TestMethod]
        public void AddSymbolicLink_WithVirtualSymbolicLink_WithOverwrite_ShouldThrowExceptionIfLinkExists()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/dir";
            vs.AddDirectory(directoryPath);
            VirtualNodeName existingLinkName = "existingLink";
            VirtualSymbolicLink existingLink = new(existingLinkName, "/existingTarget");
            VirtualSymbolicLink newLink = new(existingLinkName, "/newTarget");

            vs.AddSymbolicLink(directoryPath, existingLink, false);

            Assert.ThrowsException<InvalidOperationException>(() => vs.AddSymbolicLink(directoryPath, newLink, false));
        }

        // 上書きフラグが true の場合、既存のリンクを上書きできるか確認する。
        [TestMethod]
        public void AddSymbolicLink_WithOverwrite_ShouldOverwriteExistingLink()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/dir";
            vs.AddDirectory(directoryPath);
            VirtualNodeName linkName = "link";
            VirtualSymbolicLink originalLink = new(linkName, "/originalTarget");
            VirtualSymbolicLink newLink = new(linkName, "/newTarget");

            vs.AddSymbolicLink(directoryPath, originalLink, false);
            vs.AddSymbolicLink(directoryPath, newLink, true);

            VirtualSymbolicLink overwrittenLink = vs.GetSymbolicLink(directoryPath + linkName);
            Assert.AreEqual("/newTarget", (string)overwrittenLink.TargetPath);
        }
        
        // 存在しないディレクトリにリンクを追加しようとした場合、例外がスローされるか確認する。
        [TestMethod]
        public void AddSymbolicLink_ToNonExistingDirectory_ShouldThrowVirtualNodeNotFoundException()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/nonExistingDir";
            VirtualNodeName linkName = "link";
            VirtualSymbolicLink link = new(linkName, "/target");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.AddSymbolicLink(directoryPath, link, false));
        }

        // シンボリックリンクが指す存在しないディレクトリにリンクを追加しようとした場合、例外がスローされるか確認する。
        [TestMethod]
        public void AddSymbolicLink_ToLocationPointedBySymbolicLink_ShouldThrowException()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/dir";
            vs.AddDirectory(directoryPath);
            VirtualPath linkPath = "/link";
            VirtualPath targetPath = "/nonExistingTargetDir";
            vs.AddSymbolicLink(linkPath, targetPath);

            VirtualNodeName linkName = "link";
            VirtualSymbolicLink link = new(linkName, "/target");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.AddSymbolicLink(linkPath, link, false));
        }

        // シンボリックリンクが指すディレクトリにリンクを追加できるか確認する。
        [TestMethod]
        public void AddSymbolicLink_ToLocationPointedBySymbolicLink_ShouldAddLinkSuccessfully()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath actualDirectoryPath = "/actualDir";
            vs.AddDirectory(actualDirectoryPath);
            VirtualPath symbolicLinkPath = "/symbolicLink";
            vs.AddSymbolicLink(symbolicLinkPath, actualDirectoryPath);
            VirtualNodeName linkName = "newLink";
            VirtualSymbolicLink link = new(linkName, "/linkTarget");
            vs.AddSymbolicLink(symbolicLinkPath, link);
            Assert.IsTrue(vs.SymbolicLinkExists(actualDirectoryPath + linkName));
            Assert.IsTrue(vs.SymbolicLinkExists(symbolicLinkPath));
        }

        [TestMethod]
        public void WalkPathToTarget_Root()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/";
            VirtualPath targetPath = path;

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Directory1()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/dir1";
            vs.AddDirectory(path, true);
            VirtualPath targetPath = path;

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Directory2()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/dir1/dir2";
            vs.AddDirectory(path, true);
            VirtualPath targetPath = path;

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Item1()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/item";
            BinaryData data = [1, 2, 3];
            vs.AddItem(path, data);
            VirtualPath targetPath = path;

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_Item2()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/dir1/item";
            BinaryData data = [1, 2, 3];
            vs.AddDirectory(path.DirectoryPath, true);
            vs.AddItem(path, data);
            VirtualPath targetPath = path;

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink1()
        {
            VirtualPath targetPath = "/dir1/link1/item";
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddItem("/dir2/item", data);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink2()
        {
            VirtualPath targetPath = "/dir1/link1/dir3";
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddDirectory("/dir2/dir3", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLink3()
        {
            VirtualPath targetPath = "/dir1/link1";
            VirtualPath linkTargetPath = "/dir2";
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir1/link1", linkTargetPath);

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(linkTargetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/nonexistent";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathWithExceptionEnabled()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/nonexistent";

            VirtualNodeNotFoundException exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, true);
            });
            Debug.WriteLine($"ExceptionMessage: {exception.Message}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath2()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/dir1/item";
            BinaryData data = [1, 2, 3];
            vs.AddDirectory(path.DirectoryPath, true);
            vs.AddItem(path, data);
            VirtualPath targetPath = "/dir1/item/dir2";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath3()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "/dir1/dir2";
            vs.AddDirectory(path.DirectoryPath, true);
            VirtualPath targetPath = "/dir1/dir2/dir3";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPath4()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            VirtualPath targetPath = "/dir1/link1/dir3";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_CircularSymbolicLink()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir2/link2", "/dir1");

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget("/dir1/link1/link2/link1", NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_SymbolicLinkToNonExistentPath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/link1", "/nonexistent");

            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                VirtualNodeContext? nodeContext = vs.WalkPathToTarget("/dir1/link1", NotifyNode, null, true, true);
            });
        }

        [TestMethod]
        public void WalkPathToTarget_RelativePath()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath path = "dir2";
            vs.AddDirectory("/dir1/dir2", true);
            VirtualPath targetPath = path;

            vs.ChangeDirectory("/dir1");

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_DirAndDotDot()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1/dir2", true);
            vs.AddDirectory("/dir1/dir3", true);
            VirtualPath targetPath = "/dir1/dir2/../dir3";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_LinkAndDotDot()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddDirectory("/dir1/dir3", true);
            VirtualPath targetPath = "/dir1/link1/../dir3";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, null, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual(targetPath.NodeName, node?.Name);
            Debug.WriteLine($"NodeName: {node?.Name}");
        }


        [TestMethod]
        public void WalkPathToTarget_MultipleLink()
        {
            // テストデータの設定
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddDirectory("/dir3");
            vs.AddItem("/dir3/item", "FinalItem");
            vs.AddSymbolicLink("/dir1/link1", "/dir2");
            vs.AddSymbolicLink("/dir2/link2", "/dir3");

            // メソッドを実行
            VirtualNodeContext? nodeContext = vs.WalkPathToTarget("/dir1/link1/link2/item", NotifyNode, null, true, false);

            // 結果を検証
            Assert.IsTrue(nodeContext?.TraversalPath == "/dir1/link1/link2/item");
            Assert.IsTrue(nodeContext?.ResolvedPath == "/dir3/item");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathAndCreatePath()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/dir1";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, ActionNode, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual("dir1", node.Name.ToString());
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        [TestMethod]
        public void WalkPathToTarget_NonExistentPathAndCreatePath2()
        {
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/dir1/dir2";

            VirtualNodeContext? nodeContext = vs.WalkPathToTarget(targetPath, NotifyNode, ActionNode, true, false);
            VirtualNode? node = nodeContext?.Node;

            Assert.IsNotNull(node);
            Assert.AreEqual("dir2", node.Name.ToString());
            Debug.WriteLine($"NodeName: {node?.Name}");
        }

        private void NotifyNode(VirtualPath path, VirtualNode? node, bool isEnd)
        {
            Debug.WriteLine($"Path: {path}, Node: {node}, isEnd: {isEnd}");
        }

        private bool ActionNode(VirtualDirectory directory, VirtualNodeName nodeName, VirtualPath nodePath)
        {
            VirtualDirectory newDirectory = new(nodeName);

            directory.Add(newDirectory);

            Debug.WriteLine($"IntermediateDirectory: {nodePath}");

            return true;
        }

        [TestMethod]
        public void WalkPathTree_Test1()
        {
            VirtualStorage<string> vs = new();
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
            vs.AddSymbolicLink("/all-dir/sub", "/dir2/sub");


            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            //Assert.AreEqual(4, result.Count());

            string tree = vs.GenerateTextBasedTreeStructure("/", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void WalkPathTree_EmptyDirectory_ShouldReturnOnlyDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/emptyDir", true);

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/emptyDir", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(1, nodeContexts.Count()); // 空のディレクトリ自身のみが結果として返されるべき
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "."));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithItems_ShouldReturnItems()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dirWithItems", true);
            vs.AddItem("/dirWithItems/item1", "content1");
            vs.AddItem("/dirWithItems/item2", "content2");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dirWithItems", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(3, nodeContexts.Count()); // ディレクトリと2つのアイテムが含まれる
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item2"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithSymbolicLink_ShouldFollowLinkIfRequested()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/sourceDir", true);
            vs.AddItem("/sourceDir/item", "content");
            vs.AddSymbolicLink("/linkToSourceDir", "/sourceDir");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "linkToSourceDir/item"));
        }

        [TestMethod]
        public void WalkPathTree_EmptyDirectory_ReturnsOnlyRoot()
        {
            VirtualStorage<BinaryData> vs = new();
            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree(VirtualPath.Root, VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(1, nodeContexts.Count());
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "."));
        }

        [TestMethod]
        public void WalkPathTree_SingleItemDirectory_IncludesItem()
        {
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1", "test");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree(VirtualPath.Root, VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(2, nodeContexts.Count()); // ルートディレクトリとアイテム
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item1"));
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithSymbolicLink_IncludesLinkAndTarget()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/linkToDir1", "/dir1");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree(VirtualPath.Root, VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(3, nodeContexts.Count()); // ルートディレクトリ、dir1、およびシンボリックリンク
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "dir1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "linkToDir1"));
        }

        [TestMethod]
        public void WalkPathTree_DeepNestedDirectories_ReturnsAllNodes()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/dir2/dir3/item1", "test");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(5, nodeContexts.Count()); // ルートディレクトリ、dir1、dir2、dir3、および item1
        }

        [TestMethod]
        public void WalkPathTree_MultipleSymbolicLinks_IncludesLinksAndTargets()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/linkToDir1", "/dir1");
            vs.AddSymbolicLink("/linkToLink1", "/linkToDir1");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(4, nodeContexts.Count()); // ルートディレクトリ、dir1、linkToDir1、および linkToLink1
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithManyItems_ReturnsAllItems()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            for (int i = 1; i <= 100; i++)
            {
                vs.AddItem($"/dir1/item{i}", $"test{i}");
            }

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir1", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.AreEqual(101, nodeContexts.Count()); // dir1 および 100個のアイテム
        }

        [TestMethod]
        public void WalkPathTree_WithNonexistentPathSymbolicLink_ThrowsExceptionAndOutputsMessage()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/emptyLink", "/nonexistent");

            VirtualNodeNotFoundException exception = Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
                foreach (VirtualNodeContext nodeContext in nodeContexts)
                {
                    Debug.WriteLine(nodeContext.ToString());
                }
            });

            Debug.WriteLine($"例外が捕捉されました: {exception.Message}");
        }

        [TestMethod]
        public void WalkPathTree_ShallowNestedDirectories_ExecutesWithoutErrorAndOutputsTree()
        {
            VirtualStorage<BinaryData> vs = new();
            string basePath = "/deep";
            int depth = 100; // 最大1900くらいまでOK
            for (int i = 1; i <= depth; i++)
            {
                basePath = $"{basePath}/dir{i}";
                vs.AddDirectory(basePath, true);
            }

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine($"ノード名: {nodeContext.TraversalPath}, 解決済みパス: {nodeContext.ResolvedPath}");
            }

            // 期待される結果の数をルートディレクトリ + /deep + depth(3)のディレクトリ = 5に更新
            Assert.AreEqual(depth + 2, nodeContexts.Count()); // ルートディレクトリ + /deep + 3階層のディレクトリ
            Debug.WriteLine($"深さ{depth}のディレクトリ構造が正常に走査されました。");
        }

        [TestMethod]
        public void WalkPathTree_MultipleEmptyDirectories_ReturnsAllDirectoriesAndOutputsTree()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/empty1/empty2/empty3", true);

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine($"ノード名: {nodeContext.TraversalPath}, 解決済みパス: {nodeContext.ResolvedPath}");
            }

            Assert.AreEqual(4, nodeContexts.Count()); // ルートディレクトリ + 各空ディレクトリ
            Debug.WriteLine("複数レベルの空ディレクトリが正常に走査されました。");
        }

        [TestMethod]
        public void WalkPathTree_MultipleSymbolicLinksInSamePath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/targetDir", true); // ターゲットディレクトリ
            vs.AddDirectory("/dir", true); // シンボリックリンクのための親ディレクトリ
            vs.AddSymbolicLink("/dir/symLink1", "/targetDir");
            vs.AddSymbolicLink("/dir/symLink2", "/targetDir");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            // `/dir/symLink1` と `/dir/symLink2` の存在を確認
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString().Equals("dir/symLink1")));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString().Equals("dir/symLink2")));
        }

        [TestMethod]
        public void WalkPathTree_SymbolicLinkPointsToAnotherSymbolicLink()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/targetDir", true); // ターゲットディレクトリ
            vs.AddSymbolicLink("/symLink", "/targetDir");
            vs.AddSymbolicLink("/linkToLink", "/symLink");

            IEnumerable<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/", VirtualNodeTypeFilter.All, true, true);
            foreach (VirtualNodeContext nodeContext in nodeContexts)
            {
                Debug.WriteLine(nodeContext);
            }

            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString().Equals("symLink")));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString().Equals("linkToLink")));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnOnlyItemsInDir1()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/item1", "content1");
            vs.AddItem("/dir1/item2", "content2");

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir1", VirtualNodeTypeFilter.Item, true, true).ToList();

            // ディレクトリ自体は含まれないため、アイテムの数だけを期待する
            Assert.AreEqual(2, nodeContexts.Count); // 正しいアイテム数を確認
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item2"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnOnlyDirectoriesInDir2IncludingDirItself()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir2/subdir1", true);
            vs.AddDirectory("/dir2/subdir2", true);

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir2", VirtualNodeTypeFilter.Directory, true, true).ToList();

            // ディレクトリ自体も含む
            Assert.AreEqual(3, nodeContexts.Count);
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "."));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "subdir1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "subdir2"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnDirectoriesAndItemsIncludingDirItself()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir3", true);
            vs.AddItem("/dir3/item1", "content1");
            vs.AddDirectory("/dir3/subdir1", true);

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir3", VirtualNodeTypeFilter.Directory | VirtualNodeTypeFilter.Item, true, false).ToList();

            // ディレクトリ自体を含むため、期待される数はノード数+1
            Assert.AreEqual(3, nodeContexts.Count);
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "."));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "subdir1"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnAllTypesIncludingDirItself()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir4", true);
            vs.AddItem("/dir4/item1", "content1");
            vs.AddDirectory("/dir4/subdir1", true);
            vs.AddSymbolicLink("/dir4/link1", "/dir4/item1");

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir4", VirtualNodeTypeFilter.All, true, true).ToList();

            // ディレクトリ自体も含む
            Assert.AreEqual(4, nodeContexts.Count);
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "."));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "item1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "subdir1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "link1"));
        }

        [TestMethod]
        public void WalkPathTree_WithNoFilter_ShouldNotReturnAnyNodes()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir2", true);
            vs.AddItem("/dir2/item1", "content1");
            vs.AddDirectory("/dir2/subdir1", true);

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir2", VirtualNodeTypeFilter.None, true, true).ToList();

            Assert.AreEqual(0, nodeContexts.Count); // フィルター未適用の場合、ノードは返されない
        }

        [TestMethod]
        public void WalkPathTree_WithDirectoryFilterButNoDirectories_ShouldReturnEmptyList()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir3", true);
            vs.AddItem("/dir3/item1", "content1");
            vs.AddItem("/dir3/item2", "content2");

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir3", VirtualNodeTypeFilter.Directory, true, true).ToList();

            Assert.AreEqual(1, nodeContexts.Count); // ディレクトリが存在しない場合でも、指定したディレクトリ自体は結果に含まれる
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "."));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnOnlySymbolicLinksInDir1()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/dir1/link1", "/item1");
            vs.AddSymbolicLink("/dir1/link2", "/item2");

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir1", VirtualNodeTypeFilter.SymbolicLink, true, false).ToList();

            // シンボリックリンクのみをフィルタリングするため、シンボリックリンクの数だけを期待する
            Assert.AreEqual(2, nodeContexts.Count); // シンボリックリンクの正しい数を確認
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "link1"));
            Assert.IsTrue(nodeContexts.Any(r => r.TraversalPath.ToString() == "link2"));
        }

        [TestMethod]
        public void WalkPathTree_ShouldReturnNoNodesWhenFilterDoesNotMatch()
        {
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir3", true);
            vs.AddItem("/dir3/item1", "content1");

            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/dir3", VirtualNodeTypeFilter.SymbolicLink, true, true).ToList();

            // シンボリックリンクのみをフィルタリングし、ディレクトリにシンボリックリンクがない場合、結果は空であることを期待する
            Assert.AreEqual(0, nodeContexts.Count);
        }

        [TestMethod]
        public void WalkPathTree_DirectoryWithDeepNestingAndRecursiveFalse_ShouldReturnImmediateChildrenOnly()
        {
            VirtualStorage<string> vs = new();
            // 複数階層のディレクトリとアイテムを追加
            vs.AddDirectory("/testDir", true);
            vs.AddItem("/testDir/item1", "content1");
            vs.AddDirectory("/testDir/subDir1", true);
            vs.AddItem("/testDir/subDir1/item2", "content2");
            vs.AddDirectory("/testDir/subDir1/subSubDir1", true);
            vs.AddItem("/testDir/subDir1/subSubDir1/item3", "content3");

            // 再帰なしで走査
            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/testDir", VirtualNodeTypeFilter.All, false, false).ToList();

            // 直下のアイテムとサブディレクトリのみが返されることを確認
            Assert.AreEqual(3, nodeContexts.Count);
            Assert.IsTrue(nodeContexts[0].TraversalPath == ".");
            Assert.IsTrue(nodeContexts[1].TraversalPath == "subDir1");
            Assert.IsTrue(nodeContexts[2].TraversalPath == "item1");
        }

        [TestMethod]
        public void WalkPathTree_StartFromSubdirectoryWithRecursiveFalse_ShouldReturnOrderedImmediateChildrenOnly()
        {
            VirtualStorage<string> vs = new();
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
            List<VirtualNodeContext> nodeContexts = vs.WalkPathTree("/testDir/subDir1", VirtualNodeTypeFilter.All, false, false).ToList();

            // 結果の順番と内容を検証
            Assert.AreEqual(3, nodeContexts.Count, "Should return the starting directory and its immediate children only.");
            Assert.IsTrue(nodeContexts[0].TraversalPath.ToString() == ".");
            Assert.IsTrue(nodeContexts[1].TraversalPath.ToString() == "subSubDir1");
            Assert.IsTrue(nodeContexts[2].TraversalPath.ToString() == "item2");
        }

        private static void ResolvePath_SetData(VirtualStorage<string> vs)
        {
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddItem("/dir1/file1.txt", "data");
            vs.AddItem("/dir1/file2.log", "data");
            vs.AddItem("/dir1/file3.txt", "data");
            vs.AddItem("/dir1/file4.log", "data");
            vs.AddItem("/dir2/file1.txt", "data");
            vs.AddItem("/dir2/file2.log", "data");
            vs.AddItem("/dir2/file3.txt", "data");
            vs.AddItem("/dir2/file4.log", "data");
        }

        [TestMethod]
        public void ResolvePath_WithWildcard_FindsCorrectPaths1()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            ResolvePath_SetData(vs);

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/*.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("Resolved paths:");
            foreach (VirtualPath path in result)
            {
                Debug.WriteLine(path);
            }

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file1.txt");
            Assert.IsTrue(result[1] == "/dir1/file3.txt");
        }

        [TestMethod]
        public void ResolvePath_WithWildcard_FindsCorrectPaths2()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            ResolvePath_SetData(vs);

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir*/file1.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("Resolved paths:");
            foreach (VirtualPath path in result)
            {
                Debug.WriteLine(path);
            }

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file1.txt");
            Assert.IsTrue(result[1] == "/dir2/file1.txt");
        }

        [TestMethod]
        public void ResolvePath_WithWildcard_FindsCorrectPaths3()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            ResolvePath_SetData(vs);

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir*/*.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("Resolved paths:");
            foreach (VirtualPath path in result)
            {
                Debug.WriteLine(path);
            }

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file1.txt");
            Assert.IsTrue(result[1] == "/dir1/file3.txt");
            Assert.IsTrue(result[2] == "/dir2/file1.txt");
            Assert.IsTrue(result[3] == "/dir2/file3.txt");
        }

        [TestMethod]
        public void ResolvePath_WithWildcard_FindsCorrectPaths4()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/dir1a", true);
            vs.ChangeDirectory("/dir1/dir1a");
            vs.AddItem("file1.txt", "data");
            vs.AddItem("file2.log", "data");
            vs.AddItem("file3.txt", "data");
            vs.AddDirectory("/dir2/dir2a", true);
            vs.ChangeDirectory("/dir2/dir2a");
            vs.AddItem("file4.txt", "data");
            vs.AddItem("file5.log", "data");
            vs.AddItem("file6.txt", "data");

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/dir1a/*.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("Resolved paths:");
            foreach (VirtualPath path in result)
            {
                Debug.WriteLine(path);
            }

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/dir1a/file1.txt");
            Assert.IsTrue(result[1] == "/dir1/dir1a/file3.txt");
        }

        [TestMethod]
        public void ResolvePath_WithWildcard_FindsCorrectPaths5()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/dir2/dir3/file1.txt", "data");
            vs.AddDirectory("/dir1a/dir2/dir3", true);
            vs.AddItem("/dir1a/dir2/dir3/file2.txt", "data");
            vs.AddDirectory("/dir1/dir2a/dir3", true);
            vs.AddItem("/dir1/dir2a/dir3/file3.txt", "data");
            vs.AddDirectory("/dir1/dir2/dir3a", true);
            vs.AddItem("/dir1/dir2/dir3a/file4.txt", "data");
            vs.AddItem("/dir1/dir2/dir3/file5.log", "data");

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/dir*/dir3/file*.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("Resolved paths:");
            foreach (VirtualPath path in result)
            {
                Debug.WriteLine(path);
            }

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/dir2/dir3/file1.txt");
            Assert.IsTrue(result[1] == "/dir1/dir2a/dir3/file3.txt");
        }

        [TestMethod]
        public void ResolvePath_WithSingleCharacterWildcard_FindsCorrectPaths()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            ResolvePath_SetData(vs);

            // '?' ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/file?.txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file1.txt");
            Assert.IsTrue(result[1] == "/dir1/file3.txt");
        }

        [TestMethod]
        public void ResolvePath_WithCharacterClassWildcard_FindsCorrectPaths()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            ResolvePath_SetData(vs);

            // '[]' ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir2/file[1-3].txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir2/file1.txt");
            Assert.IsTrue(result[1] == "/dir2/file3.txt");
        }

        [TestMethod]
        public void ResolvePath_WithCharacterClassMatch_FindsCorrectPaths()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/book.txt", "data");
            vs.AddItem("/dir1/cook.txt", "data");

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/[bc]ook.txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/book.txt");
            Assert.IsTrue(result[1] == "/dir1/cook.txt");
        }

        [TestMethod]
        public void ResolvePath_WithCharacterClassNoMatch_DoesNotFindIncorrectPaths()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/hook.txt", "data");

            // '[bc]ook.txt' ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/[bc]ook.txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);  // hook.txt は '[bc]ook.txt' パターンに一致しない
        }

        [TestMethod]
        public void ResolvePath_WithMultipleWildcards_FindsAllLogFilesAcrossDirectories()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir2", true);
            vs.AddItem("/dir1/file1.txt", "data");
            vs.AddItem("/dir1/file2.log", "data");
            vs.AddItem("/dir1/file3.txt", "data");
            vs.AddItem("/dir1/file4.log", "data");
            vs.AddItem("/dir2/file1.txt", "data");
            vs.AddItem("/dir2/file2.log", "data");
            vs.AddItem("/dir2/file3.txt", "data");
            vs.AddItem("/dir2/file4.log", "data");

            // 複数のワイルドカードを含むパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir*/file*.log").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file2.log");
            Assert.IsTrue(result[1] == "/dir1/file4.log");
            Assert.IsTrue(result[2] == "/dir2/file2.log");
            Assert.IsTrue(result[3] == "/dir2/file4.log");
        }

        [TestMethod]
        public void ResolvePath_WithCaseInsensitiveExtensionMatching_FindsAllTxtFiles()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/file1.TXT", "data");  // 大文字の拡張子
            vs.AddItem("/dir1/file2.txt", "data");  // 小文字の拡張子
            vs.AddItem("/dir1/file3.TxT", "data");  // 混在する拡張子

            // 大文字小文字を区別しないパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/file?.[tT][xX][tT]").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file1.TXT");
            Assert.IsTrue(result[1] == "/dir1/file2.txt");
            Assert.IsTrue(result[2] == "/dir1/file3.TxT");
        }

        [TestMethod]
        public void ResolvePath_WithInvalidDirectoryPath_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/file1.txt", "data");

            // 存在しないディレクトリパスの解決時に例外を期待
            Assert.ThrowsException<VirtualNodeNotFoundException>(() => vs.ResolvePath("/invalid/dir/*.txt").ToList());
        }

        [TestMethod]
        public void ResolvePath_WithImproperlyFormattedPath_ThrowsVirtualNodeNotFoundException()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/file1.txt", "data");

            // 正規化されて検索される
            List<VirtualPath> result = vs.ResolvePath("/dir1/*.txt/").ToList();

            // フォーマットが正しくないパターンの場合、正規化されることを確認する
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count); // "/dir1/file1.txt"で検索される
            Assert.IsTrue(result[0] == "/dir1/file1.txt");
        }

        [TestMethod]
        public void ResolvePath_WithWildcardInMiddleOfPath_FindsCorrectFiles()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory("/dir1/subdir1", true);
            vs.AddDirectory("/dir1/subdir2", true);
            vs.AddItem("/dir1/subdir1/file1.txt", "data");
            vs.AddItem("/dir1/subdir2/file1.txt", "data");
            vs.AddItem("/dir1/file1.txt", "data");
            vs.AddItem("/file1.txt", "data");

            // パス中のワイルドカードを含むパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/*/file1.txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result[0] == "/dir1/subdir1/file1.txt");
            Assert.IsTrue(result[1] == "/dir1/subdir2/file1.txt");
        }

        [TestMethod]
        public void ResolvePath_WithEscapedWildcards_HandlesLiteralsCorrectly()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/file1.txt", "data");  // 通常のファイル
            vs.AddItem("/dir1/file*.txt", "data");  // リテラルワイルドカードを含むファイル

            // PowerShellスタイルのエスケープを含むパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/file`*.txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file*.txt");
        }

        [TestMethod]
        public void ResolvePath_WithEscapedQuestionMark_HandlesLiteralsCorrectly()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/file1.txt", "data");  // 通常のファイル
            vs.AddItem("/dir1/file?.txt", "data");  // リテラルワイルドカードを含むファイル

            // PowerShellスタイルのエスケープを含むパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/file`?.txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file?.txt");
        }

        [TestMethod]
        public void ResolvePath_WithEscapedBrackets_HandlesLiteralsCorrectly()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/file1.txt", "data");  // 通常のファイル
            vs.AddItem("/dir1/file[1].txt", "data");  // リテラルワイルドカードを含むファイル

            // PowerShellスタイルのエスケープを含むパス解決
            List<VirtualPath> result = vs.ResolvePath("/dir1/file`[1`].txt").ToList();

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0] == "/dir1/file[1].txt");
        }

        [TestMethod]
        public void ResolvePath_WithHighVolumeOfFiles_PerformanceAndAccuracyTest()
        {
            const int NumberOfFiles = 10000; // テストに使用するファイルの数を定数で定義

            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            vs.AddDirectory("/dir1", true);

            // 指定された数のファイルを生成して追加
            for (int i = 0; i < NumberOfFiles; i++)
            {
                vs.AddItem($"/dir1/file{i}.txt", "data");
            }

            // パフォーマンス計測の開始
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // ワイルドカードを使用したパス解決
            var result = vs.ResolvePath("/dir1/file*.txt").ToList();

            // ストップウォッチの停止
            stopwatch.Stop();

            // 期待される結果の確認
            Assert.AreEqual(NumberOfFiles, result.Count, "The number of matched files does not meet the expected count.");

            // 各ファイルが期待通りに結果リストに含まれているかを確認
            for (int i = 0; i < NumberOfFiles; i++)
            {
                string expectedPath = $"/dir1/file{i}.txt";
                Assert.IsTrue(result.Contains(new VirtualPath(expectedPath)), $"File {expectedPath} is missing in the results.");
            }

            // パフォーマンスの基準設定（例えば1秒以内に完了することを期待）
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, $"ResolvePath took {stopwatch.ElapsedMilliseconds} milliseconds, which is too slow.");

            Debug.WriteLine($"ResolvePath took {stopwatch.ElapsedMilliseconds} milliseconds for {NumberOfFiles} files.");
        }

        [TestMethod]
        public void ResolvePath_WithMultipleLevels_PerformanceTest()
        {
            const int Depth = 10; // ディレクトリの深さ
            const int FilesPerDepth = 10; // 各階層に生成するファイルの数

            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();

            // 指定された階層と各階層にファイルを生成
            string currentPath = "/";
            for (int i = 0; i < Depth; i++)
            {
                currentPath += $"dir{i}/";
                vs.AddDirectory(currentPath, true);
                for (int j = 0; j < FilesPerDepth; j++)
                {
                    vs.AddItem(currentPath + $"file{j}.txt", "data");
                    vs.AddItem(currentPath + $"file{j}.log", "data");
                }
            }

            // デバッグ出力でツリー構造を表示
            Debug.WriteLine("Virtual vs tree structure:");
            string tree = vs.GenerateTextBasedTreeStructure("/", true, true);
            Debug.WriteLine(tree);

            // パフォーマンス計測の開始
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 各階層でのファイル解決
            for (int i = 0; i < Depth; i++)
            {
                string searchPath = "/";
                for (int j = 0; j <= i; j++)
                {
                    searchPath += $"dir{j}/";
                }
                searchPath += "file*.txt";
                var result = vs.ResolvePath(searchPath).ToList();
                Assert.AreEqual(FilesPerDepth, result.Count, $"Failed at depth {i} with path {searchPath}");
            }

            // ストップウォッチの停止
            stopwatch.Stop();

            // デバッグ出力でタイムを表示
            Debug.WriteLine($"Total time to resolve files at each depth: {stopwatch.ElapsedMilliseconds} milliseconds.");

            // パフォーマンスの基準設定
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, $"Total resolution time {stopwatch.ElapsedMilliseconds} milliseconds is too slow.");
        }

        [TestMethod]
        public void ResolvePath_WithDeepNestedDirectories_PerformanceTest()
        {
            const int Depth = 10; // ディレクトリの深さ
            const int FilesPerDepth = 10; // 最深階層に生成するファイルの数

            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<string> vs = new();
            string currentPath = "/";

            // 指定された階層の数だけディレクトリを作成
            for (int i = 0; i < Depth; i++)
            {
                currentPath += $"dir{i}";
                vs.AddDirectory(currentPath + "/", true);
                vs.AddDirectory(currentPath + "A/", true);
                vs.AddDirectory(currentPath + "B/", true);
                currentPath += "/";
                if (i == Depth - 1)
                {  // 最深階層にファイルを追加
                    for (int j = 0; j < FilesPerDepth; j++)
                    {
                        vs.AddItem(currentPath + $"file{j}.txt", "data");
                    }
                }
            }

            // デバッグ出力でツリー構造を表示
            Debug.WriteLine("Virtual vs tree structure:");
            string tree = vs.GenerateTextBasedTreeStructure("/", true, true);
            Debug.WriteLine(tree);

            // 深い階層構造のパスを構築
            string searchPath = "/*" + string.Concat(Enumerable.Repeat("/*", Depth - 1)) + "/file*.txt";

            // searchPathのデバッグ出力
            Debug.WriteLine($"Search path: {searchPath}");

            // パフォーマンス計測の開始
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // ワイルドカードを使用してファイルを解決
            var result = vs.ResolvePath(searchPath).ToList();

            // ストップウォッチの停止
            stopwatch.Stop();

            // 期待される結果の確認
            Assert.AreEqual(FilesPerDepth, result.Count, "The number of resolved files does not match the expected count.");

            // パフォーマンスの基準設定
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000, $"ResolvePath took {stopwatch.ElapsedMilliseconds} milliseconds, which is too slow.");

            // デバッグ出力でタイムを表示
            Debug.WriteLine($"ResolvePath took {stopwatch.ElapsedMilliseconds} milliseconds for resolving files in deep nested directories.");
        }

        [TestMethod]
        public void ResolveDeepDirectoryStructureTest()
        {
            const int Depth = 300;
            const string BaseDir = "/dir1";
            VirtualStorage<string> vs = new();

            // 深い階層のディレクトリを作成し、最深部にファイルを追加
            string currentPath = BaseDir;
            for (int i = 1; i <= Depth; i++)
            {
                currentPath += $"/dir{i}";
                vs.AddDirectory(currentPath, true);
                if (i == Depth)
                {
                    vs.AddItem(currentPath + "/file1.txt", "data");
                    vs.AddItem(currentPath + "/file2.txt", "data");
                    vs.AddItem(currentPath + "/file3.txt", "data");
                }
            }

            // ツリー構造の表示
            Debug.WriteLine("Virtual vs tree structure:");
            string tree = vs.GenerateTextBasedTreeStructure(BaseDir, true, true);
            Debug.WriteLine(tree);

            // パスのデバッグ出力
            Debug.WriteLine("Resolved paths: " + currentPath + "/*.txt");

            // ファイル解決のテスト
            var result = vs.ResolvePath(currentPath + "/*.txt").ToList();

            // 期待される結果の確認
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Exists(p => p.Path.EndsWith("file1.txt")));
            Assert.IsTrue(result.Exists(p => p.Path.EndsWith("file2.txt")));
            Assert.IsTrue(result.Exists(p => p.Path.EndsWith("file3.txt")));

            // 結果のデバッグ出力
            Debug.WriteLine($"Resolved files:");
            foreach (var file in result)
            {
                Debug.WriteLine(file);
            }
        }

        [TestMethod]
        public void ResolvePath_Root()
        {
            // VirtualStorage インスタンスのセットアップ
            VirtualStorage<BinaryData> vs = new();

            // ワイルドカードを使用したパス解決
            List<VirtualPath> result = vs.ResolvePath("/").ToList();

            // デバッグ出力
            Debug.WriteLine("Resolved paths:");
            foreach (VirtualPath path in result)
            {
                Debug.WriteLine(path);
            }

            // 期待される結果の確認
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0] == "/");
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnDirectoryForDirectoryPath()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);

            VirtualNodeType result = vs.GetNodeType("/dir1");

            Assert.AreEqual(VirtualNodeType.Directory, result);
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnItemForItemPath()
        {
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1", "content1");

            VirtualNodeType result = vs.GetNodeType("/item1");

            Assert.AreEqual(VirtualNodeType.Item, result);
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnSymbolicLinkForLinkPathWithoutFollowing()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/link1", "/dir1");

            VirtualNodeType result = vs.GetNodeType("/link1", false);

            Assert.AreEqual(VirtualNodeType.SymbolicLink, result);
        }

        [TestMethod]
        public void GetNodeType_ShouldReturnDirectoryForLinkPathWhenFollowing()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink("/link1", "/dir1");

            VirtualNodeType result = vs.GetNodeType("/link1", true);

            Assert.AreEqual(VirtualNodeType.Directory, result);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_RecursiveWithLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_RecursiveNoLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_NonRecursiveWithLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/", false, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_NonRecursiveNoLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/", false, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_NonExistentPathWithLinks()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                string tree = vs.GenerateTextBasedTreeStructure("/nonexistent", true, true);
            });
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_RecursiveWithLinksFromSubDirectory()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/dir1", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_NonRecursiveWithLinksFromSubDirectory()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/dir1", false, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_BasePathIsItem()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/item1", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_LinkToItem_NoFollow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/link-to-item", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_LinkToItem_Follow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/link-to-item", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_LinkToDirectory_NoFollow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/link-to-dir", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void GenerateTextBasedTreeStructure_LinkToDirectory_Follow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTextBasedTreeStructure("/link-to-dir", true, true);
            Debug.WriteLine(tree);
        }

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
            return vs;
        }

        [TestMethod]
        public void CopyNode_CopyItemToNewItem()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/item2");

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/item2");
            VirtualItem<BinaryData> originalItem = vs.GetItem("/item1");
            Assert.IsTrue(copiedItem.Name == "item2");
            Assert.AreNotEqual(originalItem, copiedItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, copiedItem.ItemData!.Data);
            Assert.AreNotSame(originalItem, copiedItem);
            Assert.AreNotSame(originalItem.ItemData, copiedItem.ItemData);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyItemToDeepDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);
            vs.AddDirectory("/dir1/dir2", true);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/dir1/dir2/item2");

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/dir2/item2");
            VirtualItem<BinaryData> originalItem = vs.GetItem("/item1");
            Assert.IsTrue(copiedItem.Name == "item2");
            Assert.AreNotEqual(originalItem, copiedItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, copiedItem.ItemData!.Data);
            Assert.AreNotSame(originalItem, copiedItem);
            Assert.AreNotSame(originalItem.ItemData, copiedItem.ItemData);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyItemToExistingItemWithOverwrites()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData newData = [4, 5, 6];

            // テストデータ
            vs.AddItem("/item1", originalData);
            vs.AddItem("/item2", newData);

            // 実行: "/item1"を"/item2"に overwrite オプションを true でコピー
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/item2", true);

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/item2");
            VirtualItem<BinaryData> originalItem = vs.GetItem("/item1");
            Assert.IsTrue(copiedItem.Name == "item2");
            Assert.AreNotEqual(originalItem, copiedItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, copiedItem.ItemData!.Data);
            Assert.AreNotSame(originalItem, copiedItem);
            Assert.AreNotSame(originalItem.ItemData, copiedItem.ItemData);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyItemWithDoesNotExist_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();

            // 実行 & 検査: 存在しない "/item1" をコピーしようとする
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/item2");
            });
        }

        [TestMethod]
        public void CopyNode_CopyItemToSameSourceAndDestination_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);

            // 実行 & 検査: 同じパスへのコピーを試みる
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/item1");
            });
        }

        [TestMethod]
        public void CopyNode_CopyItemToExistingDestinationWithoutOverwrite_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);
            vs.AddItem("/item2", data);

            // 実行 & 検査: overwriteを指定せずに"/item1"を"/item2"にコピー
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/item2");
            });
        }

        [TestMethod]
        public void CopyNode_CopyItemToDestinationDirectoryDoesNotExist_CreatesNewItem()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddItem("/item1", data);

            // 実行: アイテムが存在しないディレクトリ "/dir1" にコピー
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/dir1/item2");

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);
            CollectionAssert.AreEqual(data.Data, copiedItem.ItemData!.Data);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyItemToExistingItemInDestinationWithoutOverwrite_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ: "/dir1/item1" として既にアイテムを追加
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", data);
            vs.AddItem("/item1", data);

            // 実行 & 検査: 上書きなしで "/item1" を "/dir1" にコピーしようとする
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/dir1/item1");
            });
        }

        [TestMethod]
        public void CopyNode_CopyItemToSymbolicLinkTargetingDirectory_SuccessfulCopy()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットディレクトリを作成し、シンボリックリンクを設定
            vs.AddDirectory("/dir1");
            vs.AddSymbolicLink("/link", "/dir1");

            // シンボリックリンクを経由して、新しいアイテム名でコピーを試みる
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/link/item2");

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyItemToSymbolicLinkTargetingIntermediateDirectory_SuccessfulCopy()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットディレクトリを作成し、シンボリックリンクを設定
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddSymbolicLink("/dir1/link", "/dir1/dir2");

            // シンボリックリンクを経由して、新しいアイテム名でコピーを試みる
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/dir1/link/dir3/item2");

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/link/dir3/item2", true);
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyItemToSymbolicLinkTargetingDirectory_ThrowsExceptionWhenItemExists()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData existingData = [4, 5, 6];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットディレクトリを作成し、シンボリックリンクを設定
            vs.AddDirectory("/dir1");
            vs.AddSymbolicLink("/link", "/dir1");

            // ターゲットディレクトリに既存のアイテムを追加
            vs.AddItem("/dir1/item2", existingData);

            // シンボリックリンクを経由して、既存のアイテム名で上書きなしのコピーを試みる
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.CopyNode("/item1", "/dir1/item2", false);
            });
        }

        [TestMethod]
        public void CopyNode_FromSymbolicLinkTargetingFile_SuccessfulCopy()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットアイテムの作成とデータ追加
            vs.AddDirectory("/dir1");

            // アイテムへのシンボリックリンクを作成
            vs.AddSymbolicLink("/linkToItem", "/item1");

            // シンボリックリンクを経由してアイテムにコピー
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/linkToItem", "/dir1/item2", false, false, true);

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);  // パスをキャストして確認
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);  // データが正しくコピーされたことを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_ToNonExistingSymbolicLinkTargetingFile_SuccessfulCopy()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットアイテムの作成とデータ追加
            vs.AddDirectory("/dir1");

            // アイテムへのシンボリックリンクを作成
            vs.AddSymbolicLink("/linkToItem", "/dir1/item2");

            // シンボリックリンクを経由してアイテムにコピーする
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/linkToItem");
 
            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);  // パスをキャストして確認
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);  // データが正しくコピーされたことを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_ToNonExistingDeepSymbolicLinkTargetingFile_SuccessfulCopy()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットアイテムの作成とデータ追加
            vs.AddDirectory("/dir1");

            // アイテムへのシンボリックリンクを作成
            vs.AddSymbolicLink("/linkToItem", "/linkToItem2");
            vs.AddSymbolicLink("/linkToItem2", "/linkToItem3");
            vs.AddSymbolicLink("/linkToItem3", "/dir1/item2");

            // シンボリックリンクを経由してアイテムにコピーする
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/linkToItem");

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);  // パスをキャストして確認
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);  // データが正しくコピーされたことを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_ToSymbolicLinkTargetingFile_ThrowsExceptionWhenNoOverwrite()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData targetItemData = [4, 5, 6];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットアイテムの作成とデータ追加
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item2", targetItemData);

            // アイテムへのシンボリックリンクを作成
            vs.AddSymbolicLink("/linkToItem", "/dir1/item2");

            // シンボリックリンクを経由して、アイテムに上書きなしでコピーを試みる
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.CopyNode("/item1", "/linkToItem");
            });
        }

        [TestMethod]
        public void CopyNode_ToSymbolicLinkTargetingFile_SuccessfulCopyWithOverwrite()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData targetItemData = [4, 5, 6];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットアイテムの作成とデータ追加
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item2", targetItemData);

            // アイテムへのシンボリックリンクを作成
            vs.AddSymbolicLink("/linkToItem", "/dir1/item2");

            // 上書きを許可してシンボリックリンクを経由してアイテムにコピー
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/linkToItem", true);

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);  // パスをキャストして確認
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);  // データが正しくコピーされたことを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_ToDeepSymbolicLinkTargetingFile_SuccessfulCopyWithOverwrite()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData targetItemData = [4, 5, 6];

            // コピー元アイテムの追加
            vs.AddItem("/item1", originalData);

            // ターゲットアイテムの作成とデータ追加
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item2", targetItemData);

            // アイテムへのシンボリックリンクを作成
            vs.AddSymbolicLink("/linkToItem", "/linkToItem2");
            vs.AddSymbolicLink("/linkToItem2", "/linkToItem3");
            vs.AddSymbolicLink("/linkToItem3", "/dir1/item2");

            // 上書きを許可してシンボリックリンクを経由してアイテムにコピー
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/item1", "/linkToItem", true);

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir1/item2");
            Assert.IsNotNull(copiedItem);
            Assert.AreEqual("item2", (string)copiedItem.Name);  // パスをキャストして確認
            CollectionAssert.AreEqual(originalData.Data, copiedItem.ItemData!.Data);  // データが正しくコピーされたことを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToNewDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", data);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dir2");

            // 検査
            VirtualDirectory copiedDirectory = vs.GetDirectory("/dir2");
            VirtualDirectory originalDirectory = vs.GetDirectory("/dir1");
            Assert.IsTrue(copiedDirectory.Name == "dir2");
            Assert.AreNotEqual(originalDirectory, copiedDirectory);
            Assert.AreNotSame(originalDirectory, copiedDirectory);
            Assert.AreEqual(0, copiedDirectory.Count);  // コピー先ディレクトリは空であることを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToDeepDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", data);
            vs.AddDirectory("/dir2/dir3", true);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dir2/dir3/dir4");

            // 検査
            VirtualDirectory copiedDirectory = vs.GetDirectory("/dir2/dir3/dir4");
            VirtualDirectory originalDirectory = vs.GetDirectory("/dir1");
            Assert.IsTrue(copiedDirectory.Name == "dir4");
            Assert.AreNotEqual(originalDirectory, copiedDirectory);
            Assert.AreNotSame(originalDirectory, copiedDirectory);
            Assert.AreEqual(0, copiedDirectory.Count);  // コピー先ディレクトリは空であることを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToExistingDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData newData = [4, 5, 6];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dst/dir1", true);
            vs.AddItem("/dir1/item1", originalData);
            vs.AddItem("/dst/dir1/item2", originalData);
            vs.AddItem("/dst/dir1/item3", originalData);

            // 実行 & 検査
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", false);
            });
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToExistingItem()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData newData = [4, 5, 6];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dst");
            vs.AddItem("/dir1/item1", originalData);
            vs.AddItem("/dst/dir1", originalData); // dir1 というアイテムがある場合

            // 実行 & 検査
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", false);
            });
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToExistingSymbolicLink()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData newData = [4, 5, 6];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dst");
            vs.AddItem("/dir1/item1", originalData);
            vs.AddSymbolicLink("/dst/dir1", "/anywhere"); // dir1 というリンクがある場合

            // 実行 & 検査
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", false);
            });
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToExistingDirectoryWithOverwrites()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dst/dir1", true);
            vs.AddItem("/dir1/item1", originalData);
            vs.AddItem("/dst/dir1/item2", originalData);
            vs.AddItem("/dst/dir1/item3", originalData);
            DateTime copiedCreatedDate = vs.GetDirectory("/dst/dir1").CreatedDate;
            DateTime copiedUpdatedDate = vs.GetDirectory("/dst/dir1").UpdatedDate;

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", true);

            // 検査
            VirtualDirectory copiedDirectory = vs.GetDirectory("/dst/dir1");
            VirtualDirectory originalDirectory = vs.GetDirectory("/dir1");
            Assert.IsTrue(copiedDirectory.Name == "dir1");
            Assert.AreNotEqual(originalDirectory, copiedDirectory);
            Assert.AreNotSame(originalDirectory, copiedDirectory);
            Assert.AreEqual(2, copiedDirectory.Count);  // コピー先ディレクトリのカウントに変化がない事を確認
            Assert.AreEqual(copiedCreatedDate, copiedDirectory.CreatedDate);  // 作成日時が変更されていないことを確認
            Assert.AreEqual(copiedUpdatedDate, copiedDirectory.UpdatedDate);  // 更新日時が変更されていることを確認

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToExistingItemWithOverwrites()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData newData = [4, 5, 6];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dst");
            vs.AddItem("/dir1/item1", originalData);
            vs.AddItem("/dst/dir1", originalData); // dir1 というアイテムがある場合

            // 実行 & 検査
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", true);
            });
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToExistingSymbolicLinkWithOverwrites()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];
            BinaryData newData = [4, 5, 6];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dst");
            vs.AddItem("/dir1/item1", originalData);
            vs.AddSymbolicLink("/dst/dir1", "/anywhere"); // dir1 というリンクがある場合

            // 実行 & 検査
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", true);
            });
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkToNewSymbolicLink()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);
            vs.AddSymbolicLink("/link1", "/item1");

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/link1", "/link2", false, false, false);

            // 検査
            VirtualSymbolicLink copiedLink = vs.GetSymbolicLink("/link2");
            VirtualSymbolicLink originalLink = vs.GetSymbolicLink("/link1");
            Assert.IsTrue(copiedLink.Name == "link2");
            Assert.AreNotEqual(originalLink, copiedLink);
            Assert.AreEqual(originalLink.TargetPath, copiedLink.TargetPath);
            Assert.AreNotSame(originalLink, copiedLink);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkToDeepDirectory()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);
            vs.AddSymbolicLink("/link1", "/item1");
            vs.AddDirectory("/dir1/dir2", true);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/link1", "/dir1/dir2/link2", false, false, false);

            // 検査
            VirtualSymbolicLink copiedLink = vs.GetSymbolicLink("/dir1/dir2/link2");
            VirtualSymbolicLink originalLink = vs.GetSymbolicLink("/link1");
            Assert.IsTrue(copiedLink.Name == "link2");
            Assert.AreNotEqual(originalLink, copiedLink);
            Assert.AreEqual(originalLink.TargetPath, copiedLink.TargetPath);
            Assert.AreNotSame(originalLink, copiedLink);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkToExistingSymbolicLinkWithOverwrites()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData originalData = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", originalData);
            vs.AddDirectory("/dst");
            vs.AddSymbolicLink("/link1", "/item1");
            vs.AddSymbolicLink("/link2", "/dst");

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/link1", "/link2", false, false, false);

            // 検査
            VirtualSymbolicLink copiedLink = vs.GetSymbolicLink("/dst/link1");
            VirtualSymbolicLink originalLink = vs.GetSymbolicLink("/link1");
            Assert.IsTrue(copiedLink.Name == "link1");
            Assert.AreNotEqual(originalLink, copiedLink);
            Assert.AreEqual(originalLink.TargetPath, copiedLink.TargetPath);
            Assert.AreNotSame(originalLink, copiedLink);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }

            // 処理後のデータ構造の表示
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkWithDoesNotExist_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();

            // 実行
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/link1", "/link2");
            });
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkToSameSourceAndDestination_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddItem("/item1", data);
            vs.AddSymbolicLink("/link1", "/item1");

            // 実行
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/link1", "/link1");
            });
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkToNewSymbolicLinkWithFollowLinks()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddDirectory("/dir2");
            vs.AddItem("/dir1/item1", data);
            vs.AddSymbolicLink("/link1", "/dir1/item1");

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/link1", "/dir2", false, false, true);

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dir2/item1");
            VirtualItem<BinaryData> originalItem = vs.GetItem("/dir1/item1");
            Assert.IsTrue(copiedItem.Name == "item1");
            Assert.AreNotEqual(originalItem, copiedItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, copiedItem.ItemData!.Data);
            Assert.AreNotSame(originalItem, copiedItem);

            // コンテキストの表示
            Debug.WriteLine("context:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }

            // 処理後のデータ構造の表示
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void CopyNode_CopySymbolicLinkToSymbolicLinkWithFollowLinks()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", data);
            vs.AddDirectory("/dir2");
            vs.AddDirectory("/dst");
            vs.AddSymbolicLink("/dst/link", "/dir2");

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1/item1", "/dst/link", false, false, true);

            // 検査
            VirtualItem<BinaryData> copiedItem = vs.GetItem("/dst/link/item1", true);
            VirtualItem<BinaryData> originalItem = vs.GetItem("/dir1/item1");
            Assert.IsTrue(copiedItem.Name == "item1");
            Assert.AreNotEqual(originalItem, copiedItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, copiedItem.ItemData!.Data);
            Assert.AreNotSame(originalItem, copiedItem);

            // コンテキストの表示
            Debug.WriteLine("\ncontext:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }

            // 処理後のデータ構造の表示
            Debug.WriteLine("\nstructure:");
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToDirectoryWithRecursiveOption()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/dir1", true);
            vs.AddItem("/dir1/item1", data);
            vs.AddDirectory("/dst", true);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst", false, true, false);

            // 検査
            VirtualDirectory copiedDirectory = vs.GetDirectory("/dst/dir1");
            VirtualDirectory originalDirectory = vs.GetDirectory("/dir1");
            Assert.IsTrue(copiedDirectory.Name == "dir1");
            Assert.AreNotEqual(originalDirectory, copiedDirectory);
            Assert.AreNotSame(originalDirectory, copiedDirectory);
            Assert.AreEqual(1, copiedDirectory.Count);
            Assert.IsTrue(vs.GetItem("/dst/dir1/item1").Name == "item1");
            CollectionAssert.AreEqual(data.Data, vs.GetItem("/dst/dir1/item1").ItemData!.Data);

            // コンテキストの表示
            Debug.WriteLine("\ncontext:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }

            // 処理後のデータ構造の表示
            Debug.WriteLine("\nstructure:");
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToDeepDirectoryWithRecursiveOption()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/data", true);
            vs.AddItem("/data/itemA", data);
            vs.AddItem("/data/itemB", data);
            vs.AddItem("/data/itemC", data);
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/item1", data);
            vs.AddSymbolicLink("/dir1/itemA", "/data/itemA");
            vs.AddItem("/dir1/dir2/item2", data);
            vs.AddSymbolicLink("/dir1/dir2/itemB", "/data/itemB");
            vs.AddItem("/dir1/dir2/dir3/item3", data);
            vs.AddSymbolicLink("/dir1/dir2/dir3/itemC", "/data/itemC");
            vs.AddDirectory("/dst1/dst2", true);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst1/dst2", false, true, false);

            // 検査
            VirtualDirectory copiedDirectory = vs.GetDirectory("/dst1/dst2/dir1");
            VirtualDirectory originalDirectory = vs.GetDirectory("/dir1");
            Assert.IsTrue(copiedDirectory.Name == "dir1");
            Assert.AreNotEqual(originalDirectory, copiedDirectory);
            Assert.AreNotSame(originalDirectory, copiedDirectory);
            Assert.AreEqual(3, copiedDirectory.Count);
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/item1").Name == "item1");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/dir2/item2").Name == "item2");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/dir2/dir3/item3").Name == "item3");
            Assert.IsTrue(vs.GetSymbolicLink("/dst1/dst2/dir1/itemA").Name == "itemA");
            Assert.IsTrue(vs.GetSymbolicLink("/dst1/dst2/dir1/dir2/itemB").Name == "itemB");
            Assert.IsTrue(vs.GetSymbolicLink("/dst1/dst2/dir1/dir2/dir3/itemC").Name == "itemC");

            // コンテキストの表示
            Debug.WriteLine("\ncontext:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }

            // 処理後のデータ構造の表示
            Debug.WriteLine("\nstructure:");
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void CopyNode_CopyDirectoryToDeepDirectoryWithRecursiveAndFollowLinks()
        {
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];

            // テストデータ
            vs.AddDirectory("/data", true);
            vs.AddItem("/data/itemA", data);
            vs.AddItem("/data/itemB", data);
            vs.AddItem("/data/itemC", data);
            vs.AddDirectory("/dir1/dir2/dir3", true);
            vs.AddItem("/dir1/item1", data);
            vs.AddSymbolicLink("/dir1/itemA", "/data/itemA");
            vs.AddItem("/dir1/dir2/item2", data);
            vs.AddSymbolicLink("/dir1/dir2/itemB", "/data/itemB");
            vs.AddItem("/dir1/dir2/dir3/item3", data);
            vs.AddSymbolicLink("/dir1/dir2/dir3/itemC", "/data/itemC");
            vs.AddDirectory("/dst1/dst2", true);

            // 実行
            IEnumerable<VirtualNodeContext> contexts = vs.CopyNode("/dir1", "/dst1/dst2", false, true, true);

            // 検査
            VirtualDirectory copiedDirectory = vs.GetDirectory("/dst1/dst2/dir1");
            VirtualDirectory originalDirectory = vs.GetDirectory("/dir1");
            Assert.IsTrue(copiedDirectory.Name == "dir1");
            Assert.AreNotEqual(originalDirectory, copiedDirectory);
            Assert.AreNotSame(originalDirectory, copiedDirectory);
            Assert.AreEqual(3, copiedDirectory.Count);
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/item1").Name == "item1");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/dir2/item2").Name == "item2");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/dir2/dir3/item3").Name == "item3");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/itemA").Name == "itemA");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/dir2/itemB").Name == "itemB");
            Assert.IsTrue(vs.GetItem("/dst1/dst2/dir1/dir2/dir3/itemC").Name == "itemC");

            // コンテキストの表示
            Debug.WriteLine("\ncontext:");
            foreach (VirtualNodeContext context in contexts)
            {
                Debug.WriteLine(context);
            }

            // 処理後のデータ構造の表示
            Debug.WriteLine("\nstructure:");
            string tree = vs.GenerateTextBasedTreeStructure("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        public void CopyNode_CopySourceToItsSubdirectory_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", new BinaryData([1, 2, 3]));

            // 実行 & 検査: コピー元がコピー先のサブディレクトリである場合
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.CopyNode("/dir1", "/dir1/subdir");
            });
        }

        [TestMethod]
        public void CopyNode_CopyDestinationToItsParentDirectory_ThrowsException()
        {
            VirtualStorage<BinaryData> vs = new();
            vs.AddDirectory("/dir1/subdir", true);
            vs.AddItem("/dir1/subdir/item1", new BinaryData([1, 2, 3]));

            // 実行 & 検査: コピー先がコピー元の親ディレクトリである場合
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                vs.CopyNode("/dir1/subdir", "/dir1");
            });
        }

        private static void DebugPrintLinkDictionary(VirtualStorage<BinaryData> vs)
        {
            var linkDictionary = vs.LinkDictionary; // リンク辞書のプロパティを利用
            Debug.WriteLine("リンク辞書の内容:");
            foreach (var entry in linkDictionary)
            {
                var targetPath = entry.Key;
                var targetNode = vs.TryGetNode(targetPath); // ターゲットパスのノードを取得
                if (targetNode != null)
                {
                    var targetNodeType = targetNode?.NodeType.ToString() ?? "Unknown";

                    Debug.WriteLine($"ターゲットパス: {targetPath} (ノードタイプ: {targetNodeType})");
                    foreach (var linkPath in entry.Value)
                    {
                        Debug.WriteLine($"  リンクパス: {linkPath}");
                    }
                }
                else
                {
                    Debug.WriteLine($"ターゲットパス: {targetPath} (ノードタイプ: None)");
                }
            }
        }

        [TestMethod]
        public void AddLinkToDictionary_Test()
        {
            // ユーザーデータ
            BinaryData data = [1, 2, 3];

            // 仮想ストレージの初期化
            VirtualStorage<BinaryData> vs = new();

            // テストデータの作成
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", data);
            vs.AddDirectory("/data");

            // リンクの作成
            vs.AddSymbolicLink("/data/linkToItem1", "/dir1/item1");

            // 検査
            var linkDictionary = vs.LinkDictionary;
            Assert.IsTrue(linkDictionary.ContainsKey("/dir1/item1"));
            Assert.IsTrue(linkDictionary["/dir1/item1"].Contains("/data/linkToItem1"));

            // リンク辞書の中身をデバッグ出力
            Debug.WriteLine("Step1:");
            DebugPrintLinkDictionary(vs);

            // リンクの作成
            vs.AddSymbolicLink("/data/linkToItem2", "/dir1/item1");

            // 検査
            Assert.IsTrue(linkDictionary.ContainsKey("/dir1/item1"));
            Assert.IsTrue(linkDictionary["/dir1/item1"].Contains("/data/linkToItem2"));

            // リンク辞書の中身をデバッグ出力
            Debug.WriteLine("Step2:");
            DebugPrintLinkDictionary(vs);
        }

        // 絶対パスのターゲットパスとリンクパスを追加するテスト
        [TestMethod]
        public void AddLinkToDictionary_AddsLinkSuccessfully()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/absolute/dir1";
            VirtualPath linkPath = "/absolute/link1";

            // Act
            vs.AddDirectory("/absolute/dir1", true);
            vs.AddSymbolicLink(linkPath, targetPath);

            // Assert
            var links = vs.GetLinksFromDictionary(targetPath);
            Assert.IsTrue(links.Contains(linkPath));
            var link = vs.GetSymbolicLink(linkPath);
            Assert.AreEqual(link.TargetNodeType, vs.GetNodeType(targetPath, true));

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // 相対パスのリンクパスを正しく変換して追加するテスト
        [TestMethod]
        public void AddLinkToDictionary_ConvertsRelativeLinkPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/absolute/dir1";
            VirtualPath linkPath = "relative/r-dir2";

            // Act
            vs.AddDirectory("/absolute/dir1", true);
            vs.AddDirectory("/relative", true);
            vs.AddSymbolicLink(linkPath, targetPath);

            // Assert
            var links = vs.GetLinksFromDictionary(targetPath);
            var absoluteLinkPath = vs.ConvertToAbsolutePath(linkPath).NormalizePath();
            Assert.IsTrue(links.Contains(absoluteLinkPath));

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // 既に存在するターゲットパスに新しいリンクパスを追加するテスト
        [TestMethod]
        public void AddLinkToDictionary_AddsNewLinkToExistingTarget()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/absolute/dir1";
            VirtualPath linkPath1 = "/absolute/link1";
            VirtualPath linkPath2 = "/absolute/link2";
            vs.AddDirectory("/absolute/dir1", true);

            // Act
            vs.AddSymbolicLink(linkPath1, targetPath);
            vs.AddSymbolicLink(linkPath2, targetPath);

            // Assert
            var links = vs.GetLinksFromDictionary(targetPath);
            Assert.IsTrue(links.Contains(linkPath1));
            Assert.IsTrue(links.Contains(linkPath2));

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // リンクパスが正しく正規化されることを確認するテスト
        [TestMethod]
        public void AddLinkToDictionary_NormalizesLinkPath()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath targetPath = "/absolute/dir1";
            VirtualPath linkPath = "/absolute/link1/../link1";
            vs.AddDirectory("/absolute/dir1", true);

            // Act
            vs.AddSymbolicLink(linkPath, targetPath);

            // Assert
            var links = vs.GetLinksFromDictionary(targetPath);
            var normalizedLinkPath = linkPath.NormalizePath();
            Assert.IsTrue(links.Contains(normalizedLinkPath));

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // リンクのターゲットノードタイプが正しく設定されることを確認するテスト
        [TestMethod]
        public void AddLinkToDictionary_SetsCorrectTargetNodeType()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();

            // テスト用のパス設定
            BinaryData data = [1, 2, 3];
            VirtualPath directoryTargetPath = "/absolute/dir1";
            VirtualPath itemTargetPath = "/absolute/item1";
            VirtualPath symbolicLinkTargetPath = "/absolute/link1";
            VirtualPath nonExistentTargetPath = "/absolute/nonexistent";

            VirtualPath directoryLinkPath = "/absolute/linkToDir";
            VirtualPath itemLinkPath = "/absolute/linkToItem";
            VirtualPath symbolicLinkPath = "/absolute/linkToLink";
            VirtualPath nonExistentLinkPath = "/absolute/linkToNonExistent";

            // ディレクトリとリンクの作成
            vs.AddDirectory(directoryTargetPath, true);
            vs.AddItem("/absolute", new VirtualItem<BinaryData>("item1", data));
            vs.AddSymbolicLink(symbolicLinkTargetPath, directoryTargetPath);

            // Act
            vs.AddSymbolicLink(directoryLinkPath, directoryTargetPath);
            vs.AddSymbolicLink(itemLinkPath, itemTargetPath);
            vs.AddSymbolicLink(symbolicLinkPath, symbolicLinkTargetPath);
            vs.AddSymbolicLink(nonExistentLinkPath, nonExistentTargetPath);

            // Assert
            Assert.AreEqual(vs.GetSymbolicLink(directoryLinkPath).TargetNodeType, vs.GetNodeType(directoryTargetPath));
            Assert.AreEqual(vs.GetSymbolicLink(itemLinkPath).TargetNodeType, vs.GetNodeType(itemTargetPath));
            Assert.AreEqual(vs.GetSymbolicLink(symbolicLinkPath).TargetNodeType, vs.GetNodeType(symbolicLinkTargetPath));
            Assert.AreEqual(vs.GetSymbolicLink(nonExistentLinkPath).TargetNodeType, vs.GetNodeType(nonExistentTargetPath));

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // ターゲットパスが存在しないシンボリックリンクを作成し、ディレクトリを追加してリンクノードタイプを更新するテスト
        [TestMethod]
        public void AddDirectory_UpdatesLinkTypeForNonExistentTargetToDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryTargetPath = "/absolute/dir1";
            VirtualPath linkPath = "/absolute/linkToDir";

            // 存在しないターゲットパスに対するシンボリックリンクを作成
            vs.AddDirectory("/absolute", true);
            vs.AddSymbolicLink(linkPath, directoryTargetPath);

            // リンクのノードタイプが None であることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Act
            // ディレクトリを追加してリンクノードタイプを更新
            vs.AddDirectory(directoryTargetPath, true);

            // Assert
            // リンクのノードタイプが Directory に更新されていることを確認
            Assert.AreEqual(VirtualNodeType.Directory, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // ターゲットパスが存在しないシンボリックリンクを作成し、アイテムを追加してリンクノードタイプを更新するテスト
        [TestMethod]
        public void AddItem_UpdatesLinkTypeForNonExistentTargetToItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath itemTargetPath = "/absolute/item1";
            VirtualPath linkPath = "/absolute/linkToItem";

            // 存在しないターゲットパスに対するシンボリックリンクを作成
            vs.AddDirectory("/absolute", true);
            vs.AddSymbolicLink(linkPath, itemTargetPath);

            // リンクのノードタイプが None であることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);

            // Act
            // アイテムを追加してリンクノードタイプを更新
            BinaryData data = [1, 2, 3];
            vs.AddItem(itemTargetPath, data);

            // Assert
            // リンクのノードタイプが Item に更新されていることを確認
            Assert.AreEqual(VirtualNodeType.Item, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // ターゲットパスが存在しないシンボリックリンクを作成し、シンボリックリンクを追加してリンクノードタイプを更新するテスト
        [TestMethod]
        public void AddSymbolicLink_UpdatesLinkTypeForNonExistentTargetToSymbolicLink()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath dir1 = "/absolute/dir1";
            VirtualPath linkToDir1 = "/absolute/link";
            VirtualPath linkToLink = "/absolute/linkToLink";

            // 存在しないターゲットパスに対するシンボリックリンクを作成
            vs.AddDirectory("/absolute", true);
            vs.AddSymbolicLink(linkToLink, linkToDir1);

            // リンクのノードタイプが None であることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToLink).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);

            // Act
            // シンボリックリンクを追加してリンクノードタイプを更新
            vs.AddSymbolicLink(linkToDir1, dir1);

            // Assert
            // リンクのノードタイプが SymbolicLink に更新されていることを確認
            Assert.AreEqual(VirtualNodeType.SymbolicLink, vs.GetSymbolicLink(linkToLink).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        public void AddDirectory_UpdatesLinkTypeForNonExistentIntermediateTargetToDirectory()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath intermediatePath = "/dir1/dir2";
            VirtualPath finalDirectoryPath = "/dir1/dir2/dir3";
            VirtualPath linkPath = "/dir1/linkToDir2";

            // 存在しない中間ディレクトリに対するシンボリックリンクを作成
            vs.AddDirectory("/dir1", true);
            vs.AddSymbolicLink(linkPath, intermediatePath);

            // リンクのノードタイプが None であることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);

            // Act
            // 中間ディレクトリを追加してリンクノードタイプを更新
            vs.AddDirectory(finalDirectoryPath, true);

            // Assert
            // リンクのノードタイプが Directory に更新されていることを確認
            Assert.AreEqual(VirtualNodeType.Directory, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);
        }
        
        // アイテム削除時のリンク辞書更新のテスト
        [TestMethod]
        public void RemoveNode_UpdatesLinkTypeForItemDeletion()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath itemPath = "/dir1/item1";
            VirtualPath linkPath = "/dir1/linkToItem1";
            
            // アイテムを追加
            BinaryData data = [1, 2, 3];
            vs.AddDirectory("/dir1", true);
            vs.AddItem(itemPath, data);

            // アイテムに対するシンボリックリンクを作成
            vs.AddSymbolicLink(linkPath, itemPath);

            // リンクのノードタイプが Item であることを確認
            Assert.AreEqual(VirtualNodeType.Item, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);

            // Act
            // アイテムを削除
            vs.RemoveNode(itemPath);

            // Assert
            // リンクのノードタイプが None に更新されていることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // ディレクトリツリー削除時のリンク辞書更新のテスト
        [TestMethod]
        public void RemoveNode_UpdatesLinkTypeForDirectoryTreeDeletion()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath directoryPath = "/dir1/dir2";
            VirtualPath subDirectoryPath = "/dir1/dir2/dir3";
            VirtualPath itemPath1 = "/dir1/dir2/item1";
            VirtualPath itemPath2 = "/dir1/dir2/dir3/item2";
            VirtualPath linkToItem1Path = "/dir1/linkToItem1";
            VirtualPath linkToItem2Path = "/dir1/linkToItem2";
            VirtualPath linkToDirectoryPath = "/dir1/linkToDir2";

            // ディレクトリとアイテムを追加
            BinaryData data1 = [1, 2, 3];
            BinaryData data2 = [4, 5, 6];
            vs.AddDirectory("/dir1", true);
            vs.AddDirectory(directoryPath, true);
            vs.AddDirectory(subDirectoryPath, true);
            vs.AddItem(itemPath1, data1);
            vs.AddItem(itemPath2, data2);

            // アイテムとディレクトリに対するシンボリックリンクを作成
            vs.AddSymbolicLink(linkToItem1Path, itemPath1);
            vs.AddSymbolicLink(linkToItem2Path, itemPath2);
            vs.AddSymbolicLink(linkToDirectoryPath, directoryPath);

            // リンクのノードタイプが適切であることを確認
            Assert.AreEqual(VirtualNodeType.Item, vs.GetSymbolicLink(linkToItem1Path).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.Item, vs.GetSymbolicLink(linkToItem2Path).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.Directory, vs.GetSymbolicLink(linkToDirectoryPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);

            // Act
            // ディレクトリツリーを削除
            vs.RemoveNode(directoryPath, recursive: true);

            // Assert
            // リンクのノードタイプが None に更新されていることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToItem1Path).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToItem2Path).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToDirectoryPath).TargetNodeType);

            // Debug print
            DebugPrintLinkDictionary(vs);
        }

        // シンボリックリンク削除時のリンク辞書更新のテスト
        [TestMethod]
        public void RemoveNode_UpdatesLinkTypeForSymbolicLinkDeletion()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath itemPath = "/dir1/item1";
            VirtualPath linkToItemPath = "/dir1/linkToItem1";
            VirtualPath linkToLinkPath = "/dir1/linkToLink";

            // アイテムを追加
            BinaryData data = [1, 2, 3];
            vs.AddDirectory("/dir1", true);
            vs.AddItem(itemPath, data);

            // アイテムに対するシンボリックリンクを作成
            vs.AddSymbolicLink(linkToItemPath, itemPath);

            // シンボリックリンクに対するシンボリックリンクを作成
            vs.AddSymbolicLink(linkToLinkPath, linkToItemPath);

            // linkToLinkのターゲットノードタイプがシンボリックリンクであることを確認
            Assert.AreEqual(VirtualNodeType.SymbolicLink, vs.GetSymbolicLink(linkToLinkPath).TargetNodeType);

            // linkToItem1のターゲットノードタイプがアイテムであることを確認
            Assert.AreEqual(VirtualNodeType.Item, vs.GetSymbolicLink(linkToItemPath).TargetNodeType);

            // Debug print before Act
            DebugPrintLinkDictionary(vs);

            // Act
            // シンボリックリンクを削除
            vs.RemoveNode(linkToItemPath);

            // Assert
            // linkToLinkのターゲットノードタイプがNoneに更新されていることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToLinkPath).TargetNodeType);

            // 削除されたリンクの情報がリンク辞書から削除されてる事を確認
            Assert.IsFalse(vs.LinkDictionary.ContainsKey(itemPath));

            // Debug print after Act
            DebugPrintLinkDictionary(vs);
        }

        // 複数のシンボリックリンクが張られている場合のリンク辞書更新のテスト
        [TestMethod]
        public void RemoveNode_UpdatesLinkTypeForMultipleSymbolicLinkDeletion()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath itemPath = "/dir1/item1";
            VirtualPath linkToItemPath1 = "/dir1/linkToItem1";
            VirtualPath linkToItemPath2 = "/dir1/linkToItem2";

            // アイテムを追加
            BinaryData data = [1, 2, 3];
            vs.AddDirectory("/dir1", true);
            vs.AddItem(itemPath, data);

            // アイテムに対するシンボリックリンクを2つ作成
            vs.AddSymbolicLink(linkToItemPath1, itemPath);
            vs.AddSymbolicLink(linkToItemPath2, itemPath);

            // Debug print before Act
            DebugPrintLinkDictionary(vs);

            // Act
            // 最初のシンボリックリンクを削除
            vs.RemoveNode(linkToItemPath1);

            // Assert
            // linkToItem1がリンク辞書から削除され、linkToItem2は残っていることを確認
            var links = vs.GetLinksFromDictionary(itemPath);
            Assert.IsFalse(links.Contains(linkToItemPath1));
            Assert.IsTrue(links.Contains(linkToItemPath2));

            // リンク辞書のエントリが削除されていないことを確認
            Assert.IsTrue(vs.LinkDictionary.ContainsKey(itemPath));

            // Debug print after Act
            DebugPrintLinkDictionary(vs);
        }

        // シンボリックリンクを考慮した再帰的削除のテスト
        [TestMethod]
        public void RemoveNode_UpdatesLinkTypesForRecursiveDeletionWithSymbolicLinks()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualPath dir1 = "/dir1";
            VirtualPath dir2 = "/dir1/dir2";
            VirtualPath itemPath = "/dir1/dir2/item";
            VirtualPath dir3 = "/dir1/dir2/dir3";
            VirtualPath linkToItemPath = "/linkToItem";
            VirtualPath linkToDir3Path = "/linkToDir3";
            VirtualPath linkToLinkPath = "/linkToLink";
            VirtualPath dir4LinkPath = "/dir1/dir2/link";
            VirtualPath dir4Target = "/dir4";

            // ディレクトリとアイテムを追加
            BinaryData data = [1, 2, 3];
            vs.AddDirectory(dir1, true);
            vs.AddDirectory(dir2, true);
            vs.AddDirectory(dir3, true);
            vs.AddDirectory(dir4Target, true);
            vs.AddItem(itemPath, data);

            // シンボリックリンクを作成
            vs.AddSymbolicLink(linkToItemPath, itemPath);
            vs.AddSymbolicLink(linkToDir3Path, dir3);
            vs.AddSymbolicLink(linkToLinkPath, dir4LinkPath);
            vs.AddSymbolicLink(dir4LinkPath, dir4Target);

            // 各リンクのターゲットノードタイプを確認
            Assert.AreEqual(VirtualNodeType.Item, vs.GetSymbolicLink(linkToItemPath).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.Directory, vs.GetSymbolicLink(linkToDir3Path).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.SymbolicLink, vs.GetSymbolicLink(linkToLinkPath).TargetNodeType);

            // リンク辞書に dir4LinkPath のエントリが存在している事を確認
            Assert.IsTrue(vs.LinkDictionary.ContainsKey(dir4Target));

            // Debug print before Act
            DebugPrintLinkDictionary(vs);

            // Act
            // シンボリックリンクを考慮した再帰的削除
            vs.RemoveNode(dir2, recursive: true, followLinks: true);

            // Assert
            // 各リンクのターゲットノードタイプがNoneに更新されていることを確認
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToItemPath).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToDir3Path).TargetNodeType);
            Assert.AreEqual(VirtualNodeType.None, vs.GetSymbolicLink(linkToLinkPath).TargetNodeType);

            // リンク辞書から dir4LinkPathの エントリが削除されている事を確認
            Assert.IsFalse(vs.LinkDictionary.ContainsKey(dir4Target));

            // Debug print after Act
            DebugPrintLinkDictionary(vs);
        }

        [TestMethod]
        public void Indexer_GetItem_ReturnsItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1", data);

            // Act
            var item = (VirtualItem<BinaryData>)vs["/dir1/item1"];

            // Assert
            Assert.AreEqual("item1", (string)item.Name);
            CollectionAssert.AreEqual(data.Data, item.ItemData!.Data);
        }

        [TestMethod]
        public void Indexer_SetItem_CreateItem()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            BinaryData data = [1, 2, 3];
            vs.AddDirectory("/dir1");

            // Act
            vs["/dir1"] = new VirtualItem<BinaryData>("item1", data);

            // Assert
            var item = vs.GetItem("/dir1/item1");
            Assert.AreEqual("item1", (string)item.Name);
            CollectionAssert.AreEqual(data.Data, item.ItemData!.Data);
        }
    }
}
