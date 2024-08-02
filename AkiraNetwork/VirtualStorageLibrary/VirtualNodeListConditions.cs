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
