using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Extensions;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.API
{
    /// <summary>
    /// Static class that contains utility methods for accessing Roblox's price floor information. All API within this class require authentication.
    /// </summary>
    [Obsolete("All API has been migrated to the MarketplaceAPI class.")]
    public static class PriceFloorAPI
    {
        /// <summary>
        /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing the price floors for every <see cref="AssetType"/> that has one.
        /// </summary>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing the <see cref="ReadOnlyDictionary{TKey, TValue}"/> upon completion.</returns>
        [Obsolete("See MarketplaceAPI::GetPriceFloorsAsync.")]
        public static async Task<ReadOnlyDictionary<AssetType, int>> GetPriceFloorsAsync(Session? session)
            => await MarketplaceAPI.GetPriceFloorsAsync(session);

        /// <summary>
        /// Gets the price floor for a specific <see cref="AssetType"/>.
        /// </summary>
        /// <param name="assetType">The <see cref="AssetType"/> to get the price floor of.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing the price floor as an <see cref="int"/>. Will be <see langword="null"/> if the provided <see cref="AssetType"/> does not have a price floor.</returns>
        [Obsolete("See MarketplaceAPI::GetPriceFloorForTypeAsync")]
        public static async Task<int?> GetPriceFloorForTypeAsync(AssetType assetType, Session? session)
            => await MarketplaceAPI.GetPriceFloorForTypeAsync(assetType, session);
    }
}
