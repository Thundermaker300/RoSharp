namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates the search filter to apply to marketplace searches. Defaults to <see cref="Relevance"/>.
    /// </summary>
    public enum MarketplaceSearchSort
    {
        /// <summary>
        /// Assets most related to the search query.
        /// </summary>
        Relevance = 0,

        /// <summary>
        /// Most favorited assets.
        /// </summary>
        MostFavorited = 1,

        /// <summary>
        /// Best selling assets.
        /// </summary>
        Bestselling = 2,

        /// <summary>
        /// Recently published assets.
        /// </summary>
        RecentlyPublished = 3,

        /// <summary>
        /// High to low prices.
        /// </summary>
        PriceHighToLow = 5,
        
        /// <summary>
        /// Low to high prices.
        /// </summary>
        PriceLowToHigh = 4,
    }
}
