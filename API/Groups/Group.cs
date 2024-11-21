using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RoSharp.API.Groups
{
    /// <summary>
    /// A class that represents a Roblox group.
    /// </summary>
    public class Group : APIMain, IRefreshable, IAssetOwner, IIdApi<Group>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("groups");

        /// <summary>
        /// Gets the unique Id of the group.
        /// </summary>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/groups/{Id}/";

        private string name;

        /// <summary>
        /// Gets the group's name.
        /// </summary>
        public string Name => name;

        private string description;

        /// <summary>
        /// Gets the group's description.
        /// </summary>
        public string Description => description;

        private User? owner;

        /// <summary>
        /// Gets the group's owner. Can be <see langword="null"/> if the group does not have an owner (abandoned).
        /// </summary>
        public User? Owner => owner;

        /// <summary>
        /// Indicates whether or not the group has an owner. Equivalent to checking if <see cref="Owner"/> is <see langword="null"/>.
        /// </summary>
        public bool HasOwner => Owner != null;

        private bool isPublic;

        /// <summary>
        /// Gets whether or not the group is publicly joinable.
        /// </summary>
        public bool IsPublic => isPublic;

        private bool verified;

        /// <summary>
        /// Gets whether or not the group is verified (blue checkmark).
        /// </summary>
        public bool Verified => verified;

        private int robux;

        /// <summary>
        /// Gets the amount of Robux this group has. Will be <c>-1</c> if the authenticated user cannot view funds.
        /// </summary>
        public int Robux => robux;


        private RoleManager roleManager;

        private MemberManager memberManager;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        internal ulong members;

        private Group(ulong groupId, Session? session = null)
        {
            Id = groupId;
            
            if (session != null)
                AttachSession(session);

            if (!RoPool<Group>.Contains(Id))
                RoPool<Group>.Add(this);
        }

        /// <summary>
        /// Returns a <see cref="Group"/> given its Id.
        /// </summary>
        /// <param name="groupId">The group Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="Group"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the group Id is invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<Group> FromId(ulong groupId, Session? session = null)
        {
            if (RoPool<Group>.Contains(groupId))
                return RoPool<Group>.Get(groupId, session.Global());

            Group newGroup = new(groupId, session.Global());
            await newGroup.RefreshAsync();

            return newGroup;
        }

        /// <summary>
        /// Gets a <see cref="RoleManager"/> class that has additional API to manage group roles.
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
        /// Gets a <see cref="MemberManager"/> class that has additional API to manage group members.
        /// </summary>
        public async Task<MemberManager> GetMemberManagerAsync()
        {
            if (memberManager == null)
            {
                memberManager = new MemberManager(this);
            }
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

        private GroupShout? shout;

        /// <summary>
        /// Gets the group's current shout.
        /// </summary>
        /// <returns>A task containing a <see cref="GroupShout"/> representing the shout upon completion. Can be <see langword="null"/> if there is no current shout.</returns>
        public async Task<GroupShout?> GetShoutAsync()
        {
            if (shout == null)
            {
                string rawData = await GetStringAsync($"/v1/groups/{Id}");
                dynamic data = JObject.Parse(rawData);
                if (data.shout != null)
                {
                    ulong posterId = Convert.ToUInt64(data.shout.poster.userId);

                    shout = new GroupShout
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
        /// Gets this group's social channels.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> upon completion. The key is the name of the social media platform, and the value is its URL.</returns>
        public async Task<ReadOnlyDictionary<string, string>> GetSocialChannelsAsync()
        {
            if (socialChannels == null)
            {
                Dictionary<string, string> dict = new();
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
        /// Gets the group's icon.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>Task that contains a URL to the icon, upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<string> GetIconAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/groups/icons?groupIds={Id}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new UnreachableException("Invalid group to get icon for.");
            return data.data[0].imageUrl;
        }

        /// <summary>
        /// Modifies the group description.
        /// </summary>
        /// <param name="text">The new group description.</param>
        /// <returns>Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task ModifyDescriptionAsync(string text)
        {
            object body = new { description = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{Id}/description", body, verifyApiName: "Group.ModifyDescriptionAsync");
        }

        /// <summary>
        /// Creates a group shout.
        /// </summary>
        /// <param name="text">The text for the shout.</param>
        /// <returns>Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task ShoutAsync(string text)
        {
            object body = new { message = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{Id}/status", body, verifyApiName: "Group.ShoutAsync");
        }

        /// <summary>
        /// Gets this group's wall posts.
        /// </summary>
        /// <param name="limit">The limit of posts to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GroupPost"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions to see the group wall.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        [UsesSession]
        public async Task<PageResponse<GroupPost>> GetGroupPostsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"v2/groups/{Id}/wall/posts?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<GroupPost>();
            string? nextPage = null;
            string? previousPage = null;
            HttpResponseMessage response = await GetAsync(url, verifyApiName: "Group.GetGroupPostsAsync");

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic post in data.data)
            {
                list.Add(new GroupPost()
                {
                    PostId = post.id,
                    PostedAt = post.updated,
                    Text = post.body,
                    RankInGroup = post.poster == null ? null : post.poster.role.name,
                    PosterId = post.poster == null ? null : new GenericId<User>(Convert.ToUInt64(post.poster.userId)),

                    group = this,
                });
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(list, nextPage, previousPage);
        }

        /// <summary>
        /// Returns this group's audit logs.
        /// </summary>
        /// <param name="limit">The maximum amount of logs to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GroupAuditLog"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<PageResponse<GroupAuditLog>> GetAuditLogsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/groups/{Id}/audit-log?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            string rawData = await GetStringAsync(url, verifyApiName: "Group.GetAuditLogsAsync");
            dynamic data = JObject.Parse(rawData);

            List<GroupAuditLog> list = new();
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                GroupAuditLog log = new GroupAuditLog()
                {
                    Type = Enum.Parse<GroupAuditLogType>(Convert.ToString(item.actionType).Replace(" ", string.Empty)),
                    Time = item.created,
                    GroupId = new(Id, session),
                    UserId = new(Convert.ToUInt64(item.actor.user.userId), session),
                    RankId = item.actor.role.rank,

                    TargetUserId = item.description?.TargetId != null ? new(Convert.ToUInt64(item.description.TargetId), session) : null,
                    TargetGroupId = item.description?.TargetGroupId != null ? new(Convert.ToUInt64(item.description.TargetGroupId), session) : null,
                };
                list.Add(log);
            }

            return new PageResponse<GroupAuditLog>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Gets this group's allies.
        /// </summary>
        /// <param name="limit">The maximum amount of groups to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        public async Task<PageResponse<GenericId<Group>>> GetAlliesAsync(int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
        {
            string url = $"/v1/groups/{Id}/relationships/allies?maxRows={limit}&sortOrder={sortOrder}&startRowIndex={startRowIndex}";

            string rawData = await GetStringAsync(url);
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Group>> list = new();
            string? nextPage = Convert.ToString(data.nextRowIndex);

            foreach (dynamic item in data.relatedGroups)
            {
                list.Add(new(Convert.ToUInt64(item.id), session));
            }

            return new PageResponse<GenericId<Group>>(list, nextPage, null);
        }
        /// <summary>
        /// Gets this group's allies.
        /// </summary>
        /// <param name="limit">The maximum amount of groups to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="GenericId{T}"/> upon completion.</returns>
        public async Task<PageResponse<GenericId<Group>>> GetEnemiesAsync(int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
        {
            string url = $"/v1/groups/{Id}/relationships/enemies?maxRows={limit}&sortOrder={sortOrder}&startRowIndex={startRowIndex}";

            string rawData = await GetStringAsync(url);
            dynamic data = JObject.Parse(rawData);

            List<GenericId<Group>> list = new();
            string? nextPage = Convert.ToString(data.nextRowIndex);

            foreach (dynamic item in data.relatedGroups)
            {
                list.Add(new(Convert.ToUInt64(item.id), session));
            }

            return new PageResponse<GenericId<Group>>(list, nextPage, null);
        }

        /// <summary>
        /// Gets this group's income statistics for the given <paramref name="timeLength"/>.
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
            Dictionary<IncomeType, int> breakdown = new();

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} [{Id}] {{{members}}}{(Verified ? " [V]" : string.Empty)}";
        }

        /// <inheritdoc/>
        public Group AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
