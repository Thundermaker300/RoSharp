﻿using Newtonsoft.Json.Linq;
using RoSharp.API.Misc;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.API.Assets
{
    public class Experience : APIMain, IRefreshable, IPoolable
    {
        /// <inheritdoc/>
        public override string BaseUrl => "https://games.roblox.com";

        private static HttpClient genericClient { get; } = new HttpClient();

        /// <summary>
        /// Gets the universe ID of this experience. Equivalent to <see cref="Id"/>.
        /// </summary>
        public ulong UniverseId { get; }
        
        /// <summary>
        /// Gets the universe ID of this experience. Equivalent to <see cref="UniverseId"/>.
        /// </summary>
        public ulong Id => UniverseId;

        private string name;

        /// <summary>
        /// Gets the name of the experience.
        /// </summary>
        public string Name => name;

        private string description;

        /// <summary>
        /// Gets the description of the experience.
        /// </summary>
        public string Description => description;

        private IAssetOwner owner;

        /// <summary>
        /// Gets the owner of the experience. Can be casted to <see cref="Group"/> or <see cref="User"/>.
        /// </summary>
        /// <seealso cref="IsGroupOwned"/>
        public IAssetOwner Owner => owner;

        /// <summary>
        /// Gets whether or not this experience is owned by a group.
        /// </summary>
        /// <seealso cref="Owner"/>
        public bool IsGroupOwned => Owner is Group;

        private DateTime created;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the date this experience was created.
        /// </summary>
        public DateTime Created => created;

        private DateTime lastUpdated;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the date this experience was last updated.
        /// </summary>
        public DateTime LastUpdated => lastUpdated;

        private int cost;

        /// <summary>
        /// Gets the price in Robux to play this experience, if it is paid access.
        /// </summary>
        public int Cost => cost;

        private bool uncopylocked;

        /// <summary>
        /// Gets whether or not this experience is uncopylocked.
        /// </summary>
        public bool Uncopylocked => uncopylocked;

        private int maxPlayers;

        /// <summary>
        /// Gets the maximum amount of players for one server.
        /// </summary>
        public int MaxPlayers => maxPlayers;

        private int playingNow;

        /// <summary>
        /// Gets the amount of players that are playing this experience now.
        /// </summary>
        public int PlayingNow => playingNow;

        private ulong visits;

        /// <summary>
        /// Gets the total amount of visits this experience has.
        /// </summary>
        public ulong Visits => visits;

        private ulong favorites;

        /// <summary>
        /// Gets the total amount of favorites this experience has.
        /// </summary>
        public ulong Favorites => favorites;

        internal bool favoritedByUser;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private Experience(ulong universeId, Session? session = null)
        {
            UniverseId = universeId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<Experience>.Contains(UniverseId))
                RoPool<Experience>.Add(this);
        }

        public static async Task<Experience> FromId(ulong placeOrUniverseId, Session? session = null)
        {
            ulong universeId = 0;

            HttpResponseMessage response = await genericClient.GetAsync($"https://apis.roblox.com/universes/v1/places/{placeOrUniverseId}/universe");
            string raw = await response.Content.ReadAsStringAsync();
            dynamic universeData = JObject.Parse(raw);
            if (universeData.universeId != null)
            {
                universeId = universeData.universeId;
            }
            else
            {
                universeId = placeOrUniverseId;
            }

            if (RoPool<Experience>.Contains(universeId))
                return RoPool<Experience>.Get(universeId, session);

            Experience newUser = new(universeId, session);
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            // Reset properties
            // TODO: These need to be updated within this method
            playabilityStatus = null;
            socialChannels = null;
            icon = null;

            HttpResponseMessage response = await GetAsync($"/v1/games?universeIds={UniverseId}");
            if (response.IsSuccessStatusCode)
            {
                dynamic whyaretheresomanywrappers = JObject.Parse(await response.Content.ReadAsStringAsync());
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
                    owner = await Group.FromId(creatorId);
                }
                else if (data.creator.type == "User")
                {
                    owner = await User.FromId(creatorId);
                }

                // configs
                await UpdateConfigurationAsync();
            }
            else
            {
                throw new ArgumentException($"Invalid universe ID '{UniverseId}'. HTTP {response.StatusCode}");
            }

            await UpdateExperienceGuidelinesDataAsync();
            await UpdateVoiceVideoAsync();
            await UpdateVotesAsync();

            RefreshedAt = DateTime.Now;
        }

        private bool? voiceEnabled = null;
        private bool? videoEnabled = null;

        private async Task UpdateVoiceVideoAsync()
        {
            string commSetting = await GetStringAsync($"/v1/settings/universe/{UniverseId}", "https://voice.roblox.com");
            dynamic data = JObject.Parse(commSetting);

            voiceEnabled = data.isUniverseEnabledForVoice;
            videoEnabled = data.isUniverseEnabledForAvatarVideo;
        }

        public bool VoiceEnabled => voiceEnabled.Value;

        public bool VideoEnabled => videoEnabled.Value;


        private PlayabilityStatus? playabilityStatus;

        /// <summary>
        /// Returns the experience's playability status.
        /// </summary>
        /// <returns>A task containing the <see cref="PlayabilityStatus"/> when completed.</returns>
        public async Task<PlayabilityStatus> GetPlayabilityStatusAsync()
        {
            if (!playabilityStatus.HasValue)
            {
                string raw = await GetStringAsync($"v1/games/multiget-playability-status?universeIds={UniverseId}");
                dynamic data = JArray.Parse(raw);
                if (Enum.TryParse<PlayabilityStatus>(Convert.ToString(data[0].playabilityStatus), out PlayabilityStatus result))
                {
                    playabilityStatus = result;
                }
                else
                {
                    playabilityStatus = PlayabilityStatus.Unknown;
                }
            }

            return playabilityStatus.Value;
        }

        private bool? profanityAllowed;

        /// <summary>
        /// Indicates whether or not profane language is allowed in the chat in this experience.
        /// </summary>
        public bool ProfanityAllowed => profanityAllowed.Value;

        private ReadOnlyDictionary<string, string>? socialChannels;

        /// <summary>
        /// Returns this experience's social channels.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> when complete.</returns>
        /// <remarks>The keys of the dictionary are the social media type, the value is the URL.</remarks>
        public async Task<ReadOnlyDictionary<string, string>> GetSocialChannelsAsync()
        {
            if (socialChannels == null)
            {
                Dictionary<string, string> dict = new();
                string rawData = await GetStringAsync($"/v1/games/{UniverseId}/social-links/list", verifyApiName: "Experience.SocialChannels");
                dynamic data = JObject.Parse(rawData);
                foreach (dynamic media in data.data)
                {
                    dict.Add(Convert.ToString(media.type), Convert.ToString(media.url));
                }
                socialChannels = dict.AsReadOnly();
            }

            return socialChannels;
        }

        private int? minimumAge;

        /// <summary>
        /// The minimum age required to play this experience.
        /// </summary>
        public int MinimumAge => minimumAge.Value;

        private ReadOnlyCollection<ExperienceDescriptor>? experienceDescriptors;

        /// <summary>
        /// Gets a read-only collection of defined descriptors that contribute to this experience's <see cref="MinimumAge"/>.
        /// </summary>
        public ReadOnlyCollection<ExperienceDescriptor> ExperienceDescriptors => experienceDescriptors;

        private async Task UpdateExperienceGuidelinesDataAsync()
        {
            // Update profanity
            string commSetting = await GetStringAsync($"/asset-text-filter-settings/public/universe/{UniverseId}", "https://apis.roblox.com");
            dynamic dataProfane = JObject.Parse(commSetting);
            profanityAllowed = dataProfane.Profanity;

            // Update guidelines
            object body = new
            {
                universeId = UniverseId.ToString()
            };

            HttpResponseMessage response = await PostAsync("/experience-guidelines-api/experience-guidelines/get-age-recommendation", body, "https://apis.roblox.com");
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic dataUseless = JObject.Parse(rawData);
            dynamic data = dataUseless.ageRecommendationDetails;

            if (data.summary.ageRecommendation != null)
                minimumAge = data.summary.ageRecommendation.minimumAge;
            else
                minimumAge = 0;

            List<ExperienceDescriptor> list = new();
            if (data.descriptorUsages.Count != 0)
            {
                foreach (dynamic item in data.descriptorUsages)
                {
                    string? getDimensionValue(string target)
                    {
                        foreach (dynamic descriptorData in item.descriptorDimensionUsages)
                        {
                            if (descriptorData.dimensionName == target)
                                return descriptorData.dimensionValue;
                        }
                        return null;
                    }

                    string itemName = Convert.ToString(item.name);
                    if (item.contains == true && Utility.Constants.DescriptorIdToEnumMapping.ContainsKey(itemName))
                    {
                        ExperienceDescriptor descriptor = new()
                        {
                            DescriptorType = Utility.Constants.DescriptorIdToEnumMapping[itemName],
                            IconUrl = item.descriptor.iconUrl,
                            DisplayText = item.descriptor.displayName,

                            Type = getDimensionValue("type"),
                            Presence = getDimensionValue("presence"),
                            Intensity = getDimensionValue("intensity"),
                            Frequency = getDimensionValue("frequency"),
                            Realism = getDimensionValue("realism"),
                            BloodLevel = getDimensionValue("blood-level"),
                        };

                        list.Add(descriptor);
                    }
                }
            }
            experienceDescriptors = list.AsReadOnly();
        }

        private int? upvotes;

        /// <summary>
        /// Gets the amount of thumb's up this experience has.
        /// </summary>
        public int Upvotes => upvotes.Value;

        private int? downvotes;

        /// <summary>
        /// Gets the amount of thumb's down this experience has.
        /// </summary>
        public int Downvotes => downvotes.Value;

        private async Task UpdateVotesAsync()
        {
            string rawData = await GetStringAsync($"/v1/games/votes?universeIds={UniverseId}");
            dynamic data = JObject.Parse(rawData);
            upvotes = data.data[0].upVotes;
            downvotes = data.data[0].downVotes;
        }

        // Configuration related properties
        private List<string>? devices = new List<string>(0);

        /// <summary>
        /// Gets the devices that this experience is playable on.
        /// </summary>
        /// <remarks>This list will be empty for experiences that the authenticated user does not have access to modify.</remarks>
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

        private async Task UpdateConfigurationAsync()
        {
            HttpResponseMessage? response = null;
            try // Catch unauthorized errors for configuration data
            {
                response = await PatchAsync($"/v2/universes/{UniverseId}/configuration", new { }, "https://develop.roblox.com", "Experience.UpdateConfigurationAsync");
            }
            catch (RobloxAPIException ex)
            {}

            if (response == null)
                return;

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
            string rawData = await GetStringAsync($"/v1/games/recommendations/game/{UniverseId}?maxRows={limit}");
            dynamic data = JObject.Parse(rawData);
            List<Experience> list = new();
            foreach (dynamic item in data.games)
            {
                try
                {
                    ulong id = Convert.ToUInt64(item.universeId);
                    Experience asset = await FromId(id, session);
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
                        ulong assetId = Convert.ToUInt64(data.imageId);
                        icon = Asset.FromId(assetId, session).Result;
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

            string rawData = await GetStringAsync(url, "https://badges.roblox.com");
            dynamic data = JObject.Parse(rawData);

            List<Badge> list = new();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(await Badge.FromId(id));
            }

            return new PageResponse<Badge>(list, nextPage, previousPage);
        }

        public async Task<ReadOnlyCollection<Asset>> GetThumbnailsAsync(ExperienceThumbnailSize size = ExperienceThumbnailSize.S768x432, bool defaultRobloxThumbnailIfNecessary = true)
        {
            string url = $"/v1/games/multiget/thumbnails?universeIds={UniverseId}&countPerUniverse=25&defaults={defaultRobloxThumbnailIfNecessary.ToString().ToLower()}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com");
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid asset to get thumbnail for.");

            List<Asset> thumbnails = new List<Asset>();
            foreach (dynamic thumbnail in data.data[0].thumbnails)
            {
                if (thumbnail.state != "Completed")
                    continue;

                ulong assetId = Convert.ToUInt64(thumbnail.targetId);

                thumbnails.Add(await Asset.FromId(assetId, session));
            }

            return thumbnails.AsReadOnly();
        }

        public async Task SetPrivacyAsync(bool isPublic)
        {
            string url = $"/v1/universes/6723876149/{(isPublic == false ? "de" : string.Empty)}activate";
            HttpResponseMessage response = await PostAsync(url, new { }, "https://develop.roblox.com", "Experience.SetPrivacyAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"Failed to {(isPublic == false ? "de" : string.Empty)}activate experience (HTTP {response.StatusCode}) (UniverseId {UniverseId}). Do you have permission to modify this experience?");
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

            HttpResponseMessage response = await PatchAsync($"/v2/universes/{UniverseId}/configuration", body, "https://develop.roblox.com", "Experience.ModifyAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"Failed to modify asset. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
            }
        }

        public async Task BanUserAsync(ulong userId, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
        {
            if (permanent == false && !length.HasValue)
                throw new InvalidOperationException("length cannot be null if permanent is false.");

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

            HttpResponseMessage response = await PatchAsync($"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}?", body, "https://apis.roblox.com", "Experience.BanUserAsync");
            if (!response.IsSuccessStatusCode)
                throw new RobloxAPIException($"Failed to add ban. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
        }

        public async Task BanUserAsync(User user, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
            => await BanUserAsync(user.Id, displayReason, privateReason, permanent, length, excludeAlts);

        public async Task BanUserAsync(string username, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
            => await BanUserAsync(await User.FromUsername(username, session), displayReason, privateReason, permanent, length, excludeAlts);

        public async Task UnbanUserAsync(ulong userId)
        {
            var body = new
            {
                gameJoinRestriction = new
                {
                    active = false,
                }
            };

            HttpResponseMessage response = await PatchAsync($"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}?", body, "https://apis.roblox.com", "Experience.UnbanUserAsync");
            if (!response.IsSuccessStatusCode)
                throw new RobloxAPIException($"Failed to unban user. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
        }

        public async Task UnbanUserAsync(User user)
            => await UnbanUserAsync(user.Id);

        public async Task UnbanUserAsync(string username)
            => await UnbanUserAsync(await User.FromUsername(username, session));

        public async Task PostUpdateAsync(string text)
        {
            HttpResponseMessage response = await PostAsync($"/game-update-notifications/v1/publish/{UniverseId}", text, "https://apis.roblox.com", "Experience.PostUpdateAsync");
            if (!response.IsSuccessStatusCode)
                throw new RobloxAPIException($"Failed to post update. Error code {response.StatusCode}. {response.Content.ReadAsStringAsync().Result}");
        }

        /// <inheritdoc/>
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

        IPoolable IPoolable.AttachSessionAndReturn(Session? session)
            => AttachSessionAndReturn(session);
    }

    public class ExperienceDescriptor
    {
        public ExperienceDescriptorType DescriptorType { get; init; }
        public string IconUrl { get; init; }
        public string DisplayText { get; init; }

        public string? Type { get; init; }
        public string? Frequency { get; init; }
        public string? Realism { get; init; }
        public string? BloodLevel { get; init; }
        public string? Intensity { get; init; }
        public string? Presence { get; init; }
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
