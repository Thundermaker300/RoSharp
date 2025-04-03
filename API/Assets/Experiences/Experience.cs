using Newtonsoft.Json.Linq;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Structures.PurchaseTypes;
using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Regex = System.Text.RegularExpressions.Regex;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// A class that represents a collection of connected places, also known as a "universe".
    /// </summary>
    /// <seealso cref="FromId(ulong, Session?)"/>
    public class Experience : APIMain, IRefreshable, IIdApi<Experience>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("games");

        /// <summary>
        /// Gets the universe ID of this experience. Equivalent to <see cref="Id"/>.
        /// </summary>
        public ulong UniverseId { get; }
        
        /// <summary>
        /// Gets the universe ID of this experience. Equivalent to <see cref="UniverseId"/>.
        /// </summary>
        public ulong Id => UniverseId;

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/games/{RootPlaceId}/";

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

        private ulong ownerId;
        private string ownerName;
        private bool isCommunityOwned;

        /// <summary>
        /// Gets the unique Id (community or user) of the owner of this experience.
        /// </summary>
        public ulong OwnerId => ownerId;

        /// <summary>
        /// Gets the name (community or user) of the owner of this experience.
        /// </summary>
        public string OwnerName => ownerName;

        /// <summary>
        /// Gets whether or not this experience is owned by a community.
        /// </summary>
        /// <seealso cref="GetOwnerAsync"/>
        public bool IsCommunityOwned => isCommunityOwned;

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

        /// <summary>
        /// Gets the price in Robux to play this experience, if it is paid access.
        /// </summary>
        [Obsolete("Use PurchaseInfo instead.")]
        public int Cost => PurchaseInfo is RobuxPurchase robux ? (int)robux.Price : 0;

        private IPurchaseType purchaseInfo;

        /// <inheritdoc cref="IPurchaseType"/>
        public IPurchaseType PurchaseInfo => purchaseInfo;

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

        private Genre genre = Genre.Unknown;

        /// <summary>
        /// Gets the main genre of this experience.
        /// </summary>
        public Genre Genre => genre;

        private Genre subgenre = Genre.Unknown;

        /// <summary>
        /// Gets the subgenre of this experience.
        /// </summary>
        public Genre Subgenre => subgenre;

        private Id<Place> rootPlaceId;

        /// <summary>
        /// Gets the Id of the game's root place, also known as the starting place.
        /// </summary>
        public Id<Place> RootPlaceId => rootPlaceId;

        private DeveloperStats statistics;

        /// <summary>
        /// Gets the experience's <see cref="DeveloperStats"/> object, which has experience data and analytics info.
        /// </summary>
        public DeveloperStats Statistics => statistics;

        private DataStoreManager dataStorage;

        /// <summary>
        /// Gets the experience's <see cref="DataStoreManager"/> object, which allows for modifying the experience's DataStores via the Cloud API.
        /// </summary>
        /// <remarks>This experience must have a session with an API key, or the global session must have an API key, in order to use this member.</remarks>
        /// <exception cref="ArgumentException">The session attached to this experience does not have an API key.</exception>
        public DataStoreManager DataStorage
        {
            get
            {
                SessionVerify.ThrowAPIKeyIfNecessary(session, "Experience.DataStorage", "datastore");
                return dataStorage;
            }
        }

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private Experience(ulong universeId, Session? session = null)
        {
            UniverseId = universeId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<Experience>.Contains(UniverseId))
                RoPool<Experience>.Add(this);

            statistics = new(this);
            dataStorage = new(this);
        }

        /// <summary>
        /// Returns a <see cref="Experience"/> given the Id of a place within the experience.
        /// </summary>
        /// <param name="placeId">The place Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the place Id is invalid or is a UniverseId (see <see cref="FromId(ulong, Session?)"/>.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This is the most common method as the Place ID is present in the URL when going to any general experience page.</remarks>
        public static async Task<Experience> FromPlaceId(ulong placeId, Session? session = null)
            => await FromId(await ExperienceUtility.GetUniverseIdAsync(placeId), session);

        /// <summary>
        /// Returns a <see cref="Experience"/> given the Id of the universe.
        /// </summary>
        /// <param name="universeId">The universe Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the universe Id is invalid or is a PlaceId (see <see cref="FromPlaceId(ulong, Session?)"/>.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>If you have a Place Id, which is most common, see <see cref="FromPlaceId(ulong, Session?)"/>.</remarks>
        public static async Task<Experience> FromId(ulong universeId, Session? session = null)
        {
            if (RoPool<Experience>.Contains(universeId))
                return RoPool<Experience>.Get(universeId, session.Global());

            Experience newUser = new(universeId, session.Global());
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            // Reset properties
            playabilityStatus = null;
            socialChannels = null;

            HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"/v1/games?universeIds={UniverseId}");
            dynamic whyaretheresomanywrappers = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (whyaretheresomanywrappers.data.Count == 0)
            {
                throw new ArgumentException($"Invalid universe ID '{UniverseId}'");
            }
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
            rootPlaceId = new Id<Place>(Convert.ToUInt64(data.rootPlaceId), session);
            genre = ExperienceUtility.GetGenre(Convert.ToString(data.genre_l1));
            subgenre = ExperienceUtility.GetGenre(Convert.ToString(data.genre_l2));

            if (data.localizedFiatPrice != null)
            {
                string fiatDisplay = data.localizedFiatPrice;
                double cost = Convert.ToDouble(fiatDisplay.Replace("$", string.Empty));
                bool found = false;
                foreach (var fiatOption in await ExperienceUtility.GetFiatOptionsAsync(session))
                {
                    if (fiatOption.Price == cost)
                    {
                        purchaseInfo = fiatOption;
                        found = true;
                    }
                }
                if (!found) purchaseInfo = new UnknownPurchase();
            }
            else if (data.price != null)
            {
                purchaseInfo = new RobuxPurchase() { Price = data.price };
            }
            else
            {
                if (!SessionVerify.Verify(session))
                    purchaseInfo = new UnknownPurchase();
                else
                    purchaseInfo = new FreePurchase();
            }

            ulong creatorId = Convert.ToUInt64(data.creator.id);
            ownerId = creatorId;
            ownerName = data.creator.name;
            isCommunityOwned = data.creator.type == "Group";

            // configs
            await UpdateExperienceGuidelinesDataAsync();
            await UpdateVotesAsync();
            await UpdateAvatarStartInfoAsync();

            if (SessionVerify.Verify(session.Global()))
            {
                // All the below methods only return helpful data if the user is authenticated at minimum.
                // So let's just skip it to save roughly 700ms.
                await UpdateVoiceVideoAsync();
                await UpdateConfigurationAsync();
                await UpdatePrivateServerInfoAndDevicesAsync();
            }
            else
            {
                privateServers = false;
                privateServerCost = 0;
            }

            await Statistics.RefreshAsync();
            RefreshedAt = DateTime.Now;
        }

        /// <summary>
        /// Returns the owner of this experience.
        /// </summary>
        /// <returns>A task containing the owner of this experience.</returns>
        public async Task<IAssetOwner> GetOwnerAsync()
        {
            if (IsCommunityOwned)
            {
                return await Community.FromId(OwnerId);
            }
            return await User.FromId(OwnerId);
        }

        private bool? voiceEnabled = null;
        private bool? videoEnabled = null;

        private async Task UpdateVoiceVideoAsync()
        {
            if (!SessionVerify.Verify(session))
                return;

            string commSetting = await SendStringAsync(HttpMethod.Get, $"/v1/settings/universe/{UniverseId}", Constants.URL("voice"));
            dynamic data = JObject.Parse(commSetting);

            voiceEnabled = data.isUniverseEnabledForVoice;
            videoEnabled = data.isUniverseEnabledForAvatarVideo;
        }

        /// <summary>
        /// Gets whether or not voice chat is enabled.
        /// </summary>
        public bool VoiceEnabled => voiceEnabled.GetValueOrDefault();

        /// <summary>
        /// Gets whether or not facial tracking is enabled.
        /// </summary>
        public bool VideoEnabled => videoEnabled.GetValueOrDefault();


        private PlayabilityStatus? playabilityStatus;

        /// <summary>
        /// Returns the experience's playability status.
        /// </summary>
        /// <returns>A task containing the <see cref="PlayabilityStatus"/> when completed.</returns>
        public async Task<PlayabilityStatus> GetPlayabilityStatusAsync()
        {
            if (!playabilityStatus.HasValue)
            {
                string raw = await SendStringAsync(HttpMethod.Get, $"/v1/games/multiget-playability-status?universeIds={UniverseId}");
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
        public bool ProfanityAllowed => profanityAllowed.GetValueOrDefault();

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
                var message = new HttpMessage(HttpMethod.Get, $"/v1/games/{UniverseId}/social-links/list")
                {
                    AuthType = AuthType.RobloSecurity,
                    ApiName = nameof(GetSocialChannelsAsync)
                };

                Dictionary<string, string> dict = [];
                string rawData = await SendStringAsync(message);
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
        public int MinimumAge => minimumAge.GetValueOrDefault();

        private ExperienceMaturityLevel maturityLevel;

        /// <summary>
        /// Gets a <see cref="ExperienceMaturityLevel"/> representing the maturity level defined by Roblox for this experience.
        /// </summary>
        public ExperienceMaturityLevel MaturityLevel => maturityLevel;

        private ReadOnlyCollection<ExperienceDescriptor> experienceDescriptors
            = new List<ExperienceDescriptor>(0).AsReadOnly();

        /// <summary>
        /// Gets a read-only collection of defined descriptors that contribute to this experience's <see cref="MinimumAge"/>.
        /// </summary>
        public ReadOnlyCollection<ExperienceDescriptor> ExperienceDescriptors => experienceDescriptors;

        private async Task UpdateExperienceGuidelinesDataAsync()
        {
            // Update profanity
            string commSetting = await SendStringAsync(HttpMethod.Get, $"/asset-text-filter-settings/public/universe/{UniverseId}", Constants.URL("apis"));
            dynamic dataProfane = JObject.Parse(commSetting);
            profanityAllowed = dataProfane.Profanity;

            // Update guidelines
            object body = new
            {
                universeId = UniverseId.ToString()
            };

            string rawData = await SendStringAsync(HttpMethod.Post, "/experience-guidelines-api/experience-guidelines/get-age-recommendation", Constants.URL("apis"), body);
            dynamic dataUseless = JObject.Parse(rawData);
            dynamic data = dataUseless.ageRecommendationDetails;

            if (data.summary.ageRecommendation != null)
            {
                minimumAge = data.summary.ageRecommendation.minimumAge;
                maturityLevel = Enum.Parse<ExperienceMaturityLevel>(Convert.ToString(data.summary.ageRecommendation.displayName));
            }
            else
            {
                maturityLevel = ExperienceMaturityLevel.Unknown;
                minimumAge = 0;
            }

            List<ExperienceDescriptor> list = [];
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
                    if (item.contains == true)
                    {
                        ExperienceDescriptor descriptor = new()
                        {
                            DescriptorType = Constants.DescriptorIdToEnumMapping[itemName],
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
        public int Upvotes => upvotes.GetValueOrDefault();

        private int? downvotes;

        /// <summary>
        /// Gets the amount of thumb's down this experience has.
        /// </summary>
        public int Downvotes => downvotes.GetValueOrDefault();

        private async Task UpdateVotesAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/games/votes?universeIds={UniverseId}");
            dynamic data = JObject.Parse(rawData);
            upvotes = data.data[0].upVotes;
            downvotes = data.data[0].downVotes;
        }

        private AvatarType avatarType = AvatarType.Unknown;

        /// <summary>
        /// Gets the avatar type specified for this experience.
        /// </summary>
        public AvatarType AvatarType => avatarType;

        private bool customAnimationsAllowed = false;

        /// <summary>
        /// Gets whether or not custom animations are allowed in this experience.
        /// </summary>
        public bool CustomAnimationsAllowed => customAnimationsAllowed;

        private ReadOnlyDictionary<AvatarScaleType, double> minimumAvatarScales;

        /// <summary>
        /// Gets the minimum for different types of avatar scale types within this experience.
        /// </summary>
        public ReadOnlyDictionary<AvatarScaleType, double> MinimumAvatarScales => minimumAvatarScales;

        private ReadOnlyDictionary<AvatarScaleType, double> maximumAvatarScales;

        /// <summary>
        /// Gets the maximum for different types of avatar scale types within this experience.
        /// </summary>
        public ReadOnlyDictionary<AvatarScaleType, double> MaximumAvatarScales => maximumAvatarScales;

        private async Task UpdateAvatarStartInfoAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/game-start-info?universeId={UniverseId}", Constants.URL("avatar"));
            dynamic data = JObject.Parse(rawData);
            avatarType = Convert.ToString(data.gameAvatarType) switch
            {
                "MorphToR6" => AvatarType.R6,
                "MorphToR15" => AvatarType.R15,
                "PlayerChoice" => AvatarType.PlayerChoice,
                _ => AvatarType.Unknown,
            };

            customAnimationsAllowed = Convert.ToBoolean(data.allowCustomAnimations);

            Dictionary<AvatarScaleType, double> mins = [];
            Dictionary<AvatarScaleType, double> maxs = [];

            foreach (dynamic scale in data.universeAvatarMinScales)
            {
                if (Enum.TryParse<AvatarScaleType>(Convert.ToString(scale.Name), true, out AvatarScaleType result))
                    mins.Add(result, Convert.ToDouble(scale.Value));
            }

            foreach (dynamic scale in data.universeAvatarMaxScales)
            {
                if (Enum.TryParse<AvatarScaleType>(Convert.ToString(scale.Name), true, out AvatarScaleType result))
                    maxs.Add(result, Convert.ToDouble(scale.Value));
            }

            minimumAvatarScales = mins.AsReadOnly();
            maximumAvatarScales = maxs.AsReadOnly();
        }

        // Access related properties
        private bool? privateServers;

        /// <summary>
        /// Gets whether or not private servers are enabled in this experience.
        /// </summary>
        /// <remarks>This value will always be <see langword="false"/> if this instance is not authenticated.</remarks>
        /// <seealso cref="PrivateServerCost"/>
        public bool PrivateServers => privateServers.GetValueOrDefault();

        private int? privateServerCost;

        /// <summary>
        /// Gets the cost of private servers.
        /// </summary>
        /// <remarks>This value will always be <c>0</c> if this instance is not authenticated.</remarks>
        /// <seealso cref="PrivateServers"/>
        public int PrivateServerCost => privateServerCost.GetValueOrDefault();

        private List<Device> devices = [];

        /// <summary>
        /// Gets the devices that this experience is playable on.
        /// </summary>
        public ReadOnlyCollection<Device> Devices => devices.AsReadOnly();
        private async Task UpdatePrivateServerInfoAndDevicesAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/user/cloud/v2/universes/{UniverseId}", Constants.URL("apis"));
            dynamic data = JObject.Parse(rawData);

            if (data.privateServerPriceRobux != null)
            {
                privateServers = true;
                privateServerCost = data.privateServerPriceRobux;
            }
            else
            {
                privateServers = false;
                privateServerCost = 0;
            }

            List<Device> devices = [];

            if (data.desktopEnabled == true)
                devices.Add(Device.Computer);
            if (data.mobileEnabled == true)
                devices.Add(Device.Phone);
            if (data.tabletEnabled == true)
                devices.Add(Device.Tablet);
            if (data.consoleEnabled == true)
                devices.Add(Device.Console);
            if (data.vrEnabled == true)
                devices.Add(Device.VR);

            this.devices = devices;
        }

        /// <summary>
        /// Gets whether or not players must pay to play the experience.
        /// </summary>
        public bool PurchaseRequired => purchaseInfo is not FreePurchase;

        private bool? friendsOnly;

        /// <summary>
        /// Gets whether or not players have to befriend the owner to play the experience.
        /// </summary>
        /// <remarks>This value will always be <see langword="false"/> for experiences that the authenticated user does not have access to modify.</remarks>
        public bool FriendsOnly => friendsOnly.GetValueOrDefault();

        private bool studioAccessToAPIsAllowed;

        /// <summary>
        /// Gets whether or not Roblox Studio instances can access remote APIs such as HttpService and DataStoreService.
        /// </summary>
        /// <remarks>This value will always be <see langword="false"/> for experiences that the authenticated user does not have access to modify.</remarks>
        public bool StudioAccessToAPIsAllowed => studioAccessToAPIsAllowed;

        private async Task UpdateConfigurationAsync()
        {
            var message = new HttpMessage(HttpMethod.Post, $"/v2/universes/{UniverseId}/configuration", new { })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(UpdateConfigurationAsync),
                SilenceExceptions = true
            };
            HttpResponseMessage response = await SendAsync(message, Constants.URL("develop"));

            if (!response.IsSuccessStatusCode)
                return;

            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            friendsOnly = data.isFriendsOnly;
            studioAccessToAPIsAllowed = Convert.ToBoolean(data.studioAccessToApisAllowed);

        }

        /// <summary>
        /// Gets this experience's places.
        /// </summary>
        /// <param name="limit">The maximum amount of places to get at one time.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<Id<Place>>> GetPlacesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/universes/{Id}/places?sortOrder={sortOrder}&limit={limit.Limit()}&isUniverseCreation=false";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.URL("develop"));
            dynamic data = JObject.Parse(rawData);

            List<Id<Place>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<Place>>(list, nextPage, previousPage);
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
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/games/recommendations/game/{UniverseId}?maxRows={limit}");
            dynamic data = JObject.Parse(rawData);
            List<Experience> list = [];
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

        /// <summary>
        /// Gets this experience's icon.
        /// </summary>
        /// <param name="size">The size to use for the icon.</param>
        /// <returns>A task containing the string URL for the icon upon completion.</returns>
        public async Task<string> GetIconAsync(IconSize size = IconSize.S512x512)
            => await (await RootPlaceId.GetInstanceAsync()).GetIconAsync(size);

        /// <summary>
        /// Gets this experience's badges.
        /// </summary>
        /// <param name="limit">The amount of badges to get at one time.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<Id<Badge>>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/universes/{UniverseId}/badges?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.URL("badges"));
            dynamic data = JObject.Parse(rawData);

            List<Id<Badge>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<Badge>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets this experience's thumbnails.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="ExperienceThumbnail"/>s that are the experience's thumbnails.</returns>
        /// <exception cref="ArgumentException">Invalid experience to get thumbnails for.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<ReadOnlyCollection<ExperienceThumbnail>> GetThumbnailsAsync()
        {
            string url = $"/v2/games/{UniverseId}/media";
            string rawData = await SendStringAsync(HttpMethod.Get, url);
            dynamic data = JObject.Parse(rawData);
            List<ExperienceThumbnail> thumbnails = [];
            if (data.data.Count == 0)
                return thumbnails.AsReadOnly();

            foreach (dynamic thumbnail in data.data)
            {
                if (thumbnail.assetType != "Image")
                    continue;

                ulong assetId = Convert.ToUInt64(thumbnail.imageId);
                Asset a = await Asset.FromId(assetId, session);

                thumbnails.Add(new() { Asset = a, AltText = thumbnail.altText });
            }

            return thumbnails.AsReadOnly();
        }

        /// <summary>
        /// Returns a youtu.be URL to the experience's video trailer.
        /// </summary>
        /// <returns>A task containing a <see cref="string"/> upon completion. Will be <see langword="null"/> if the experience does not have a video.</returns>
        public async Task<string?> GetVideoAsync()
        {
            string url = $"/v2/games/{UniverseId}/media";
            string rawData = await SendStringAsync(HttpMethod.Get, url);
            dynamic data = JObject.Parse(rawData);

            foreach (dynamic thumbnail in data.data)
            {
                if (thumbnail.assetType != "YouTubeVideo")
                    continue;

                return $"https://www.youtu.be/{thumbnail.videoHash}";
            }

            return null;
        }

        /// <summary>
        /// Sets the privacy of the experience.
        /// </summary>
        /// <param name="isPublic">Whether or not the experience is open to the public.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <seealso cref="ModifyAsync(ExperienceModifyOptions)"/>
        public async Task SetPrivacyAsync(bool isPublic)
        {
            string url = $"/v1/universes/{Id}/{(isPublic == false ? "de" : string.Empty)}activate";
            var message = new HttpMessage(HttpMethod.Post, url, new { })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(SetPrivacyAsync)
            };

            await SendAsync(message, Constants.URL("develop"));
        }

        /// <summary>
        /// Modifies the experience. See <see cref="SetPrivacyAsync(bool)"/> for modifying the privacy of the experience.
        /// </summary>
        /// <param name="options">The options to use.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <seealso cref="SetPrivacyAsync(bool)"/>
        public async Task ModifyAsync(ExperienceModifyOptions options)
        {
            var message = new HttpMessage(HttpMethod.Patch, $"/v2/universes/{UniverseId}/configuration", new
            {
                name = options.Name ?? Name,
                description = options.Description ?? Description,
                allowPrivateServers = options.EnablePrivateServers ?? PrivateServers,
                privateServerPrice = options.PrivateServerPrice ?? PrivateServerCost,
                isForSale = options.PurchaseRequired ?? PurchaseRequired,
                price = options.Cost ?? Cost,
                isFriendsOnly = options.FriendsOnly ?? FriendsOnly,
                playableDevices = options.PlayableDevices ?? Devices.ToList(),
                studioAccessToApisAllowed = options.StudioAccessToAPIsAllowed ?? StudioAccessToAPIsAllowed
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyAsync),
            };

            await SendAsync(message, Constants.URL("develop"));
        }

        /// <summary>
        /// Sets the experience's genre to the given <see cref="Enums.Genre"/>.
        /// </summary>
        /// <param name="newGenre">The genre to set. Will throw an exception for <see cref="Genre.Unknown"/> and <see cref="Genre.None"/>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <remarks>Genres can only be changed ONCE every three months.</remarks>
        public async Task ModifyGenreAsync(Genre newGenre)
        {
            var message = new HttpMessage(HttpMethod.Post, "/experience-genre-api/v1/Creator/ExperienceGenre", new
            {
                universeId = UniverseId,
                genreTaxonomyVersion = 1,
                genre = ExperienceUtility.ToInternalKey(newGenre),
                responseOptions = new
                {
                    includeUpdateLockExpirationTime = true,
                }
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyGenreAsync)
            };

            await SendAsync(message, Constants.URL("apis"));
            
        }

        /// <summary>
        /// Bans a user from the experience.
        /// </summary>
        /// <param name="userId">The user Id of the user to ban.</param>
        /// <param name="displayReason">The public reason to display to the user.</param>
        /// <param name="privateReason">The private ban reason.</param>
        /// <param name="permanent">True for a permanent ban.</param>
        /// <param name="length">If <paramref name="permanent"/> is <see langword="false"/>, the length of time to use for the ban.</param>
        /// <param name="excludeAlts">If suspected alts should be included in the ban.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">length cannot be null if permanent is false.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task BanUserAsync(ulong userId, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
        {
            if (permanent == false && !length.HasValue)
                throw new InvalidOperationException("length cannot be null if permanent is false.");

            var message = new HttpMessage(HttpMethod.Patch, $"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}", new
            {
                gameJoinRestriction = new
                {
                    active = true,
                    displayReason = displayReason,
                    privateReason = privateReason,
                    duration = (permanent ? null : $"{length.GetValueOrDefault().TotalSeconds}s"),
                    excludeAltAccounts = excludeAlts,
                }
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(BanUserAsync),
            };

            await SendAsync(message, Constants.URL("apis"));
        }

        /// <summary>
        /// Bans a user from the experience.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        /// <param name="displayReason">The public reason to display to the user.</param>
        /// <param name="privateReason">The private ban reason.</param>
        /// <param name="permanent">True for a permanent ban.</param>
        /// <param name="length">If <paramref name="permanent"/> is <see langword="false"/>, the length of time to use for the ban.</param>
        /// <param name="excludeAlts">If suspected alts should be included in the ban.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">length cannot be null if permanent is false.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task BanUserAsync(User user, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
            => await BanUserAsync(user.Id, displayReason, privateReason, permanent, length, excludeAlts);

        /// <summary>
        /// Bans a user from the experience.
        /// </summary>
        /// <param name="username">The username of the user to ban.</param>
        /// <param name="displayReason">The public reason to display to the user.</param>
        /// <param name="privateReason">The private ban reason.</param>
        /// <param name="permanent">True for a permanent ban.</param>
        /// <param name="length">If <paramref name="permanent"/> is <see langword="false"/>, the length of time to use for the ban.</param>
        /// <param name="excludeAlts">If suspected alts should be included in the ban.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">length cannot be null if permanent is false.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task BanUserAsync(string username, string displayReason, string privateReason, bool permanent, TimeSpan? length = null, bool excludeAlts = true)
            => await BanUserAsync(await User.FromUsername(username, session), displayReason, privateReason, permanent, length, excludeAlts);

        /// <summary>
        /// Unbans a user from the experience.
        /// </summary>
        /// <param name="userId">The user Id to unban.</param>
        /// <returns>A task that completes when the task is finished.</returns>
        public async Task UnbanUserAsync(ulong userId)
        {
            var message = new HttpMessage(HttpMethod.Patch, $"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}?", new
            {
                gameJoinRestriction = new
                {
                    active = false,
                }
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(UnbanUserAsync),
            };

            await SendAsync(message, Constants.URL("apis"));
        }

        /// <summary>
        /// Unbans a user from the experience.
        /// </summary>
        /// <param name="user">The user to unban.</param>
        /// <returns>A task that completes when the task is finished.</returns>
        public async Task UnbanUserAsync(User user)
            => await UnbanUserAsync(user.Id);

        /// <summary>
        /// Unbans a user from the experience.
        /// </summary>
        /// <param name="username">The username of the user to unban.</param>
        /// <returns>A task that completes when the task is finished.</returns>
        public async Task UnbanUserAsync(string username)
            => await UnbanUserAsync(await User.FromUsername(username, session));

        /// <summary>
        /// Posts an experience update that followers will receive in their notification stream.
        /// </summary>
        /// <param name="text">The text of the post.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task PostUpdateAsync(string text)
        {
            await SendAsync(HttpMethod.Post, $"/game-update-notifications/v1/publish/{UniverseId}", Constants.URL("apis"), text);
        }

        /// <summary>
        /// Publishes a message through <c>MessagingService</c> to the experience.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="data">The message.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <remarks>This API member requires a session with an API key, and the API key must have the <c>universe-messaging-service:publish</c> permission.</remarks>
        public async Task PublishMessageAsync(string topic, string data)
        {
            var message = new HttpMessage(HttpMethod.Post, $"/cloud/v2/universes/{UniverseId}:publishMessage", new
            {
                topic = topic,
                message = data,
            })
            {
                AuthType = AuthType.ApiKey,
                ApiName = nameof(PublishMessageAsync),
                ApiKeyPermission = "universe-messaging-service:publish",
            };

            await SendAsync(message, Constants.URL("apis"));
        }

        /// <summary>
        /// Leaves a vote on the experience.
        /// </summary>
        /// <param name="positive">Whether to leave a positive or negative vote.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task VoteAsync(bool positive)
        {
            HttpMessage message = new(HttpMethod.Post, $"/voting-api/vote/asset/{RootPlaceId.UniqueId}?vote={positive}", null)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(VoteAsync),
            };

            await SendAsync(message, Constants.URL("apis"));
        }

        /// <summary>
        /// Removes a previously placed vote.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task RemoveVoteAsync()
        {
            HttpMessage message = new(HttpMethod.Post, $"/voting-api/vote/asset/{RootPlaceId.UniqueId}?vote=null", null)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(RemoveVoteAsync),
            };

            await SendAsync(message, Constants.URL("apis"));
        }

        /// <summary>
        /// Returns the URL to the experience's community wiki, if one exists.
        /// <para>
        /// Note that this method is powered by RoSeal. See more information about RoSeal <see href="https://www.roseal.live/">on their website</see>.
        /// </para>
        /// </summary>
        /// <returns>A task containing a <see cref="Uri"/> upon completion.</returns>
        public async Task<Uri?> GetCommunityWikiUrlAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, "/v2/experience-links.json", "https://data.roseal.live");
            JArray data = JArray.Parse(rawData);
            foreach (JToken item in data.Children())
            {
#pragma warning disable CS8602
#pragma warning disable CS8604
                if (item["universeIds"] != null && item["universeIds"].Any(t => t.Value<ulong>() == UniverseId) && item["links"] != null)
                {
                    foreach (JToken link in item["links"].Children())
                    {
                        if (link["type"].ToString() == "communityWiki")
                            return new Uri("https://" + link["url"].ToString());
                    }
                }
            }
