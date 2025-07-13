using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Structures;
using RoSharp.Utility;
using System;
using System.Diagnostics;

namespace RoSharp.API.Communities
{
    /// <summary>
    /// Class used for managing members of a community.
    /// </summary>
    public class MemberManager
    {
        private Community community;

        internal MemberManager(Community community) { this.community = community; }

        /// <summary>
        /// Gets the total amount of members in the community.
        /// </summary>
        public ulong Members => community.members;

        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are requesting to join the community.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<Id<User>>>> GetPendingRequestsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/join-requests?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetPendingRequestsAsync)
            };

            HttpResponseMessage response = await community.SendAsync(message);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            var list = new List<Id<User>>();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.requester.userId);
                list.Add(new Id<User>(userId, community.session));
            }

            return new(response, new(list, nextPage, previousPage));
        }


        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are currently in the community.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<Id<User>>>> GetMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/users?limit={limit.Limit()}&sortOrder={sortOrder}";
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
            HttpResponseMessage response = await community.SendAsync(message);

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.user.userId);
                list.Add(new Id<User>(userId, community.session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(response, new(list, nextPage, previousPage));
        }

        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are currently banned from the community.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<Id<User>>>> GetBannedMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/bans?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetBannedMembersAsync)
            };

            var list = new List<Id<User>>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await community.SendAsync(message);

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.user.userId);
                list.Add(new Id<User>(userId, community.session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(response, new(list, nextPage, previousPage));
        }

        /// <summary>
        /// Gets whether or not the user with the given Id is in the community.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<bool>> IsInCommunityAsync(ulong userId)
        {
            HttpResponseMessage response = await community.SendAsync(HttpMethod.Get, $"/v1/users/{userId}/groups/roles?includeLocked=true");
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic community in data.data)
            {
                if (Convert.ToUInt64(community.community.id) == this.community.Id)
                {
                    return new(response, true);
                }
            }
            return new(response, false);
        }

        /// <summary>
        /// Gets whether or not the user with the given username is in the community.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<bool>> IsInCommunityAsync(string username) => await IsInCommunityAsync(await User.FromUsername(username));

        /// <summary>
        /// Gets whether or not the <see cref="User"/> is in the community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<bool>> IsInCommunityAsync(User user) => await IsInCommunityAsync(user.Id);

        /// <summary>
        /// Gets the role of the user with the given Id.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <returns>A task containing the <see cref="Role"/> upon completion. Will be <see langword="null"/> if the user is not in the community.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<Role?>> GetRoleInCommunityAsync(ulong userId)
        {
            var response = await community.SendAsync(HttpMethod.Get, $"/v1/users/{userId}/groups/roles?includeLocked=true");
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic community in data.data)
            {
                if (Convert.ToUInt64(community.community.id) == this.community.Id)
                {
                    return new(response, (await this.community.GetRoleManagerAsync()).Roles.FirstOrDefault(r => r.Id == Convert.ToUInt64(community.role.id)));
                }
            }
            return new(response, null);
        }

        /// <summary>
        /// Gets the role of the user with the given username.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <returns>A task containing the <see cref="Role"/> upon completion. Will be <see langword="null"/> if the user is not in the community.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<Role?>> GetRoleInCommunityAsync(string username) => await GetRoleInCommunityAsync(await User.FromUsername(username));

        /// <summary>
        /// Gets the role of the <see cref="User"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task containing the <see cref="Role"/> upon completion. Will be <see langword="null"/> if the user is not in the community.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<Role?>> GetRoleInCommunityAsync(User user) => await GetRoleInCommunityAsync(user.Id);

        /// <summary>
        /// Modifies the user's join request.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <param name="action">Whether to accept or deny the request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure, lack of permissions, or an invalid request (eg. the provided user is not trying to join the community).</exception>
        public async Task<HttpResult> ModifyJoinRequestAsync(ulong userId, JoinRequestAction action)
        {
            string url = $"/v1/groups/{community.Id}/join-requests/users/{userId}";
            var message = new HttpMessage(action switch
            {
                JoinRequestAction.Accept => HttpMethod.Post,
                JoinRequestAction.Decline => HttpMethod.Delete,
                _ => throw new UnreachableException("JoinRequestAction must be Accept or Decline.")
            }, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyJoinRequestAsync)
            };

            return new(await community.SendAsync(message));
        }

        /// <summary>
        /// Modifies the user's join request.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="action">Whether to accept or deny the request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure, lack of permissions, or an invalid request (eg. the provided user is not trying to join the community).</exception>
        public async Task<HttpResult> ModifyJoinRequestAsync(User user, JoinRequestAction action)
            => await ModifyJoinRequestAsync(user.Id, action);

        /// <summary>
        /// Modifies the user's join request.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="action">Whether to accept or deny the request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure, lack of permissions, or an invalid request (eg. the provided user is not trying to join the community).</exception>
        public async Task<HttpResult> ModifyJoinRequestAsync(string username, JoinRequestAction action)
            => await ModifyJoinRequestAsync(await UserUtility.GetUserIdAsync(username), action);

        internal async Task<HttpResult> SetRankAsyncInternal(ulong userId, ulong newRoleId)
        {
            var message = new HttpMessage(HttpMethod.Patch, $"/v1/groups/{community.Id}/users/{userId}", new { roleId = newRoleId })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(SetRankAsync)
            };

            return new(await community.SendAsync(message));
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="role">The new role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(ulong userId, Role role)
        {
            if (role == null || role.roleManager.group.Id != community.Id)
            {
                throw new ArgumentException("Invalid role provided.", nameof(role));
            }
            return new(await SetRankAsyncInternal(userId, role.Id));
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="rankId">The rank Id (<c>0-255</c>) to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(ulong userId, byte rankId)
        {
            Role? role = (await community.GetRoleManagerAsync()).Roles.FirstOrDefault(r => r.Rank == rankId)
                ?? throw new ArgumentException("Invalid rank Id provided.", nameof(rankId));
            return new(await SetRankAsync(userId, role));
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="roleName">The name of the role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(ulong userId, string roleName)
        {
            Role? role = (await community.GetRoleManagerAsync()).Roles.FirstOrDefault(r => r.Name == roleName)
                ?? throw new ArgumentException("Invalid role name provided.", nameof(roleName));
            return new(await SetRankAsync(userId, role));
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The new role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(User user, Role role)
            => await SetRankAsync(user.Id, role);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="rankId">The rank Id (<c>0-255</c>) to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(User user, byte rankId)
            => await SetRankAsync(user.Id, rankId);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roleName">The name of the role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(User user, string roleName)
            => await SetRankAsync(user.Id, roleName);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="role">The new role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(string username, Role role)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), role);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="rankId">The rank Id (<c>0-255</c>) to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(string username, byte rankId)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), rankId);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="roleName">The name of the role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> SetRankAsync(string username, string roleName)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), roleName);

        /// <summary>
        /// Kicks the user with the given Id.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <param name="deletePosts">If <see langword="true"/>, will also delete their wall posts.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> KickMemberAsync(ulong userId, bool deletePosts = true)
        {
            if (deletePosts)
                await community.DeletePostsFromMemberAsync(userId);

            var message = new HttpMessage(HttpMethod.Delete, $"/v1/groups/{community.Id}/users/{userId}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(KickMemberAsync)
            };

            return new(await community.SendAsync(message));
        }

        /// <summary>
        /// Kicks the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="deletePosts">If <see langword="true"/>, will also delete their wall posts.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> KickMemberAsync(User user, bool deletePosts = true) => await KickMemberAsync(user.Id, deletePosts);

        /// <summary>
        /// Bans a user from the community.
        /// </summary>
        /// <param name="userId">The unique Id of the user to ban.</param>
        /// <param name="deletePosts">If <see langword="true"/>, will also delete their wall posts.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> BanMemberAsync(ulong userId, bool deletePosts = false)
        {
            if (deletePosts)
                await community.DeletePostsFromMemberAsync(userId);

            var message = new HttpMessage(HttpMethod.Post, $"/v1/groups/{community.Id}/bans/{userId}", new { })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(BanMemberAsync)
            };

            return new(await community.SendAsync(message));
        }

        /// <summary>
        /// Bans a user from the community.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        /// <param name="deletePosts">If <see langword="true"/>, will also delete their wall posts.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> BanMemberAsync(User user, bool deletePosts = false)
            => await BanMemberAsync(user.Id, deletePosts);

        /// <summary>
        /// Unbans a previously-banned user from the community.
        /// </summary>
        /// <param name="userId">The unique Id of the user to unban.</param>
        /// <returns></returns>
        public async Task<HttpResult> UnbanMemberAsync(ulong userId)
        {
            var message = new HttpMessage(HttpMethod.Delete, $"https://groups.roblox.com/v1/groups/{community.Id}/bans/{userId}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(UnbanMemberAsync)
            };

            return new(await community.SendAsync(message));
        }

        /// <summary>
        /// Unbans a previously-banned user from the community.
        /// </summary>
        /// <param name="user">The user to unban.</param>
        /// <returns></returns>
        public async Task<HttpResult> UnbanMemberAsync(User user)
            => await UnbanMemberAsync(user.Id);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"MemberManager [#{Members}]";
        }
    }
}
