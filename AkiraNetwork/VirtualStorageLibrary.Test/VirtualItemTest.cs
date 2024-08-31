// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU Lesser General Public License (version 3, or at your option, any later 
// version). This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for 
// more details.
//
// You should have received a copy of the GNU Lesser General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using System.Diagnostics;
using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualItemTest : VirtualTestBase

    {
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            VirtualStorageSettings.Initialize();
            VirtualNodeName.ResetCounter();
        }

        // ノード名を指定しない場合、自動生成された名前で VirtualItem オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void VirtualItemConstructorDefault_CreatesObjectCorrectly()
        {
            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualItem<BinaryData> item = new();

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);
            Assert.AreEqual($"{VirtualStorageState.State.PrefixItem}1", (string)item.Name);
            Assert.AreEqual(default, item.ItemData);
        }

        [TestMethod]
        public void VirtualItemConstructorByName_CreatesObjectCorrectly()
        {
            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> item = new(name);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(default, item.ItemData);
        }

        [TestMethod]
        public void VirtualItemConstructor_CreatesObjectCorrectly()
        {
            // テストデータ
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new(testData);

            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> item = new(name, binaryData);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(binaryData, item.ItemData);
            CollectionAssert.AreEqual(item.ItemData!.Data, testData);
        }

        [TestMethod]
        public void VirtualItemConstructorWithDates_CreatesObjectCorrectly()
        {
            // テストデータ
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new(testData);

            // 日付
            DateTime createdDate = new(2000, 1, 1, 0, 0, 0);
            DateTime updatedDate = new(2000, 1, 2, 0, 0, 0);

            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> item = new(name, binaryData, createdDate, updatedDate);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(binaryData, item.ItemData);
            CollectionAssert.AreEqual(item.ItemData!.Data, testData);
            Assert.AreEqual(createdDate, item.CreatedDate);
            Assert.AreEqual(updatedDate, item.UpdatedDate);
        }

        [TestMethod]
        public void VirtualItemConstructorWithCreatedDate_CreatesObjectCorrectly()
        {
            // テストデータ
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new(testData);

            // 日付
            DateTime createdDate = new(2000, 1, 1, 0, 0, 0);

            // VirtualItem<BinaryData> オブジェクトを作成
            VirtualNodeName name = "TestBinaryItem";
            VirtualItem<BinaryData> item = new(name, binaryData, createdDate);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(binaryData, item.ItemData);
            CollectionAssert.AreEqual(item.ItemData!.Data, testData);
            Assert.AreEqual(createdDate, item.CreatedDate);
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
        public void ToString_ReturnsItemInformation()
        {
            int value = 10;
            VirtualItem<int> item = new("TestItem", value);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        [TestMethod]
        public void ToString_ReturnsItemInformationByVirtualNode()
        {
            int value = 10;
            VirtualItem<int> item = new("TestItem", value);

            string result = ((VirtualNode)item).ToString()!;
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
            SimpleStruct data = new() { Value = 10 };

            VirtualItem<SimpleStruct> item = new("TestItem", data);

            string result = item.ToString();
            Debug.WriteLine(result);

            Assert.IsTrue(result.Contains("TestItem"));
        }

        // タプルからの暗黙的な変換で VirtualItem オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromTuple_CreatesObjectCorrectly()
        {
            // テストデータ
            VirtualNodeName nodeName = "TestBinaryItem";
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new(testData);

            // タプルを利用して VirtualItem<BinaryData> オブジェクトを作成
            VirtualItem<BinaryData> item = (nodeName, binaryData);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);
            Assert.AreEqual(nodeName, item.Name);
            Assert.AreEqual(binaryData, item.ItemData);
            CollectionAssert.AreEqual(item.ItemData!.Data, testData);
        }

        // データからの暗黙的な変換でデフォルトの名前を持つ VirtualItem オブジェクトが作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromData_CreatesObjectWithDefaultName()
        {
            // テストデータ
            byte[] testData = [1, 2, 3];

            // BinaryData オブジェクトを作成
            BinaryData binaryData = new(testData);

            // データを利用して VirtualItem<BinaryData> オブジェクトを作成
            VirtualItem<BinaryData> item = binaryData;

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(item);

            // プレフィックスの後の番号まで検証
            string expectedPrefix = VirtualStorageState.State.PrefixItem;
            string expectedName = $"{expectedPrefix}1";
            Assert.AreEqual(expectedName, item.Name.ToString());

            Assert.AreEqual(binaryData, item.ItemData);
            CollectionAssert.AreEqual(item.ItemData!.Data, testData);

            Debug.WriteLine($"Generated NodeName: {item.Name}");
        }

        // ノード名からの暗黙的な変換でVirtualItem オブジェクトが作成されることを検証します。
        //[TestMethod]
        //public void ImplicitConversionFromNodeName_CreatesObjectWithDefaultName()
        //{
        //    // テストデータ
        //    VirtualNodeName nodeName = "TestBinaryItem";

        //    // データを利用して VirtualItem<BinaryData> オブジェクトを作成
        //    VirtualItem<BinaryData> item = nodeName;

        //    // オブジェクトが正しく作成されたか検証
        //    Assert.IsNotNull(item);

        //    Debug.WriteLine(item.Name);
        //}

        [TestMethod]
        public void Update_ValidItem_UpdatesCorrectly()
        {
            // Arrange
            VirtualItem<BinaryData> originalItem = new("OriginalItem", [1, 2, 3]);
            VirtualItem<BinaryData> newItem = new("NewItem", [4, 5, 6]);

            // Act
            originalItem.Update(newItem);

            // Assert
            Assert.AreEqual(originalItem.CreatedDate, newItem.CreatedDate);
            Assert.IsTrue(originalItem.UpdatedDate > newItem.UpdatedDate);
            Assert.AreEqual(originalItem.ItemData, newItem.ItemData);
            Assert.IsFalse(originalItem.IsReferencedInStorage);
        }

        [TestMethod]
        public void Update_InvalidNode_ThrowsArgumentException()
        {
            // Arrange
            VirtualItem<BinaryData> originalItem = new("OriginalItem", [1, 2, 3]);
            VirtualDirectory directoryNode = new("DirectoryNode");

            // Act & Assert
            Exception err = Assert.ThrowsException<ArgumentException>(() => originalItem.Update(directoryNode));

            Assert.AreEqual("The specified node [DirectoryNode] is not of type VirtualItem<BinaryData>. (Parameter 'node')", err.Message);

            Debug.WriteLine(err.Message);

        }

        [TestMethod]
        public void Update_ReferencedItem_CreatesClone()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            VirtualItem<BinaryData> originalItem = new("OriginalItem", [1, 2, 3]);
            vs.AddItem("/NewItem", [4, 5, 6]);

            // Act
            VirtualItem<BinaryData> newItem = vs.GetItem("/NewItem");
            originalItem.Update(newItem);

            // Assert
            Assert.AreNotSame(originalItem, newItem);
            CollectionAssert.AreEqual(originalItem.ItemData!.Data, newItem.ItemData!.Data);
            Assert.IsTrue(originalItem.UpdatedDate > newItem.UpdatedDate);
            Assert.IsFalse(originalItem.IsReferencedInStorage);
        }

        [TestMethod]
        public void Update_ItemDataIsUpdated()
        {
            // Arrange
            VirtualItem<BinaryData> originalItem = new("OriginalItem", [1, 2, 3]);
            VirtualItem<BinaryData> newItem = new("NewItem", [4, 5, 6]);

            // Act
            originalItem.Update(newItem);

            // Assert
            Assert.AreEqual(originalItem.ItemData, newItem.ItemData);
            Assert.IsFalse(originalItem.IsReferencedInStorage);
        }

        [TestMethod]
        public void Update_UpdatedDateIsChanged()
        {
            // Arrange
            VirtualItem<BinaryData> originalItem = new("OriginalItem", [1, 2, 3]);
            VirtualItem<BinaryData> newItem = new("NewItem", [4, 5, 6]);
            DateTime beforeDate = originalItem.UpdatedDate;

            // Act
            originalItem.Update(newItem);

            // Assert
            Assert.IsTrue(originalItem.UpdatedDate > beforeDate);
            Assert.IsFalse(originalItem.IsReferencedInStorage);
        }
    }
}
