using RoSharp.API;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a single reseller entry for a collectible item.
    /// </summary>
    public struct CollectibleReseller
    {

        /// <summary>
        /// Gets the collectible product Id.
        /// </summary>
        public string CollectibleProductId { get; init; }

        /// <summary>
        /// Gets the collectible item instance Id.
        /// </summary>
        public string CollectibleItemInstanceId { get; init; }

        /// <summary>
        /// Gets Id of the seller.
        /// </summary>
        public Id<User> Seller { get; init; }

        /// <summary>
        /// Gets the price of this entry.
        /// </summary>
        public int Price { get; init; }

        /// <summary>
        /// Gets the item's serial number of this entry.
        /// </summary>
        public int SerialNumber { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{CollectibleProductId}] #{SerialNumber} [R${Price}] @{Seller.UniqueId}";
        }
    }
}
