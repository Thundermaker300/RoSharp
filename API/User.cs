using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Misc;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
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

        public static async Task<User> FromId(ulong userId, Session? session = null)
        {
            if (RoPool<User>.Contains(userId))
                return RoPool<User>.Get(userId, session.Global());

            User newUser = new(userId, session.Global());
            await newUser.RefreshAsync();

            return newUser;
        }

        public static async Task<User> FromUsername(string username, Session? session = null)
            => await FromId(await UserUtility.GetUserIdAsync(username), session);

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v1/users/{Id}");

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
            currentlyWearing = null;
            collections = null;
            groups = null;

            // Update premium
            if (SessionVerify.Verify(session))
            {
                // Premium
                HttpResponseMessage premiumResponse = await GetAsync($"/v1/users/{Id}/validate-membership", Constants.URL("premiumfeatures"), "User.IsPremium [RefreshAsync]");
                isPremium = Convert.ToBoolean(await premiumResponse.Content.ReadAsStringAsync());

                // Private Inventory
                HttpResponseMessage inventoryResponse = await GetAsync($"/v1/users/{Id}/can-view-inventory", Constants.URL("inventory"), "User.PrivateInventory [RefreshAsync]");
                dynamic inventoryData = JObject.Parse(await response.Content.ReadAsStringAsync());
                privateInventory = !Convert.ToBoolean(inventoryData.canView);
            }
            else
            {
                isPremium = null;
                privateInventory = true;
            }


            // Updating Following/Followers
            await UpdateFollowingsAsync();
            RefreshedAt = DateTime.Now;
        }

        public async Task UpdateFollowingsAsync()
        {
            dynamic followingsData = JObject.Parse(await GetStringAsync($"/v1/users/{Id}/followings/count", Constants.URL("friends")));
            dynamic followersData = JObject.Parse(await GetStringAsync($"/v1/users/{Id}/followers/count", Constants.URL("friends")));

            following = followingsData.count;
            followers = followersData.count;
        }

        private bool? isPremium;
        public bool IsPremium
        {
            get
            {
                if (!isPremium.HasValue)
                {
                    SessionVerify.ThrowRefresh("User.IsPremium");
                }
                return isPremium.Value;
            }
        }

        private int following = -1;
        public int Following => following;

        private int followers = -1;
        public int Followers => followers;

        private ReadOnlyCollection<string>? robloxBadges;
        public async Task<ReadOnlyCollection<string>> GetRobloxBadgesAsync()
        {
            if (robloxBadges == null)
            {
                List<string> badges = new();
                string rawData = await GetStringAsync($"/v1/users/{Id}/roblox-badges", Constants.URL("accountinformation"));
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

            List<string> history = new();
            string rawData = await GetStringAsync(url);
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic historyData in data.data)
            {
                history.Add(historyData.name.ToString());
            }

            return history.AsReadOnly();
        }

        private Group? primaryGroup;
        public async Task<Group?> GetPrimaryGroupAsync()
        {
            if (primaryGroup == null)
            {
                string rawData = await GetStringAsync($"/v1/users/{Id}/groups/primary/role", Constants.URL("groups"));
                if (rawData == "null")
                    return null;

                dynamic data = JObject.Parse(rawData);
                ulong groupId = Convert.ToUInt64(data.group.id);
                primaryGroup = await Group.FromId(groupId, session);
            }
            return primaryGroup;
        }

        private ReadOnlyDictionary<Group, Role>? groups;
        public async Task<ReadOnlyDictionary<Group, Role>> GetGroupsAsync(int limit = -1)
        {
            if (groups == null)
            {
                string rawData = await GetStringAsync($"/v1/users/{Id}/groups/roles", Constants.URL("groups"));
                dynamic data = JObject.Parse(rawData);

                Dictionary<Group, Role> dict = new();
                int count = 0;
                foreach (dynamic groupData in data.data)
                {
                    if (limit > 0 && count >= limit)
                        break;

                    ulong groupId = Convert.ToUInt64(groupData.group.id);
                    Group group = await Group.FromId(groupId, session);
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
                Dictionary<string, string> dict = new();
                string rawData = await GetStringAsync($"/v1/users/{Id}/promotion-channels?alwaysReturnUrls=true", Constants.URL("accountinformation"));
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

        private ReadOnlyCollection<Asset>? currentlyWearing;

        /// <summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> of <see cref="Asset"/> this user is currently wearing.
        /// </summary>
        /// <returns><see cref="ReadOnlyCollection{T}"/></returns>
        /// <remarks>The <see cref="Session"/> attached to this <see cref="User"/> will automatically be added to the returned <see cref="Asset"/> instances. This method will throw an exception if this User instance has no <see cref="Session"/> attached, as <see cref="Asset"/> instances must have a session attached.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<ReadOnlyCollection<Asset>> GetCurrentlyWearingAsync()
        {
            if (currentlyWearing == null)
            {
                string rawData = await GetStringAsync($"/v1/users/{Id}/currently-wearing", Constants.URL("avatar"));
                dynamic data = JObject.Parse(rawData);

                List<Asset> list = new List<Asset>();
                foreach (dynamic item in data.assetIds)
                {
                    ulong assetId = Convert.ToUInt64(item);
                    list.Add(await Asset.FromId(assetId, session));
                }
                currentlyWearing = list.AsReadOnly();
            }
            return currentlyWearing;
        }

        private ReadOnlyCollection<Asset>? collections;

        /// <summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> of <see cref="Asset"/> that are in this user's collection.
        /// </summary>
        /// <returns><see cref="ReadOnlyCollection{T}"/></returns>
        /// <remarks>The <see cref="Session"/> attached to this <see cref="User"/> will automatically be added to the returned <see cref="Asset"/> instances. This method will throw an exception if this User instance has no <see cref="Session"/> attached, as <see cref="Asset"/> instances must have a session attached.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<ReadOnlyCollection<Asset>> GetCollectionItemsAsync()
        {
            if (collections == null)
            { // TODO: "infinity value error"
                string rawData = await GetStringAsync($"/users/profile/robloxcollections-json?userId={Id}", Constants.ROBLOX_URL);
                dynamic data = JObject.Parse(rawData);

                List<Asset> list = new List<Asset>();
                foreach (dynamic item in data.CollectionsItems)
                {
                    ulong assetId = Convert.ToUInt64(item.Id);
                    list.Add(await Asset.FromId(assetId, session));
                }
                collections = list.AsReadOnly();
            }
            return collections;
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> of <see cref="User"/> instances that are friends with this user.
        /// </summary>
        /// <param name="limit">The maximum amount of friends to return.</param>
        /// <returns><see cref="ReadOnlyCollection{T}"/></returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<ReadOnlyCollection<GenericId<User>>> GetFriendsAsync(int limit = 50)
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/friends", Constants.URL("friends"));
            dynamic data = JObject.Parse(rawData);
            List<GenericId<User>> friends = new List<GenericId<User>>();
            int count = 0;
            foreach (dynamic friendData in data.data)
            {
                count++;

                ulong friendId = Convert.ToUInt64(friendData.id);
                friends.Add(new GenericId<User>(friendId, session));

                if (count >= limit)
                    break;
            }

            return friends.AsReadOnly();
        }

        /// <summary>
        /// Gets whether or not this user is in the given <paramref name="group"/>.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>A task containing a bool.</returns>
        public async Task<bool> IsInGroupAsync(Group group) => await (await group.GetMemberManagerAsync()).IsInGroupAsync(Id);

        /// <summary>
        /// Gets whether or not this user is in the group with the given Id.
        /// </summary>
        /// <param name="groupId">The groupId.</param>
        /// <returns>A task containing a bool.</returns>
        public async Task<bool> IsInGroupAsync(ulong groupId) => await (await (await Group.FromId(groupId)).GetMemberManagerAsync()).IsInGroupAsync(Id);

        // Thumbnails
        /// <summary>
        /// Returns a thumbnail of the given user.
        /// </summary>
        /// <param name="type">The type of thumbnail.</param>
        /// <param name="size">The size of the thumbnail.</param>
        /// <returns>A task containing a string URL to the thumbnail upon completion.</returns>
        /// <exception cref="ArgumentException">Invalid user to get thumbnail for.</exception>
        public async Task<string> GetThumbnailAsync(ThumbnailType type = ThumbnailType.Full, ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = "/v1/users/avatar" + type switch
            {
                ThumbnailType.Bust => "-bust",
                ThumbnailType.Headshot => "-headshot",
                _ => string.Empty,
            } + $"?userIds={Id}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid user to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        /// <summary>
        /// Gets whether or not this user owns the asset with the given Id.
        /// </summary>
        /// <param name="assetId">The asset Id.</param>
        /// <param name="assetItemType">The assetItemType. For most assets this value should be <c>0</c>.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        public async Task<bool> OwnsAssetAsync(ulong assetId, int assetItemType = 0)
        {
            string result = await GetStringAsync($"/v1/users/{Id}/items/{assetItemType}/{assetId}/is-owned", Constants.URL("inventory"));
            return Convert.ToBoolean(result);
        }

        /// <summary>
        /// Gets whether or not this user owns the given <paramref name="asset"/>.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        public async Task<bool> OwnsAssetAsync(Asset asset)
            => await OwnsAssetAsync(asset.Id);

        /// <summary>
        /// Gets whether or not this user owns the badge with the given Id.
        /// </summary>
        /// <param name="badgeId">The badge Id.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        public async Task<bool> HasBadgeAsync(ulong badgeId)
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/badges/awarded-dates?badgeIds={badgeId}", Constants.URL("badges"));
            dynamic data = JObject.Parse(rawData);

            return data.data.Count > 0;
        }

        /// <summary>
        /// Gets whether or not this user owns the given <paramref name="badge"/>.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        public async Task<bool> HasBadgeAsync(Badge badge)
            => await HasBadgeAsync(badge.Id);

        /// <summary>
        /// Gets this user's player badges.
        /// </summary>
        /// <param name="limit">The amount to get at one time.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<GenericId<Badge>>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/badges?sortOrder={sortOrder}&limit={limit.Limit()}";
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
        /// Returns items that the user owns. This method WILL throw an exception if the authenticated user cannot see this user's inventory.
        /// </summary>
        /// <param name="assetType">The <see cref="AssetType"/> to use for this request.</param>
        /// <param name="limit">The limit of assets to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <remarks>See <see cref="GetBadgesAsync(FixedLimit, RequestSortOrder, string?)"/> for badges.</remarks>
        /// <seealso cref="PrivateInventory"/>
        /// <seealso cref="GetBadgesAsync(FixedLimit, RequestSortOrder, string?)"/>
        public async Task<PageResponse<GenericId<Asset>>> GetInventoryAsync(AssetType assetType, FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/users/{Id}/inventory/{(int)assetType}?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, Constants.URL("inventory"));
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Asset>> list = new();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.assetId);
                list.Add(new(id, session));
            }

            return new PageResponse<GenericId<Asset>>(list, nextPage, previousPage);
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

            HttpResponseMessage response = await PostAsync("/v1/presence/users", body, Constants.URL("presence"));
            dynamic uselessData = JObject.Parse(await response.Content.ReadAsStringAsync());
            dynamic data = uselessData.userPresences[0];

            UserLocationType type = (UserLocationType)Convert.ToInt32(data.userPresenceType);
            Experience? exp = data.universeId != null ? await Experience.FromId(Convert.ToUInt64(data.universeId)) : null;
            DateTime lastOnline = data.lastOnline;
            return new UserPresence(type, exp, lastOnline);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DisplayName} (@{Name}) [{Id}]{(Verified ? " [V]" : string.Empty)}";
        }

        public User AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    public class UserPresence
    {
        public UserLocationType Location { get; }
        public Experience? PresenceLocation { get; }
        public DateTime LastOnline { get; }
        public bool IsOnline => Location is not UserLocationType.Offline;

        internal UserPresence(UserLocationType type, Experience? location, DateTime lastOnline)
        {
            Location = type;
            PresenceLocation = location;
            LastOnline = lastOnline;
        }
    }
}
