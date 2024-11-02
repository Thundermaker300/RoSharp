using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoSharp.API.Assets
{
    public class Asset : APIMain
    {
        public override string BaseUrl => "https://catalog.roblox.com";

        public virtual int AssetItemType { get; } = 0;

        public ulong Id { get; }
        public string Name { get; }
        public string Description { get; }
        public AssetOwner Owner { get; }
        public DateTime Created { get; }
        public DateTime LastUpdated { get; }
        public int Price { get; }
        public bool OnSale { get; }
        public int Sales { get; }
        public bool IsLimited { get; }
        public bool IsLimitedUnique { get; }
        public AssetType AssetType { get; }
        public bool HasOwner => Owner != null;

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
            this.session = session;

            HttpResponseMessage response = GetAsync($"/v2/assets/{assetId}/details", "https://economy.roblox.com").Result;
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
                dynamic data = JObject.Parse(raw);

                Id = data.TargetId;
                Name = data.Name;
                Description = data.Description;
                Sales = data.Sales;
                IsLimited = data.IsLimited;
                IsLimitedUnique = data.IsLimitedUnique;
                OnSale = data.IsForSale;
                Created = data.Created;
                LastUpdated = data.Updated;
                AssetType = (AssetType)Convert.ToInt32(data.AssetTypeId);

                if (data.PriceInRobux != null)
                {
                    Price = data.PriceInRobux;
                }

                if (data.Creator.CreatorType == "Group")
                {
                    Owner = new(AssetOwnerType.Group, new Group(Convert.ToUInt64(data.Creator.CreatorTargetId)).AttachSessionAndReturn(session), null);
                }
                else if (data.Creator.CreatorType == "User")
                {
                    Owner = new(AssetOwnerType.User, null, new User(Convert.ToUInt64(data.Creator.CreatorTargetId)).AttachSessionAndReturn(session));
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid asset ID. HTTP {response.StatusCode}");
            }
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
