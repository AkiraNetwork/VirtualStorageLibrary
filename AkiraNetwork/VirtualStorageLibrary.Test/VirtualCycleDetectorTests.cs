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

using System.Globalization;

namespace AkiraNetwork.VirtualStorageLibrary.Test
{
    [TestClass]
    public class VirtualCycleDetectorTests : VirtualTestBase
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
