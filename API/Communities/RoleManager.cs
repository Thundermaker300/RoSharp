﻿using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Interfaces;
using RoSharp.Structures;
using System;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace RoSharp.API.Communities
{
    /// <summary>
    /// Class used for managing roles in a community.
    /// </summary>
    public class RoleManager : IRefreshable
    {
        internal Community group;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private ReadOnlyCollection<Role> roles = new List<Role>(0).AsReadOnly();

        /// <summary>
        /// Gets all roles in the community.
        /// </summary>
        public ReadOnlyCollection<Role> Roles => roles;

        internal bool areConfigurationsAccessible = true;

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            List<Role> list = [];
            string rawData = await group.SendStringAsync(HttpMethod.Get, $"/v1/groups/{group.Id}/roles");
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

        internal RoleManager(Community group) { this.group = group; }

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
        /// Purchases and creates a new role in this community.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The private description of the role.</param>
        /// <param name="rank">The rank of the role.</param>
        /// <param name="purchaseWithCommunityFunds">Purchase with community funds if <see langword="true"/>. Purchase with the authenticated user's funds if <see langword="false"/>.</param>
        /// <returns>A task containing a <see cref="Role"/> representing the newly-created Role.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This method will call <see cref="RefreshAsync"/> upon completion, forcing <see cref="Roles"/> to be updated automatically. As of November 16th, 2024, roles cost R$25 to make. This method will bypass a confirmation prompt and purchase the role immediately. Use caution in order to not accidentally create roles and burn through money!</remarks>
        public async Task<HttpResult<Role>> CreateRoleAsync(string name, string description, byte rank, bool purchaseWithCommunityFunds)
        {
            var message = new HttpMessage(HttpMethod.Post, $"/v1/groups/{group.Id}/rolesets/create", new
            {
                name = name,
                description = description,
                rank = rank,
                usingGroupFunds = purchaseWithCommunityFunds,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(CreateRoleAsync),
            };

            HttpResponseMessage response = await group.SendAsync(message);
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            await RefreshAsync();
            return new(response, GetRole(Convert.ToByte(data.rank)));
        }

        internal async Task<HttpResult> RequestDeleteRole(ulong roleId)
        {
            var message = new HttpMessage(HttpMethod.Delete, $"/v1/groups/{group.Id}/rolesets/{roleId}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(Role.DeleteAsync)
            };

            return new(await group.SendAsync(message));
        }

        internal async Task<HttpResult> RequestUpdateRole(Role roleId, string newName)
        {
            object body = new { name = newName, description = roleId.Description, rank = roleId.Rank };
            var message = new HttpMessage(HttpMethod.Patch, $"/v1/groups/{group.Id}/rolesets/{roleId.Id}", body)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(Role.UpdateAsync)
            };
            return new(await group.SendAsync(message));
        }

        internal async Task<HttpResult> RequestUpdateRole(Role roleId, int newRank)
        {
            object body = new { name = roleId.Name, description = roleId.Description, rank = newRank };
            var message = new HttpMessage(HttpMethod.Patch, $"/v1/groups/{group.Id}/rolesets/{roleId.Id}", body)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(Role.UpdateAsync)
            };

            return new(await group.SendAsync(message));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"RoleManager [#{Roles.Count}]";
        }
    }

    /// <summary>
    /// Represents a role within a community.
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
            Role r = new()
            {
                roleManager = manager,

                id = id,
                name = name,
                description = description,
                rank = rank,
                memberCount = memberCount,
                canAccessPermissions = false
            };

            List<GroupPermission> perms = [];

            if (manager.areConfigurationsAccessible)
            {
                HttpMessage message = new HttpMessage(HttpMethod.Get, $"/v1/groups/{manager.group.Id}/roles/permissions")
                {
                    AuthType = AuthType.RobloSecurity,
                    ApiName = "Community.GetRolePermissions",
                    SilenceExceptions = true,
                };
                HttpResponseMessage response = await manager.group.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    string rawData = await response.Content.ReadAsStringAsync();
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
                else
                {
                    manager.areConfigurationsAccessible = false;
                }
            }

            r.permissions = perms.AsReadOnly();

            return r;
        }

        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are currently in the community under this role.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<PageResponse<Id<User>>>> GetMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{roleManager.group.Id}/roles/{Id}/users?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetMembersAsync)
            };

            var list = new List<Id<User>>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await roleManager.group.SendAsync(message);

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.userId);
                list.Add(new Id<User>(userId, roleManager.group.session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(response, new(list, nextPage, previousPage));
        }


        /// <summary>
        /// Changes the name of the role.
        /// </summary>
        /// <param name="newName">The new name.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> UpdateAsync(string newName) => new(await roleManager.RequestUpdateRole(this, newName));

        /// <summary>
        /// Changes the rank of the role.
        /// </summary>
        /// <param name="newRank">The new rank.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> UpdateAsync(int newRank) => new(await roleManager.RequestUpdateRole(this, newRank));


        /// <summary>
        /// Deletes the role. ALL USERS MUST BE REMOVED FROM THE ROLE BEFORE IT CAN BE DELETED.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> DeleteAsync() => await roleManager.RequestDeleteRole(Id);
    }
}
