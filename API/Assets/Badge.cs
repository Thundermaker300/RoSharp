using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoSharp.API.Assets
{
    public class Badge : APIMain, IRefreshable
    {
        public override string BaseUrl => "https://badges.roblox.com";

        public ulong Id { get; }

        private string name;
        public string Name => name;

        private string description;
        public string Description => description;

        private Experience experience;
        public Experience Experience => experience;

        private AssetOwner owner; // TODO not assigned yet
        public AssetOwner Owner => owner;

        private DateTime created;
        public DateTime Created => created;

        private DateTime lastUpdated;
        public DateTime LastUpdated => lastUpdated;

        private bool isEnabled;
        public bool IsEnabled => isEnabled;

        private int awardedCount;
        public int AwardedCount => awardedCount;

        private int yesterdayAwardedCount;
        public int YesterdayAwardedCount => yesterdayAwardedCount;

        public DateTime RefreshedAt { get; set; }

        public Badge(ulong assetId, Session? session = null)
        {
            Id = assetId;

            if (session != null)
                AttachSession(session);

            Refresh();
        }

        public void Refresh()
        {
            HttpResponseMessage response = GetAsync($"/v1/badges/{Id}", verifySession: false).Result;
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
                dynamic data = JObject.Parse(raw);

                name = data.displayName;
                description = (data.displayDescription == null ? string.Empty : data.displayDescription);
                created = data.created;
                lastUpdated = data.updated;
                experience = new(Convert.ToUInt64(data.awardingUniverse.id));
                awardedCount = Convert.ToInt32(data.statistics.awardedCount);
                yesterdayAwardedCount = Convert.ToInt32(data.statistics.pastDayAwardedCount);
            }
            else
            {
                throw new InvalidOperationException($"Invalid badge ID '{Id}'. HTTP {response.StatusCode}");
            }

            RefreshedAt = DateTime.Now;
        }

        public async Task<string> GetThumbnailAsync()
        {
            string url = $"/v1/badges/icons?badgeIds={Id}&size=150x150&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com", verifySession: false);
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new InvalidOperationException("Invalid badge to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        /*public async Task ModifyAsync(AssetModifyOptions options)
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
        }*/

        public bool IsOwnedBy(User target) => target.HasBadge(this);
        public bool IsOwnedBy(ulong targetId) => IsOwnedBy(new User(targetId, session));
        public bool IsOwnedBy(string targetUsername) => IsOwnedBy(new User(targetUsername, session));

        public override string ToString()
        {
            return $"{Name} [{Id}] {{{(Owner.OwnerType == AssetOwnerType.User ? "@" : string.Empty)}{Owner.Name}}} <{AwardedCount}>";
        }
    }
}
