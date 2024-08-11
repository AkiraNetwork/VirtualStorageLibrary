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
    public class VirtualIDTests : VirtualTestBase
    {
        private const int GuidCount = 10000;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            VirtualStorageSettings.Initialize();
            VirtualNodeName.ResetCounter();
        }

        [TestMethod]
        public void VirtualID_ShouldReturnValidGuid()
        {
            // Act
            VirtualID vID = new();
            Guid guid = vID.ID;

            // Assert
            Assert.AreNotEqual(Guid.Empty, guid);
        }

        [TestMethod]
        public void VirtualID_ShouldReturnUniqueGuids()
        {
            // Arrange
            HashSet<Guid> guidSet = [];

            // Act
            for (int i = 0; i < GuidCount; i++)
            {
                VirtualID vID = new();
                Guid newGuid = vID.ID;
                bool isAdded = guidSet.Add(newGuid);

                // Assert
                Assert.IsTrue(isAdded, $"Duplicate GUID found: {newGuid}");
            }

            // Final Assert
            Assert.AreEqual(GuidCount, guidSet.Count);
        }
    }
}
