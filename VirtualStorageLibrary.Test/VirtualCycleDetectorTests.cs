namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualCycleDetectorTests
    {
        [TestMethod]
        public void Clear_RemovesAllCheckedNodes_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualPath path1 = new("/Item1");
            VirtualPath path2 = new("/Dir1");

            detector.IsNodeInCycle(path1);
            detector.IsNodeInCycle(path2);

            detector.Clear();

            Assert.IsFalse(detector.CheckedNodeSet.Contains(path1));
            Assert.IsFalse(detector.CheckedNodeSet.Contains(path2));
        }

        [TestMethod]
        public void IsNodeInCycle_ReturnsFalseForUniqueNodes_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualPath path1 = new("/Item1");
            VirtualPath path2 = new("/Dir1");

            Assert.IsFalse(detector.IsNodeInCycle(path1));
            Assert.IsFalse(detector.IsNodeInCycle(path2));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(path1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(path2));
        }

        [TestMethod]
        public void IsNodeInCycle_ReturnsTrueForDuplicateNode_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualPath path1 = new("/Item1");

            Assert.IsFalse(detector.IsNodeInCycle(path1));
            Assert.IsTrue(detector.IsNodeInCycle(path1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(path1));
        }

        [TestMethod]
        public void IsNodeInCycle_ReturnsTrueForCyclicNode_Success()
        {
            VirtualCycleDetector detector = new();
            VirtualPath path1 = new("/Item1");
            VirtualPath path2 = new("/Dir1");

            Assert.IsFalse(detector.IsNodeInCycle(path1));
            Assert.IsFalse(detector.IsNodeInCycle(path2));
            Assert.IsTrue(detector.IsNodeInCycle(path1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(path1));
            Assert.IsTrue(detector.CheckedNodeSet.Contains(path2));
        }
    }
}
