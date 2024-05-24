namespace AkiraNet.VirtualStorageLibrary
{
    public static class VirtualStorageLinqExtensions
    {
        public static IEnumerable<T> GroupAndSort<T>(
            this IEnumerable<T> source,
            VirtualGroupCondition<T, object>? groupCondition,
            List<VirtualSortCondition<T>>? sortConditions)
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
            List<VirtualSortCondition<T>>? sortConditions)
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

            // ソートの指定がなかったらそのまま返す
            return orderedQuery ?? source;
        }
    }
}
