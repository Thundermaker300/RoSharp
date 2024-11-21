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
            customName = null;

            // Update premium
            if (SessionVerify.Verify(session))
            {
                // Premium
                HttpResponseMessage premiumResponse = await GetAsync($"/v1/users/{Id}/validate-membership", Constants.URL("premiumfeatures"), "User.IsPremium [RefreshAsync]");
                isPremium = Convert.ToBoolean(await premiumResponse.Content.ReadAsStringAsync());

                // Private Inventory
                HttpResponseMessage inventoryResponse = await GetAsync($"/v1/users/{Id}/can-view-inventory", Constants.URL("inventory"), "User.PrivateInventory [RefreshAsync]");
                dynamic inventoryData = JObject.Parse(await inventoryResponse.Content.ReadAsStringAsync());
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

        private async Task UpdateFollowingsAsync()
        {
            dynamic followingsData = JObject.Parse(await GetStringAsync($"/v1/users/{Id}/followings/count", Constants.URL("friends")));
            dynamic followersData = JObject.Parse(await GetStringAsync($"/v1/users/{Id}/followers/count", Constants.URL("friends")));

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

            List<string> history = [];
            string rawData = await GetStringAsync(url);
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
        /// <returns>A task containing a <see cref="Community"/> on completion. Will be <see langword="null"/> if the user does not have a primary community.</returns>
        public async Task<Community?> GetPrimaryGroupAsync()
        {
            if (primaryGroup == null)
            {
                string rawData = await GetStringAsync($"/v1/users/{Id}/groups/primary/role", Constants.URL("groups"));
                if (rawData == "null")
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
                string rawData = await GetStringAsync($"/v1/users/{Id}/groups/roles", Constants.URL("groups"));
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

                List<Asset> list = [];
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

                List<Asset> list = [];
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
            List<GenericId<User>> friends = [];
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
        /// Gets the Ids of the users this user is following.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<GenericId<User>>> GetFollowingAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/followings?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<GenericId<User>>();
            HttpResponseMessage response = await GetAsync(url, Constants.URL("friends"));
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = item.id;
                list.Add(new(id, session));
            }

            return new PageResponse<GenericId<User>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets the Ids of the users this user is followed by.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<GenericId<User>>> GetFollowersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/followers?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<GenericId<User>>();
            HttpResponseMessage response = await GetAsync(url, Constants.URL("friends"));
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = item.id;
                list.Add(new(id, session));
            }

            return new PageResponse<GenericId<User>>(list, nextPage, previousPage);
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
                object body = new
                {
                    targetUserIds = new[] { Id }
                };
                HttpResponseMessage response = await PostAsync("/v1/user/get-tags", body, Constants.URL("contacts"));
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

            object body = new
            {
                targetUserId = Id,
                userTag = name,
            };

            HttpResponseMessage response = await PostAsync("/v1/user/tag", body, Constants.URL("contacts"), "User.SetCustomName");
        }

        /// <summary>
        /// Gets whether or not this user is in the given community.
        /// </summary>
        /// <param name="community">The community.</param>
        /// <returns>A task containing a bool.</returns>
        public async Task<bool> IsInGroupAsync(Community community) => await (await community.GetMemberManagerAsync()).IsInGroupAsync(Id);

        /// <summary>
        /// Gets whether or not this user is in the community with the given Id.
        /// </summary>
        /// <param name="communityId">The communityId.</param>
        /// <returns>A task containing a bool.</returns>
        public async Task<bool> IsInGroupAsync(ulong communityId) => await (await (await Community.FromId(communityId)).GetMemberManagerAsync()).IsInGroupAsync(Id);
        
        // Thumbnails
        /// <summary>
        /// Returns a thumbnail of the given user.
        /// </summary>
        /// <param name="type">The type of thumbnail.</param>
        /// <param name="size">The size of the thumbnail.</param>
        /// <returns>A task containing a string URL to the thumbnail upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
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
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
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
            string rawData = await GetStringAsync($"/v1/users/{Id}/badges/awarded-dates?badgeIds={badgeId}", Constants.URL("badges"));
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
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<GenericId<Badge>>> GetBadgesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/users/{Id}/badges?sortOrder={sortOrder}&limit={limit.Limit()}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, Constants.URL("badges"));
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Badge>> list = [];
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
        /// <remarks>This API method does not cache and will make a request each time it is called. This API also does not work for Badges, Bundles, and GamePasses -- see their respective APIs.</remarks>
        /// <seealso cref="PrivateInventory"/>
        /// <seealso cref="GetBadgesAsync(FixedLimit, RequestSortOrder, string?)"/>
        public async Task<PageResponse<GenericId<Asset>>> GetInventoryAsync(AssetType assetType, FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/users/{Id}/inventory/{(int)assetType}?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, Constants.URL("inventory"));
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Asset>> list = [];
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
        /// Returns items that the user owns. This method WILL throw an exception if the authenticated user cannot see this user's inventory.
        /// </summary>
        /// <param name="assetType">The <see cref="AssetType"/> to use for this request.</param>
        /// <param name="limit">The limit of assets to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called. This API also does not work for Badges, Bundles, and GamePasses -- see their respective APIs.</remarks>
        public async Task<PageResponse<GenericId<Asset>>> GetFavoritesAsync(AssetType assetType, FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/users/favorites/list-json?userId={Id}&assetTypeId={(int)assetType}&itemsPerPage={limit.Limit()}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, Constants.ROBLOX_URL_WWW);
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Asset>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.Data.Items)
            {
                ulong id = Convert.ToUInt64(item.Item.AssetId);
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
