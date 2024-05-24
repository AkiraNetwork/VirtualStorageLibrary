using System.Diagnostics;

namespace AkiraNet.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualItemTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            VirtualStorageSettings.Initialize();
        }

        [TestMethod]
        public void VirtualItemConstructorDefault_CreatesObjectCorrectly()
        {
            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> virtualItem = new(name);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(virtualItem);
            Assert.AreEqual(name, virtualItem.Name);
            Assert.AreEqual(default, virtualItem.ItemData);
        }

        [TestMethod]
        public void VirtualItemConstructor_CreatesObjectCorrectly()
        {
            // テストデータ
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new BinaryData(testData);

            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> virtualItem = new(name, binaryData);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(virtualItem);
            Assert.AreEqual(name, virtualItem.Name);
            Assert.AreEqual(binaryData, virtualItem.ItemData);
            CollectionAssert.AreEqual(virtualItem.ItemData!.Data, testData);
        }

        [TestMethod]
        public void VirtualItemDeepClone_CreatesDistinctCopyWithSameData()
        {
            // BinaryData オブジェクトを作成
            byte[] testData = [1, 2, 3];
            VirtualItem<BinaryData> originalItem = new("TestItem", new(testData));

            // DeepClone メソッドを使用してクローンを作成
            VirtualItem<BinaryData>? clonedItem = originalItem.DeepClone() as VirtualItem<BinaryData>;

            // クローンが正しく作成されたか検証
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            Assert.AreNotSame(originalItem.ItemData, clonedItem.ItemData);

            // BinaryData の Data プロパティが適切にクローンされていることを検証
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, clonedItem.ItemData!.Data);
            Assert.AreNotSame(originalItem.ItemData!.Data, clonedItem.ItemData!.Data);
        }

        [TestMethod]
        public void DeepClone_ReturnsDeepCopyOfVirtualItem()
        {
            // Arrange
            VirtualItem<BinaryData> originalItem = new("item", [1, 2, 3]);

            // Act
            VirtualItem<BinaryData> clonedItem = (VirtualItem<BinaryData>)originalItem.DeepClone();

            // Assert
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, clonedItem.ItemData!.Data);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);
            Assert.AreNotSame(originalItem.ItemData, clonedItem.ItemData);
        }

        [TestMethod]
        public void DeepClone_WithNonDeepCloneableItem_ReturnsShallowCopyOfVirtualItem()
        {
            // Arrange
            VirtualItem<SimpleData> originalItem = new("item", new(5));

            // Act
            VirtualItem<SimpleData> clonedItem = (VirtualItem<SimpleData>)originalItem.DeepClone();

            // Assert
            Assert.IsNotNull(clonedItem);
            Assert.AreNotSame(originalItem, clonedItem);
            Assert.AreEqual(originalItem.ItemData!.Value, clonedItem.ItemData!.Value);
            Assert.AreEqual(originalItem.Name, clonedItem.Name);

            // SimpleDataインスタンスがシャローコピーされていることを確認
            Assert.AreSame(originalItem.ItemData, clonedItem.ItemData);
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithInt()
        {
            int value = 10;
            VirtualItem<int> item = new("TestItem", value);

            string result = item.ToString();
            Debug.WriteLine(result);
            
            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithBinaryDataOfLong()
        {
            BinaryData data = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F];
            VirtualItem<BinaryData> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithBinaryDataOfShort()
        {
            BinaryData data = [0x01, 0x02, 0x03];
            VirtualItem<BinaryData> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithSimpleData()
        {
            SimpleData data = new(10);
            VirtualItem<SimpleData> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        struct SimpleStruct
        {
            public int Value { get; set; }
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationWithStruct()
        {
            SimpleStruct data = new SimpleStruct { Value = 10 };

            VirtualItem<SimpleStruct> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }
    }
}
