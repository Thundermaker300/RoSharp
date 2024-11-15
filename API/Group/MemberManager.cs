using Newtonsoft.Json.Linq;
using RoSharp.API.Misc;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Utility;
using System.Diagnostics;

namespace RoSharp.API
{
    public class MemberManager
    {
        private Group group;

        internal MemberManager(Group group) { this.group = group; }

        public ulong Members => group.members;

        public async Task<PageResponse<User>> GetPendingRequestsAsync(FixedLimit limit = FixedLimit.Limit100, string? cursor = null)
        {
            string url = $"/v1/groups/{group.Id}/join-requests?limit={limit.Limit()}&sortOrder=Desc";
            if (cursor != null)
                url += "&cursor=" + cursor;

            HttpResponseMessage response = await group.GetAsync(url, verifyApiName: "Group.GetPendingRequestsAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"Cannot view pending requests for group (HTTP {response.StatusCode}). Do you have permission to see pending requests?");
            }
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            var list = new List<User>();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.requester.userId);
                list.Add(await User.FromId(userId, group.session));
            }

            return new(list, nextPage, previousPage);
        }

        public async Task<bool> IsInGroupAsync(ulong userId)
        {
            string rawData = await group.GetStringAsync($"/v1/users/{userId}/groups/roles?includeLocked=true");
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic group in data.data)
            {
                if (Convert.ToUInt64(group.group.id) == this.group.Id)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> IsInGroupAsync(string username) => await IsInGroupAsync(await User.FromUsername(username));
        public async Task<bool> IsInGroupAsync(User user) => await IsInGroupAsync(user.Id);

        public async Task<Role?> GetRoleInGroupAsync(ulong userId)
        {
            string rawData = await group.GetStringAsync($"/v1/users/{userId}/groups/roles?includeLocked=true");
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic group in data.data)
            {
                if (Convert.ToUInt64(group.group.id) == this.group.Id)
                {
                    return this.group.RoleManager.Roles.FirstOrDefault(r => r.Id == Convert.ToUInt64(group.role.id));
                }
            }
            return null;
        }

        public async Task<Role?> GetRoleInGroupAsync(string username) => await GetRoleInGroupAsync(await User.FromUsername(username));
        public async Task<Role?> GetRoleInGroupAsync(User user) => await GetRoleInGroupAsync(user.Id);

        [UsesSession]
        public async Task ModifyJoinRequestAsync(ulong userId, JoinRequestAction action)
        {
            string url = $"/v1/groups/{group.Id}/join-requests/users/{userId}";
            HttpResponseMessage response = action switch
            {
                JoinRequestAction.Accept => await group.PostAsync(url, new { }, verifyApiName: "MemberManager.ModifyJoinRequestAsync"),
                JoinRequestAction.Decline => await group.DeleteAsync(url, "MemberManager.ModifyJoinRequestAsync"),
                _ => throw new UnreachableException("JoinRequestAction must be Accept or Decline."),
            };
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"Join request modify failed (HTTP {response.StatusCode}). Do you have permission to accept/decline join requests?");
            }
        }

        [UsesSession]
        public async Task ModifyJoinRequestAsync(User user, JoinRequestAction action)
            => await ModifyJoinRequestAsync(user.Id, action);

        [UsesSession]
        public async Task ModifyJoinRequestAsync(string username, JoinRequestAction action)
            => await ModifyJoinRequestAsync(await UserUtility.GetUserIdAsync(username), action);

        internal async Task SetRankAsyncInternal(ulong userId, ulong newRoleId)
        {
            object body = new { roleId = newRoleId };
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/users/{userId}", body, verifyApiName: "MemberManager.SetRankAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"User modify failed (HTTP {response.StatusCode}). Do you have permission change this user's role?");
            }
        }

        [UsesSession]
        public async Task SetRankAsync(ulong userId, Role role)
        {
            if (role == null)
            {
                throw new ArgumentException("Invalid role provided.");
            }
            await SetRankAsyncInternal(userId, role.Id);
        }

        [UsesSession]
        public async Task SetRankAsync(ulong userId, int rankId)
            => await SetRankAsync(userId, group.RoleManager.Roles.FirstOrDefault(r => r.Rank == rankId));

        [UsesSession]
        public async Task SetRankAsync(ulong userId, string roleName)
            => await SetRankAsync(userId, group.RoleManager.Roles.FirstOrDefault(r => r.Name == roleName));

        [UsesSession]
        public async Task SetRankAsync(User user, Role role)
            => await SetRankAsync(user.Id, role);

        [UsesSession]
        public async Task SetRankAsync(User user, int rankId)
            => await SetRankAsync(user.Id, rankId);

        [UsesSession]
        public async Task SetRankAsync(User user, string roleName)
            => await SetRankAsync(user.Id, roleName);

        [UsesSession]
        public async Task SetRankAsync(string username, Role role)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), role);

        [UsesSession]
        public async Task SetRankAsync(string username, int rankId)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), rankId);

        [UsesSession]
        public async Task SetRankAsync(string username, string roleName)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), roleName);

        [UsesSession]
        public async Task KickMemberAsync(ulong userId)
        {
            HttpResponseMessage response = await group.DeleteAsync($"/v1/groups/{group.Id}/users/{userId}", verifyApiName: "MemberManager.KickMemberAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"User kick failed (HTTP {response.StatusCode}). Do you have permission to kick members?");
            }
        }

        [UsesSession]
        public async Task KickMemberAsync(User user) => await KickMemberAsync(user.Id);

        public override string ToString()
        {
            return $"MemberManager [#{Members}]";
        }
    }
}