#pragma warning restore CS8602
#pragma warning restore CS8604
            return null;
        }


        /// <summary>
        /// Gets a list of virtual events for this experience.
        /// </summary>
        /// <param name="includeFinished">Include events that have already finished.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        public async Task<PageResponse<Id<VirtualEvent>>> GetVirtualEventsAsync(bool includeFinished = false, string? cursor = null)
        {
            string url = $"/virtual-events/v1/universes/{Id}/virtual-events" + (!includeFinished ? $"?fromUtc={DateTime.UtcNow.ToString("s") + ".000Z"}" : string.Empty);
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.URL("apis"));
            dynamic data = JObject.Parse(rawData);

            List<Id<VirtualEvent>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<VirtualEvent>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Creates a virtual event.
        /// </summary>
        /// <param name="settings">The settings for the virtual event.</param>
        /// <returns>A task that contains a <see cref="VirtualEvent"/> upon completion.</returns>
        /// <exception cref="InvalidOperationException">Missing any of the following properties: Title, Subtitle, Description, Category, Visibility, StartTime, EndTime.</exception>
        public async Task<VirtualEvent> CreateVirtualEventAsync(VirtualEventConfiguration settings)
        {
            Place p = await Place.FromId(RootPlaceId);
            return await p.CreateVirtualEventAsync(settings);
        }

        /// <summary>
        /// Gets feedback provided for this experience.
        /// </summary>
        /// <param name="startTime">The start time for the feedback search.</param>
        /// <param name="endTime">The end time for the feedback search.</param>
        /// <param name="limit">The limit of feedback to return.</param>
        /// <param name="voteTypeFilter">Whether to vote by positive (<see langword="true"/>) feedback, negative (<see langword="false"/>) feedback, or both (<see langword="null"/>).</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="ExperienceReview"/> upon completion.</returns>
        public async Task<PageResponse<ExperienceReview>> GetFeedbackAsync(DateTime? startTime = null, DateTime? endTime = null, FixedLimit limit = FixedLimit.Limit50, bool? voteTypeFilter = null, string? cursor = null)
        {
            Place p = await Place.FromId(RootPlaceId, session);
            return await p.GetFeedbackAsync(startTime, endTime, limit, voteTypeFilter, cursor);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Experience {Name} [{UniverseId}] {{{(!IsCommunityOwned ? "@" : string.Empty)}{OwnerName}}} {PurchaseInfo}";
        }

        /// <inheritdoc/>
        public Experience AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    /// <summary>
    /// Represents an experience descriptor that determines an experience's minimum age and Maturity level.
    /// </summary>
    public readonly struct ExperienceDescriptor
    {
        /// <summary>
        /// The type of descriptor.
        /// </summary>
        public ExperienceDescriptorType DescriptorType { get; init; }

        /// <summary>
        /// The icon url of the descriptor's icon.
        /// </summary>
        public string IconUrl { get; init; }

        /// <summary>
        /// The display text of the descriptor.
        /// </summary>
        public string DisplayText { get; init; }


        /// <summary>
        /// The type of the content described in this descriptor.
        /// </summary>
        /// <remarks>Valid for the following types: CrudeHumor, Romance, Gambling</remarks>
        public string? Type { get; init; }

        /// <summary>
        /// The frequency of the content described in this descriptor.
        /// </summary>
        /// <remarks>Valid for the following types: Violence, Fear</remarks>
        public string? Frequency { get; init; }

        /// <summary>
        /// The realism of the content described in this descriptor.
        /// </summary>
        /// <remarks>Valid for the following types: Blood</remarks>
        public string? Realism { get; init; }

        /// <summary>
        /// The blood level of the content described in this descriptor.
        /// </summary>
        /// <remarks>Valid for the following types: Blood</remarks>
        public string? BloodLevel { get; init; }

        /// <summary>
        /// The intensity of the content described in this descriptor.
        /// </summary>
        /// <remarks>Valid for the following types: Violence, Fear</remarks>
        public string? Intensity { get; init; }

        /// <summary>
        /// The presence of the content described in this descriptor.
        /// </summary>
        /// <remarks>Valid for the following types: Alcohol, StrongLanguage, SocialHangout, FreeFormUserCreation</remarks>
        public string? Presence { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ExperienceDescriptor {DescriptorType} [{DisplayText}] IconUrl: <{IconUrl}> Type: <{Type}> Frequency: <{Frequency}> Realism: <{Realism}> BloodLevel: <{BloodLevel}> Intensity: <{Intensity}> Presence: <{Presence}>";
        }
    }

    /// <summary>
    /// Represents an action log within an experience's activity history.
    /// </summary>
    public readonly struct ExperienceAuditLog
    {
        /// <summary>
        /// The type of experience log.
        /// </summary>
        public ExperienceAuditLogType Type { get; init; }

        /// <summary>
        /// The time the action occurred.
        /// </summary>
        public DateTime Time { get; init; }

        /// <summary>
        /// The unique Id of the action.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// The universe Id the action occurred in.
        /// </summary>
        public Id<Experience> UniverseId { get; init; }

        /// <summary>
        /// The place Id that is targeted by the action. Can be <see langword="null"/> for non-place actions.
        /// </summary>
        public ulong? PlaceId { get; init; }

        /// <summary>
        /// The user Id of the user that performed the action.
        /// </summary>
        public Id<User> UserId { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ExperienceAuditLog {Type} [{Time}] {{{Id}}} ExpID: {UniverseId.UniqueId} UserID: {UserId.UniqueId}";
        }
    }

    /// <summary>
    /// Specifies the options to use for experience modification.
    /// </summary>
    /// <remarks>Any property in this class that is not changed will not modify the website.</remarks>
    public class ExperienceModifyOptions
    {
        /// <summary>
        /// The new name of the experience.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The new description of the experience.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether or not to enable or disable private servers.
        /// </summary>
        public bool? EnablePrivateServers { get; set; }

        /// <summary>
        /// The new price for private servers.
        /// </summary>
        public int? PrivateServerPrice { get; set; }

        /// <summary>
        /// Whether or not the experience is paid access.
        /// </summary>
        public bool? PurchaseRequired { get; set; }

        /// <summary>
        /// The cost to purchase the game.
        /// </summary>
        public int? Cost { get; set; }

        /// <summary>
        /// Whether or not the experience is friends-only.
        /// </summary>
        public bool? FriendsOnly { get; set; }

        /// <summary>
        /// The new list of devices that can play.
        /// </summary>
        public List<Device>? PlayableDevices { get; set; }

        /// <summary>
        /// Whether or not studio access to APIs is allowed.
        /// </summary>
        public bool? StudioAccessToAPIsAllowed { get; set; }
    }
}
