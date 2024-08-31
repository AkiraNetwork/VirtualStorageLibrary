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

using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;
using System.Diagnostics;
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

        [TestMethod]
        public void SetWildcardMatcher_UpdatesMatcherCorrectly()
        {
            // Arrange
            VirtualStorage<BinaryData> vs = new();
            DefaultWildcardMatcher matcher = new DefaultWildcardMatcher();

            // Act
            VirtualStorageState.SetWildcardMatcher(matcher);

            // Assert
            Assert.AreEqual(matcher, VirtualStorageState.State.WildcardMatcher);

            // Arrange
            vs.AddDirectory("/dir1");
            vs.AddItem("/dir1/item1.txt");
            vs.AddItem("/dir1/item2.txt");
            vs.AddItem("/dir1/item3.bin");

            // Act
            var paths = vs.ExpandPath(@"/dir1/item.*\.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("*:");
            foreach (var node in paths)
            {
                Debug.WriteLine(node);
            }

            // Assert
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/dir1/item1.txt", (string)paths[0]);
            Assert.AreEqual("/dir1/item2.txt", (string)paths[1]);

            // Act
            paths = vs.ExpandPath(@"/dir1/item.?\.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("\n?:");
            foreach (var node in paths)
            {
                Debug.WriteLine(node);
            }

            // Assert
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/dir1/item1.txt", (string)paths[0]);
            Assert.AreEqual("/dir1/item2.txt", (string)paths[1]);

            // Act
            paths = vs.ExpandPath(@"/dir1/item.+\.txt").ToList();

            // デバッグ出力
            Debug.WriteLine("\n+:");
            foreach (var node in paths)
            {
                Debug.WriteLine(node);
            }

            // Assert
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("/dir1/item1.txt", (string)paths[0]);
            Assert.AreEqual("/dir1/item2.txt", (string)paths[1]);
        }
    }
}
