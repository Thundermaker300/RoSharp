using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Collections.ObjectModel;

namespace RoSharp.API
{
    /// <summary>
    /// A class that represents a Roblox user.
    /// </summary>
    /// <seealso cref="FromId(ulong, Session?)"/>
    /// <seealso cref="FromUsername(string, Session?)"/>
    public class User : APIMain, IRefreshable, IAssetOwner, IIdApi<User>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("users");

        /// <summary>
        /// Gets the user's unique Id.
        /// </summary>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/users/{Id}/profile";

        /// <inheritdoc/>
        public AssetOwnerType OwnerType => AssetOwnerType.User;

        private string name;

        /// <summary>
        /// Gets the user's username. Equivalent to <see cref="Username"/>.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the user's username. Equivalent to <see cref="Name"/>.
        /// </summary>
        public string Username => Name;

        private string displayName;

        /// <summary>
        /// Gets the user's display name, or their <see cref="Username"/> if one is not set.
        /// </summary>
        public string DisplayName => displayName;

        private string bio;
        
        /// <summary>
        /// Gets the user's bio.
        /// </summary>
        public string Bio => bio;

        private bool verified;

        /// <summary>
        /// Gets whether or not this user is verified (blue checkmark).
        /// </summary>
        public bool Verified => verified;

        private DateTime joinDate;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the user joined Roblox.
        /// </summary>
        public DateTime JoinDate => joinDate;

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> representing the age of the account.
        /// </summary>
        public TimeSpan AccountAge => DateTime.UtcNow - JoinDate;

        private bool profileHidden;

