using Newtonsoft.Json.Linq;
using RoSharp.API;
using RoSharp.API.Assets;
using RoSharp.Exceptions;
using RoSharp.Http;

namespace RoSharp.Structures
{
    /// <summary>
    /// Contains an asset's collectible item data.
    /// </summary>
    public struct CollectibleItemData
    {
        internal Asset controller;

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

        /// <summary>
        /// Gets reseller entries for this collectible item.
        /// </summary>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="CollectibleReseller"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<CollectibleReseller>>> GetResellersAsync(string? cursor = null)
        {
            string url = $"/marketplace-sales/v1/item/{ItemId}/resellers?limit=100";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var response = await controller.SendAsync(HttpMethod.Get, url, Constants.URL("apis"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            List<CollectibleReseller> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                CollectibleReseller reseller = new()
                {
                    CollectibleProductId = item.collectibleProductId,
                    CollectibleItemInstanceId = item.collectibleItemInstanceId,
                    Seller = new(Convert.ToUInt64(item.seller.sellerId), controller.session),
                    Price = Convert.ToInt32(item.price),
                    SerialNumber = Convert.ToInt32(item.serialNumber),
                };
                list.Add(reseller);
            }

            return new(response, new PageResponse<CollectibleReseller>(list, nextPage, previousPage));
        }
    }
}
