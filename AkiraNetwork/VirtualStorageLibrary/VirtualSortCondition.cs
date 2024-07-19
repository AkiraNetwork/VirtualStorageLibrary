namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualSortCondition<T>(Expression<Func<T, object>> sortBy, bool ascending = true)
    {
        public Expression<Func<T, object>> SortBy { get; set; } = sortBy;

        public bool Ascending { get; set; } = ascending;
    }
}
