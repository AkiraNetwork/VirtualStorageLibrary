using System.Linq.Expressions;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualSortCondition<T>(Expression<Func<T, object>> sortBy, bool ascending = true)
    {
        public Expression<Func<T, object>> SortBy { get; set; } = sortBy;

        public bool Ascending { get; set; } = ascending;
    }
}
