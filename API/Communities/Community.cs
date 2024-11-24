using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Structures;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RoSharp.API.Communities
{
    /// <summary>
    /// A class that represents a Roblox community (formerly group).
    /// </summary>
    public class Community : APIMain, IRefreshable, IAssetOwner, IIdApi<Community>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("groups");

        /// <summary>
        /// Gets the unique Id of the community.
        /// </summary>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/groups/{Id}/";

        private string name;

        /// <summary>
        /// Gets the community's name.
        /// </summary>
        public string Name => name;

        private string description;

        /// <summary>
        /// Gets the community's description.
        /// </summary>
        public string Description => description;

        private User? owner;

        /// <summary>
        /// Gets the community's owner. Can be <see langword="null"/> if the community does not have an owner (abandoned).
        /// </summary>
        public User? Owner => owner;

        /// <summary>
        /// Indicates whether or not the community has an owner. Equivalent to checking if <see cref="Owner"/> is <see langword="null"/>.
        /// </summary>
        public bool HasOwner => Owner != null;

        private bool isPublic;

        /// <summary>
        /// Gets whether or not the community is publicly joinable.
        /// </summary>
        public bool IsPublic => isPublic;

        private bool verified;

        /// <summary>
        /// Gets whether or not the community is verified (blue checkmark).
        /// </summary>
        public bool Verified => verified;

        private int robux = -1;

        /// <summary>
        /// Gets the amount of Robux this community has. Will be <c>-1</c> if the authenticated user cannot view funds.
        /// </summary>
        public int Robux => robux;


        private RoleManager roleManager;

        private MemberManager memberManager;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        internal ulong members;

        private Community(ulong communityId, Session? session = null)
        {
            Id = communityId;
            
            if (session != null)
                AttachSession(session);

            if (!RoPool<Community>.Contains(Id))
                RoPool<Community>.Add(this);
        }

        /// <summary>
        /// Returns a <see cref="Community"/> given its Id.
        /// </summary>
        /// <param name="communityId">The community Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="Community"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the community Id is invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<Community> FromId(ulong communityId, Session? session = null)
        {
            if (RoPool<Community>.Contains(communityId))
                return RoPool<Community>.Get(communityId, session.Global());

            Community newGroup = new(communityId, session.Global());
            await newGroup.RefreshAsync();

            return newGroup;
        }

        /// <summary>
        /// Gets a <see cref="RoleManager"/> class that has additional API to manage community roles.
        /// </summary>
        public async Task<RoleManager> GetRoleManagerAsync()
        {
            if (roleManager == null)
            {
                roleManager = new RoleManager(this);
                await roleManager.RefreshAsync();
            }
            return roleManager;
        }

        /// <summary>
        /// Gets a <see cref="MemberManager"/> class that has additional API to manage community members.
        /// </summary>
        public async Task<MemberManager> GetMemberManagerAsync()
        {
            memberManager ??= new MemberManager(this);
            return memberManager;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v1/groups/{Id}");
            string raw = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(raw);

            name = data.name;
            description = data.description;

            if (data.owner != null)
            {
                ulong ownerId = Convert.ToUInt64(data.owner.userId);
                owner = await User.FromId(ownerId, session);
            }

            isPublic = data.publicEntryAllowed;
            verified = data.hasVerifiedBadge;

            members = data.memberCount;

            try
            {
                string rawData = await GetStringAsync($"/v1/groups/{Id}/currency", Constants.URL("economy"), verifyApiName: "Group.GetGroupFunds");
                dynamic robuxData = JObject.Parse(rawData);
                robux = robuxData.robux;
            }
            catch
            {
                robux = -1;
            }

            // Reset properties
            shout = null;
            socialChannels = null;

            if (roleManager != null)
                await roleManager.RefreshAsync();

            RefreshedAt = DateTime.Now;
        }

        private CommunityShout? shout;

        /// <summary>
        /// Gets the community's current shout.
        /// </summary>
        /// <returns>A task containing a <see cref="CommunityShout"/> representing the shout upon completion. Can be <see langword="null"/> if there is no current shout.</returns>
        public async Task<CommunityShout?> GetShoutAsync()
        {
            if (shout == null)
            {
                string rawData = await GetStringAsync($"/v1/groups/{Id}");
                dynamic data = JObject.Parse(rawData);
                if (data.shout != null)
                {
                    ulong posterId = Convert.ToUInt64(data.shout.poster.userId);

                    shout = new CommunityShout
                    {
                        Text = data.shout.body,
                        Poster = await User.FromId(posterId, session),
                        PostedAt = data.shout.updated,
                    };
                }
            }
            return shout;
        }

        private ReadOnlyDictionary<string, string>? socialChannels;

        /// <summary>
        /// Gets this community's social channels.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> upon completion. The key is the name of the social media platform, and the value is its URL.</returns>
        public async Task<ReadOnlyDictionary<string, string>> GetSocialChannelsAsync()
        {
            if (socialChannels == null)
            {
                Dictionary<string, string> dict = [];
                string rawData = await GetStringAsync($"/v1/groups/{Id}/social-links");
                dynamic data = JObject.Parse(rawData);
                foreach (dynamic media in data.data)
                {
                    dict.Add(Convert.ToString(media.type), Convert.ToString(media.url));
                }
                socialChannels = dict.AsReadOnly();
            }

            return socialChannels;
        }

        /// <summary>
        /// Gets the community's icon.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>Task that contains a URL to the icon, upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<string> GetIconAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/groups/icons?communityIds={Id}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new UnreachableException("Invalid group to get icon for.");
            return data.data[0].imageUrl;
        }

        /// <summary>
        /// Modifies the community's description.
        /// </summary>
        /// <param name="text">The new community's description.</param>
        /// <returns>Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task ModifyDescriptionAsync(string text)
        {
            object body = new { description = text };
            await PatchAsync($"/v1/groups/{Id}/description", body, verifyApiName: "Group.ModifyDescriptionAsync");
        }

        /// <summary>
        /// Creates a community shout.
        /// </summary>
        /// <param name="text">The text for the shout.</param>
        /// <returns>Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task ShoutAsync(string text)
        {
            object body = new { message = text };
            await PatchAsync($"/v1/groups/{Id}/status", body, verifyApiName: "Group.ShoutAsync");
        }

        /// <summary>
        /// Gets this community's public experiences.
        /// </summary>
        /// <param name="limit">The limit of experiences to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions to see experiences.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<Id<Experience>>> GetExperiencesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/groups/{Id}/games?accessFilter=Public&limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<Id<Experience>>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await GetAsync(url, Constants.URL("games"));

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic exp in data.data)
            {
                list.Add(new Id<Experience>(Convert.ToUInt64(exp.id), session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(list, nextPage, previousPage);
        }

        /// <summary>
        /// Returns this community's audit logs.
        /// </summary>
        /// <param name="limit">The maximum amount of logs to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="CommunityAuditLog"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<CommunityAuditLog>> GetAuditLogsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/groups/{Id}/audit-log?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, verifyApiName: "Group.GetAuditLogsAsync");
            dynamic data = JObject.Parse(rawData);

            List<CommunityAuditLog> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                CommunityAuditLog log = new()
                {
                    Type = Enum.Parse<CommunityAuditLogType>(Convert.ToString(item.actionType).Replace(" ", string.Empty)),
                    Time = item.created,
                    CommunityId = new(Id, session),
                    UserId = new(Convert.ToUInt64(item.actor.user.userId), session),
                    RankId = item.actor.role.rank,

                    TargetUserId = item.description?.TargetId != null ? new(Convert.ToUInt64(item.description.TargetId), session) : null,
                    TargetCommunityId = item.description?.TargetGroupId != null ? new(Convert.ToUInt64(item.description.TargetGroupId), session) : null,
                };
                list.Add(log);
            }

            return new PageResponse<CommunityAuditLog>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets this community's allies.
        /// </summary>
        /// <param name="limit">The maximum amount of communities to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        public async Task<PageResponse<Id<Community>>> GetAlliesAsync(int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
        {
            string url = $"/v1/groups/{Id}/relationships/allies?maxRows={limit}&sortOrder={sortOrder}&startRowIndex={startRowIndex}";

            string rawData = await GetStringAsync(url);
            dynamic data = JObject.Parse(rawData);

            List<Id<Community>> list = [];
            string? nextPage = Convert.ToString(data.nextRowIndex);

            foreach (dynamic item in data.relatedGroups)
            {
                list.Add(new(Convert.ToUInt64(item.id), session));
            }

            return new PageResponse<Id<Community>>(list, nextPage, null);
        }
        /// <summary>
        /// Gets this community's allies.
        /// </summary>
        /// <param name="limit">The maximum amount of communities to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        public async Task<PageResponse<Id<Community>>> GetEnemiesAsync(int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
        {
            string url = $"/v1/groups/{Id}/relationships/enemies?maxRows={limit}&sortOrder={sortOrder}&startRowIndex={startRowIndex}";

            string rawData = await GetStringAsync(url);
            dynamic data = JObject.Parse(rawData);

            List<Id<Community>> list = [];
            string? nextPage = Convert.ToString(data.nextRowIndex);

            foreach (dynamic item in data.relatedGroups)
            {
                list.Add(new(Convert.ToUInt64(item.id), session));
            }

            return new PageResponse<Id<Community>>(list, nextPage, null);
        }

        /// <summary>
        /// Gets this community's income statistics for the given <paramref name="timeLength"/>.
        /// </summary>
        /// <param name="timeLength">The length of time to use for the breakdown.</param>
        /// <returns>A task containing a <see cref="EconomyBreakdown"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<EconomyBreakdown> GetIncomeAsync(AnalyticTimeLength timeLength = AnalyticTimeLength.Day)
        {
            var url = $"/v1/groups/{Id}/revenue/summary/{timeLength.ToString().ToLower()}";
            string rawData = await GetStringAsync(url, Constants.URL("economy"), verifyApiName: "Group.GetIncomeAsync");
            dynamic data = JObject.Parse(rawData);

            int amount = 0;
            int pending = 0;
            Dictionary<IncomeType, int> breakdown = [];

            foreach (dynamic cat in data)
            {
                string catName = Convert.ToString(cat.Name);

                if (catName == "isShowImmersiveAdPayoutSummaryOnZeroEnabled") // thanks roblox
                    continue;

                IncomeType incomeType = Enum.Parse<IncomeType>(catName, true);
                int myAmount = Convert.ToInt32(cat.Value);
                if (myAmount != 0)
                {
                    breakdown.Add(incomeType, myAmount);

                    if (cat.Name != "pendingRobux")
                        amount += myAmount;
                    else
                        pending = myAmount;
                }
            }

            return new EconomyBreakdown(timeLength, amount, breakdown, pending);
        }

        /// <summary>
        /// Gets this community's wall posts.
        /// </summary>
        /// <param name="limit">The limit of posts to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="CommunityPost"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions to see the community wall.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<CommunityPost>> GetPostsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/groups/{Id}/wall/posts?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<CommunityPost>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await GetAsync(url, verifyApiName: "Group.GetGroupPostsAsync");

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic post in data.data)
            {
                list.Add(new CommunityPost()
                {
                    PostId = post.id,
                    PostedAt = post.updated,
                    Text = post.body,
                    RankInCommunity = post.poster == null ? null : post.poster.role.name,
                    PosterId = post.poster == null ? null : new Id<User>(Convert.ToUInt64(post.poster.userId)),

                    group = this,
                });
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(list, nextPage, previousPage);
        }

        /// <summary>
        /// Delete all wall posts from the given user.
        /// </summary>
        /// <param name="userId">The user Id of the posts to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task DeletePostsFromMemberAsync(ulong userId)
        {
            await DeleteAsync($"/v1/groups/{Id}/wall/users/{userId}/posts");
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
        /// Deletes the wall post with the given Id.
        /// </summary>
        /// <param name="postId">The post Id to delete. Can be obtained from <see cref="GetPostsAsync(FixedLimit, RequestSortOrder, string?)"/>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task DeletePostAsync(ulong postId)
        {
            await DeleteAsync($"/v1/groups/{Id}/wall/posts/{postId}");
        }

        /// <summary>
        /// Deletes the specified wall post. Convenience method, equivalent to calling <see cref="DeletePostAsync(ulong)"/> with the <see cref="CommunityPost"/>'s <see cref="CommunityPost.PostId"/>.
        /// </summary>
        /// <param name="post">The post to delete. Can be obtained from <see cref="GetPostsAsync(FixedLimit, RequestSortOrder, string?)"/>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task DeletePostAsync(CommunityPost post)
            => await DeletePostAsync(post.PostId);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} [{Id}] {{{members}}}{(Verified ? " [V]" : string.Empty)}";
        }

        /// <inheritdoc/>
        public Community AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
