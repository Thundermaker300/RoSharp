using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.API;
using RoSharp.API.Assets;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Interfaces;
using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class User : APIMain, IRefreshable, IAssetOwner, IPoolable
    {
        public override string BaseUrl => "https://users.roblox.com";

        public ulong Id { get; }

        private string name;
        public string Name => name;
        public string Username => Name;

        private string displayName;
        public string DisplayName => displayName;

        private string bio;
        public string Bio => bio;

        private bool verified;
        public bool Verified => verified;

        private DateTime joinDate;
        public DateTime JoinDate => joinDate;
        public TimeSpan AccountAge => DateTime.UtcNow - JoinDate;

        private bool profileHidden;
        public bool ProfileHidden => ProfileHidden;
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

        public static async Task<User> FromUsername(string username, Session? session = null)
            => await FromId(await UserUtility.GetUserIdAsync(username), session);

        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v1/users/{Id}", verifySession: false);
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
                throw new InvalidOperationException($"Invalid user ID '{Id}'. HTTP {response.StatusCode}");
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
                HttpResponseMessage premiumResponse = await GetAsync($"/v1/users/{Id}/validate-membership", "https://premiumfeatures.roblox.com");
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
                    HttpResponseMessage response = Get($"/v1/users/{Id}/followings/count", "https://friends.roblox.com", false);
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
                    HttpResponseMessage response = Get($"/v1/users/{Id}/followers/count", "https://friends.roblox.com", false);
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
        public ReadOnlyCollection<string> RobloxBadges
        {
            get
            {
                if (robloxBadges == null)
                {
                    List<string> badges = new();
                    string rawData = GetString($"/v1/users/{Id}/roblox-badges", "https://accountinformation.roblox.com", false);
                    JArray data = JArray.Parse(rawData);
                    foreach (dynamic badgeData in data.Children<JObject>())
                    {
                        badges.Add(badgeData.name.ToString());
                    }
                    robloxBadges = badges.AsReadOnly();
                }

                return robloxBadges;
            }
        }

        private ReadOnlyCollection<string>? renameHistory;
        public ReadOnlyCollection<string> RenameHistory
        {
            get
            {
                if (renameHistory == null)
                {
                    List<string> history = new();
                    string rawData = GetString($"/v1/users/{Id}/username-history?limit=100&sortOrder=Desc", verifySession: false);
                    dynamic data = JObject.Parse(rawData);
                    foreach (dynamic historyData in data.data)
                    {
                        history.Add(historyData.name.ToString());
                    }
                    renameHistory = history.AsReadOnly();
                }
                return renameHistory;
            }
        }

        private Group? primaryGroup;
        public Group? PrimaryGroup
        {
            get
            {
                if (primaryGroup == null)
                {
                    string rawData = GetString($"/v1/users/{Id}/groups/primary/role", "https://groups.roblox.com", verifySession: false);
                    if (rawData == "null")
                        return null;

                    dynamic data = JObject.Parse(rawData);
                    ulong groupId = Convert.ToUInt64(data.group.id);
                    primaryGroup = Group.FromId(groupId, session).Result;
                }
                return primaryGroup;
            }
        }

        private ReadOnlyDictionary<Group, Role>? groups;
        public async Task<ReadOnlyDictionary<Group, Role>> GetGroupsAsync(int limit = -1)
        {
            if (groups == null)
            {
                string rawData = await GetStringAsync($"/v1/users/{Id}/groups/roles", "https://groups.roblox.com");
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
                HttpResponseMessage response = Get($"/v1/users/{Id}/can-view-inventory", "https://inventory.roblox.com", false);
                if (response.IsSuccessStatusCode)
                {
                    dynamic data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    return !Convert.ToBoolean(data.canView);
                }
                return false;
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
                    string rawData = GetString($"/v1/users/{Id}/promotion-channels?alwaysReturnUrls=true", "https://accountinformation.roblox.com/");
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
        }

        private ReadOnlyCollection<Asset>? currentlyWearing;
        public async Task<ReadOnlyCollection<Asset>> GetCurrentlyWearingAsync()
        {
            if (currentlyWearing == null)
            {
                string rawData = await GetStringAsync($"/v1/users/{Id}/currently-wearing", "https://avatar.roblox.com");
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
        public async Task<ReadOnlyCollection<Asset>> GetCollectionItemsAsync()
        {
            if (collections == null)
            {
                string rawData = await GetStringAsync($"/users/profile/robloxcollections-json?userId={Id}", "https://www.roblox.com");
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

        public async Task<ReadOnlyCollection<User>> GetFriendsAsync(int limit = 50)
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/friends", "https://friends.roblox.com");
            dynamic data = JObject.Parse(rawData);
            List<User> friends = new List<User>();
            int count = 0;
            foreach (dynamic friendData in data.data)
            {
                count++;

                ulong friendId = Convert.ToUInt64(friendData.id);
                friends.Add(await FromId(friendId, session));

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
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com", verifySession: false);
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new InvalidOperationException("Invalid user to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        public async Task<bool> OwnsAssetAsync(ulong assetId, int assetItemType = 0)
        {
            string result = await GetStringAsync($"/v1/users/{Id}/items/{assetItemType}/{assetId}/is-owned", "https://inventory.roblox.com");
            return Convert.ToBoolean(result);
        }

        public async Task<bool> OwnsAssetAsync(Asset asset)
            => await OwnsAssetAsync(asset.Id);

        public async Task<bool> HasBadgeAsync(ulong badgeId)
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/badges/awarded-dates?badgeIds={badgeId}", "https://badges.roblox.com");
            dynamic data = JObject.Parse(rawData);

            return data.data.Count > 0;
        }

        public async Task<bool> HasBadgeAsync(Badge badge)
            => await HasBadgeAsync(badge.Id);

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
}
