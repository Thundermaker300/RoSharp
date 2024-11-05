using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoSharp.API.Assets
{
    public class Asset : APIMain, IRefreshable
    {
        public override string BaseUrl => "https://catalog.roblox.com";

        public virtual int AssetItemType { get; } = 0;

        public ulong Id { get; }

        private string name;
        public string Name { get; }

        private string description;
        public string Description { get; }

        private AssetOwner owner;
        public AssetOwner Owner { get; }

        private DateTime created;
        public DateTime Created { get; }

        private DateTime lastUpdated;
        public DateTime LastUpdated { get; }

        private int price;
        public int Price { get; }

        private bool onSale;
        public bool OnSale { get; }

        private int sales;
        public int Sales { get; }

        private int remaining;
        public int Remaining { get; }

        private int initialQuantity;
        public int InitialQuantity { get; }

        private int quantityLimitPerUser;
        public int QuantityLimitPerUser { get; }

        private bool isLimited;
        public bool IsLimited { get; }

        private bool isLimitedUnique;
        public bool IsLimitedUnique { get; }

        private AssetType assetType;
        public AssetType AssetType { get; }

        public bool HasOwner => Owner != null;

        public DateTime RefreshedAt { get; set; }

        private int favorites = -1;
        public int Favorites
        {
            get
            {
                if (favorites == -1)
                {
                    string rawData = GetString($"/v1/favorites/assets/{Id}/count", verifySession: false);
                    favorites = Convert.ToInt32(rawData);
                }
                return favorites;
            }
        }

        public Asset(ulong assetId, Session session)
        {
            Id = assetId;

            AttachSession(session);
            Refresh();
        }

        public void Refresh()
        {
            HttpResponseMessage response = GetAsync($"/v2/assets/{Id}/details", "https://economy.roblox.com").Result;
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
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
                remaining = data.Remaining;

                initialQuantity = data.CollectiblesItemDetails.TotalQuantity != null
                    ? Convert.ToInt32(data.CollectiblesItemDetails.TotalQuantity)
                    : -1;
                quantityLimitPerUser = data.CollectiblesItemDetails.CollectibleQuantityLimitPerUser != null
                    ? Convert.ToInt32(data.CollectiblesItemDetails.CollectibleQuantityLimitPerUser)
                    : -1;

                if (data.PriceInRobux != null)
                {
                    price = data.PriceInRobux;
                }

                if (data.Creator.CreatorType == "Group")
                {
                    owner = new(AssetOwnerType.Group, new Group(Convert.ToUInt64(data.Creator.CreatorTargetId)).AttachSessionAndReturn(session), null);
                }
                else if (data.Creator.CreatorType == "User")
                {
                    owner = new(AssetOwnerType.User, null, new User(Convert.ToUInt64(data.Creator.CreatorTargetId)).AttachSessionAndReturn(session));
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid asset ID. HTTP {response.StatusCode}");
            }

            RefreshedAt = DateTime.Now;
        }

        public async Task<string> GetThumbnailAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"https://thumbnails.roblox.com/v1/assets?assetIds={Id}&returnPolicy=PlaceHolder&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com", verifySession: false);
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

            HttpResponseMessage response = await PatchAsync($"/v1/assets/{Id}", body, "https://develop.roblox.com");
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
            HttpResponseMessage response = await PostAsync("/v1/assets/3307894526/release", body, "https://itemconfiguration.roblox.com");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to modify asset. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
            }
        }

        public bool IsOwnedBy(User target) => target.OwnsAsset(this);
        public bool IsOwnedBy(ulong targetId) => new User(targetId, session).OwnsAsset(this);
        public bool IsOwnedBy(string targetUsername) => new User(targetUsername, session).OwnsAsset(this);

        public override string ToString()
        {
            return $"{Name} [{Id}] ({AssetType}) {{{(Owner.OwnerType == AssetOwnerType.User ? "@" : string.Empty)}{Owner.Name}}} <R${(OnSale == true ? Price : "0")}>";
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
