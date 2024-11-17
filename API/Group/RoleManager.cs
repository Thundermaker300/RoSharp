using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class RoleManager : IRefreshable
    {
        internal Group group;

        public DateTime RefreshedAt { get; set; }

        private ReadOnlyCollection<Role>? roles;
        public ReadOnlyCollection<Role> Roles => roles;

        internal bool areConfigurationsAccessible = true;

        public async Task RefreshAsync()
        {
            List<Role> list = [];
            string rawData = await group.GetStringAsync($"/v1/groups/{group.Id}/roles");
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic rank in data.roles)
            {
                Role role = await Role.MakeNew(this,
                    Convert.ToUInt64(rank.id),
                    Convert.ToString(rank.name),
                    Convert.ToString(rank.description),
                    Convert.ToByte(rank.rank),
                    Convert.ToUInt64(rank.memberCount)
                );
                list.Add(role);
            }

            roles = list.AsReadOnly();
            RefreshedAt = DateTime.Now;
        }

        internal RoleManager(Group group) { this.group = group; }

        public Role? GetRole(int rankId)
            => Roles.FirstOrDefault(role => role.Rank == rankId);

        public Role? GetRole(string roleName)
            => Roles.FirstOrDefault(role => role.Name == roleName);

        /// <summary>
        /// Purchases and creates a new role in this group.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The private description of the role.</param>
        /// <param name="rank">The rank of the role.</param>
        /// <param name="purchaseWithGroupFunds">Purchase with group funds if <see langword="true"/>. Purchase with the authenticated user's funds if <see langword="false"/>.</param>
        /// <returns></returns>
        /// <exception cref="RobloxAPIException">Roblox API error or lack of permissions.</exception>
        /// <remarks>As of November 16th, 2024, roles cost R$25 to make. This method will bypass a confirmation prompt and purchase the role immediately. Use caution in order to not accidentally create roles and burn through money!</remarks>
        public async Task<Role> CreateRoleAsync(string name, string description, byte rank, bool purchaseWithGroupFunds)
        {
            object body = new
            {
                name = name,
                description = description,
                rank = rank,
                usingGroupFunds = purchaseWithGroupFunds,
            };

            HttpResponseMessage response = await group.PostAsync($"/v1/groups/{group.Id}/rolesets/create", body, verifyApiName: "RoleManager.CreateRoleAsync");
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            await RefreshAsync();
            return GetRole(Convert.ToByte(data.rank));
        }

        internal async Task RequestDeleteRole(ulong roleId)
        {
            HttpResponseMessage response = await group.DeleteAsync($"/v1/groups/{group.Id}/rolesets/{roleId}", verifyApiName: "Role.DeleteAsync");
            if (!response.IsSuccessStatusCode)
            {
                string errorText = await response.Content.ReadAsStringAsync();
                if (errorText.Contains("There are users in this role."))
                    throw new InvalidOperationException($"Cannot delete role '{roleId}' because there are users still in this role. Please remove all users from this role and try again.");
                throw new RobloxAPIException($"Roleset delete failed (HTTP {response.StatusCode}). Do you have permission to delete this group's rolesets?");
            }
        }

        internal async Task RequestUpdateRole(Role roleId, string newName)
        {
            object body = new { name = newName, rank = roleId.Rank };
            JsonContent content = JsonContent.Create(body);
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/rolesets/{roleId.Id}", body, verifyApiName: "Role.UpdateAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"Roleset modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's rolesets?");
            }
        }

        internal async Task RequestUpdateRole(Role roleId, int newRank)
        {
            object body = new { name = roleId.Name, rank = newRank };
            JsonContent content = JsonContent.Create(body);
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/rolesets/{roleId.Id}", body, verifyApiName: "Role.UpdateAsync");
            if (!response.IsSuccessStatusCode)
            {
                throw new RobloxAPIException($"Roleset modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's rolesets?");
            }
        }

        public override string ToString()
        {
            return $"RoleManager [#{Roles.Count}]";
        }
    }

    public class Role
    {
        internal RoleManager roleManager;


        private ulong id;
        public ulong Id => id;


        private string name;
        public string Name => name;


        private string? description;
        public string? Description => description;


        private byte rank;
        public byte Rank => rank;


        private ulong memberCount;
        public ulong MemberCount => memberCount;

        private bool canAccessPermissions;
        public bool CanAccessPermissions => canAccessPermissions;

        private ReadOnlyCollection<GroupPermission> permissions;
        public ReadOnlyCollection<GroupPermission> Permissions => permissions;

        private Role() { }

        internal async static Task<Role> MakeNew(RoleManager manager, ulong id, string name, string description, byte rank, ulong memberCount)
        {
            Role r = new();
            r.roleManager = manager;

            r.id = id;
            r.name = name;
            r.description = description;
            r.rank = rank;
            r.memberCount = memberCount;
            r.canAccessPermissions = false;

            List<GroupPermission> perms = new();

            if (manager.areConfigurationsAccessible)
            {
                try
                {
                    string rawData = await manager.group.GetStringAsync($"/v1/groups/{manager.group.Id}/roles/permissions", verifyApiName: "Group.GetRolePermissions");
                    dynamic data = JObject.Parse(rawData);
                    foreach (dynamic group in data.data)
                    {
                        if (group.role.id == id)
                        {
                            foreach (dynamic permGroup in group.permissions)
                            {
                                foreach (dynamic permission in permGroup.Value)
                                {
                                    if (permission.Value == true)
                                    {
                                        string permName = permission.Name;
                                        if (Enum.TryParse(permName, true, out GroupPermission result))
                                            perms.Add(result);
                                    }
                                }
                            }
                            break;
                        }
                    }
                    r.canAccessPermissions = true;
                }
                catch
                {
                    manager.areConfigurationsAccessible = false;
                }
            }

            r.permissions = perms.AsReadOnly();

            return r;
        }

        [UsesSession]
        public async Task UpdateAsync(string newName)
        {
            await roleManager.RequestUpdateRole(this, newName);
        }

        [UsesSession]
        public async Task UpdateAsync(int newRank)
        {
            await roleManager.RequestUpdateRole(this, newRank);
        }

        [UsesSession]
        public async Task DeleteAsync()
        {
            await roleManager.RequestDeleteRole(Id);
        }
    }
}
