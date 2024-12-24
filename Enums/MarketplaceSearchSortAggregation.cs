namespace RoSharp.Enums
{
    /// <summary>
    /// The aggregation mode to use for <see cref="MarketplaceSearchSort.MostFavorited"/> and <see cref="MarketplaceSearchSort.Bestselling"/>. Defaults to <see cref="AllTime"/>.
    /// </summary>
    public enum MarketplaceSearchSortAggregation
    {
        /// <summary>
        /// All time bestsellers/most favorited.
        /// </summary>
        AllTime = 0,

        /// <summary>
        /// Past day bestsellers/most favorited.
        /// </summary>
        PastDay = 1,

        /// <summary>
        /// Past week bestsellers/most favorited.
        /// </summary>
        PastWeek = 3,
    }
}
