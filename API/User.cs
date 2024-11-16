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
    public class User : APIMain, IRefreshable, IAssetOwner, IPoolable, IIdApi<User>
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
                return RoPool<User>.Get(userId, session);

            User newUser = new(userId, session);
            await newUser.RefreshAsync();

            return newUser;
        }

        public static async Task<User> FromGenericId(GenericId<User> id, Session? session) => await id.GetInstanceAsync(session);

        public static async Task<User> FromUsername(string username, Session? session = null)
            => await FromId(await UserUtility.GetUserIdAsync(username), session);

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v1/users/{Id}");
            if (response.IsSuccessStatusCode)
            {
                string raw = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(raw);

                name = data.name;
                displayName = data.displayName;
                verified = data.hasVerifiedBadge;
                joinDate = data.created;
                profileHidden = data.isBanned;
                bio = data.description;
            }
            else
            {
                throw new ArgumentException($"Invalid user ID '{Id}'. HTTP {response.StatusCode}");
            }

            // TODO: These properties should be reset through this method.
            following = -1;
            followers = -1;
            robloxBadges = null;
            renameHistory = null;
            primaryGroup = null;
            socialChannels = null;

            currentlyWearing = null;
            collections = null;
            groups = null;

            // Update premium
            if (SessionVerify.Verify(session))
            {
                HttpResponseMessage premiumResponse = await GetAsync($"/v1/users/{Id}/validate-membership", Constants.URL("premiumfeatures"));
                if (premiumResponse.IsSuccessStatusCode)
                {
                    isPremium = Convert.ToBoolean(await premiumResponse.Content.ReadAsStringAsync());
                }
            }
            else
            {
                isPremium = null;
            }

            RefreshedAt = DateTime.Now;
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
        public int Following
        {
            get
            {
                if (following == -1)
                {
                    HttpResponseMessage response = Get($"/v1/users/{Id}/followings/count", Constants.URL("friends"));
                    if (response.IsSuccessStatusCode)
                    {
                        string raw = response.Content.ReadAsStringAsync().Result;
                        dynamic data = JObject.Parse(raw);
                        following = data.count;
                    }
                    else
                    {
                        following = 0;
                    }
                }
                return following;
            }
        }

        private int followers = -1;
        public int Followers
        {
            get
            {
                if (followers == -1)
                {
                    HttpResponseMessage response = Get($"/v1/users/{Id}/followers/count", Constants.URL("friends"));
                    if (response.IsSuccessStatusCode)
                    {
                        string raw = response.Content.ReadAsStringAsync().Result;
                        dynamic data = JObject.Parse(raw);
                        followers = data.count;
                    }
                    else
                    {
                        followers = 0;
                    }
                }
                return followers;
            }
        }

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

        private ReadOnlyCollection<string>? renameHistory;
        public async Task<ReadOnlyCollection<string>> GetRenameHistoryAsync()
        {
            if (renameHistory == null)
            {
                List<string> history = new();
                string rawData = await GetStringAsync($"/v1/users/{Id}/username-history?limit=100&sortOrder=Desc");
                dynamic data = JObject.Parse(rawData);
                foreach (dynamic historyData in data.data)
                {
                    history.Add(historyData.name.ToString());
                }
                renameHistory = history.AsReadOnly();
            }
            return renameHistory;
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
                primaryGroup = Group.FromId(groupId, session).Result;
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
                    dict.Add(group, group.RoleManager.GetRole(Convert.ToInt32(groupData.role.rank)));
                    count++;
                }

                groups = dict.AsReadOnly();
            }

            return groups;
        }

        public bool PrivateInventory
        {
            get
            {
                HttpResponseMessage response = Get($"/v1/users/{Id}/can-view-inventory", Constants.URL("inventory"));
                if (response.IsSuccessStatusCode)
                {
                    dynamic data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    return !Convert.ToBoolean(data.canView);
                }
                return false;
            }
        }

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

        public async Task<bool> IsInGroupAsync(Group group) => await group.MemberManager.IsInGroupAsync(Id);
        public async Task<bool> IsInGroupAsync(ulong groupId) => await (await Group.FromId(groupId)).MemberManager.IsInGroupAsync(Id);

        // Thumbnails
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

        public async Task<bool> OwnsAssetAsync(ulong assetId, int assetItemType = 0)
        {
            string result = await GetStringAsync($"/v1/users/{Id}/items/{assetItemType}/{assetId}/is-owned", Constants.URL("inventory"));
            return Convert.ToBoolean(result);
        }

        public async Task<bool> OwnsAssetAsync(Asset asset)
            => await OwnsAssetAsync(asset.Id);

        public async Task<bool> HasBadgeAsync(ulong badgeId)
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/badges/awarded-dates?badgeIds={badgeId}", Constants.URL("badges"));
            dynamic data = JObject.Parse(rawData);

            return data.data.Count > 0;
        }

        public async Task<bool> HasBadgeAsync(Badge badge)
            => await HasBadgeAsync(badge.Id);

        public async Task<PageResponse<GenericId<Badge>>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/badges?sortOrder=Desc&limit={limit.Limit()}";
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
        /// Gets this user's current presence status, including their location and last online.
        /// </summary>
        /// <returns>A task containing a <see cref="UserPresence"/>, when completed.</returns>
        /// <remarks>Unlike other API, this method will never cache and will always make an API request when called. Authentication is not required for this API, however providing an authenticated session may return more data.</remarks>
        /// <exception cref="RobloxAPIException">Failed to retrieve presence information, please try again later.</exception>
        public async Task<UserPresence> GetPresenceAsync()
        {
            object body = new
            {
                userIds = new[] { Id },
            };

            HttpResponseMessage response = await PostAsync("/v1/presence/users", body, Constants.URL("presence"));
            if (response.IsSuccessStatusCode)
            {
                dynamic uselessData = JObject.Parse(await response.Content.ReadAsStringAsync());
                dynamic data = uselessData.userPresences[0];

                UserLocationType type = (UserLocationType)Convert.ToInt32(data.userPresenceType);
                Experience? exp = data.universeId != null ? await Experience.FromId(Convert.ToUInt64(data.universeId)) : null;
                DateTime lastOnline = data.lastOnline;
                return new UserPresence(type, exp, lastOnline);
            }

            throw new RobloxAPIException($"Failed to retrieve presence information, please try again later. HTTP {response.StatusCode}.");
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DisplayName} (@{Name}) [{Id}] {(Verified ? "[V]" : string.Empty)}";
        }

        public User AttachSessionAndReturn(Session? session)
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

    public class UserPresence
    {
        public UserLocationType PresenceType { get; }
        public Experience? PresenceLocation { get; }
        public DateTime LastOnline { get; }
        public bool IsOnline => PresenceType is not UserLocationType.Offline;

        internal UserPresence(UserLocationType type, Experience? location, DateTime lastOnline)
        {
            PresenceType = type;
            PresenceLocation = location;
            LastOnline = lastOnline;
        }
    }
}
