using Newtonsoft.Json.Linq;
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

        public Role? GetRoleInGroup(ulong userId)
        {
            string rawData = group.GetString($"/v1/users/{userId}/groups/roles?includeLocked=true");
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

        internal async Task SetRankAsyncInternal(ulong userId, ulong newRoleId)
        {
            object body = new { roleId = newRoleId };
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/users/{userId}", body);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                throw new InvalidOperationException($"User modify failed (HTTP {response.StatusCode}). Do you have permission change this user's role?");
            }
        }

        public async Task SetRankAsync(ulong userId, Role role)
        {
            if (role == null)
            {
                throw new InvalidOperationException("Invalid role provided.");
            }
            await SetRankAsyncInternal(userId, role.Id);
        }

        public async Task SetRankAsync(ulong userId, int rankId)
            => await SetRankAsync(userId, group.RoleManager.Roles.FirstOrDefault(r => r.Rank == rankId));

        public async Task SetRankAsync(ulong userId, string roleName)
            => await SetRankAsync(userId, group.RoleManager.Roles.FirstOrDefault(r => r.Name == roleName));

        public async Task SetRankAsync(User user, Role role)
            => await SetRankAsync(user.Id, role);

        public async Task SetRankAsync(User user, int rankId)
            => await SetRankAsync(user.Id, rankId);

        public async Task SetRankAsync(User user, string roleName)
            => await SetRankAsync(user.Id, roleName);

        public async Task SetRankAsync(string username, Role role)
            => await SetRankAsync(UserUtility.GetUserId(username), role);

        public async Task SetRankAsync(string username, int rankId)
            => await SetRankAsync(UserUtility.GetUserId(username), rankId);

        public async Task SetRankAsync(string username, string roleName)
            => await SetRankAsync(UserUtility.GetUserId(username), roleName);

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
        public async Task KickMemberAsync(User user) => await KickMemberAsync(user.Id);
    }
}
