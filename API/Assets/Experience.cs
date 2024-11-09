using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.API.Misc;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Extensions;
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
    public class Experience : APIMain, IRefreshable
    {
        public override string BaseUrl => "https://games.roblox.com";

        public ulong UniverseId { get; }

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

        private int cost;
        public int Cost => cost;

        private bool uncopylocked;
        public bool Uncopylocked => uncopylocked;

        private int maxPlayers;
        public int MaxPlayers => maxPlayers;

        private int playingNow;
        public int PlayingNow => playingNow;

        private ulong visits;
        public ulong Visits => visits;

        private ulong favorites;
        public ulong Favorites => favorites;

        internal bool favoritedByUser;

        public DateTime RefreshedAt { get; set; }

        public Experience(ulong placeOrUniverseId, Session? session = null)
        {
            HttpResponseMessage response = Get($"/universes/v1/places/{placeOrUniverseId}/universe", "https://apis.roblox.com", verifySession: false);
            string raw = response.Content.ReadAsStringAsync().Result;
            dynamic universeData = JObject.Parse(raw);
            if (universeData.universeId != null)
            {
                UniverseId = universeData.universeId;
            }
            else
            {
                UniverseId = placeOrUniverseId;
            }

            if (session != null)
                AttachSession(session);

            Refresh();
            if (!ExperiencePool.Contains(UniverseId))
                ExperiencePool.Add(this);
        }

        public void Refresh()
        {
            // Reset properties
            voiceEnabled = null;
            videoEnabled = null;
            playabilityStatus = null;
            profanityAllowed = null;
            socialChannels = null;
            icon = null;

            HttpResponseMessage response = Get($"/v1/games?universeIds={UniverseId}", verifySession: false);
            if (response.IsSuccessStatusCode)
            {
                dynamic whyaretheresomanywrappers = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                dynamic data = whyaretheresomanywrappers.data[0];
                name = data.sourceName;
                description = data.sourceDescription;
                created = data.created;
                lastUpdated = data.updated;
                uncopylocked = data.copyingAllowed;
                maxPlayers = data.maxPlayers;
                playingNow = data.playing;
                visits = data.visits;
                favorites = data.favoritedCount;

                favoritedByUser = data.isFavoritedByUser;

                if (data.price == null)
                    cost = 0;
                else
                    cost = data.price;

                ulong creatorId = Convert.ToUInt64(data.creator.id);
                if (data.creator.type == "Group")
                {
                    owner = GroupPool.Get(creatorId) ?? new Group(creatorId, session);
                }
                else if (data.creator.type == "User")
                {
                    owner = UserPool.Get(creatorId) ?? new User(creatorId);
                }

                // configs
                UpdateConfiguration();
            }
            else
            {
                throw new InvalidOperationException($"Invalid universe ID '{UniverseId}'. HTTP {response.StatusCode}");
            }

            RefreshedAt = DateTime.Now;
        }

        private bool? voiceEnabled = null;
        private bool? videoEnabled = null;

        private void UpdateVoiceVideo()
        {
            string commSetting = GetString($"/v1/settings/universe/1318971886", "https://voice.roblox.com");
            dynamic data = JObject.Parse(commSetting);

            voiceEnabled = data.isUniverseEnabledForVoice;
            videoEnabled = data.isUniverseEnabledForAvatarVideo;
        }

        [UsesSession]
        public bool VoiceEnabled
        {
            get
            {
                if (!voiceEnabled.HasValue)
                    UpdateVoiceVideo();

                return voiceEnabled.Value;
            }
        }

        [UsesSession]
        public bool VideoEnabled
        {
            get
            {
                if (!videoEnabled.HasValue)
                    UpdateVoiceVideo();

                return videoEnabled.Value;
            }
        }

        private PlayabilityStatus? playabilityStatus;
        public PlayabilityStatus PlayabilityStatus
        {
            get
            {
                string raw = GetString($"v1/games/multiget-playability-status?universeIds={UniverseId}");
                dynamic data = JArray.Parse(raw);
                return Enum.Parse<PlayabilityStatus>(Convert.ToString(data[0].playabilityStatus));
            }
        }

        private bool? profanityAllowed;
        public bool ProfanityAllowed
        {
            get
            {
                if (!profanityAllowed.HasValue)
                {
                    string commSetting = GetString($"/asset-text-filter-settings/public/universe/{UniverseId}", "https://apis.roblox.com");
                    dynamic data = JObject.Parse(commSetting);


                    profanityAllowed = data.Profanity;
                }
                return profanityAllowed.Value;
            }
        }

        private ReadOnlyDictionary<string, string>? socialChannels;

        public ReadOnlyDictionary<string, string> SocialChannels
        {
            get
            {
                if (socialChannels == null)
                {
                    Dictionary<string, string> dict = new();
                    string rawData = GetString($"/v1/games/{UniverseId}/social-links/list");
                    dynamic data = JObject.Parse(rawData);
                    foreach (dynamic media in data.data)
                    {
                        dict.Add(Convert.ToString(media.type), Convert.ToString(media.url));
                    }
                    socialChannels = dict.AsReadOnly();
                }

                return socialChannels;
            }
        }

        // Configuration related properties
        private List<string>? devices = new List<string>(0);
        public ReadOnlyCollection<Device> Devices => devices.Select(Enum.Parse<Device>).ToList().AsReadOnly();

        private bool? privateServers;
        public bool PrivateServers => privateServers.GetValueOrDefault();

        private int? privateServerCost;
        public int PrivateServerCost => privateServerCost.GetValueOrDefault();

        public bool PurchaseRequired => Cost != 0;

        private bool? friendsOnly;
        public bool FriendsOnly => friendsOnly.GetValueOrDefault();

        private bool studioAccessToAPIsAllowed;
        public bool StudioAccessToAPIsAllowed => studioAccessToAPIsAllowed;

        private void UpdateConfiguration()
        {
            HttpResponseMessage response = PatchAsync($"/v2/universes/{UniverseId}/configuration", new { }, "https://develop.roblox.com").Result;
            string rawData = response.Content.ReadAsStringAsync().Result;
            dynamic data = JObject.Parse(rawData);

            if (data.playableDevices != null) {
                List<string> devices = ((JArray)data.playableDevices).ToObject<List<string>>();
                this.devices = devices;
            }

            privateServers = data.allowPrivateServers;
            privateServerCost = data.privateServerPrice;
            friendsOnly = data.isFriendsOnly;
            studioAccessToAPIsAllowed = Convert.ToBoolean(data.studioAccessToApisAllowed);

        }

        /// <summary>
        /// Returns a list of assets that are shown under the "Recommended" section based on this asset.
        /// This method makes an API call for each asset, and as such is very time consuming the more assets are requested.
        /// </summary>
        /// <param name="limit">The limit of assets to return. Maximum: 45.</param>
        /// <returns>A task representing a list of assets shown as recommended.</returns>
        /// <remarks>Occasionally, Roblox's API will produce a 'bad recommendation' that leads to an asset that doesn't exist (either deleted or hidden). If this is the case, RoSharp will skip over it automatically. However, if the limit is set to Roblox's maximum of 45, this will result in less than 45 assets being returned.</remarks>
        public async Task<ReadOnlyCollection<Experience>> GetRecommendedAsync(int limit = 6)
        {
            string rawData = await GetStringAsync($"/v1/games/recommendations/game/{UniverseId}?maxRows={limit}", verifySession: false);
            dynamic data = JObject.Parse(rawData);
            List<Experience> list = new();
            foreach (dynamic item in data.games)
            {
                try
                {
                    Experience asset = new Experience(Convert.ToUInt64(item.universeId), session);
                    list.Add(asset);
                }
                catch { }

                if (list.Count >= limit)
                    break;
            }
            return list.AsReadOnly();
        }

        private Asset? icon;
        public Asset Icon
        {
            get
            {
                if (icon == null)
                {
                    string rawData = GetString($"/v1/games/{UniverseId}/icon");
                    dynamic data = JObject.Parse(rawData);
                    if (data.imageId != null)
                    {
                        icon = new(Convert.ToUInt64(data.imageId), session);
                    }
                }

                return icon;
            }
        }

        public async Task<PageResponse<Badge>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, string? cursor = null)
        {
            string url = $"v1/universes/{UniverseId}/badges?limit={limit.Limit()}&sortOrder=Asc";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, "https://badges.roblox.com", false);
            dynamic data = JObject.Parse(rawData);

            List<Badge> list = new();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(BadgePool.Get(id) ?? new Badge(id, session));
            }

            return new PageResponse<Badge>(list, nextPage, previousPage);
        }

        public async Task<ReadOnlyCollection<string>> GetThumbnailsAsync(ExperienceThumbnailSize size = ExperienceThumbnailSize.S768x432, bool defaultRobloxThumbnailIfNecessary = true)
        {
            string url = $"/v1/games/multiget/thumbnails?universeIds={UniverseId}&countPerUniverse=25&defaults={defaultRobloxThumbnailIfNecessary.ToString().ToLower()}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com", verifySession: false);
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new InvalidOperationException("Invalid asset to get thumbnail for.");

            List<string> thumbnails = new List<string>();
            foreach (dynamic thumbnail in data.data[0].thumbnails)
            {
                if (thumbnail.state != "Completed")
                    continue;

                thumbnails.Add(Convert.ToString(thumbnail.imageUrl));
            }

            return thumbnails.AsReadOnly();
        }

        public async Task SetPrivacyAsync(bool isPublic)
        {
            string url = $"/v1/universes/6723876149/{(isPublic == false ? "de" : string.Empty)}activate";
            HttpResponseMessage response = await PostAsync(url, new { }, "https://develop.roblox.com");
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to {(isPublic == false ? "de" : string.Empty)}activate experience (HTTP {response.StatusCode}) (UniverseId {UniverseId}). Do you have permission to modify this experience?");
            }
        }

        public async Task ModifyAsync(ExperienceModifyOptions options)
        {
            object body = new
            {
                name = options.Name,
                description = options.Description,
                allowPrivateServers = options.EnablePrivateServers,
                privateServerPrice = options.PrivateServerPrice,
                isForSale = options.PurchaseRequired,
                price = options.Cost,
                isFriendsOnly = options.FriendsOnly,
                playableDevices = options.PlayableDevices,
                studioAccessToApisAllowed = options.StudioAccessToAPIsAllowed
            };

            HttpResponseMessage response = await PatchAsync($"/v2/universes/{UniverseId}/configuration", body, "https://develop.roblox.com");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to modify asset. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
            }
        }

        public async Task BanUserAsync(ulong userId, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
        {
            if (permanent == false && !length.HasValue)
                throw new ArgumentException("length cannot be null if permanent is false.");

            var body = new
            {
                gameJoinRestriction = new
                {
                    active = true,
                    displayReason = displayReason,
                    privateReason = privateReason,
                    duration = (permanent ? null : $"{length.Value.TotalSeconds}s"),
                    excludeAltAccounts = excludeAlts,
                }
            };

            HttpResponseMessage response = await PatchAsync($"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}?", body, "https://apis.roblox.com");
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to add ban. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
        }

        public async Task BanUserAsync(User user, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
            => await BanUserAsync(user.Id, displayReason, privateReason, permanent, length, excludeAlts);

        public async Task BanUserAsync(string username, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
            => await BanUserAsync(new User(username, session), displayReason, privateReason, permanent, length, excludeAlts);

        public async Task UnbanUserAsync(ulong userId)
        {
            var body = new
            {
                gameJoinRestriction = new
                {
                    active = false,
                }
            };

            HttpResponseMessage response = await PatchAsync($"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}?", body, "https://apis.roblox.com");
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to unban user. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
        }

        public async Task UnbanUserAsync(User user)
            => await UnbanUserAsync(user.Id);

        public async Task UnbanUserAsync(string username)
            => await UnbanUserAsync(new User(username, session));

        public async Task PostUpdateAsync(string text) // TODO: This api may not work? (Not authorized)
        {
            HttpResponseMessage response = await PostAsync($"/game-update-notifications/v1/publish/{UniverseId}", text, "https://apis.roblox.com");
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to post update. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
        }

        public override string ToString()
        {
            return $"{Name} [{UniverseId}] {{{(Owner is User ? "@" : string.Empty)}{Owner.Name}}} <R${Cost}>";
        }

        public Experience AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    public class ExperienceModifyOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool EnablePrivateServers { get; set; }
        public int PrivateServerPrice { get; set; }
        public bool PurchaseRequired { get; set; }
        public int Cost { get; set; }
        public bool FriendsOnly { get; set; }
        public List<Device> PlayableDevices { get; set; }
        public bool StudioAccessToAPIsAllowed { get; set; }

        public ExperienceModifyOptions(Experience experience)
        {
            Name = experience.Name;
            Description = experience.Description;
            EnablePrivateServers = experience.PrivateServers;
            PrivateServerPrice = experience.PrivateServerCost;
            PurchaseRequired = experience.PurchaseRequired;
            Cost = experience.Cost;
            FriendsOnly = experience.FriendsOnly;
            PlayableDevices = experience.Devices.ToList();
            StudioAccessToAPIsAllowed = experience.StudioAccessToAPIsAllowed;
        }
    }
}
