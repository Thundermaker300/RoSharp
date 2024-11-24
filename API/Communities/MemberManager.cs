using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Utility;
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
        public async Task<PageResponse<Id<User>>> GetPendingRequestsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/join-requests?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            HttpResponseMessage response = await community.GetAsync(url, verifyApiName: "community.GetPendingRequestsAsync");
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

            return new(list, nextPage, previousPage);
        }


        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are currently in the community.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<Id<User>>> GetMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/users?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<Id<User>>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await community.GetAsync(url, verifyApiName: "community.GetMembersAsync");

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.user.userId);
                list.Add(new Id<User>(userId, community.session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets a <see cref="PageResponse{T}"/> containing IDs of users that are currently banned from the community.
        /// </summary>
        /// <param name="limit">The limit of users to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<Id<User>>> GetBannedMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/bans?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<Id<User>>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await community.GetAsync(url, verifyApiName: "community.GetBannedMembersAsync");

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic user in data.data)
            {
                ulong userId = Convert.ToUInt64(user.user.userId);
                list.Add(new Id<User>(userId, community.session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets whether or not the user with the given Id is in the community.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> IsInCommunityAsync(ulong userId)
        {
            string rawData = await community.GetStringAsync($"/v1/users/{userId}/groups/roles?includeLocked=true");
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic community in data.data)
            {
                if (Convert.ToUInt64(community.community.id) == this.community.Id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets whether or not the user with the given username is in the community.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> IsInCommunityAsync(string username) => await IsInCommunityAsync(await User.FromUsername(username));

        /// <summary>
        /// Gets whether or not the <see cref="User"/> is in the community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task containing a bool upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<bool> IsInCommunityAsync(User user) => await IsInCommunityAsync(user.Id);

        /// <summary>
        /// Gets the role of the user with the given Id.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <returns>A task containing the <see cref="Role"/> upon completion. Will be <see langword="null"/> if the user is not in the community.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<Role?> GetRoleInCommunityAsync(ulong userId)
        {
            string rawData = await community.GetStringAsync($"/v1/users/{userId}/groups/roles?includeLocked=true");
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic community in data.data)
            {
                if (Convert.ToUInt64(community.community.id) == this.community.Id)
                {
                    return (await this.community.GetRoleManagerAsync()).Roles.FirstOrDefault(r => r.Id == Convert.ToUInt64(community.role.id));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the role of the user with the given username.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <returns>A task containing the <see cref="Role"/> upon completion. Will be <see langword="null"/> if the user is not in the community.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<Role?> GetRoleInCommunityAsync(string username) => await GetRoleInCommunityAsync(await User.FromUsername(username));

        /// <summary>
        /// Gets the role of the <see cref="User"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task containing the <see cref="Role"/> upon completion. Will be <see langword="null"/> if the user is not in the community.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<Role?> GetRoleInCommunityAsync(User user) => await GetRoleInCommunityAsync(user.Id);

        /// <summary>
        /// Modifies the user's join request.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <param name="action">Whether to accept or deny the request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure, lack of permissions, or an invalid request (eg. the provided user is not trying to join the community).</exception>
        public async Task ModifyJoinRequestAsync(ulong userId, JoinRequestAction action)
        {
            string url = $"/v1/groups/{community.Id}/join-requests/users/{userId}";
            HttpResponseMessage response = action switch
            {
                JoinRequestAction.Accept => await community.PostAsync(url, new { }, verifyApiName: "MemberManager.ModifyJoinRequestAsync"),
                JoinRequestAction.Decline => await community.DeleteAsync(url, "MemberManager.ModifyJoinRequestAsync"),
                _ => throw new UnreachableException("JoinRequestAction must be Accept or Decline."),
            };
        }

        /// <summary>
        /// Modifies the user's join request.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="action">Whether to accept or deny the request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure, lack of permissions, or an invalid request (eg. the provided user is not trying to join the community).</exception>
        public async Task ModifyJoinRequestAsync(User user, JoinRequestAction action)
            => await ModifyJoinRequestAsync(user.Id, action);

        /// <summary>
        /// Modifies the user's join request.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="action">Whether to accept or deny the request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure, lack of permissions, or an invalid request (eg. the provided user is not trying to join the community).</exception>
        public async Task ModifyJoinRequestAsync(string username, JoinRequestAction action)
            => await ModifyJoinRequestAsync(await UserUtility.GetUserIdAsync(username), action);

        internal async Task SetRankAsyncInternal(ulong userId, ulong newRoleId)
        {
            object body = new { roleId = newRoleId };
            await community.PatchAsync($"/v1/groups/{community.Id}/users/{userId}", body, verifyApiName: "MemberManager.SetRankAsync");
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="role">The new role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(ulong userId, Role role)
        {
            if (role == null || role.roleManager.group.Id != community.Id)
            {
                throw new ArgumentException("Invalid role provided.", nameof(role));
            }
            await SetRankAsyncInternal(userId, role.Id);
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="rankId">The rank Id (<c>0-255</c>) to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(ulong userId, byte rankId)
        {
            Role? role = (await community.GetRoleManagerAsync()).Roles.FirstOrDefault(r => r.Rank == rankId)
                ?? throw new ArgumentException("Invalid rank Id provided.", nameof(rankId));
            await SetRankAsync(userId, role);
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="roleName">The name of the role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(ulong userId, string roleName)
        {
            Role? role = (await community.GetRoleManagerAsync()).Roles.FirstOrDefault(r => r.Name == roleName)
                ?? throw new ArgumentException("Invalid role name provided.", nameof(roleName));
            await SetRankAsync(userId, role);
        }

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The new role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(User user, Role role)
            => await SetRankAsync(user.Id, role);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="rankId">The rank Id (<c>0-255</c>) to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(User user, byte rankId)
            => await SetRankAsync(user.Id, rankId);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roleName">The name of the role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(User user, string roleName)
            => await SetRankAsync(user.Id, roleName);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="role">The new role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(string username, Role role)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), role);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="rankId">The rank Id (<c>0-255</c>) to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(string username, byte rankId)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), rankId);

        /// <summary>
        /// Sets the rank of a user in this community.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="roleName">The name of the role to set them as.</param>
        /// <returns>A task that completes when the operation is completed.</returns>
        /// <exception cref="ArgumentException">Invalid role provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task SetRankAsync(string username, string roleName)
            => await SetRankAsync(await UserUtility.GetUserIdAsync(username), roleName);

        /// <summary>
        /// Kicks the user with the given Id.
        /// </summary>
        /// <param name="userId">The user's Id.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task KickMemberAsync(ulong userId)
        {
            await community.DeleteAsync($"/v1/groups/{community.Id}/users/{userId}", verifyApiName: "MemberManager.KickMemberAsync");
        }

        /// <summary>
        /// Kicks the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task KickMemberAsync(User user) => await KickMemberAsync(user.Id);

        /// <summary>
        /// Delete all wall posts from the given user.
        /// </summary>
        /// <param name="userId">The user Id of the posts to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task DeletePostsFromMemberAsync(ulong userId)
        {
            await community.DeleteAsync($"/v1/groups/{community.Id}/wall/users/{userId}/posts");
        }

        /// <summary>
        /// Delete all wall posts from the given user.
        /// </summary>
        /// <param name="user">The user whose posts to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task DeletePostsFromMemberAsync(User user)
            => await DeletePostsFromMemberAsync(user.Id);

        /// <summary>
        /// Bans a user from the community.
        /// </summary>
        /// <param name="userId">The unique Id of the user to ban.</param>
        /// <param name="deletePosts">If true, will also delete their wall posts.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task BanMemberAsync(ulong userId, bool deletePosts = false)
        {
            if (deletePosts)
                await DeletePostsFromMemberAsync(userId);

            await community.PostAsync($"/v1/groups/{community.Id}/bans/{userId}", new { }, verifyApiName: "MemberManager.BanMemberAsync");
        }

        /// <summary>
        /// Bans a user from the community.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        /// <param name="deletePosts">If true, will also delete their wall posts.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task BanMemberAsync(User user, bool deletePosts = false)
            => await BanMemberAsync(user.Id, deletePosts);

        /// <summary>
        /// Unbans a previously-banned user from the community.
        /// </summary>
        /// <param name="userId">The unique Id of the user to unban.</param>
        /// <returns></returns>
        public async Task UnbanMemberAsync(ulong userId)
        {
            await community.DeleteAsync($"https://groups.roblox.com/v1/groups/{community.Id}/bans/{userId}");
        }

        /// <summary>
        /// Unbans a previously-banned user from the community.
        /// </summary>
        /// <param name="user">The user to unban.</param>
        /// <returns></returns>
        public async Task UnbanMemberAsync(User user)
            => await UnbanMemberAsync(user.Id);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"MemberManager [#{Members}]";
        }
    }
}
