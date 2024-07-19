using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualResourceManagerTests
    {
        [TestMethod]
        public void Initialize_WithCulture_InitializesCorrectMessages()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;

            // Act
            manager.Initialize(new CultureInfo("ja-JP"));

            // Assert
            Assert.AreEqual("ノードが見つかりません。 テストノード", manager["ID01", "テストノード"]);
            Assert.AreEqual("ルートディレクトリは既に存在します。", manager["ID02"]);
        }

        [TestMethod]
        public void GetMessage_WithArgs_ReturnsFormattedMessage()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;
            manager.Initialize(new CultureInfo("ja-JP"));

            // Act
            string result = manager["ID01", "path/to/node"];

            // Assert
            Assert.AreEqual("ノードが見つかりません。 path/to/node", result);
        }

        [TestMethod]
        public void GetMessage_WithoutArgs_ReturnsMessage()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;
            manager.Initialize(new CultureInfo("ja-JP"));

            // Act
            string result = manager["ID02"];

            // Assert
            Assert.AreEqual("ルートディレクトリは既に存在します。", result);
        }

        [TestMethod]
        public void GetMessage_WithNeutralCulture_ReturnsNeutralMessage()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;
            manager.Initialize(CultureInfo.InvariantCulture);

            // Act
            string result1 = manager["ID01", "path/to/node"];
            string result2 = manager["ID02"];

            // Assert
            Assert.AreEqual("Node not found. path/to/node", result1);
            Assert.AreEqual("The root directory already exists.", result2);
        }

        [TestMethod]
        public void GetMessage_WithArgs_FormatsCorrectlyInNeutralCulture()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;
            manager.Initialize(CultureInfo.InvariantCulture);

            // Act
            string result = manager["ID01", "path/to/node"];

            // Assert
            Assert.AreEqual("Node not found. path/to/node", result);
        }

        [TestMethod]
        public void Initialize_WithNullCulture_InitializesNeutralMessages()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;

            // Act
            manager.Initialize(null);

            // Assert
            string result1 = manager["ID01", "path/to/node"];
            string result2 = manager["ID02"];

            Assert.AreEqual("Node not found. path/to/node", result1);
            Assert.AreEqual("The root directory already exists.", result2);
        }

        [TestMethod]
        public void Initialize_WithoutCulture_InitializesNeutralMessages()
        {
            // Arrange
            var manager = VirtualResourceManager.Resources;

            // Act
            manager.Initialize();

            // Assert
            string result1 = manager["ID01", "path/to/node"];
            string result2 = manager["ID02"];

            Assert.AreEqual("Node not found. path/to/node", result1);
            Assert.AreEqual("The root directory already exists.", result2);
        }
    }
}
