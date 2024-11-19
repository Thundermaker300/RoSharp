﻿using Newtonsoft.Json.Linq;
using RoSharp.API.Misc;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Utility;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A class that represents a collection of connected places, also known as a "universe".
    /// </summary>
    /// <seealso cref="FromId(ulong, Session?)"/>
    public class Experience : APIMain, IRefreshable, IIdApi<Experience>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("games");

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

        private ulong ownerId;
        private string ownerName;
        private bool isGroupOwned;

        /// <summary>
        /// Gets the unique Id (group or user) of the owner of this experience.
        /// </summary>
        public ulong OwnerId => ownerId;

        /// <summary>
        /// Gets the name (group or user) of the owner of this experience.
        /// </summary>
        public string OwnerName => ownerName;

        /// <summary>
        /// Gets whether or not this experience is owned by a group.
        /// </summary>
        /// <seealso cref="GetOwnerAsync"/>
        public bool IsGroupOwned => isGroupOwned;

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
        {
            ulong universeId = 0;

            HttpResponseMessage response = await genericClient.GetAsync($"{Constants.URL("apis")}/universes/v1/places/{placeId}/universe");
            string raw = await response.Content.ReadAsStringAsync();
            dynamic universeData = JObject.Parse(raw);
            if (universeData.universeId != null)
            {
                universeId = universeData.universeId;
            }
            else
            {
                throw new ArgumentException("Invalid place ID provided.", nameof(placeId));
            }

            return await FromId(universeId, session);
        }

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
            // TODO: These need to be updated within this method
            playabilityStatus = null;
            socialChannels = null;
            icon = null;
            history = null;

            HttpResponseMessage response = await GetAsync($"/v1/games?universeIds={UniverseId}");
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
            genre = ExperienceUtility.GetGenre(Convert.ToString(data.genre_l1));
            subgenre = ExperienceUtility.GetGenre(Convert.ToString(data.genre_l2));

            favoritedByUser = data.isFavoritedByUser;

            if (data.price == null)
                cost = 0;
            else
                cost = data.price;

            ulong creatorId = Convert.ToUInt64(data.creator.id);
            ownerId = creatorId;
            ownerName = data.creator.name;
            isGroupOwned = data.creator.type == "Group";

            // configs
            await UpdateConfigurationAsync();

            await UpdateExperienceGuidelinesDataAsync();
            await UpdateVoiceVideoAsync();
            await UpdateVotesAsync();

            RefreshedAt = DateTime.Now;
        }

        /// <summary>
        /// Returns the owner of this experience.
        /// </summary>
        /// <returns>A task containing the owner of this experience.</returns>
        public async Task<IAssetOwner> GetOwnerAsync()
        {
            if (IsGroupOwned)
            {
                return await Group.FromId(OwnerId);
            }
            return await User.FromId(OwnerId);
        }

        private bool? voiceEnabled = null;
        private bool? videoEnabled = null;

        private async Task UpdateVoiceVideoAsync()
        {
            if (!SessionVerify.Verify(session))
                return;

            string commSetting = await GetStringAsync($"/v1/settings/universe/{UniverseId}", Constants.URL("voice"));
            dynamic data = JObject.Parse(commSetting);

            voiceEnabled = data.isUniverseEnabledForVoice;
            videoEnabled = data.isUniverseEnabledForAvatarVideo;
        }

        /// <summary>
        /// Gets whether or not voice chat is enabled.
        /// </summary>
        public bool VoiceEnabled => voiceEnabled.Value;

        /// <summary>
        /// Gets whether or not facial tracking is enabled.
        /// </summary>
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

        private ExperienceMaturityLevel maturityLevel;

        /// <summary>
        /// Gets a <see cref="ExperienceMaturityLevel"/> representing the maturity level defined by Roblox for this experience.
        /// </summary>
        public ExperienceMaturityLevel MaturityLevel => maturityLevel;

        private ReadOnlyCollection<ExperienceDescriptor>? experienceDescriptors;

        /// <summary>
        /// Gets a read-only collection of defined descriptors that contribute to this experience's <see cref="MinimumAge"/>.
        /// </summary>
        public ReadOnlyCollection<ExperienceDescriptor> ExperienceDescriptors => experienceDescriptors;

        private async Task UpdateExperienceGuidelinesDataAsync()
        {
            // Update profanity
            string commSetting = await GetStringAsync($"/asset-text-filter-settings/public/universe/{UniverseId}", Constants.URL("apis"));
            dynamic dataProfane = JObject.Parse(commSetting);
            profanityAllowed = dataProfane.Profanity;

            // Update guidelines
            object body = new
            {
                universeId = UniverseId.ToString()
            };

            HttpResponseMessage response = await PostAsync("/experience-guidelines-api/experience-guidelines/get-age-recommendation", body, Constants.URL("apis"));
            string rawData = await response.Content.ReadAsStringAsync();
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
                response = await PatchAsync($"/v2/universes/{UniverseId}/configuration", new { }, Constants.URL("develop"), "Experience.UpdateConfigurationAsync");
            }
            catch (Exception ex)
            {}

            if (response == null)
                return;

            string rawData = await response.Content.ReadAsStringAsync();
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
        
        /// <summary>
        /// Gets this experience's icon.
        /// </summary>
        /// <returns>A task containing the <see cref="Asset"/> upon completion. Can be <see langword="null"/>.</returns>
        public async Task<Asset?> GetIconAsync()
        {
            if (icon == null)
            {
                string rawData = await GetStringAsync($"/v1/games/{UniverseId}/icon");
                dynamic data = JObject.Parse(rawData);
                if (data.imageId != null)
                {
                    ulong assetId = Convert.ToUInt64(data.imageId);
                    icon = await Asset.FromId(assetId, session);
                }
            }

            return icon;
        }

        /// <summary>
        /// Gets this experience's badges.
        /// </summary>
        /// <param name="limit">The amount of badges to get at one time.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<GenericId<Badge>>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"v1/universes/{UniverseId}/badges?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, Constants.URL("badges"));
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Badge>> list = new();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(new(id, session));
            }

            return new PageResponse<GenericId<Badge>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets this experience's thumbnails.
        /// </summary>
        /// <param name="size">The thumbnail size to use.</param>
        /// <param name="defaultRobloxThumbnailIfNecessary">If the experience has no thumbnails, return the default Roblox thumbnail?</param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> of <see cref="Asset"/>s that are the experience's thumbnails.</returns>
        /// <exception cref="ArgumentException">Invalid experience to get thumbnails for.</exception>
        public async Task<ReadOnlyCollection<Asset>> GetThumbnailsAsync(ExperienceThumbnailSize size = ExperienceThumbnailSize.S768x432, bool defaultRobloxThumbnailIfNecessary = true)
        {
            string url = $"/v1/games/multiget/thumbnails?universeIds={UniverseId}&countPerUniverse=25&defaults={defaultRobloxThumbnailIfNecessary.ToString().ToLower()}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, Constants.URL("thumbnails"));
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

        /// <summary>
        /// Sets the privacy of the experience.
        /// </summary>
        /// <param name="isPublic">Whether or not the experience is open to the public.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task SetPrivacyAsync(bool isPublic)
        {
            string url = $"/v1/universes/{Id}/{(isPublic == false ? "de" : string.Empty)}activate";
            HttpResponseMessage response = await PostAsync(url, new { }, Constants.URL("develop"), "Experience.SetPrivacyAsync");
        }

        /// <summary>
        /// Modifies the experience.
        /// </summary>
        /// <param name="options">The options to use.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task ModifyAsync(ExperienceModifyOptions options)
        {
            object body = new
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
            };

            HttpResponseMessage response = await PatchAsync($"/v2/universes/{UniverseId}/configuration", body, Constants.URL("develop"), "Experience.ModifyAsync");
        }

        /// <summary>
        /// Sets the experience's genre to the given <see cref="Enums.Genre"/>.
        /// </summary>
        /// <param name="newGenre">The genre to set. Will throw an exception for <see cref="Genre.Unknown"/> and <see cref="Genre.None"/>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <remarks>Genres can only be changed ONCE every three months.</remarks>
        public async Task ModifyGenreAsync(Genre newGenre)
        {
            object body = new
            {
                universeId = UniverseId,
                genreTaxonomyVersion = 1,
                genre = ExperienceUtility.ToInternalKey(newGenre),
                responseOptions = new
                {
                    includeUpdateLockExpirationTime = true,
                }
            };

            HttpResponseMessage response = await PostAsync("/experience-genre-api/v1/Creator/ExperienceGenre", body, "https://apis.roblox.com", "Experience.ModifyGenreAsync");
            
        }

        /// <summary>
        /// Bans a user from the experience.
        /// </summary>
        /// <param name="userId">The user Id to ban.</param>
        /// <param name="displayReason">The public reason to display to the user.</param>
        /// <param name="privateReason">The private ban reason.</param>
        /// <param name="permanent">True for a permanent ban.</param>
        /// <param name="length">If <paramref name="permanent"/> is <see langword="false"/>, the length of time to use for the ban.</param>
        /// <param name="excludeAlts">If suspected alts should be included in the ban.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="InvalidOperationException">length cannot be null if permanent is false.</exception>
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

            HttpResponseMessage response = await PatchAsync($"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}", body, Constants.URL("apis"), "Experience.BanUserAsync");
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

            HttpResponseMessage response = await PatchAsync($"/user/cloud/v2/universes/{UniverseId}/user-restrictions/{userId}?", body, Constants.URL("apis"), "Experience.UnbanUserAsync");
        }

        public async Task UnbanUserAsync(User user)
            => await UnbanUserAsync(user.Id);

        public async Task UnbanUserAsync(string username)
            => await UnbanUserAsync(await User.FromUsername(username, session));

        public async Task PostUpdateAsync(string text)
        {
            HttpResponseMessage response = await PostAsync($"/game-update-notifications/v1/publish/{UniverseId}", text, Constants.URL("apis"), "Experience.PostUpdateAsync");
        }

        private ReadOnlyCollection<ExperienceActivityHistory>? history;
        public async Task<ReadOnlyCollection<ExperienceActivityHistory>> GetActivityHistoryAsync()
        {
            if (history == null)
            {
                string rawData = await GetStringAsync("/activity-feed-api/v1/history?clientType=1&universeId=3744484651", "https://apis.roblox.com", "Experience.GetActivityHistoryAsync");
                dynamic data = JObject.Parse(rawData);

                List<ExperienceActivityHistory> list = new();
                foreach (dynamic ev in data.events)
                {
                    list.Add(new()
                    {
                        Id = ev.id,
                        Type = (ExperienceActivityHistoryType)ev.eventType,
                        UniverseId = new GenericId<Experience>(Convert.ToUInt64(ev.universeId)),
                        PlaceId = ev.placeId,
                        UserId = new GenericId<User>(Convert.ToUInt64(ev.userId)),
                        Time = DateTime.UnixEpoch.AddMilliseconds(Convert.ToInt64(ev.createdUnixTimeMs)),
                    });
                }

                history = list.AsReadOnly();
            }
            return history;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} [{UniverseId}] {{{(!IsGroupOwned ? "@" : string.Empty)}{OwnerName}}} <R${Cost}>";
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

    public struct ExperienceDescriptor
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DescriptorType} [{DisplayText}] IconUrl: <{IconUrl}> Type: <{Type}> Frequency: <{Frequency}> Realism: <{Realism}> BloodLevel: <{BloodLevel}> Intensity: <{Intensity}> Presence: <{Presence}>";
        }
    }

    public struct ExperienceActivityHistory
    {
        public ExperienceActivityHistoryType Type { get; init; }
        public DateTime Time { get; init; }
        public string Id { get; init; }
        public GenericId<Experience> UniverseId { get; init; }
        public ulong? PlaceId { get; init; }
        public GenericId<User> UserId { get; init; }

        public override string ToString()
        {
            return $"{Type} [{Time}] {{{Id}}} ExpID: {UniverseId.Id} UserID: {UserId.Id}";
        }
    }

    public class ExperienceModifyOptions
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? EnablePrivateServers { get; set; }
        public int? PrivateServerPrice { get; set; }
        public bool? PurchaseRequired { get; set; }
        public int? Cost { get; set; }
        public bool? FriendsOnly { get; set; }
        public List<Device>? PlayableDevices { get; set; }
        public bool? StudioAccessToAPIsAllowed { get; set; }
    }
}
