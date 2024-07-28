using System.Diagnostics;
using AkiraNetwork.VirtualStorageLibrary.Utilities;

namespace AkiraNetwork.VirtualStorageLibrary.Test.Utilities
{
    [TestClass]
    public class VirtualTextFormatterTests
    {
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

            // ツリー構造の表示
            Debug.WriteLine("tree structure (SetupVirtualStorage):");
            string tree = vs.GenerateTreeDebugText("/", true, false);

            Debug.WriteLine(tree);

            return vs;
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_RecursiveWithLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_RecursiveNoLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonRecursiveWithLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", false, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonRecursiveNoLinksFromRoot()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/", false, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonExistentPathWithLinks()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            Assert.ThrowsException<VirtualNodeNotFoundException>(() =>
            {
                string tree = vs.GenerateTreeDebugText("/nonexistent", true, true);
            });
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_RecursiveWithLinksFromSubDirectory()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/dir1", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_NonRecursiveWithLinksFromSubDirectory()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/dir1", false, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_BasePathIsItem()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/item1", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToItem_NoFollow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-item", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToItem_Follow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-item", true, true);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToDirectory_NoFollow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-dir", true, false);
            Debug.WriteLine(tree);
        }

        [TestMethod]
        [TestCategory("GenerateTreeDebugText")]
        public void GenerateTreeDebugText_LinkToDirectory_Follow()
        {
            VirtualStorage<string> vs = SetupVirtualStorage();
            string tree = vs.GenerateTreeDebugText("/link-to-dir", true, true);
            Debug.WriteLine(tree);
        }
    }
}
