namespace RoSharp.Structures
{
    /// <summary>
    /// Contains an asset's collectible item data.
    /// </summary>
    public struct CollectibleItemData
    {
        /// <summary>
        /// Gets the collectible's item Id.
        /// </summary>
        public string ItemId { get; init; }

        /// <summary>
        /// Gets the collectible's product Id.
        /// </summary>
        public string ProductId { get; init; }

        /// <summary>
        /// Gets the total quantity available of the collectible. Will be <c>-1</c> for non-limited collectibles.
        /// </summary>
        public int TotalQuantity { get; init; }

        /// <summary>
        /// Gets the collectible's quantity-limit-per-user. Will be <c>-1</c> for non-limited collectibles.
        /// </summary>
        public int QuantityLimitPerUser { get; init; }

        /// <summary>
        /// Gets the collectible's lowest available resale price. Will be <c>-1</c> for non-limited collectibles.
        /// </summary>
        public int LowestResalePrice { get; init; }

        /// <summary>
        /// Gets whether or not this collectible is limited (fixed amount of quantity, re-sellable from user to user).
        /// </summary>
        public bool IsLimited { get; init; }
    }
}
