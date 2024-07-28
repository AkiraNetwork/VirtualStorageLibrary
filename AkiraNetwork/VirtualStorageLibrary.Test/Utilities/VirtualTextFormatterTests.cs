using System;
using System.Diagnostics;
using System.Net;
using AkiraNetwork.VirtualStorageLibrary.Utilities;

namespace AkiraNetwork.VirtualStorageLibrary.Test.Utilities
{
    public class CustomClassWithoutProperties
    {
        public override string ToString()
        {
            return "CustomClassWithoutProperties";
        }
    }

    public class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Address? Address { get; set; }
    }

    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public override string ToString()
        {
            return $"Street: {Street}; City: {City}";
        }
    }

    public class EmptyClass
    {
        public override string ToString()
        {
            return "EmptyClassInstance";
        }
    }

    public class ObjectWithEnumerableProperty
    {
        public IEnumerable<int>? Values { get; set; }
    }

    public class ObjectWithNonOverriddenToString
    {
        public NonOverriddenToString? Value { get; set; }
    }

    public class NonOverriddenToString
    {
        // このクラスでは ToString メソッドをオーバーライドしない
    }

    public class ObjectWithExceptionInGetter
    {
        public string Value
        {
            get { throw new InvalidOperationException("Exception in getter"); }
        }
    }

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

        [TestCategory("GenerateLinkTableDebugText")]
        [TestMethod]
        public void GenerateLinkTableDebugText_EmptyLinkDictionary_ReturnsEmptyMessage()
        {
            // Arrange
            VirtualStorage<string> vs = new();

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Assert.AreEqual("(リンク辞書は空です。)", result);
        }

        [TestCategory("GenerateLinkTableDebugText")]
        [TestMethod]
        public void GenerateLinkTableDebugText_WithLinks1_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/linkToItem1", "/item1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateLinkTableDebugText")]
        [TestMethod]
        public void GenerateLinkTableDebugText_WithLinks2_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/linkToItem1", "/item1");
            vs.AddSymbolicLink("/linkToItem2", "/item1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateLinkTableDebugText")]
        [TestMethod]
        public void GenerateLinkTableDebugText_WithFullWidth1_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク1", "/アイテム1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateLinkTableDebugText")]
        [TestMethod]
        public void GenerateLinkTableDebugText_WithFullWidth2_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク1", "/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク2", "/アイテム1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateLinkTableDebugText")]
        [TestMethod]
        public void GenerateLinkTableDebugText_Test1_ReturnsLinkTable()
        {
            // Arrange
            VirtualStorage<string> vs = new();
            vs.AddItem("/item1");
            vs.AddSymbolicLink("/linkToItem1", "/item1");
            vs.AddSymbolicLink("/linkToItem2", "/item1");
            vs.AddItem("/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク1", "/アイテム1");
            vs.AddSymbolicLink("/アイテム1へのリンク2", "/アイテム1");

            // Act
            string result = vs.GenerateLinkTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_EmptyTable_ReturnsEmptyMessage()
        {
            // Arrange
            List<string> messages = [];

            // Act
            string result = messages.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.AreEqual("(コレクションは空です。)", result);
        }

        [TestCategory("GenerateTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_StringTable_ReturnsEmptyMessage()
        {
            // Arrange
            List<string> messages = ["Hello, ", "World!", null];

            // Act
            string result = messages.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_IntTable_ReturnsEmptyMessage()
        {
            // Arrange
            List<int> numbers = [100, 200, 300];

            // Act
            string result = numbers.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_NullableTable_ReturnsEmptyMessage()
        {
            // Arrange
            List<int?> numbers = [100, 200, 300, null];

            // Act
            string result = numbers.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_NoPropertyList_ReturnsEmptyMessage()
        {
            // Arrange
            List<CustomClassWithoutProperties?> customs =
            [
                new CustomClassWithoutProperties(),
                new CustomClassWithoutProperties(),
                null
            ];

            // Act
            string result = customs.GenerateTableDebugText();

            // Assert
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_Person_ReturnsFormattedTable()
        {
            // Arrange
            var person = new Person
            {
                Name = "John Doe",
                Age = 30,
                Address = new Address { Street = "123 Main St", City = "Anytown" }
            };

            // Act
            string result = person.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("John"));
            Assert.IsTrue(result.Contains("30"));
            Assert.IsTrue(result.Contains("123 Main St"));
            Assert.IsTrue(result.Contains("Anytown"));
            // Expected output:
            // Name, Age, Address
            // John Doe, 30, 123 Main St, Anytown
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_PersonWithNull_ReturnsFormattedTable()
        {
            // Arrange
            var person = new Person
            {
                Name = null,
                Age = 30,
                Address = null
            };

            // Act
            string result = person.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("(null)"));
            Assert.IsTrue(result.Contains("30"));
            // Expected output:
            // Name, Age, Address
            // (null), 30, (null)
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_EmptyClass_ReturnsFormattedTable()
        {
            // Arrange
            var emptyClass = new EmptyClass();

            // Act
            string result = emptyClass.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("EmptyClassInstance"));
            // Expected output:
            // EmptyClassInstance
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_NullableInt_ReturnsFormattedTable()
        {
            // Arrange
            int? nullableInt = 42;

            // Act
            string result = nullableInt.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("42"));
            // Expected output:
            // Value
            // 42
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_NullableIntWithNull_ReturnsFormattedTable()
        {
            // Arrange
            int? nullableInt = null;

            // Act
            string result = nullableInt.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("(null)"));
            // Expected output:
            // Value
            // (null))
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_SimpleString_ReturnsFormattedTable()
        {
            // Arrange
            string simpleString = "Hello, world!";

            // Act
            string result = simpleString.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("Hello, world!"));
            // Expected output:
            // Value
            // Hello, world!
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_SimpleStringWithNull_ReturnsFormattedTable()
        {
            // Arrange
            string? simpleString = null;

            // Act
            string result = simpleString.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("(null)"));
            // Expected output:
            // Value
            // (null)
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_SimpleInt_ReturnsFormattedTable()
        {
            // Arrange
            int simpleInt = 123;

            // Act
            string result = simpleInt.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("123"));
            // Expected output:
            // Value
            // 123
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_Enumerable_ReturnsFormattedTable()
        {
            // Arrange
            IEnumerable<int> intList = [123, 456, 789];

            // Act
            string result = intList.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("(no output)"));
            // Expected output:
            // (no output)
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_ObjectWithEnumerableProperty_ReturnsNoOutput()
        {
            // Arrange
            var obj = new ObjectWithEnumerableProperty
            {
                Values = new List<int> { 1, 2, 3 }
            };

            // Act
            string result = obj.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("(no output)"));
            // Expected output:
            // Values
            // (no output)
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_ObjectWithNonOverriddenToString_ReturnsFormattedTable()
        {
            // Arrange
            var obj = new ObjectWithNonOverriddenToString
            {
                Value = new NonOverriddenToString()
            };

            // Act
            string result = obj.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("AkiraNetwork.VirtualStorageLibrary.Test.Utilities.NonOverriddenToString"));
            // Expected output:
            // Value
            // AkiraNetwork.VirtualStorageLibrary.Test.Utilities.NonOverriddenToString
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateSingleTableDebugText_ObjectWithExceptionInGetter_ReturnsNoOutput()
        {
            // Arrange
            var obj = new ObjectWithExceptionInGetter();

            // Act
            string result = obj.GenerateSingleTableDebugText();

            // Assert
            Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("(exception)"));
            // Expected output:
            // Value
            // (exception)
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_NoProperty_ReturnsEmptyMessage()
        {
            // Arrange
            CustomClassWithoutProperties custom = new();

            // Act
            string result = custom.GenerateSingleTableDebugText();

            // Assert
            Assert.IsTrue(result.Contains("CustomClassWithoutProperties"));
            Debug.WriteLine(result);
        }

        [TestCategory("GenerateSingleTableDebugText")]
        [TestMethod]
        public void GenerateTableDebugText_Null_ReturnsEmptyMessage()
        {
            // Arrange
            CustomClassWithoutProperties? custom = null;

            // Act
            string result = custom.GenerateSingleTableDebugText();

            // Assert
            //Assert.IsTrue(result.Contains("CustomClassWithoutProperties"));
            Debug.WriteLine(result);
        }
    }
}
