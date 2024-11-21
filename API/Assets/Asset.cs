using Newtonsoft.Json.Linq;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Structures;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A class that represents a Roblox asset.
    /// </summary>
    /// <remarks>This class consists of every Roblox item type except for Bundles, Game Passes, and Badges. See their respective types. Places can be made through this API but will have much less data as opposed to <see cref="Experiences.Experience"/> classes.</remarks>
    /// <seealso cref="Badge"/>
    /// <seealso cref="Experiences.Experience"/>
    /// <seealso cref="FromId(ulong, Session)"/>
    public class Asset : APIMain, IRefreshable, IIdApi<Asset>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("catalog");

        /// <summary>
        /// Gets the unique Id of the asset.
        /// </summary>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url
        {
            get
            {
                if (AssetType is AssetType.Place)
                    return $"{Constants.ROBLOX_URL}/games/{Id}";
                return IsCreatorHubAsset ? $"{Constants.URL("create")}/store/asset/{Id}/" : $"{Constants.ROBLOX_URL}/catalog/{Id}/";
            }
        }

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

        private ulong ownerId;
        private string ownerName;
        private bool isCommunityOwned;

        /// <summary>
        /// Gets the unique Id (community or user) of the owner of this asset.
        /// </summary>
        public ulong OwnerId => ownerId;

        /// <summary>
        /// Gets the name (community or user) of the owner of this asset.
        /// </summary>
        public string OwnerName => ownerName;

        /// <summary>
        /// Gets whether or not this asset is owned by a community.
        /// </summary>
        /// <seealso cref="GetOwnerAsync"/>
        public bool IsCommunityOwned => isCommunityOwned;

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

        private double usdPrice;
        
        /// <summary>
        /// Gets the USD price of this asset.
        /// </summary>
        public double UsdPrice => usdPrice;

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
        public bool Free => OnSale && Price == 0 && UsdPrice == 0;

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

        private bool isCreatorHubAsset;

        /// <summary>
        /// Indicates whether or not this asset is a creator hub asset (Decals, Sounds, Models, etc).
        /// </summary>
        public bool IsCreatorHubAsset => isCreatorHubAsset;

        private bool hasScripts;

        /// <summary>
        /// Indicates whether or not this asset has scripts in it.
        /// </summary>
        /// <remarks>This will always be <see langword="false"/> if <see cref="AssetType"/> is not equal to <see cref="AssetType.Model"/>.</remarks>
        public bool HasScripts => hasScripts;

        private int duration;

        /// <summary>
        /// Gets the length of this asset.
        /// </summary>
        /// <remarks>This will always be <see cref="TimeSpan.Zero"/> if <see cref="AssetType"/> is not equal to <see cref="AssetType.Audio"/>.</remarks>
        public TimeSpan Duration => TimeSpan.FromSeconds(duration);

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private Asset(ulong assetId, Session? session = null)
        {
            Id = assetId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<Asset>.Contains(Id))
                RoPool<Asset>.Add(this);
        }

        /// <summary>
        /// Returns a <see cref="Asset"/> given its unique Id.
        /// </summary>
        /// <param name="assetId">The asset Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="Asset"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the asset Id invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>Authentication is not required but suggested as the Economy API hits ratelimits fast without user authentication.</remarks>
        public static async Task<Asset> FromId(ulong assetId, Session? session = null)
        {
            if (RoPool<Asset>.Contains(assetId))
                return RoPool<Asset>.Get(assetId, session.Global());

            Asset newUser = new(assetId, session.Global());
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v2/assets/{Id}/details", Constants.URL("economy"));
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
            ownerId = creatorId;
            ownerName = data.Creator.Name;
            isCommunityOwned = data.Creator.CreatorType == "Group";

            // Update favorites
            try
            {
                favorites = Convert.ToUInt64(await GetStringAsync($"/v1/favorites/assets/{Id}/count"));
            }
            catch
            {
                favorites = 0;
            }

            // Reset properties
            thumbnailUrl = await GetThumbnailAsync(ThumbnailSize.S420x420);

            // Check if creator hub asset
            HttpResponseMessage catalogResponse = await GetAsync($"/toolbox-service/v1/items/details?assetIds={Id}", Constants.URL("apis"), doNotThrowException: true);
            if (catalogResponse.StatusCode == HttpStatusCode.OK)
            {
                isCreatorHubAsset = true;
                
                dynamic toolboxDataUseless = JObject.Parse(await catalogResponse.Content.ReadAsStringAsync());
                dynamic toolboxData = toolboxDataUseless.data[0];
                double usdPrice =
                    Convert.ToUInt64(toolboxData.fiatProduct.purchasePrice.quantity.significand)
                    * Math.Pow(10, Convert.ToInt32(toolboxData.fiatProduct.purchasePrice.quantity.exponent));

                hasScripts = toolboxData.asset.hasScripts;
                duration = toolboxData.asset.duration;
                this.usdPrice = usdPrice;
            }
            else
            {
                isCreatorHubAsset = false;
                hasScripts = false;
                duration = 0;
                usdPrice = 0;
            }

            RefreshedAt = DateTime.Now;
        }

        /// <summary>
        /// Returns the owner of this asset.
        /// </summary>
        /// <returns>A task containing the owner of this asset.</returns>
        public async Task<IAssetOwner> GetOwnerAsync()
        {
            if (IsCommunityOwned)
            {
                return await Community.FromId(OwnerId);
            }
            return await User.FromId(OwnerId);
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

        /// <summary>
        /// Retrieves a thumbnail URL for this asset.
        /// </summary>
        /// <param name="size">The size of the thumbnail.</param>
        /// <returns>A task containing a thumbnail URL.</returns>
        /// <exception cref="ArgumentException">Invalid asset to get thumbnail for.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
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
                name = options.Name ?? Name,
                description = options.Description ?? Description,
            };

            await PatchAsync($"/v1/assets/{Id}", body, Constants.URL("develop"), "Asset.ModifyAsync");
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
            await PostAsync("/v1/assets/3307894526/release", body, Constants.URL("itemconfiguration"), "Asset.SetSaleStatusAsync");
        }

        /// <summary>
        /// Returns asset reviews. The amount of time this API request takes scales up with a higher <paramref name="limit"/>.
        /// </summary>
        /// <param name="limit">The maximum amount of reviews to return.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="AssetReview"/> upon completion.</returns>
        /// <remarks>This method will return an empty <see cref="PageResponse{T}"/> if <see cref="IsCreatorHubAsset"/> is <see langword="false"/>.</remarks>
        public async Task<PageResponse<AssetReview>> GetReviewsAsync(int limit = 50, string? cursor = null)
        {
            if (!IsCreatorHubAsset)
                return PageResponse<AssetReview>.Empty;

            string url = $"/asset-reviews-api/v1/assets/{Id}/comments?hideAuthenticatedUserComment=true&limit={limit}&sortByHelpfulCount=true";
            if (cursor != null)
                url += $"&cursor={cursor}";

            string rawData = await GetStringAsync(url, Constants.URL("apis"));
            dynamic data = JObject.Parse(rawData);
            string? nextPage = data.nextCursor;

            if (data.hasMore == false)
                nextPage = null;

            List<AssetReview> reviews = [];
            foreach (dynamic comment in data.commentResponses)
            {
                bool? isRecommended = comment.isRecommended;

                AssetReview yes = new()
                {
                    ReviewId = comment.id,
                    Text = comment.text,
                    IsRecommended = isRecommended,
                    Poster = new GenericId<User>(Convert.ToUInt64(comment.commentingUserId), session),
                };
                reviews.Add(yes);
            }

            return new PageResponse<AssetReview>(reviews, nextPage, null);
        }

        /// <summary>
        /// Gets whether or not the <paramref name="target"/> owns this asset.
        /// </summary>
        /// <param name="target">The user to target.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> IsOwnedByAsync(User target) => await target.OwnsAssetAsync(this);

        /// <summary>
        /// Gets whether or not the user with the provided Id owns this asset.
        /// </summary>
        /// <param name="targetId">The user Id.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> IsOwnedByAsync(ulong targetId) => await IsOwnedByAsync(await User.FromId(targetId, session));

        /// <summary>
        /// Gets whether or not the user with the provided name owns this asset.
        /// </summary>
        /// <param name="targetUsername">The username.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> IsOwnedByAsync(string targetUsername) => await IsOwnedByAsync(await User.FromUsername(targetUsername, session));

        /// <summary>
        /// Returns a list of assets that are shown under the "Recommended" section based on this asset.
        /// </summary>
        /// <param name="limit">The limit of assets to return. Maximum: 45.</param>
        /// <returns>A task representing a list of assets shown as recommended.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called. Occasionally, Roblox's API will produce a 'bad recommendation' that leads to an asset that doesn't exist (either deleted or hidden). If this is the case, RoSharp will skip over it automatically. However, if the limit is set to Roblox's maximum of 45, this will result in less than 45 assets being returned.</remarks>
        public async Task<ReadOnlyCollection<GenericId<Asset>>> GetRecommendedAsync(int limit = 7)
        {
            string rawData = await GetStringAsync($"/v2/recommendations/assets?assetId={Id}&assetTypeId={(int)AssetType}&numItems=45");
            dynamic data = JObject.Parse(rawData);
            List<GenericId<Asset>> list = [];
            foreach (dynamic item in data.data)
            {
                try
                {
                    ulong itemId = Convert.ToUInt64(item);
                    list.Add(new GenericId<Asset>(itemId, session));
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
            return $"{Name} [{Id}] ({AssetType}) {{{(!IsCommunityOwned ? "@" : string.Empty)}{OwnerName}}} <R${(OnSale == true ? Price : "0")}>";
        }

        /// <inheritdoc/>
        public Asset AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    /// <summary>
    /// Specifies the options to use for asset modification.
    /// </summary>
    /// <remarks>Any property in this class that is not changed will not modify the website.</remarks>
    public class AssetModifyOptions
    {
        /// <summary>
        /// The new name of the asset.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The new description of the asset.
        /// </summary>
        public string? Description { get; set; }
    }
}
