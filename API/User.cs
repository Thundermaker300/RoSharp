using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.API;
using RoSharp.API.Assets;
using RoSharp.Enums;
using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class User : APIMain
    {
        public override string BaseUrl => "https://users.roblox.com";

        public ulong Id { get; }
        public string Name { get; }
        public string Username => Name;
        public string DisplayName { get; }
        public string Bio { get; }
        public bool Verified { get; }
        public DateTime JoinDate { get; }
        public bool ProfileHidden { get; }

        public User(ulong userId)
        {
            HttpResponseMessage response = Get($"/v1/users/{userId}", verifySession: false);
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
                dynamic data = JObject.Parse(raw);

                Id = data.id;
                Name = data.name;
                DisplayName = data.displayName;
                Verified = data.hasVerifiedBadge;
                JoinDate = data.created;
                ProfileHidden = data.isBanned;
                Bio = data.description;
            }
            else
            {
                throw new InvalidOperationException("Invalid user ID");
            }
        }

        public User(string username) : this(UserUtility.GetUserId(username)) { }

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

        private bool privateInventory;
        public bool PrivateInventory
        {
            get
            {
                HttpResponseMessage response = Get($"/v1/users/{Id}/can-view-inventory", "https://inventory.roblox.com", false);
                if (response.IsSuccessStatusCode)
                {
                    dynamic data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    privateInventory = !Convert.ToBoolean(data.canView);
                }
                return privateInventory;
            }
        }

        public async Task<ReadOnlyCollection<User>> GetFriendsAsync()
        {
            string rawData = await GetStringAsync($"/v1/users/{Id}/friends");
            dynamic data = JObject.Parse(rawData);
            List<User> friends = new List<User>();
            foreach (dynamic friendData in data.data)
            {
                friends.Add(new User(friendData.id).AttachSessionAndReturn(session));
            }

            return friends.AsReadOnly();
        }

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
