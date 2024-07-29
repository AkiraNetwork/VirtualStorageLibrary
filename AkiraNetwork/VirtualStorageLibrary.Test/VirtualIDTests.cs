using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualIDTests : VirtualTestBase
    {
        private const int GuidCount = 10000;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            VirtualStorageSettings.Initialize();
            VirtualNodeName.ResetCounter();
        }

        [TestMethod]
        public void VirtualID_ShouldReturnValidGuid()
        {
            // Act
            VirtualID vID = new();
            Guid guid = vID.ID;

            // Assert
            Assert.AreNotEqual(Guid.Empty, guid);
        }

        [TestMethod]
        public void VirtualID_ShouldReturnUniqueGuids()
        {
            // Arrange
            HashSet<Guid> guidSet = [];

            // Act
            for (int i = 0; i < GuidCount; i++)
            {
                VirtualID vID = new();
                Guid newGuid = vID.ID;
                bool isAdded = guidSet.Add(newGuid);

                // Assert
                Assert.IsTrue(isAdded, $"Duplicate GUID found: {newGuid}");
            }

            // Final Assert
            Assert.AreEqual(GuidCount, guidSet.Count);
        }
    }
}
