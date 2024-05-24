using System.Linq.Expressions;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualSortCondition<T>
    {
        public Expression<Func<T, object>> SortBy { get; set; }
        public bool Ascending { get; set; } = true; // デフォルトは昇順

        public VirtualSortCondition(Expression<Func<T, object>> sortBy, bool ascending = true)
        {
            SortBy = sortBy;
            Ascending = ascending;
        }
    }
}
