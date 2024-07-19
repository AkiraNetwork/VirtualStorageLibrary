namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualNodeTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            VirtualStorageSettings.Initialize();
        }

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
            DateTime now = DateTime.Now;
            DateTime createdDate = now;
            DateTime updatedDate = now;

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
}
