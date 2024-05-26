using System.Linq.Expressions;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualGroupCondition<T, TKey>(Expression<Func<T, TKey>> groupBy, bool ascending = true)
    {
        public Expression<Func<T, TKey>> GroupBy { get; set; } = groupBy;

        public bool Ascending { get; set; } = ascending;
    }
}
