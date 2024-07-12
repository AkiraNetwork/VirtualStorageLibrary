namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualSymbolicLinkTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            VirtualStorageSettings.Initialize();
            VirtualNodeName.ResetCounter();
        }

        [TestMethod]
        public void VirtualSymbolicLink_Constructor_Default()
        {
            // Act
            VirtualSymbolicLink link = new();

            // Assert
            Assert.AreEqual($"{VirtualStorageState.State.PrefixSymbolicLink}1", (string)link.Name);
            Assert.AreEqual(null, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_Constructor_ByName()
        {
            // Arrange
            VirtualNodeName nodeName = "TestLink";

            // Act
            VirtualSymbolicLink link = new(nodeName);

            // Assert
            Assert.AreEqual(nodeName, link.Name);
            Assert.AreEqual(null, link.TargetPath);
        }

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
            DateTime now = DateTime.Now;
            DateTime createdDate = now;
            DateTime updatedDate = now;

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
            Assert.AreNotEqual(link.CreatedDate, clone.CreatedDate);
            Assert.AreNotEqual(link.UpdatedDate, clone.UpdatedDate);
        }

        // VirtualPathからの暗黙的な変換で VirtualSymbolicLink オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromVirtualPath_CreatesObjectWithDefaultName()
        {
            // テストデータ
            VirtualPath targetPath = "/some/path";

            // VirtualPathを利用して VirtualSymbolicLink オブジェクトを作成
            VirtualSymbolicLink link = targetPath;

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(link);

            // プレフィックスの後の番号まで検証
            string expectedPrefix = VirtualStorageState.State.PrefixSymbolicLink;
            string expectedName = $"{expectedPrefix}1";
            Assert.AreEqual(expectedName, link.Name.ToString());

            Assert.AreEqual(targetPath, link.TargetPath);
        }

        // タプルからの暗黙的な変換で VirtualSymbolicLink オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromTuple_CreatesObjectCorrectly()
        {
            // テストデータ
            VirtualNodeName nodeName = "MyLink";
            VirtualPath targetPath = "/other/path";

            // タプルを利用して VirtualSymbolicLink オブジェクトを作成
            VirtualSymbolicLink link = (nodeName, targetPath);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(link);
            Assert.AreEqual(nodeName, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }
    }
}
