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

        public ulong Id { get; }
        public string Name { get; }
        public string Description { get; }
        public AssetOwner Owner { get; }
        public int Price { get; }
        public int Sales { get; }
        public int Favorites { get; }
        public bool HasResellers { get; }
        public bool HasOwner => Owner != null;

        public Asset(ulong assetId, int itemTypeId = 1)
        {
            object body = new
            {
                items = new[]
                {
                    new {
                        itemType = itemTypeId,
                        id = assetId,
                    }
                }
            };

            HttpResponseMessage response = PostAsync($"/v1/catalog/items/details", body, verifySession: false).Result;
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
                dynamic allData = JObject.Parse(raw);

                if (allData.data.Count == 0)
                {
                    throw new InvalidOperationException($"Invalid asset ID.");
                }

                dynamic data = allData.data[0];

                Id = data.id;
                Name = data.name;
                Description = data.description;
                Price = data.price;
                Sales = data.purchaseCount;
                Favorites = data.favoriteCount;
                HasResellers = data.hasResellers;

                if (data.creatorType == "Group")
                {
                    Owner = new(AssetOwnerType.Group, new Group(Convert.ToUInt64(data.creatorTargetId)).AttachSessionAndReturn(session), null);
                }
                else if (data.creatorType == "User")
                {
                    Owner = new(AssetOwnerType.User, null, new User(Convert.ToUInt64(data.creatorTargetId)).AttachSessionAndReturn(session));
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid asset ID. HTTP {response.StatusCode}");
            }
        }
    }
}
