using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.API;
using RoSharp.API.Assets;
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
    public class User : APIMain, IRefreshable
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

        public User(ulong userId, Session? session = null)
        {
            Id = userId;

            Refresh();

            if (session != null)
                AttachSession(session);
        }

        public void Refresh()
        {
            HttpResponseMessage response = Get($"/v1/users/{Id}", verifySession: false);
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
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
                throw new InvalidOperationException("Invalid user ID");
            }

            // Reset properties
            following = -1;
            followers = -1;
            robloxBadges = null;
            renameHistory = null;
            primaryGroup = null;
            groups = null;
            socialChannels = null;
            currentlyWearing = null;
            collections = null;

            RefreshedAt = DateTime.Now;
        }

        public User(string username, Session? session = null) : this(UserUtility.GetUserId(username), session) { }

        [UsesSession]
        public bool IsPremium
        {
            get
            {
                HttpResponseMessage response = Get($"/v1/users/{Id}/validate-membership", "https://premiumfeatures.roblox.com");
                if (response.IsSuccessStatusCode)
                {
                    return Convert.ToBoolean(response.Content.ReadAsStringAsync().Result);
                }
                return false;
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
                    primaryGroup = new Group(Convert.ToUInt64(data.group.id));
                }
                return primaryGroup;
            }
        }

        private ReadOnlyDictionary<Group, Role>? groups;
        public async Task<ReadOnlyDictionary<Group, Role>> GetGroupsAsync(int limit = -1)
        {
            if (groups == null)
            {
                string rawData = await GetStringAsync("/v1/users/39979813/groups/roles", "https://groups.roblox.com");
                dynamic data = JObject.Parse(rawData);

                Dictionary<Group, Role> dict = new();
                int count = 0;
                foreach (dynamic groupData in data.data)
                {
                    if (limit > 0 && count >= limit)
                        break;

                    Group group = new(Convert.ToUInt64(groupData.group.id));
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
                string rawData = GetString("/v1/users/39979813/currently-wearing", "https://avatar.roblox.com");
                dynamic data = JObject.Parse(rawData);

                List<Asset> list = new List<Asset>();
                foreach (dynamic item in data.assetIds)
                {
                    list.Add(new Asset(Convert.ToUInt64(item), session));
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
                string rawData = GetString($"/users/profile/robloxcollections-json?userId={Id}", "https://www.roblox.com");
                dynamic data = JObject.Parse(rawData);

                List<Asset> list = new List<Asset>();
                foreach (dynamic item in data.CollectionsItems)
                {
                    list.Add(new Asset(Convert.ToUInt64(item.Id), session));
                }
                collections = list.AsReadOnly();
            }
            return collections;
        }

        public async Task<ReadOnlyCollection<User>> GetFriendsAsync()
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/friends", "https://friends.roblox.com");
            dynamic data = JObject.Parse(rawData);
            List<User> friends = new List<User>();
            foreach (dynamic friendData in data.data)
            {
                friends.Add(new User(friendData.id).AttachSessionAndReturn(session));
            }

            return friends.AsReadOnly();
        }

        public bool IsInGroup(Group group) => group.MemberManager.IsInGroup(Id);
        public bool IsInGroup(ulong groupId) => new Group(groupId).MemberManager.IsInGroup(Id);

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

        public bool OwnsAsset(ulong assetId, int assetItemType = 0)
        {
            string result = GetString($"v1/users/{Id}/items/{assetItemType}/{assetId}/is-owned", "https://inventory.roblox.com");
            return Convert.ToBoolean(result);
        }

        public bool OwnsAsset(Asset asset) => OwnsAsset(asset.Id, asset.AssetItemType);

        public User AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }

        public override string ToString()
        {
            return $"{DisplayName} (@{Name}) [{Id}] {(Verified ? "[V]" : string.Empty)}";
        }
    }
}
