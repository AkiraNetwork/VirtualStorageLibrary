namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualCycleDetectorTests
    {
        [TestMethod]
        public void IsNodeInCycle_NodeNotInCycle_ReturnsFalseAndAddsNode()
        {
            // Arrange
            VirtualCycleDetector detector = new();
            VirtualSymbolicLink link = new("link");

            // Act
            bool result = detector.IsNodeInCycle(link);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(detector.CycleDictionary.ContainsKey(link.VID));
        }

        [TestMethod]
        public void IsNodeInCycle_NodeInCycle_ReturnsTrue()
        {
            // Arrange
            VirtualCycleDetector detector = new();
            VirtualSymbolicLink link = new("link");
            detector.IsNodeInCycle(link);

            // Act
            bool result = detector.IsNodeInCycle(link);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Clear_RemovesAllNodes()
        {
            // Arrange
            VirtualCycleDetector detector = new();
            VirtualSymbolicLink link = new("link");
            detector.IsNodeInCycle(link);

            // Act
            detector.Clear();

            // Assert
            Assert.AreEqual(0, detector.Count);
        }
    }
}
