using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualExceptionTests : VirtualTestBase
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            VirtualStorageSettings.Initialize();
            VirtualNodeName.ResetCounter();
        }

        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            VirtualNodeNotFoundException exception = new();
            Assert.IsNotNull(exception);
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
