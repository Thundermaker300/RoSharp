using Newtonsoft.Json.Linq;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Interfaces;
using System.Collections.ObjectModel;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A class that represents a Roblox asset.
    /// </summary>
    /// <remarks>This class consists of every Roblox item type except for Bundles, Game Passes, Experiences, and Badges. See their respective types.</remarks>
    /// <seealso cref="Badge"/>
    /// <seealso cref="Experience"/>
    /// <seealso cref="FromId(ulong, Session)"/>
    public class Asset : APIMain, IRefreshable, IIdApi<Asset>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("catalog");

        /// <summary>
        /// Gets the unique Id of the asset.
        /// </summary>
        public ulong Id { get; }

        private string name;

        /// <summary>
        /// Gets the name of the asset.
        /// </summary>
        public string Name => name;

        private string description;

        /// <summary>
        /// Gets the description of the asset.
        /// </summary>
        public string Description => description;

        private IAssetOwner owner;

        /// <summary>
        /// Gets the owner of the asset.
        /// </summary>
        /// <remarks>The returned <see cref="IAssetOwner"/> can be casted to <see cref="Group"/> or <see cref="User"/>.</remarks>
        /// <seealso cref="IsGroupOwned"/>
        public IAssetOwner Owner => owner;

        /// <summary>
        /// Gets whether or not this asset is owned by a group.
        /// </summary>
        /// <seealso cref="Owner"/>
        public bool IsGroupOwned => Owner is Group;

        private DateTime created;
        
        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time this asset was created.
        /// </summary>
        public DateTime Created => created;

        private DateTime lastUpdated;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time this asset was last updated.
        /// </summary>
        public DateTime LastUpdated => lastUpdated;

        private int price;

        /// <summary>
        /// Gets the Robux price of this asset.
        /// </summary>
        public int Price => price;

        private bool onSale;

        /// <summary>
        /// Gets whether or not this asset is available for purchase.
        /// </summary>
        public bool OnSale => onSale;

        private int sales;

        /// <summary>
        /// Gets the amount of sales this asset has.
        /// </summary>
        /// <remarks>This will be <c>0</c> unless the authenticated user has the ability to see this asset's sales.</remarks>
        public int Sales => sales;

        /// <summary>
        /// Gets whether or not this asset can be purchased for free.
        /// </summary>
        public bool Free => OnSale && Price == 0;

        private int remaining;

        /// <summary>
        /// For limited items, gets the amount of quantity remaining before the item becomes resellable.
        /// </summary>
        public int Remaining => remaining;

        private int initialQuantity;

        /// <summary>
        /// For limited items, gets the initial quantity amount.
        /// </summary>
        public int InitialQuantity => initialQuantity;

        private int quantityLimitPerUser;
        
        /// <summary>
        /// For limited items, gets the total amount of copies one user is allowed to have.
        /// </summary>
        public int QuantityLimitPerUser => quantityLimitPerUser;

        private bool isLimited;

        /// <summary>
        /// Gets whether or not this asset is limited.
        /// </summary>
        public bool IsLimited => isLimited;

        private bool isLimitedUnique;

        /// <summary>
        /// Gets whether or not this asset is limited unique.
        /// </summary>
        public bool IsLimitedUnique => isLimitedUnique;

        private AssetType assetType;

        /// <summary>
        /// Gets the <see cref="Enums.AssetType"/> of this asset.
        /// </summary>
        public AssetType AssetType => assetType;

        private SaleLocationType saleLocation;

        /// <summary>
        /// Gets the <see cref="Enums.SaleLocationType"/> of this asset, indicating where it can be purchased.
        /// </summary>
        public SaleLocationType SaleLocation => saleLocation;

        /// <summary>
        /// Indicates whether or not this asset has an owner.
        /// </summary>
        public bool HasOwner => Owner != null;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private Asset(ulong assetId, Session session)
        {
            Id = assetId;

            AttachSession(session);

            if (!RoPool<Asset>.Contains(Id))
                RoPool<Asset>.Add(this);
        }

        public static async Task<Asset> FromId(ulong assetId, Session session)
        {
            if (RoPool<Asset>.Contains(assetId))
                return RoPool<Asset>.Get(assetId, session);

            Asset newUser = new(assetId, session);
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v2/assets/{Id}/details", Constants.URL("economy"), "Asset.RefreshAsync");
            if (response.IsSuccessStatusCode)
            {
                string raw = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(raw);

                name = data.Name;
                description = data.Description;
                sales = data.Sales;
                isLimited = data.IsLimited;
                isLimitedUnique = data.IsLimitedUnique;
                onSale = data.IsForSale;
                created = data.Created;
                lastUpdated = data.Updated;
                assetType = (AssetType)Convert.ToInt32(data.AssetTypeId);
                if (data.SaleLocation == null)
                    saleLocation = SaleLocationType.NotApplicable;
                else
                    saleLocation = (SaleLocationType)Convert.ToInt32(data.SaleLocation.SaleLocationType);

                if (data.Remaining != null)
                    remaining = Convert.ToInt32(data.Remaining);

                if (data.CollectiblesItemDetails != null)
                {
                    if (data.CollectiblesItemDetails.TotalQuantity != null)
                        initialQuantity = Convert.ToInt32(data.CollectiblesItemDetails.TotalQuantity);
                    else
                        initialQuantity = -1;

                    if (data.CollectiblesItemDetails.CollectibleQuantityLimitPerUser != null)
                        quantityLimitPerUser = Convert.ToInt32(data.CollectiblesItemDetails.CollectibleQuantityLimitPerUser);
                    else
                        quantityLimitPerUser = -1;
                }
                else
                {
                    initialQuantity = -1;
                    quantityLimitPerUser = -1;
                }

                if (data.PriceInRobux != null)
                {
                    price = data.PriceInRobux;
                }

                ulong creatorId = Convert.ToUInt64(data.Creator.CreatorTargetId);
                if (data.Creator.CreatorType == "Group")
                {
                    owner = await Group.FromId(creatorId, session);
                }
                else if (data.Creator.CreatorType == "User")
                {
                    owner = await User.FromId(creatorId, session);
                }
            }
            else
            {
                throw new ArgumentException($"Invalid asset ID '{Id}'. HTTP {response.StatusCode}");
            }

            // Update favorites
            favorites = Convert.ToUInt64(await GetStringAsync($"/v1/favorites/assets/{Id}/count"));

            // Reset properties
            thumbnailUrl = await GetThumbnailAsync(ThumbnailSize.S420x420);

            RefreshedAt = DateTime.Now;
        }

        private ulong favorites = 0;

        /// <summary>
        /// Gets this asset's total amount of favorites.
        /// </summary>
        public ulong Favorites => favorites;

        private string thumbnailUrl;

        /// <summary>
        /// Returns this asset's thumbnail. Equivalent to calling <see cref="GetThumbnailAsync(ThumbnailSize)"/> with <see cref="ThumbnailSize.S420x420"/>, except this value is cached.
        /// </summary>
        public string ThumbnailUrl => thumbnailUrl;

        public async Task<string> GetThumbnailAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/assets?assetIds={Id}&returnPolicy=PlaceHolder&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid asset to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        /// <summary>
        /// Modifies the asset using the provided options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task ModifyAsync(AssetModifyOptions options)
        {
            object body = new
            {
                name = options.Name,
                description = options.Description,
            };

            HttpResponseMessage response = await PatchAsync($"/v1/assets/{Id}", body, Constants.URL("develop"), "Asset.ModifyAsync");
        }

        /// <summary>
        /// Toggles the sale status of the asset.
        /// </summary>
        /// <param name="isOnSale">Whether or not the asset is on sale.</param>
        /// <param name="cost">The cost to purchase the asset.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetSaleStatusAsync(bool isOnSale, int cost)
        {
            int? priceInRobux = !isOnSale ? null : cost;
            Dictionary<int, int> saleAvailabilityLocations = new() { [0] = 0, [1] = 1 };
            object body = new
            {
                saleStatus = (isOnSale ? "OnSale" : "OffSale"),
                priceConfiguration = new
                {
                    priceInRobux = priceInRobux,
                },
                releaseConfiguration = new
                {
                    saleAvailabilityLocations = saleAvailabilityLocations
                }
            };
            HttpResponseMessage response = await PostAsync("/v1/assets/3307894526/release", body, Constants.URL("itemconfiguration"), "Asset.SetSaleStatusAsync");
        }

        public async Task<bool> IsOwnedByAsync(User target) => await target.OwnsAssetAsync(this);
        public async Task<bool> IsOwnedByAsync(ulong targetId) => await IsOwnedByAsync(await User.FromId(targetId, session));
        public async Task<bool> IsOwnedByAsync(string targetUsername) => await IsOwnedByAsync(await User.FromUsername(targetUsername, session));

        /// <summary>
        /// Returns a list of assets that are shown under the "Recommended" section based on this asset.
        /// This method makes an API call for each asset, and as such is very time consuming the more assets are requested.
        /// </summary>
        /// <param name="limit">The limit of assets to return. Maximum: 45.</param>
        /// <returns>A task representing a list of assets shown as recommended.</returns>
        /// <remarks>Occasionally, Roblox's API will produce a 'bad recommendation' that leads to an asset that doesn't exist (either deleted or hidden). If this is the case, RoSharp will skip over it automatically. However, if the limit is set to Roblox's maximum of 45, this will result in less than 45 assets being returned.</remarks>
        public async Task<ReadOnlyCollection<Asset>> GetRecommendedAsync(int limit = 7)
        {
            string rawData = await GetStringAsync($"/v2/recommendations/assets?assetId={Id}&assetTypeId={(int)AssetType}&numItems=45");
            dynamic data = JObject.Parse(rawData);
            List<Asset> list = new();
            foreach (dynamic item in data.data)
            {
                try
                {
                    ulong itemId = Convert.ToUInt64(item);
                    Asset asset = await FromId(itemId, session);
                    list.Add(asset);
                }
                catch { }

                if (list.Count >= limit)
                    break;
            }
            return list.AsReadOnly();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} [{Id}] ({AssetType}) {{{(Owner is User ? "@" : string.Empty)}{Owner.Name}}} <R${(OnSale == true ? Price : "0")}>";
        }

        public Asset AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    public class AssetModifyOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public AssetModifyOptions(Asset target)
        {
            Name = target.Name;
            Description = target.Description;
        }
    }
}
