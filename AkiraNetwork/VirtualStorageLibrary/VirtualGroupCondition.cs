namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Represents the conditions for grouping data, holding the property to group by and the order (ascending or descending).
    /// </summary>
    /// <typeparam name="T">The type of the entity to be grouped.</typeparam>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    public class VirtualGroupCondition<T, TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualGroupCondition{T, TKey}"/> class with the specified grouping property and order.
        /// </summary>
        /// <param name="groupBy">The property to group by.</param>
        /// <param name="ascending">A value indicating whether the grouping order is ascending.</param>
        public VirtualGroupCondition(Expression<Func<T, TKey>> groupBy, bool ascending = true)
        {
            GroupBy = groupBy;
            Ascending = ascending;
        }

        /// <summary>
        /// Gets or sets the property used for grouping.
        /// </summary>
        /// <value>
        /// An expression that specifies the property to use for grouping.
        /// </value>
        public Expression<Func<T, TKey>> GroupBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grouping order is ascending.
        /// True if the order is ascending; otherwise, false.
        /// </summary>
        /// <value>
        /// A boolean value indicating whether the grouping order is ascending.
        /// </value>
        public bool Ascending { get; set; }
    }
}