        /// <summary>
        /// Gets whether or not their profile is hidden (either banned or disabled account).
        /// </summary>
        public bool ProfileHidden => ProfileHidden;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private User(ulong userId, Session? session = null)
        {
            Id = userId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<User>.Contains(Id))
                RoPool<User>.Add(this);
        }

        /// <summary>
        /// Returns a <see cref="User"/> given a user's unique Id.
        /// </summary>
        /// <param name="userId">The user Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="User"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the asset Id invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<User> FromId(ulong userId, Session? session = null)
        {
            if (RoPool<User>.Contains(userId))
                return RoPool<User>.Get(userId, session.Global());

            User newUser = new(userId, session.Global());
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <summary>
        /// Returns a <see cref="User"/> given a username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="User"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the asset Id invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<User> FromUsername(string username, Session? session = null)
            => await FromId(await UserUtility.GetUserIdAsync(username), session);

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"/v1/users/{Id}");

            string raw = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(raw);

            name = data.name;
            displayName = data.displayName;
            verified = data.hasVerifiedBadge;
            joinDate = data.created;
            profileHidden = data.isBanned;
            bio = data.description;

            robloxBadges = null;
            primaryGroup = null;
            socialChannels = null;
            groups = null;
            customName = null;
            experiences = null;

            // Update premium + inventory
            if (SessionVerify.Verify(session))
            {
                var message = new HttpMessage(HttpMethod.Get, $"/v1/users/{Id}/validate-membership")
                {
                    SilenceExceptions = true
                };

                // Premium
                HttpResponseMessage premiumResponse = await SendAsync(message, Constants.URL("premiumfeatures"));
                if (premiumResponse.IsSuccessStatusCode)
                    isPremium = Convert.ToBoolean(await premiumResponse.Content.ReadAsStringAsync());

                // Private Inventory
                message.Url = $"/v1/users/{Id}/can-view-inventory";

                HttpResponseMessage inventoryResponse = await SendAsync(message, Constants.URL("inventory"));
                if (inventoryResponse.IsSuccessStatusCode)
                {
                    dynamic inventoryData = JObject.Parse(await inventoryResponse.Content.ReadAsStringAsync());
                    privateInventory = !Convert.ToBoolean(inventoryData.canView);
                }
            }
            else
            {
                isPremium = null;
                privateInventory = true;
            }


            // Updating data
            await UpdateFollowingsAsync();
            await UpdateAvatarAsync();
            RefreshedAt = DateTime.Now;
        }

        private async Task UpdateAvatarAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v2/avatar/users/{Id}/avatar", Constants.URL("avatar"));
            dynamic data = JObject.Parse(rawData);

            avatarType = Enum.Parse<AvatarType>(Convert.ToString(data.playerAvatarType), true);

            Dictionary<AvatarScaleType, double> scales = [];
            foreach (dynamic scale in data.scales)
            {
                if (Enum.TryParse<AvatarScaleType>(Convert.ToString(scale.Name), true, out AvatarScaleType result))
                    scales.Add(result, Convert.ToDouble(scale.Value));
            }
            avatarScales = scales.AsReadOnly();

            List<Id<Asset>> assets = [];
            foreach (dynamic asset in data.assets)
            {
                assets.Add(new(Convert.ToUInt64(asset.id), session));
            }
            currentlyWearing = assets.AsReadOnly();

            Dictionary<BodyColorType, Color> colors = new(6);
            foreach (dynamic color in data.bodyColor3s)
            {
                string name = color.Name;
                name = name.Replace("Color3", string.Empty);

                BodyColorType type = Enum.Parse<BodyColorType>(name, true);
                Color colorObj = new(Convert.ToString(color.Value));
                colors.Add(type, colorObj);
            }
            bodyColors = colors.AsReadOnly();
        }

        private async Task UpdateFollowingsAsync()
        {
            dynamic followingsData = JObject.Parse(await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/followings/count", Constants.URL("friends")));
            dynamic followersData = JObject.Parse(await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/followers/count", Constants.URL("friends")));

            following = followingsData.count;
            followers = followersData.count;
        }

        private bool? isPremium;

        /// <summary>
        /// Gets whether or not this user has Roblox Premium.
        /// </summary>
        /// <remarks>This property is unavailable and will throw a <see cref="RobloxAPIException"/> if this user is not authenticated.</remarks>
        /// <exception cref="RobloxAPIException"></exception>
        public bool IsPremium
        {
            get
            {
                if (!isPremium.HasValue)
                {
                    SessionVerify.ThrowRefresh("User.IsPremium");
                }
                return isPremium.GetValueOrDefault();
            }
        }

        private AvatarType avatarType;

        /// <summary>
        /// Gets the avatar type of the user. Will either be <see cref="AvatarType.R6"/> or <see cref="AvatarType.R15"/>.
        /// </summary>
        public AvatarType AvatarType => avatarType;

        private ReadOnlyDictionary<AvatarScaleType, double> avatarScales;

        /// <summary>
        /// Gets the user's choice of avatar scaling.
        /// </summary>
        public ReadOnlyDictionary<AvatarScaleType, double> AvatarScales => avatarScales;

        private ReadOnlyDictionary<BodyColorType, Color> bodyColors;

        /// <summary>
        /// Gets the user's body colors.
        /// </summary>
        public ReadOnlyDictionary<BodyColorType, Color> BodyColors => bodyColors;

        private int following = -1;

        /// <summary>
        /// Gets the amount of users this user is following.
        /// </summary>
        public int Following => following;

        private int followers = -1;

        /// <summary>
        /// Gets the amount of users that are following this user.
        /// </summary>
        public int Followers => followers;



        private ReadOnlyCollection<string>? robloxBadges;

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of Roblox badges this user has.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="string"/>s upon completion, each being the name of a Roblox badge.</returns>
        public async Task<ReadOnlyCollection<string>> GetRobloxBadgesAsync()
        {
            if (robloxBadges == null)
            {
                List<string> badges = [];
                string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/roblox-badges", Constants.URL("accountinformation"));
                JArray data = JArray.Parse(rawData);
                foreach (dynamic badgeData in data.Children<JObject>())
                {
                    badges.Add(badgeData.name.ToString());
                }
                robloxBadges = badges.AsReadOnly();
            }

            return robloxBadges;
        }

        /// <summary>
        /// Gets this user's rename history.
        /// </summary>
        /// <param name="limit">The limit of usernames to retrieve.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of strings when completed.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<ReadOnlyCollection<string>> GetRenameHistoryAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/username-history?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            List<string> history = [];
            string rawData = await SendStringAsync(HttpMethod.Get, url);
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic historyData in data.data)
            {
                history.Add(historyData.name.ToString());
            }

            return history.AsReadOnly();
        }

        private Community? primaryGroup;

        /// <summary>
        /// Gets the user's primary community.
        /// </summary>
        /// <returns>A task containing a <see cref="Community"/> on completion. Can be <see langword="null"/> if the user does not have a primary community.</returns>
        public async Task<Community?> GetPrimaryGroupAsync()
        {
            if (primaryGroup == null)
            {
                string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/groups/primary/role", Constants.URL("groups"));
                if (rawData == "null") // love roblox
                    return null;

                dynamic data = JObject.Parse(rawData);
                ulong communityId = Convert.ToUInt64(data.group.id);
                primaryGroup = await Community.FromId(communityId, session);
            }
            return primaryGroup;
        }

        private ReadOnlyDictionary<Community, Role>? groups;

        /// <summary>
        /// Gets the communities this user is in as well as the role they are.
        /// </summary>
        /// <param name="limit">The limit of communities to return. Set to <c>-1</c> for all.</param>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> of communities and the role the user has in them.</returns>
        public async Task<ReadOnlyDictionary<Community, Role>> GetGroupsAsync(int limit = -1)
        {
            if (groups == null)
            {
                string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/groups/roles", Constants.URL("groups"));
                dynamic data = JObject.Parse(rawData);

                Dictionary<Community, Role> dict = [];
                int count = 0;
                foreach (dynamic groupData in data.data)
                {
                    if (limit > 0 && count >= limit)
                        break;

                    ulong communityId = Convert.ToUInt64(groupData.group.id);
                    Community group = await Community.FromId(communityId, session);
                    dict.Add(group, (await group.GetRoleManagerAsync()).GetRole(Convert.ToByte(groupData.role.rank)));
                    count++;
                }

                groups = dict.AsReadOnly();
            }

            return groups;
        }

        private bool privateInventory;

        /// <summary>
        /// Indicates whether or not the authenticated user can see this user's inventory.
        /// </summary>
        public bool PrivateInventory
            => privateInventory;

        private ReadOnlyDictionary<string, string>? socialChannels;

        /// <summary>
        /// Returns this user's social channels.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> when complete.</returns>
        /// <remarks>The keys of the dictionary are the social media type, the value is the URL.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<ReadOnlyDictionary<string, string>> GetSocialChannelsAsync()
        {
            if (socialChannels == null)
            {
                Dictionary<string, string> dict = [];
                string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/promotion-channels?alwaysReturnUrls=true", Constants.URL("accountinformation"));
                dynamic data = JObject.Parse(rawData);
                foreach (dynamic media in data)
                {
                    if (media.Value == null) continue;
                    dict.Add(Convert.ToString(media.Name), Convert.ToString(media.Value));
                }
                socialChannels = dict.AsReadOnly();
            }

            return socialChannels;
        }

        private ReadOnlyCollection<Id<Asset>> currentlyWearing;

        /// <summary>
        /// Gets a list of assets the user is currently wearing.
        /// </summary>
        // [TODO BEFORE RELEASE] Convert this to a new type with information on the avatar's x,y,z of position, rotation, and scale.
        public ReadOnlyCollection<Id<Asset>> CurrentlyWearing => currentlyWearing;

        /// <summary>
        /// Returns a list of assets this user is currently wearing.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        [Obsolete("Use User.CurrentlyWearing.")]
        public async Task<ReadOnlyCollection<Id<Asset>>> GetCurrentlyWearingAsync()
            => CurrentlyWearing;

        /// <summary>
        /// Returns a list of assets that are in this user's collection.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <exception cref="ArgumentException">Invalid user.</exception>
        public async Task<ReadOnlyCollection<Id<Asset>>> GetCollectionItemsAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/users/profile/robloxcollections-json?userId={Id}", Constants.ROBLOX_URL_WWW);
            if (rawData.Contains("Invalid user"))
                throw new ArgumentException("Invalid user.");
            Console.WriteLine(rawData);
            dynamic data = JObject.Parse(rawData);

            List<Id<Asset>> list = [];
            foreach (dynamic item in data.CollectionsItems)
            {
                ulong assetId = Convert.ToUInt64(item.Id);
                list.Add(new(assetId, session));
            }
            return list.AsReadOnly();
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> of <see cref="User"/> instances that are friends with this user.
        /// </summary>
        /// <param name="limit">The maximum amount of friends to return.</param>
        /// <returns><see cref="ReadOnlyCollection{T}"/></returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<ReadOnlyCollection<Id<User>>> GetFriendsAsync(int limit = 50)
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/friends", Constants.URL("friends"));
            dynamic data = JObject.Parse(rawData);
            List<Id<User>> friends = [];
            int count = 0;
            foreach (dynamic friendData in data.data)
            {
                count++;

                ulong friendId = Convert.ToUInt64(friendData.id);
                friends.Add(new Id<User>(friendId, session));

                if (count >= limit)
                    break;
            }

            return friends.AsReadOnly();
        }

        /// <summary>
        /// Gets the Ids of the users this user is following.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<Id<User>>> GetFollowingAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/followings?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<Id<User>>();
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, url, Constants.URL("friends"));
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = item.id;
                list.Add(new(id, session));
            }

            return new PageResponse<Id<User>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets the Ids of the users this user is followed by.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<Id<User>>> GetFollowersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/followers?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<Id<User>>();
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, url, Constants.URL("friends"));
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = item.id;
                list.Add(new(id, session));
            }

            return new PageResponse<Id<User>>(list, nextPage, previousPage);
        }

        private string? customName;

        /// <summary>
        /// Gets the user's custom name for the authenticated user.
        /// </summary>
        /// <returns>A task containing the custom name, or <see langword="null"/> if there is not a custom name for this user.</returns>
        public async Task<string?> GetCustomNameAsync()
        {
            if (customName == null)
            {
                var message = new HttpMessage(HttpMethod.Post, "/v1/user/get-tags", new
                {
                    targetUserIds = new[] { Id }
                })
                {
                    AuthType = AuthType.RobloSecurity,
                    ApiName = nameof(GetCustomNameAsync),
                };

                HttpResponseMessage response = await SendAsync(message, Constants.URL("contacts"));
                JArray data = JArray.Parse(await response.Content.ReadAsStringAsync());
                if (data.Count != 0)
                {
                    JToken wanted = data[0];
                    customName = Convert.ToString(wanted["targetUserTag"]);
                }
            }
            return customName;
        }

        /// <summary>
        /// Sets this user's custom name for the authenticated user. Set to <see cref="string.Empty"/> to clear custom name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetCustomName(string name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));

            var message = new HttpMessage(HttpMethod.Post, "/v1/user/tag", new
            {
                targetUserId = Id,
                userTag = name,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(SetCustomName),
            };

            await SendAsync(message, Constants.URL("contacts"));
        }

        /// <summary>
        /// Gets whether or not this user is in the given community.
        /// </summary>
        /// <param name="community">The community.</param>
        /// <returns>A task containing a bool.</returns>
        public async Task<bool> IsInGroupAsync(Community community) => await (await community.GetMemberManagerAsync()).IsInCommunityAsync(Id);

        /// <summary>
        /// Gets whether or not this user is in the community with the given Id.
        /// </summary>
        /// <param name="communityId">The communityId.</param>
        /// <returns>A task containing a bool.</returns>
        public async Task<bool> IsInGroupAsync(ulong communityId) => await (await (await Community.FromId(communityId)).GetMemberManagerAsync()).IsInCommunityAsync(Id);
        
        // Thumbnails
        /// <summary>
        /// Returns a thumbnail of the given user.
        /// </summary>
        /// <param name="type">The type of thumbnail.</param>
        /// <param name="size">The size of the thumbnail.</param>
        /// <returns>A task containing a string URL to the thumbnail upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <exception cref="ArgumentException">Invalid user to get thumbnail for.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<string> GetThumbnailAsync(ThumbnailType type = ThumbnailType.Full, ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = "/v1/users/avatar" + type switch
            {
                ThumbnailType.Bust => "-bust",
                ThumbnailType.Headshot => "-headshot",
                _ => string.Empty,
            } + $"?userIds={Id}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid user to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        private ReadOnlyCollection<Id<Experience>>? experiences;

        /// <summary>
        /// Returns experiences that are owned by the user and shown on their profile.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<ReadOnlyCollection<Id<Experience>>> GetExperiencesAsync()
        {
            if (experiences == null)
            {
                string rawData = await SendStringAsync(HttpMethod.Get, $"/users/profile/playergames-json?userId={Id}", Constants.ROBLOX_URL_WWW);
                dynamic data = JObject.Parse(rawData);

                List<Id<Experience>> list = [];
                foreach (dynamic experience in data.Games)
                {
                    ulong id = experience.UniverseID;
                    list.Add(new(id, session));
                }
                experiences = list.AsReadOnly();
            }
            return experiences;
        }

        /// <summary>
        /// Gets whether or not this user owns the asset with the given Id.
        /// </summary>
        /// <param name="assetId">The asset Id.</param>
        /// <param name="assetItemType">The assetItemType. For most assets this value should be <c>0</c>.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> OwnsAssetAsync(ulong assetId, int assetItemType = 0)
        {
            string result = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/items/{assetItemType}/{assetId}/is-owned", Constants.URL("inventory"));
            return Convert.ToBoolean(result);
        }

        /// <summary>
        /// Gets whether or not this user owns the given <paramref name="asset"/>.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> OwnsAssetAsync(Asset asset)
            => await OwnsAssetAsync(asset.Id);

        /// <summary>
        /// Gets whether or not this user owns the badge with the given Id.
        /// </summary>
        /// <param name="badgeId">The badge Id.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> HasBadgeAsync(ulong badgeId)
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/users/{Id}/badges/awarded-dates?badgeIds={badgeId}", Constants.URL("badges"));
            dynamic data = JObject.Parse(rawData);

            return data.data.Count > 0;
        }

        /// <summary>
        /// Gets whether or not this user owns the given <paramref name="badge"/>.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> HasBadgeAsync(Badge badge)
            => await HasBadgeAsync(badge.Id);

        /// <summary>
        /// Gets this user's player badges.
        /// </summary>
        /// <param name="limit">The amount to get at one time.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<Id<Badge>>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/badges?sortOrder={sortOrder}&limit={limit.Limit()}";
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
        /// Returns items that the user owns. This method WILL throw an exception if the authenticated user cannot see this user's inventory.
        /// </summary>
        /// <param name="assetType">The <see cref="AssetType"/> to use for this request.</param>
        /// <param name="limit">The limit of assets to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called. This API also does not work for Badges, Bundles, and GamePasses -- see their respective APIs.</remarks>
        /// <seealso cref="PrivateInventory"/>
        /// <seealso cref="GetBadgesAsync(FixedLimit, RequestSortOrder, string?)"/>
        /// <exception cref="RobloxAPIException">Roblox API failure or the authenticated user cannot see this user's inventory.</exception>
        public async Task<PageResponse<Id<Asset>>> GetInventoryAsync(AssetType assetType, FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/users/{Id}/inventory/{(int)assetType}?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.URL("inventory"));
            dynamic data = JObject.Parse(rawData);

            List<Id<Asset>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.assetId);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<Asset>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Returns items that the user has favorited.
        /// </summary>
        /// <param name="assetType">The <see cref="AssetType"/> to use for this request.</param>
        /// <param name="limit">The limit of assets to return.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called. This API also does not work for Badges, Bundles, and GamePasses -- see their respective APIs.</remarks>
        public async Task<PageResponse<Id<Asset>>> GetFavoritesAsync(AssetType assetType, FixedLimit limit = FixedLimit.Limit100, string? cursor = null)
        {
            string url = $"/users/favorites/list-json?userId={Id}&assetTypeId={(int)assetType}&itemsPerPage={limit.Limit()}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.ROBLOX_URL_WWW);
            dynamic data = JObject.Parse(rawData);

            List<Id<Asset>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.Data.Items)
            {
                ulong id = Convert.ToUInt64(item.Item.AssetId);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<Asset>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets this user's current presence status, including their location and last online.
        /// </summary>
        /// <returns>A task containing a <see cref="UserPresence"/>, when completed.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called. Authentication is not required for this API, however providing an authenticated session may return more data.</remarks>
        /// <exception cref="RobloxAPIException">Failed to retrieve presence information, please try again later.</exception>
        public async Task<UserPresence> GetPresenceAsync()
        {
            object body = new
            {
                userIds = new[] { Id },
            };

            HttpResponseMessage response = await SendAsync(HttpMethod.Post, "/v1/presence/users", Constants.URL("presence"), body);
            dynamic uselessData = JObject.Parse(await response.Content.ReadAsStringAsync());
            dynamic data = uselessData.userPresences[0];

            UserLocationType type = (UserLocationType)Convert.ToInt32(data.userPresenceType);
            Experience? exp = data.universeId != null ? await Experience.FromId(Convert.ToUInt64(data.universeId)) : null;
            DateTime lastOnline = data.lastOnline;
            return new UserPresence(type, exp, lastOnline);
        }

        /// <summary>
        /// Sends a friend request to the user.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task SendFriendRequestAsync()
            => await SendAsync(HttpMethod.Post, $"/v1/contacts/{Id}/request-friendship", body: new { });

        /// <summary>
        /// Unfriends the user.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task UnfriendAsync() =>
            await SendAsync(HttpMethod.Post, $"/v1/users/{Id}/unfriend", body: new { });


        /// <inheritdoc/>
        public override string ToString()
        {
            return $"User {DisplayName} (@{Name}) [{Id}]{(Verified ? " [V]" : string.Empty)}";
        }

        /// <inheritdoc/>
        public User AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
