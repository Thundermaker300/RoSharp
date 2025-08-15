using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Structures.PurchaseTypes;
using RoSharp.Utility;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A class that represents a Roblox asset.
    /// </summary>
    /// <remarks>This class consists of every Roblox item type except for Bundles and Badges. See their respective types. Places can be made through this API but will have much less data as opposed to <see cref="Experiences.Experience"/> and <see cref="Experiences.Place"/> classes.</remarks>
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

        /// <summary>
        /// Gets whether or not this asset is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        private IPurchaseType purchaseInfo;

        /// <inheritdoc cref="IPurchaseType"/>
        public IPurchaseType PurchaseInfo => purchaseInfo;

        /// <summary>
        /// Gets whether or not this asset is available for purchase.
        /// </summary>
        public bool OnSale => purchaseInfo is not NotForSalePurchase;

        private CollectibleItemData? collectibleItemData;

        /// <summary>
        /// Gets this asset's collectible data.
        /// </summary>
        /// <remarks>
        /// This struct will be <see langword="null"/> for non-collectible items.
        /// </remarks>
        /// <seealso cref="IsCollectible"/>
        public CollectibleItemData? CollectibleItemData => collectibleItemData;

        /// <summary>
        /// Indicates whether or not this asset is collectible.
        /// </summary>
        public bool IsCollectible => CollectibleItemData is not null;

        private int sales;

        /// <summary>
        /// Gets the amount of sales this asset has.
        /// </summary>
        /// <remarks>This will be <c>0</c> unless the authenticated user has the ability to see this asset's sales.</remarks>
        public int Sales => sales;

        /// <summary>
        /// Gets whether or not this asset can be purchased for free.
        /// </summary>
        public bool Free => PurchaseInfo is FreePurchase;

        private int remaining;

        /// <summary>
        /// For limited items, gets the amount of quantity remaining before the item becomes resellable.
        /// </summary>
        public int Remaining => remaining;

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

        private Id<Asset> imageAsset;

        /// <summary>
        /// Gets the asset representing the image for this asset.
        /// </summary>
        public Id<Asset> ImageAsset => imageAsset;

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

        private string? downloadUrl;

        /// <summary>
        /// Gets a URL that can be used to download the asset as an rbxm format. This URL can be used with the <see cref="HttpClient"/> library to download the asset to a specific directory. Once downloaded, a framework like <see href="https://github.com/MaximumADHD/Roblox-File-Format">MaximumADHD's 'Roblox File Format' library</see> can be used to parse the file.
        /// </summary>
        /// <remarks>This property will be <see langword="null"/> if the authenticated user cannot access the asset (such as a paid asset).</remarks>
        public string? DownloadUrl => downloadUrl;

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

        private ModelDetails? modelDetails;

        /// <summary>
        /// Gets specific details of this asset, if it is a <see cref="AssetType.Model"/> and the details are available.
        /// </summary>
        /// <remarks>This will always be <see langword="null"/> if <see cref="AssetType"/> is not equal to <see cref="AssetType.Model"/>.</remarks>
        public ModelDetails? ModelDetails => modelDetails;

        /// <summary>
        /// Override the type of asset.
        /// </summary>
        protected string assetTypeOverride;

        /// <summary>
        /// Gets if this asset represents a game-pass.
        /// </summary>
        public bool IsGamePass => assetTypeOverride is "gamepass";

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        /// <summary>
        /// Instantiates a new <see cref="Asset"/>.
        /// </summary>
        /// <param name="assetId">The Id of the asset.</param>
        /// <param name="session">The session to attach, optional.</param>
        protected Asset(ulong assetId, Session? session = null)
        {
            Id = assetId;

            if (session != null)
                AttachSession(session);
        }

        /// <summary>
        /// Returns a <see cref="Asset"/> given its unique Id.
        /// </summary>
        /// <param name="assetId">The asset Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="Asset"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the asset Id invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>Authentication is not required but suggested as the Economy API hits ratelimits fast without user authentication.</remarks>
        public static async Task<Asset> FromId(ulong assetId, Session? session = null)
        {
            if (RoPool<Asset>.Contains(assetId))
                return RoPool<Asset>.Get(assetId, session.Global());

            Asset newUser = new(assetId, session.Global());
            await newUser.RefreshAsync();

            RoPool<Asset>.Add(newUser);

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response;
            if (assetTypeOverride is "gamepass")
                response = await SendAsync(HttpMethod.Get, $"/game-passes/v1/game-passes/{Id}/product-info", Constants.URL("apis"));
            else
                response = await SendAsync(HttpMethod.Get, $"/v2/assets/{Id}/details", Constants.URL("economy"));

            string raw = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(raw);

            name = data.Name;
            description = data.Description;
            sales = data.Sales;
            isLimited = data.IsLimited;
            isLimitedUnique = data.IsLimitedUnique;
            created = data.Created;
            lastUpdated = data.Updated;
            assetType = (AssetType)Convert.ToInt32(data.AssetTypeId);
            imageAsset = new(Convert.ToUInt64(data.IconImageAssetId), session);
            if (data.SaleLocation == null)
                saleLocation = SaleLocationType.NotApplicable;
            else
                saleLocation = (SaleLocationType)Convert.ToInt32(data.SaleLocation.SaleLocationType);

            if (data.Remaining != null)
                remaining = Convert.ToInt32(data.Remaining);

            if (data.CollectiblesItemDetails != null)
            {
                int initialQuantity = -1;
                int quantityLimitPerUser = -1;
                int lowestResalePrice = -1;

                if (data.CollectiblesItemDetails.TotalQuantity != null)
                    initialQuantity = Convert.ToInt32(data.CollectiblesItemDetails.TotalQuantity);

                if (data.CollectiblesItemDetails.CollectibleQuantityLimitPerUser != null)
                    quantityLimitPerUser = Convert.ToInt32(data.CollectiblesItemDetails.CollectibleQuantityLimitPerUser);

                if (data.CollectiblesItemDetails.CollectibleLowestResalePrice != null)
                    lowestResalePrice = Convert.ToInt32(data.CollectiblesItemDetails.CollectibleLowestResalePrice);

                collectibleItemData = new CollectibleItemData()
                {
                    ItemId = data.CollectibleItemId,
                    ProductId = data.CollectibleProductId,
                    TotalQuantity = initialQuantity,
                    QuantityLimitPerUser = quantityLimitPerUser,
                    LowestResalePrice = lowestResalePrice,
                    IsLimited = Convert.ToBoolean(data.CollectiblesItemDetails.IsLimited),
                };
            }
            else
            {
                collectibleItemData = null;
            }

            if (data.PriceInRobux != null)
            {
                purchaseInfo = new RobuxPurchase() { Price = data.PriceInRobux };
            }
            else
            {
                if (data.IsPublicDomain == true || AssetType is AssetType.Place)
                    // Places are never public domain
                    purchaseInfo = new FreePurchase();
                else
                    purchaseInfo = new NotForSalePurchase();
            }

            ulong creatorId = Convert.ToUInt64(data.Creator.CreatorTargetId);
            ownerId = creatorId;
            ownerName = data.Creator.Name;
            isCommunityOwned = data.Creator.CreatorType == "Group";

            // Update favorites
            HttpMessage message = new(HttpMethod.Get, $"/v1/favorites/assets/{Id}/count")
            {
                SilenceExceptions = true
            };

            var favoritesRequest = await SendAsync(message);
            if (favoritesRequest.IsSuccessStatusCode)
                favorites = Convert.ToUInt64(await favoritesRequest.Content.ReadAsStringAsync());
            else
                favorites = 0;

            // Reset properties
            thumbnailUrl = await GetThumbnailAsync(ThumbnailSize.S420x420);

            // Find Download link
            var assetDeliveryMessage = new HttpMessage(HttpMethod.Get, $"/v2/asset/?id={Id}")
            {
                SilenceExceptions = true
            };

            downloadUrl = null;
            var assetDeliveryRequest = await SendAsync(assetDeliveryMessage, Constants.URL("assetdelivery"));
            if (assetDeliveryRequest.IsSuccessStatusCode)
            {
                dynamic downloadData = JObject.Parse(await assetDeliveryRequest.Content.ReadAsStringAsync());
                if (downloadData.locations.Count > 0)
                {
                    downloadUrl = downloadData.locations[0].location ?? null;
                }
            }

            // Check if creator hub asset
            modelDetails = null;

            message.Url = $"/toolbox-service/v1/items/details?assetIds={Id}";
            HttpResponseMessage catalogResponse = await SendAsync(message, Constants.URL("apis"));
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

                if (usdPrice > 0)
                {
                    purchaseInfo = new FiatPurchase()
                    {
                        Price = usdPrice,
                        CurrencyCode = toolboxData.fiatProduct.purchasePrice.currencyCode,
                    };
                }

                if (toolboxData.asset.modelTechnicalDetails != null)
                {
                    var structure = new ModelDetails();
                    structure.AssetId = Id;

                    if (toolboxData.asset.modelTechnicalDetails.objectMeshSummary != null)
                    {
                        structure.Triangles = toolboxData.asset.modelTechnicalDetails.objectMeshSummary.triangles ?? 0;
                        structure.Vertices = toolboxData.asset.modelTechnicalDetails.objectMeshSummary.vertices ?? 0;
                    }
                    if (toolboxData.asset.modelTechnicalDetails.instanceCounts != null)
                    {
                        structure.ScriptCount = toolboxData.asset.modelTechnicalDetails.instanceCounts.script ?? 0;
                        structure.MeshPartCount = toolboxData.asset.modelTechnicalDetails.instanceCounts.meshPart ?? 0;
                        structure.AnimationCount = toolboxData.asset.modelTechnicalDetails.instanceCounts.animation ?? 0;
                        structure.DecalCount = toolboxData.asset.modelTechnicalDetails.instanceCounts.decal ?? 0;
                        structure.AudioCount = toolboxData.asset.modelTechnicalDetails.instanceCounts.audio ?? 0;
                        structure.ToolCount = toolboxData.asset.modelTechnicalDetails.instanceCounts.tool ?? 0;
                    }

                    modelDetails = structure;
                }
            }
            else
            {
                isCreatorHubAsset = false;
                hasScripts = false;
                duration = 0;
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
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<string>> GetThumbnailAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            if (ImageAsset != null && ImageAsset.UniqueId != 0)
                return await (await ImageAsset.GetInstanceAsync()).GetThumbnailAsync(size);
            string url = $"/v1/assets?assetIds={Id}&returnPolicy=PlaceHolder&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            var response = await SendAsync(HttpMethod.Get, url, Constants.URL("thumbnails"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid asset to get thumbnail for.");
            return new(response, Convert.ToString(data.data[0].imageUrl));
        }

        /// <summary>
        /// Modifies the asset using the provided options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyAsync(AssetModifyOptions options)
        {
            object body = new
            {
                name = options.Name ?? Name,
                description = options.Description ?? Description,
            };

            HttpMessage message = new(HttpMethod.Patch, $"/v1/assets/{Id}", body)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyAsync),
            };

            return new(await SendAsync(message, Constants.URL("develop")));
        }

        /// <summary>
        /// Toggles the sale status of the asset.
        /// </summary>
        /// <param name="isOnSale">Whether or not the asset is on sale.</param>
        /// <param name="cost">The cost to purchase the asset.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="InvalidOperationException">Use <c>Experience.ModifyAsync</c> to modify the experience's sale status.</exception>
        public async Task<HttpResult> SetSaleStatusAsync(bool isOnSale, int cost)
        {
            if (AssetType is AssetType.Place)
                throw new InvalidOperationException($"Use {nameof(Experience.ModifyAsync)} to modify the experience's sale status.");

            int? priceInRobux = !isOnSale ? null : cost;
            Dictionary<int, int> saleAvailabilityLocations = new() { [0] = 0, [1] = 1 };
            var message = new HttpMessage(HttpMethod.Post, $"/v1/assets/{Id}/release", new
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
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(SetSaleStatusAsync),
            };
            return new(await SendAsync(message, Constants.URL("itemconfiguration")));
        }

        /// <summary>
        /// Returns asset reviews. The amount of time this API request takes scales up with a higher <paramref name="limit"/>.
        /// </summary>
        /// <param name="limit">The maximum amount of reviews to return.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="AssetReview"/> upon completion.</returns>
        /// <remarks>This method will return an empty <see cref="PageResponse{T}"/> if <see cref="IsCreatorHubAsset"/> is <see langword="false"/>.</remarks>
        public async Task<HttpResult<PageResponse<AssetReview>>> GetReviewsAsync(int limit = 50, string? cursor = null)
        {
            if (!IsCreatorHubAsset)
                return new(null, PageResponse<AssetReview>.Empty);

            string url = $"/asset-reviews-api/v1/assets/{Id}/comments?hideAuthenticatedUserComment=true&limit={limit}&sortByHelpfulCount=true";
            if (cursor != null)
                url += $"&cursor={cursor}";

            var response = await SendAsync(HttpMethod.Get, url, Constants.URL("apis"));
            string rawData = await response.Content.ReadAsStringAsync();
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
                    Poster = new Id<User>(Convert.ToUInt64(comment.commentingUserId), session),
                };
                reviews.Add(yes);
            }

            return new(response, new PageResponse<AssetReview>(reviews, nextPage, null));
        }

        /// <summary>
        /// Gets this asset's tags.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="AssetTag"/> upon completion.</returns>
        public async Task<HttpResult<ReadOnlyCollection<AssetTag>>> GetTagsAsync()
        {
            if (assetTypeOverride != null)
                return new(null, new(new List<AssetTag>().AsReadOnly()));

            var response = await SendAsync(HttpMethod.Get, $"/v1/item-tags?itemIds=AssetId%3A{Id}", Constants.URL("itemconfiguration"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            List<AssetTag> tags = [];
            foreach (dynamic tag in data.data[0].itemTags)
            {
                AssetTag newTag = new()
                {
                    Name = tag.tag.name,
                    TagId = tag.tag.tagId,
                };
                tags.Add(newTag);
            }
            return new(response, tags.AsReadOnly());
        }

        /// <summary>
        /// Gets whether or not the <paramref name="target"/> owns this asset.
        /// </summary>
        /// <param name="target">The user to target.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<bool>> IsOwnedByAsync(User target) => await target.OwnsAssetAsync(this);

        /// <summary>
        /// Gets whether or not the user with the provided Id owns this asset.
        /// </summary>
        /// <param name="targetId">The user Id.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<bool>> IsOwnedByAsync(ulong targetId) => await IsOwnedByAsync(await User.FromId(targetId, session));

        /// <summary>
        /// Gets whether or not the user with the provided name owns this asset.
        /// </summary>
        /// <param name="targetUsername">The username.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<bool>> IsOwnedByAsync(string targetUsername) => await IsOwnedByAsync(await User.FromUsername(targetUsername, session));

        /// <summary>
        /// Removes this asset from the authenticated user's inventory.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> RemoveFromInventoryAsync()
        {
            var message = new HttpMessage(HttpMethod.Delete, $"/v2/inventory/asset/{Id}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(RemoveFromInventoryAsync)
            };
            return new(await SendAsync(message, Constants.URL("inventory")));
        }

        public async Task<HttpResult> SaveAsync()
        {

            var message = new HttpMessage(HttpMethod.Post, $"/toolbox-service/v1/saves", new
            {
                targetType = AssetType.ToString(),
                targetId = Id,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(SaveAsync)
            };
            return new(await SendAsync(message, Constants.URL("apis")));
        }

        public async Task<HttpResult> RemoveFromSavesAsync()
        {

            var message = new HttpMessage(HttpMethod.Delete, $"/toolbox-service/v1/saves?targetType={AssetType}&targetId={Id}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(RemoveFromSavesAsync)
            };
            return new(await SendAsync(message, Constants.URL("apis")));
        }

        /// <summary>
        /// Returns a list of assets that are shown under the "Recommended" section based on this asset.
        /// </summary>
        /// <param name="limit">The limit of assets to return. Maximum: 45.</param>
        /// <returns>A task representing a list of assets shown as recommended.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called. Occasionally, Roblox's API will produce a 'bad recommendation' that leads to an asset that doesn't exist (either deleted or hidden). If this is the case, RoSharp will skip over it automatically. However, if the limit is set to Roblox's maximum of 45, this will result in less than 45 assets being returned.</remarks>
        public async Task<HttpResult<ReadOnlyCollection<Id<Asset>>>> GetRecommendedAsync(int limit = 7)
        {
            var response = await SendAsync(HttpMethod.Get, $"/v2/recommendations/assets?assetId={Id}&assetTypeId={(int)AssetType}&numItems=45");
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            List<Id<Asset>> list = [];
            foreach (dynamic item in data.data)
            {
                try
                {
                    ulong itemId = Convert.ToUInt64(item);
                    list.Add(new Id<Asset>(itemId, session));
                }
                catch { }

                if (list.Count >= limit)
                    break;
            }
            return new(response, list.AsReadOnly());
        }

        /// <summary>
        /// Modifies a collaborator's (user or community) access to this asset. By default, access will be added, but setting <paramref name="remove"/> to <see langword="true"/> will remove it.
        /// <para>Note: Attempting to add an invalid user, such as a terminated user, will result in a Forbidden <see cref="RobloxAPIException"/>.</para>
        /// <para>Note 2: Communities can only have 'Use' permission, NOT 'Edit' permissions. Attempting to do so will result in an <see cref="InvalidOperationException"/>.</para>
        /// </summary>
        /// <param name="collaboratorType">The type of collaborator.</param>
        /// <param name="id">The unique Id of the collaborator.</param>
        /// <param name="permissionType">The type of permission to modify.</param>
        /// <param name="remove">True to remove access, False to grant it. Defaults to False.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">Communities cannot have 'Edit' permission on assets.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyCollaboratorAsync(AssetOwnerType collaboratorType, ulong id, AssetPermissionType permissionType, bool remove = false)
        {
            if (collaboratorType is AssetOwnerType.Community && permissionType is AssetPermissionType.Edit)
                throw new InvalidOperationException("Communities cannot have 'Edit' permission on assets.");

            string subjectType = collaboratorType is AssetOwnerType.Community ? "Group" : "User";
            HttpMethod method = remove ? HttpMethod.Delete : HttpMethod.Patch;

            var message = new HttpMessage(method, $"/asset-permissions-api/v1/assets/{Id}/permissions", new
            {
                requests = new[]
                {
                    new
                    {
                        subjectType = subjectType,
                        subjectId = id.ToString(),
                        action = permissionType.ToString()
                    },
                }
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyCollaboratorAsync)
            };

            return new(await SendAsync(message, Constants.URL("apis")));
        }

        /// <summary>
        /// Modifies a collaborator's (user or community) access to this asset. By default, access will be added, but setting <paramref name="remove"/> to <see langword="true"/> will remove it.
        /// <para>Note: Attempting to add an invalid user, such as a terminated user, will result in a Forbidden <see cref="RobloxAPIException"/>.</para>
        /// <para>Note 2: Communities can only have 'Use' permission, NOT 'Edit' permissions. Attempting to do so will result in an <see cref="InvalidOperationException"/>.</para>
        /// </summary>
        /// <param name="collaboratorType">The type of collaborator.</param>
        /// <param name="name">The unique name (not display name for users) of the collaborator.</param>
        /// <param name="permissionType">The type of permission to modify.</param>
        /// <param name="remove">True to remove access, False to grant it. Defaults to False.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">Communities cannot have 'Edit' permission on assets.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyCollaboratorAsync(AssetOwnerType collaboratorType, string name, AssetPermissionType permissionType, bool remove = false)
        {
            ulong? id;

            if (collaboratorType is AssetOwnerType.Community)
                id = await CommunityUtility.GetCommunityIdAsync(name);
            else
                id = await UserUtility.GetUserIdAsync(name);

            if (!id.HasValue)
            {
                throw new ArgumentException($"Invalid name '{name}' provided. No user or group matches.");
            }

            return await ModifyCollaboratorAsync(collaboratorType, id.Value, permissionType, remove);
        }

        /// <summary>
        /// Modifies a user's access to this asset. By default, access will be added, but setting <paramref name="remove"/> to <see langword="true"/> will remove it.
        /// <para>Note: Attempting to add an invalid user, such as a terminated user, will result in a Forbidden <see cref="RobloxAPIException"/>.</para>
        /// </summary>
        /// <param name="user">The <see cref="User"/> to modify.</param>
        /// <param name="permissionType">The type of permission to modify.</param>
        /// <param name="remove">True to remove access, False to grant it. Defaults to False.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyCollaboratorAsync(User user, AssetPermissionType permissionType, bool remove = false)
            => await ModifyCollaboratorAsync(AssetOwnerType.User, user.Id, permissionType, remove);

        /// <summary>
        /// Modifies a community's access to this asset. By default, access will be added, but setting <paramref name="remove"/> to <see langword="true"/> will remove it.
        /// <para>Note: Communities can only have 'Use' permission, NOT 'Edit' permissions. Attempting to do so will result in an <see cref="InvalidOperationException"/>.</para>
        /// </summary>
        /// <param name="community">The <see cref="Communities.Community"/> to modify..</param>
        /// <param name="permissionType">The type of permission to modify.</param>
        /// <param name="remove">True to remove access, False to grant it. Defaults to False.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">Communities cannot have 'Edit' permission on assets.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyCollaboratorAsync(Community community, AssetPermissionType permissionType, bool remove = false)
            => await ModifyCollaboratorAsync(AssetOwnerType.Community, community.Id, permissionType, remove);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Asset {Name} [{Id}] ({AssetType}) {{{(!IsCommunityOwned ? "@" : string.Empty)}{OwnerName}}} <{PurchaseInfo}>";
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
