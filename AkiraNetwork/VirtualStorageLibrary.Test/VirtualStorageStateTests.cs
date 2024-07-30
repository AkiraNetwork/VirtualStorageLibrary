using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
