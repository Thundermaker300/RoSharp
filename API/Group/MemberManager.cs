using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class MemberManager
    {
        private Group group;

        internal MemberManager(Group group) { this.group = group; }

        public ulong Members => group.members;

        public async Task<ReadOnlyCollection<User>> GetPendingRequestsAsync()
        {
            HttpResponseMessage response = await group.GetAsync($"/v1/groups/{group.Id}/join-requests?limit=100&sortOrder=Desc");
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cannot view pending requests for group (HTTP {response.StatusCode}). Do you have permission to see pending requests?");
            }
            string rawData = response.Content.ReadAsStringAsync().Result;
            dynamic data = JObject.Parse(rawData);

            var list = new List<User>();
            foreach (dynamic user in data.data)
            {
                list.Add(new User(Convert.ToUInt64(user.requester.userId)));
            }
            return list.AsReadOnly();
        }

        public Role? GetRoleInGroup(ulong userId)
        {
            string rawData = group.GetString($"/v1/users/{userId}/groups/roles?includeLocked=true", verifySession: false);
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

        public Role? GetRoleInGroup(string username) => GetRoleInGroup(new User(username));
        public Role? GetRoleInGroup(User user) => GetRoleInGroup(user.Id);

        [UsesSession]
        public async Task ModifyJoinRequestAsync(ulong userId, JoinRequestAction action)
        {
            string url = $"/v1/groups/{group.Id}/join-requests/users/{userId}";
            HttpResponseMessage response = action switch
            {
                JoinRequestAction.Accept => await group.PostAsync(url, null),
                JoinRequestAction.Decline => await group.DeleteAsync(url),
                _ => throw new NotImplementedException("JoinRequestAction must be Accept or Decline."),
            };
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Join request modify failed (HTTP {response.StatusCode}). Do you have permission to accept/decline join requests?");
            }
        }

        [UsesSession]
        public async Task ModifyJoinRequestAsync(User user, JoinRequestAction action)
            => await ModifyJoinRequestAsync(user.Id, action);

        [UsesSession]
        public async Task ModifyJoinRequestAsync(string username, JoinRequestAction action)
            => await ModifyJoinRequestAsync(UserUtility.GetUserId(username), action);

        internal async Task SetRankAsyncInternal(ulong userId, ulong newRoleId)
        {
            object body = new { roleId = newRoleId };
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/users/{userId}", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"User modify failed (HTTP {response.StatusCode}). Do you have permission change this user's role?");
            }
        }

        [UsesSession]
        public async Task SetRankAsync(ulong userId, Role role)
        {
            if (role == null)
            {
                throw new InvalidOperationException("Invalid role provided.");
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
            => await SetRankAsync(UserUtility.GetUserId(username), role);

        [UsesSession]
        public async Task SetRankAsync(string username, int rankId)
            => await SetRankAsync(UserUtility.GetUserId(username), rankId);

        [UsesSession]
        public async Task SetRankAsync(string username, string roleName)
            => await SetRankAsync(UserUtility.GetUserId(username), roleName);

        [UsesSession]
        public async Task KickMemberAsync(ulong userId)
        {
            // TODO look into
            HttpResponseMessage response = await group.DeleteAsync($"/v1/groups/{group.Id}/users/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                throw new InvalidOperationException($"User kick failed (HTTP {response.StatusCode}). Do you have permission to kick members?");
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
