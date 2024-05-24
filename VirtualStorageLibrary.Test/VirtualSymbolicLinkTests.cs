namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualSymbolicLinkTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            VirtualStorageSettings.Initialize();
        }

        [TestMethod]
        public void VirtualSymbolicLink_Constructor_Default()
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
    }
}
