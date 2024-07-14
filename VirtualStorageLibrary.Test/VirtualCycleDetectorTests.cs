namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualCycleDetectorTests
    {
        [TestMethod]
        public void Clear_RemovesAllCheckedNodes_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualItem<string> item1 = new("Item1", "Data1");
            VirtualDirectory dir1 = new("Dir1");

            detector.IsNodeInCycle(item1);
            detector.IsNodeInCycle(dir1);

            detector.Clear();

            Assert.IsFalse(detector.CheckedNodeSet.Contains(item1.VID));
            Assert.IsFalse(detector.CheckedNodeSet.Contains(dir1.VID));
        }

        [TestMethod]
        public void IsNodeInCycle_ReturnsFalseForUniqueNodes_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualItem<string> item1 = new("Item1", "Data1");
            VirtualDirectory dir1 = new("Dir1");

            Assert.IsFalse(detector.IsNodeInCycle(item1));
            Assert.IsFalse(detector.IsNodeInCycle(dir1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(item1.VID));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(dir1.VID));
        }

        [TestMethod]
        public void IsNodeInCycle_ReturnsTrueForDuplicateNode_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualItem<string> item1 = new("Item1", "Data1");

            Assert.IsFalse(detector.IsNodeInCycle(item1));
            Assert.IsTrue(detector.IsNodeInCycle(item1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(item1.VID));
        }

        [TestMethod]
        public void IsNodeInCycle_ReturnsTrueForCyclicNode_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualItem<string> item1 = new("Item1", "Data1");
            VirtualDirectory dir1 = new("Dir1");

            Assert.IsFalse(detector.IsNodeInCycle(item1));
            Assert.IsFalse(detector.IsNodeInCycle(dir1));
            Assert.IsTrue(detector.IsNodeInCycle(item1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(item1.VID));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(dir1.VID));
        }
    }
}
