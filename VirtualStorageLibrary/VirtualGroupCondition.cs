using System.Linq.Expressions;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualGroupCondition<T, TKey>
    {
        public Expression<Func<T, TKey>> GroupBy { get; set; }
        public bool Ascending { get; set; } = true; // デフォルトは昇順

        public VirtualGroupCondition(Expression<Func<T, TKey>> groupBy, bool ascending = true)
        {
            GroupBy = groupBy;
            Ascending = ascending;
        }
    }
}
