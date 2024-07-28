namespace AkiraNetwork.VirtualStorageLibrary
{
    public static class VirtualStorageExtensions
    {
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

                // グループ内のアイテムに対して追加のソート条件を適用
                return groupedData.SelectMany(group => group.ApplySortConditions(sortConditions));
            }
            else
            {
                // グルーピングなしで全体にソート条件を適用
                return query.ApplySortConditions(sortConditions);
            }
        }

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
                    // 最初のソート条件を適用
                    orderedQuery = condition.Ascending
                        ? sourceQuery.AsQueryable().OrderBy(condition.SortBy)
                        : sourceQuery.AsQueryable().OrderByDescending(condition.SortBy);
                }
                else
                {
                    // 2番目以降のソート条件を適用
                    orderedQuery = condition.Ascending
                        ? orderedQuery.ThenBy(condition.SortBy)
                        : orderedQuery.ThenByDescending(condition.SortBy);
                }
            }

            return orderedQuery!;
        }
    }

    public static class VirtualNodeExtensions
    {
        public static VirtualNodeType ResolveNodeType(this VirtualNode node)
        {
            if (node is VirtualSymbolicLink link)
            {
                return link.TargetNodeType;
            }
            return node.NodeType;
        }
    }

    public static class VirtualNodeTypeFilterExtensions
    {
        public static VirtualNodeTypeFilter ToFilter(this VirtualNodeType nodeType)
        {
            return nodeType switch
            {
                VirtualNodeType.Directory => VirtualNodeTypeFilter.Directory,
                VirtualNodeType.Item => VirtualNodeTypeFilter.Item,
                VirtualNodeType.SymbolicLink => VirtualNodeTypeFilter.SymbolicLink,
                _ => VirtualNodeTypeFilter.None,
            };
        }
    }
}
