using Newtonsoft.Json.Linq;
using RoSharp.API.Pooling;
using RoSharp.Interfaces;

namespace RoSharp.API.Assets
{
    public class Badge : APIMain, IRefreshable, IPoolable
    {
        public override string BaseUrl => "https://badges.roblox.com";

        public ulong Id { get; }

        private string name;
        public string Name => name;

        private string description;
        public string Description => description;

        private Experience experience;
        public Experience Experience => experience;
        public IAssetOwner Owner => experience.Owner;

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

        private Badge(ulong assetId, Session? session = null)
        {
            Id = assetId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<Badge>.Contains(Id))
                RoPool<Badge>.Add(this);
        }

        public static async Task<Badge> FromId(ulong badgeId, Session? session = null)
        {
            if (RoPool<Badge>.Contains(badgeId))
                return RoPool<Badge>.Get(badgeId, session);

            Badge newUser = new(badgeId, session);
            await newUser.RefreshAsync();

            return newUser;
        }

        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v1/badges/{Id}");
            if (response.IsSuccessStatusCode)
            {
                string raw = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(raw);

                ulong experienceId = Convert.ToUInt64(data.awardingUniverse.id);

                name = data.displayName;
                description = (data.displayDescription == null ? string.Empty : data.displayDescription);
                created = data.created;
                lastUpdated = data.updated;
                experience = await Experience.FromId(experienceId);
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
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com");
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new InvalidOperationException("Invalid badge to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        public async Task ModifyAsync(BadgeModifyOptions options)
        {
            object body = new
            {
                name = options.Name,
                description = options.Description,
                enabled = options.IsEnabled,
            };

            HttpResponseMessage response = await PatchAsync($"/v1/badges/{Id}", body, verifyApiName: "Badge.ModifyAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to modify asset. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
            }
        }

        public async Task<bool> IsOwnedByAsync(User target) => await target.HasBadgeAsync(this);
        public async Task<bool> IsOwnedByAsync(ulong targetId) => await IsOwnedByAsync(await User.FromId(targetId, session));
        public async Task<bool> IsOwnedByAsync(string targetUsername) => await IsOwnedByAsync(await User.FromUsername(targetUsername, session));

        public override string ToString()
        {
            return $"{Name} [{Id}] {{{(Owner is User ? "@" : string.Empty)}{Owner.Name}}} <{AwardedCount}>";
        }

        public Badge AttachSessionAndReturn(Session? session)
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

    public class BadgeModifyOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }

        public BadgeModifyOptions(Badge target)
        {
            Name = target.Name;
            Description = target.Description;
            IsEnabled = target.IsEnabled;
        }
    }
}
