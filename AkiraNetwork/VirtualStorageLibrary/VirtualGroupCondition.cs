namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Represents the conditions for grouping data, holding the property to group by and the order (ascending or descending).
    /// </summary>
    /// <typeparam name="T">The type of the entity to be grouped.</typeparam>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    public class VirtualGroupCondition<T, TKey>(Expression<Func<T, TKey>> groupBy, bool ascending = true)
    {
        /// <summary>
        /// Gets or sets the property used for grouping.
        /// </summary>
        public Expression<Func<T, TKey>> GroupBy { get; set; } = groupBy;

        /// <summary>
        /// Gets or sets a value indicating whether the grouping order is ascending.
        /// True for ascending, false for descending.
        /// </summary>
        public bool Ascending { get; set; } = ascending;
    }
}
