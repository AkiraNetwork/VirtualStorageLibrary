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
    public class VirtualStorageStateTests : VirtualTestBase
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
        public void SetNodeListConditions_UpdatesConditionsCorrectly()
        {
            // Arrange
            var filter = VirtualNodeTypeFilter.Item | VirtualNodeTypeFilter.SymbolicLink;
            var groupCondition = new VirtualGroupCondition<VirtualNode, object>(node => node.NodeType, true);
            var sortConditions = new List<VirtualSortCondition<VirtualNode>>
            {
                new(node => node.Name, true)
            };

            // Act
            VirtualStorageState.SetNodeListConditions(new VirtualNodeListConditions(filter, groupCondition, sortConditions));
            var state = VirtualStorageState.State;

            // Assert
            Assert.AreEqual(filter, state.NodeListConditions.Filter);
            Assert.AreEqual(groupCondition, state.NodeListConditions.GroupCondition);
            CollectionAssert.AreEqual(sortConditions, state.NodeListConditions.SortConditions!.ToList());
        }

        [TestMethod]
        public void SetNodeListConditions_IndividualParameters_UpdatesConditionsCorrectly()
        {
            // Arrange
            var filter = VirtualNodeTypeFilter.Directory; // ディレクトリのみを対象とするフィルター
            var groupCondition = new VirtualGroupCondition<VirtualNode, object>(node => node.CreatedDate, false); // 作成日時で降順にグループ化
            var sortConditions = new List<VirtualSortCondition<VirtualNode>>
            {
                new(node => node.Name, true)
            };

            // Act
            VirtualStorageState.SetNodeListConditions(filter, groupCondition, sortConditions);
            var state = VirtualStorageState.State;

            // Assert
            Assert.AreEqual(filter, state.NodeListConditions.Filter);
            Assert.AreEqual(groupCondition, state.NodeListConditions.GroupCondition);
            CollectionAssert.AreEqual(sortConditions, state.NodeListConditions.SortConditions!.ToList());
        }
    }
}
