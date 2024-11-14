using Newtonsoft.Json.Linq;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Interfaces;
using System.Collections.ObjectModel;

namespace RoSharp.API.Assets
{
    public class Asset : APIMain, IRefreshable, IPoolable
    {
        public override string BaseUrl => "https://catalog.roblox.com";

        public ulong Id { get; }

        private string name;
        public string Name => name;

        private string description;
        public string Description => description;

        private IAssetOwner owner;
        public IAssetOwner Owner => owner;

        private DateTime created;
        public DateTime Created => created;

        private DateTime lastUpdated;
        public DateTime LastUpdated => lastUpdated;

        private int price;
        public int Price => price;

        private bool onSale;
        public bool OnSale => onSale;

        private int sales;
        public int Sales => sales;

        public bool Free => OnSale && Price == 0;

        private int remaining;
        public int Remaining => remaining;

        private int initialQuantity;
        public int InitialQuantity => initialQuantity;

        private int quantityLimitPerUser;
        public int QuantityLimitPerUser => quantityLimitPerUser;

        private bool isLimited;
        public bool IsLimited => isLimited;

        private bool isLimitedUnique;
        public bool IsLimitedUnique => isLimitedUnique;

        private AssetType assetType;
        public AssetType AssetType => assetType;

        private SaleLocationType saleLocation;
        public SaleLocationType SaleLocation => saleLocation;

        public bool HasOwner => Owner != null;

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

        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v2/assets/{Id}/details", "https://economy.roblox.com", "Asset.RefreshAsync");
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
                throw new InvalidOperationException($"Invalid asset ID '{Id}'. HTTP {response.StatusCode}");
            }

            // Reset properties
            favorites = 0;
            thumbnailUrl = await GetThumbnailAsync(ThumbnailSize.S420x420);

            RefreshedAt = DateTime.Now;
        }

        private ulong favorites = 0;
        public ulong Favorites
        {
            get
            {
                if (favorites == 0)
                {
                    string rawData = GetString($"/v1/favorites/assets/{Id}/count");
                    favorites = Convert.ToUInt64(rawData);
                }
                return favorites;
            }
        }

        private string thumbnailUrl;
        public string ThumbnailUrl => thumbnailUrl;

        public async Task<string> GetThumbnailAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/assets?assetIds={Id}&returnPolicy=PlaceHolder&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com");
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new InvalidOperationException("Invalid asset to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        public async Task ModifyAsync(AssetModifyOptions options)
        {
            object body = new
            {
                name = options.Name,
                description = options.Description,
            };

            HttpResponseMessage response = await PatchAsync($"/v1/assets/{Id}", body, "https://develop.roblox.com", "Asset.ModifyAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to modify asset. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
            }
        }

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
            HttpResponseMessage response = await PostAsync("/v1/assets/3307894526/release", body, "https://itemconfiguration.roblox.com", "Asset.SetSaleStatusAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to modify asset. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
            }
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

        IPoolable IPoolable.AttachSessionAndReturn(Session? session)
            => AttachSessionAndReturn(session);
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
