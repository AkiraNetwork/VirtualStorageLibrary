// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU General Public License (version 3, or at your option, any later version). 
// This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using System.Diagnostics;
using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualSymbolicLinkTests : VirtualTestBase
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
        public void VirtualSymbolicLink_Constructor_Default()
        {
            // Act
            VirtualSymbolicLink link = new();

            // Assert
            Assert.AreEqual($"{VirtualStorageState.State.PrefixSymbolicLink}1", (string)link.Name);
            Assert.AreEqual(null, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_Constructor_ByName()
        {
            // Arrange
            VirtualNodeName nodeName = "TestLink";

            // Act
            VirtualSymbolicLink link = new(nodeName);

            // Assert
            Assert.AreEqual(nodeName, link.Name);
            Assert.AreEqual(null, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_Constructor_SetsNameAndTargetPath()
        {
            // Arrange
            VirtualNodeName nodeName = "TestLink";
            VirtualPath targetPath = "/target/path";

            // Act
            VirtualSymbolicLink link = new(nodeName, targetPath);

            // Assert
            Assert.AreEqual(nodeName, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithCreatedDate_SetsNameTargetPathAndDates()
        {
            // Arrange
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "/target/path";
            DateTime now = DateTime.Now;
            DateTime createdDate = now;

            // Act
            VirtualSymbolicLink link = new(name, targetPath, createdDate);

            // Assert
            Assert.AreEqual(name, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
            Assert.AreEqual(createdDate, link.CreatedDate);
            Assert.AreEqual(createdDate, link.UpdatedDate);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithDates_SetsNameTargetPathAndDates()
        {
            // Arrange
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "/target/path";
            DateTime now = DateTime.Now;
            DateTime createdDate = now;
            DateTime updatedDate = now;

            // Act
            VirtualSymbolicLink link = new(name, targetPath, createdDate, updatedDate);

            // Assert
            Assert.AreEqual(name, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
            Assert.AreEqual(createdDate, link.CreatedDate);
            Assert.AreEqual(updatedDate, link.UpdatedDate);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithInvalidTargetPath_ThrowsArgumentException()
        {
            // Arrange
            VirtualStorageState.State.InvalidNodeNameCharacters.Add('@');
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "/dir1/dir2/@dir3";

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => 
            { 
                new VirtualSymbolicLink(name, targetPath);
            });
            Debug.WriteLine(ex.Message);
            Assert.AreEqual("Target path [/dir1/dir2/@dir3] is invalid. Forbidden characters are used. (Parameter 'targetPath')", ex.Message);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithValidTargetPathDot_Successful()
        {
            // Arrange
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "./dir1/dir2";

            // Act
            VirtualSymbolicLink link = new VirtualSymbolicLink(name, targetPath);

            // Assert
            Assert.AreEqual(name, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_ConstructorWithValidTargetPathDotDot_Successful()
        {
            // Arrange
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "../dir1/dir2";

            // Act
            VirtualSymbolicLink link = new VirtualSymbolicLink(name, targetPath);

            // Assert
            Assert.AreEqual(name, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void VirtualSymbolicLink_DeepClone_CreatesExactCopy()
        {
            // Arrange
            VirtualNodeName name = "TestLink";
            VirtualPath targetPath = "/target/path";
            DateTime createdDate = DateTime.Now;
            DateTime updatedDate = DateTime.Now;
            VirtualSymbolicLink link = new(name, targetPath, createdDate, updatedDate);

            // Act
            VirtualSymbolicLink? clone = link.DeepClone() as VirtualSymbolicLink;

            // Assert
            Assert.IsNotNull(clone);
            Assert.AreEqual(link.Name, clone.Name);
            Assert.AreEqual(link.TargetPath, clone.TargetPath);
            Assert.AreNotEqual(link.CreatedDate, clone.CreatedDate);
            Assert.AreNotEqual(link.UpdatedDate, clone.UpdatedDate);
        }

        // VirtualPathからの暗黙的な変換で VirtualSymbolicLink オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromVirtualPath_CreatesObjectWithDefaultName()
        {
            // テストデータ
            VirtualPath targetPath = "/some/path";

            // VirtualPathを利用して VirtualSymbolicLink オブジェクトを作成
            VirtualSymbolicLink link = targetPath;

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(link);

            // プレフィックスの後の番号まで検証
            string expectedPrefix = VirtualStorageState.State.PrefixSymbolicLink;
            string expectedName = $"{expectedPrefix}1";
            Assert.AreEqual(expectedName, link.Name.ToString());

            Assert.AreEqual(targetPath, link.TargetPath);
        }

        // タプルからの暗黙的な変換で VirtualSymbolicLink オブジェクトが正しく作成されることを検証します。
        [TestMethod]
        public void ImplicitConversionFromTuple_CreatesObjectCorrectly()
        {
            // テストデータ
            VirtualNodeName nodeName = "MyLink";
            VirtualPath targetPath = "/other/path";

            // タプルを利用して VirtualSymbolicLink オブジェクトを作成
            VirtualSymbolicLink link = (nodeName, targetPath);

            // オブジェクトが正しく作成されたか検証
            Assert.IsNotNull(link);
            Assert.AreEqual(nodeName, link.Name);
            Assert.AreEqual(targetPath, link.TargetPath);
        }

        [TestMethod]
        public void Update_UpdatesTargetPathAndDates()
        {
            // Arrange
            VirtualNodeName originalName = "OriginalLink";
            VirtualPath originalTargetPath = "/original/path";
            VirtualSymbolicLink originalLink = new(originalName, originalTargetPath);

            VirtualNodeName newName = "NewLink";
            VirtualPath newTargetPath = "/new/path";
            VirtualSymbolicLink newLink = new(newName, newTargetPath);

            // Act
            DateTime beforeUpdate = originalLink.UpdatedDate;
            originalLink.Update(newLink);
            DateTime afterUpdate = originalLink.UpdatedDate;

            // Assert
            Assert.AreEqual(newLink.TargetPath, originalLink.TargetPath);
            Assert.AreEqual(newLink.CreatedDate, originalLink.CreatedDate);
            Assert.IsTrue(afterUpdate > beforeUpdate);
        }

        [TestMethod]
        public void Update_ThrowsArgumentExceptionWhenNodeIsNotVirtualSymbolicLink()
        {
            // Arrange
            VirtualNodeName originalName = "OriginalLink";
            VirtualSymbolicLink originalLink = new(originalName);
            VirtualItem<BinaryData> itemNode = new("item");

            // Act & Assert
            Exception err = Assert.ThrowsException<ArgumentException>(() => originalLink.Update(itemNode));

            Assert.AreEqual("The specified node [item] is not of type VirtualSymbolicLink. (Parameter 'node')", err.Message);

            Debug.WriteLine(err.Message);
        }
    }
}
