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

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Represents the conditions for creating a list of virtual nodes.
    /// </summary>
    public struct VirtualNodeListConditions
    {
        /// <summary>
        /// Specifies the filter criteria, determining which types of virtual nodes
        /// to include.
        /// </summary>
        /// <value>
        /// The filter specifying the types of virtual nodes. The default value is
        /// <see cref="VirtualNodeTypeFilter.All"/>.
        /// </value>
        public VirtualNodeTypeFilter Filter { get; set; }

        /// <summary>
        /// Specifies the grouping condition, determining how to group the virtual
        /// nodes.
        /// </summary>
        /// <value>
        /// The condition for grouping virtual nodes. The default value is <c>null</c>.
        /// You can specify <c>null</c> if no grouping condition is needed.
        /// </value>
        public VirtualGroupCondition<VirtualNode, object>? GroupCondition { get; set; }

        /// <summary>
        /// Specifies the sorting conditions, determining the order in which the
        /// virtual nodes are arranged.
        /// </summary>
        /// <value>
        /// A list of conditions for sorting the virtual nodes. The default value is
        /// <c>null</c>. You can specify <c>null</c> if no sorting condition is needed.
        /// </value>
        public List<VirtualSortCondition<VirtualNode>>? SortConditions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeListConditions"/>
        /// class with default conditions.
        /// </summary>
        public VirtualNodeListConditions()
        {
            Filter = VirtualNodeTypeFilter.All;
            GroupCondition = null;
            SortConditions = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeListConditions"/>
        /// class with the specified conditions.
        /// </summary>
        /// <param name="filter">The filter criteria for the virtual nodes.</param>
        /// <param name="groupCondition">The grouping condition for the virtual
        /// nodes.</param>
        /// <param name="sortConditions">The sorting conditions for the virtual
        /// nodes.</param>
        public VirtualNodeListConditions(VirtualNodeTypeFilter filter,
            VirtualGroupCondition<VirtualNode, object>? groupCondition,
            List<VirtualSortCondition<VirtualNode>>? sortConditions)
        {
            Filter = filter;
            GroupCondition = groupCondition;
            SortConditions = sortConditions;
        }
    }
}
