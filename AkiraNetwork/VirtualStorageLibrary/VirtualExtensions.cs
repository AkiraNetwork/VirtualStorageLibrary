namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Provides extension methods for various operations on collections in the Virtual Storage Library.
    /// </summary>
    public static class VirtualStorageExtensions
    {
        /// <summary>
        /// Groups and sorts the elements of the source sequence based on the specified conditions.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The sequence of elements to group and sort.</param>
        /// <param name="groupCondition">
        /// The condition that defines how to group the elements. If null, no grouping is applied.
        /// </param>
        /// <param name="sortConditions">
        /// A list of conditions that define the sort order. If null or empty, no sorting is applied.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the grouped and sorted elements.</returns>
        public static IEnumerable<T> GroupAndSort<T>(
            this IEnumerable<T> source,
            VirtualGroupCondition<T, object>? groupCondition = null,
            List<VirtualSortCondition<T>>? sortConditions = null)
        {
            var query = source.AsQueryable();

            if (groupCondition != null)
            {
                var groupedData = groupCondition.Ascending
                    ? query.GroupBy(groupCondition.GroupBy).OrderBy(g => g.Key)
                    : query.GroupBy(groupCondition.GroupBy).OrderByDescending(g => g.Key);

                // Apply additional sort conditions within each group
                return groupedData.SelectMany(group => group.ApplySortConditions(sortConditions));
            }
            else
            {
                // Apply sort conditions to the entire collection without grouping
                return query.ApplySortConditions(sortConditions);
            }
        }

        /// <summary>
        /// Applies the specified sort conditions to the elements of the source sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The sequence of elements to sort.</param>
        /// <param name="sortConditions">
        /// A list of conditions that define the sort order. If null or empty, no sorting is applied.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the sorted elements.</returns>
        public static IEnumerable<T> ApplySortConditions<T>(
            this IEnumerable<T> source,
            List<VirtualSortCondition<T>>? sortConditions = null)
        {
            if (sortConditions == null || sortConditions.Count == 0)
            {
                return source;
            }

            IQueryable<T> sourceQuery = source.AsQueryable();
            IOrderedQueryable<T>? orderedQuery = null;

            for (int i = 0; i < sortConditions.Count; i++)
            {
                var condition = sortConditions[i];
                if (orderedQuery == null)
                {
                    // Apply the first sort condition
                    orderedQuery = condition.Ascending
                        ? sourceQuery.AsQueryable().OrderBy(condition.SortBy)
                        : sourceQuery.AsQueryable().OrderByDescending(condition.SortBy);
                }
                else
                {
                    // Apply subsequent sort conditions
                    orderedQuery = condition.Ascending
                        ? orderedQuery.ThenBy(condition.SortBy)
                        : orderedQuery.ThenByDescending(condition.SortBy);
                }
            }

            return orderedQuery!;
        }
    }

    /// <summary>
    /// Provides extension methods for operations on virtual nodes in the Virtual Storage Library.
    /// </summary>
    public static class VirtualNodeExtensions
    {
        /// <summary>
        /// Resolves the type of the specified virtual node. If the node is a symbolic link, 
        /// returns the type of the target node. Otherwise, returns the type of the node itself.
        /// </summary>
        /// <param name="node">The virtual node to resolve the type for.</param>
        /// <returns>
        /// The type of the node or, if the node is a symbolic link, the type of the target node.
        /// </returns>
        public static VirtualNodeType ResolveNodeType(this VirtualNode node)
        {
            if (node is VirtualSymbolicLink link)
            {
                return link.TargetNodeType;
            }
            return node.NodeType;
        }
    }
}
