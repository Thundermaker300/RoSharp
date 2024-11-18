using Newtonsoft.Json.Linq;
using RoSharp.API.Misc;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
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
    /// <summary>
    /// Class used for managing roles in a group.
    /// </summary>
    public class RoleManager : IRefreshable
    {
        internal Group group;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private ReadOnlyCollection<Role>? roles;

        /// <summary>
        /// Gets all roles in the group.
        /// </summary>
        public ReadOnlyCollection<Role> Roles => roles;

        internal bool areConfigurationsAccessible = true;

        /// <inheritdoc/>
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

        /// <summary>
        /// Gets a role with the given rank, from <c>0-255</c>.
        /// </summary>
        /// <param name="rankId">The rank ID.</param>
        /// <returns>The role if it exists.</returns>
        public Role? GetRole(byte rankId)
            => Roles.FirstOrDefault(role => role.Rank == rankId);

        /// <summary>
        /// Gets a role with the given name.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        /// <returns>The role if it exists.</returns>
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
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This method will call <see cref="RefreshAsync"/> upon completion, forcing <see cref="Roles"/> to be updated automatically. As of November 16th, 2024, roles cost R$25 to make. This method will bypass a confirmation prompt and purchase the role immediately. Use caution in order to not accidentally create roles and burn through money!</remarks>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"RoleManager [#{Roles.Count}]";
        }
    }

    /// <summary>
    /// Represents a role within a group.
    /// </summary>
    public class Role
    {
        internal RoleManager roleManager;


        private ulong id;

        /// <summary>
        /// Gets the internal id of the role.
        /// </summary>
        public ulong Id => id;


        private string name;

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        public string Name => name;


        private string? description;

        /// <summary>
        /// Gets the description of the role. Can be <see langword="null"/> if the authenticated user cannot see role descriptions.
        /// </summary>
        public string? Description => description;


        private byte rank;

        /// <summary>
        /// Gets the rank of the role, from <c>0-255</c>.
        /// </summary>
        public byte Rank => rank;


        private ulong memberCount;

        /// <summary>
        /// Gets the amount of members within this role.
        /// </summary>
        public ulong MemberCount => memberCount;

        private bool canAccessPermissions;

        /// <summary>
        /// Gets whether or not the authenticated user can see the role's <see cref="Permissions"/>.
        /// </summary>
        public bool CanAccessPermissions => canAccessPermissions;

        private ReadOnlyCollection<GroupPermission> permissions;

        /// <summary>
        /// Gets the permissions of the role. This list will be empty if <see cref="CanAccessPermissions"/> is <see langword="false"/>.
        /// </summary>
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

        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are currently in the group under this role.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<GenericId<User>>> GetMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{roleManager.group.Id}/roles/{Id}/users?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<GenericId<User>>();
            string? nextPage = null;
            string? previousPage = null;
            HttpResponseMessage response = await roleManager.group.GetAsync(url, verifyApiName: "Role.GetMembersAsync");
            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (dynamic user in data.data)
                {
                    ulong userId = Convert.ToUInt64(user.userId);
                    list.Add(new GenericId<User>(userId, roleManager.group.session));
                }
                nextPage = data.nextPageCursor;
                previousPage = data.previousPageCursor;
            }

            return new(list, nextPage, previousPage);
        }


        /// <summary>
        /// Changes the name of the role.
        /// </summary>
        /// <param name="newName">The new name.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task UpdateAsync(string newName)
        {
            await roleManager.RequestUpdateRole(this, newName);
        }

        /// <summary>
        /// Changes the rank of the role.
        /// </summary>
        /// <param name="newRank">The new rank.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task UpdateAsync(int newRank)
        {
            await roleManager.RequestUpdateRole(this, newRank);
        }


        /// <summary>
        /// Deletes the role. ALL USERS MUST BE REMOVED FROM THE ROLE BEFORE IT CAN BE DELETED.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task DeleteAsync()
        {
            await roleManager.RequestDeleteRole(Id);
        }
    }
}
