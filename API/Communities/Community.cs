using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Communities.Forum;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

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
        public string Url => $"{Constants.ROBLOX_URL}/communities/{Id}";

        /// <inheritdoc/>
        public AssetOwnerType OwnerType => AssetOwnerType.Community;

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

        private bool isLocked = false;

        /// <summary>
        /// Gets whether or not this group has been locked.
        /// </summary>
        public bool IsLocked => isLocked;

        private bool isPending;
        private bool isMember;
        private bool isPrimary;

        /// <summary>
        /// Gets whether or not the authenticated user has a pending request to join this community.
        /// </summary>
        public bool IsPending => isPending;

        /// <summary>
        /// Gets whether or not the authenticated user is in the community.
        /// </summary>
        public bool IsMember => isMember;

        /// <summary>
        /// Gets whether or not this community is the authenticated user's primary community.
        /// </summary>
        public bool IsPrimary => isPrimary;


        private RoleManager roleManager;

        private MemberManager memberManager;

        private CommunityForum forum;

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
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public static async Task<Community> FromId(ulong communityId, Session? session = null)
        {
            if (RoPool<Community>.Contains(communityId))
                return RoPool<Community>.Get(communityId, session.Global());

            Community newCommunity = new(communityId, session.Global());
            await newCommunity.RefreshAsync();

            return newCommunity;
        }

        /// <summary>
        /// Returns a <see cref="Community"/> given its name.
        /// </summary>
        /// <param name="communityName">The community name.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="Community"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the community name is invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public static async Task<Community> FromName(string communityName, Session? session = null)
        {
            ulong? groupId = await CommunityUtility.GetCommunityIdAsync(communityName);
            if (!groupId.HasValue)
                throw new ArgumentException($"Invalid group name '{communityName}'.");

            return await FromId(groupId.Value, session);
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
            await Task.CompletedTask;

            memberManager ??= new MemberManager(this);
            return memberManager;
        }

        /// <summary>
        /// Retrieves a <see cref="CommunityForum"/> representing this community's forum.
        /// </summary>
        /// <returns>A task containing a <see cref="CommunityForum"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>Please note: Community forums are a Roblox beta feature, and very few groups have them. This method will throw a <see cref="RobloxAPIException"/> if the specified community does not have access to this feature yet. Keep an eye out for the official release of this feature!</remarks>
        public async Task<CommunityForum> GetForumAsync()
        {
            if (forum == null)
            {
                HttpMessage message = new(HttpMethod.Get, $"/v1/groups/{Id}/forums")
                {
                    SilenceExceptions = true,
                };
                var response = await SendAsync(message, Constants.URL("groups"));
                if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    throw new RobloxAPIException($"The group '{Name}' (ID {Id}) does not have access to the community forums yet.");
                }
                forum = new(this);
            }
            return forum;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"/v1/groups/{Id}");
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

            if (data.isLocked != null && data.isLocked == true)
                isLocked = true;
            else
                isLocked = false;

            HttpMessage currencyMessage = new(HttpMethod.Get, $"/v1/groups/{Id}/currency")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(RefreshAsync) + "-GetCurrency",
                SilenceExceptions = true,
            };
            HttpResponseMessage currencyResponse = await SendAsync(currencyMessage, Constants.URL("economy"));
            if (currencyResponse.IsSuccessStatusCode)
            {
                dynamic robuxData = JObject.Parse(await currencyResponse.Content.ReadAsStringAsync());
                robux = robuxData.robux;
            }
            else
            {
                robux = -1;
            }

            // Membership
            HttpMessage membershipMessage = new(HttpMethod.Get, $"/v1/groups/{Id}/membership")
            {
                AuthType = AuthType.None,
                ApiName = nameof(RefreshAsync)+"-GetMembership",
                SilenceExceptions = true,
            };
            HttpResponseMessage membershipResponse = await SendAsync(membershipMessage);
            if (currencyResponse.IsSuccessStatusCode)
            {
                dynamic data7 = JObject.Parse(await membershipResponse.Content.ReadAsStringAsync());
                isMember = data7.userRole.role.rank != 0;
                isPending = data7.isPendingJoin;
                isPrimary = data7.isPrimary;
            }

            // Reset properties
            shout = null;
            shoutGuilded = null;
            socialChannels = null;

            if (roleManager != null)
                await roleManager.RefreshAsync();

            RefreshedAt = DateTime.Now;
        }

        private HttpResult<CommunityShout>? shout;

        /// <summary>
        /// Gets the community's current shout.
        /// </summary>
        /// <returns>A task containing a <see cref="CommunityShout"/> representing the shout upon completion. Can be <see langword="null"/> if there is no current shout.</returns>
        public async Task<HttpResult<CommunityShout>?> GetShoutAsync()
        {
            if (shout == null)
            {
                var response = await SendAsync(HttpMethod.Get, $"/v1/groups/{Id}");
                string rawData = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(rawData);
                if (data.shout != null)
                {
                    ulong posterId = Convert.ToUInt64(data.shout.poster.userId);

                    shout = new(response, new CommunityShout
                    {
                        Text = data.shout.body,
                        Poster = await User.FromId(posterId, session),
                        PostedAt = data.shout.updated,

                        community = this,
                    });
                }
            }
            return shout;
        }

        private HttpResult<GuildedShout>? shoutGuilded;

        /// <summary>
        /// Gets the community's guilded shout, if it has a Guilded server linked.
        /// </summary>
        /// <returns>A task containing a <see cref="GuildedShout"/> if the community has one.</returns>
        public async Task<HttpResult<GuildedShout>?> GetGuildedShoutAsync()
        {
            if (shoutGuilded == null)
            {
                HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"/community-links/v1/groups/{Id}/shout", Constants.URL("apis"));
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    string rawData = await response.Content.ReadAsStringAsync();
                    dynamic data = JObject.Parse(rawData);
                    ulong posterId = Convert.ToUInt64(data.createdBy);

                    shoutGuilded = new(response, new GuildedShout
                    {
                        GuildedId = data.communityId,
                        ShoutId = data.announcementId,
                        ShoutChannelId = data.announcementChannelId,
                        Title = data.title,
                        Content = data.content,
                        ImageUrl = data.imageURL,
                        LikeCount = data.likeCount,
                        Poster = await User.FromId(posterId, session),
                        PostedAt = data.createdAt,
                        UpdatedAt = data.updatedAt,
                        ReactionsVisible = data.areReactionCountsVisible,
                    });
                }
            }

            return shoutGuilded;
        }

        private ReadOnlyDictionary<string, string>? socialChannels;

        /// <summary>
        /// Gets this community's social channels.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> upon completion. The key is the name of the social media platform, and the value is its URL.</returns>
        /// <exception cref="InvalidOperationException">Group is locked.</exception>
        /// <remarks>This method will throw an <see cref="InvalidOperationException"/> if <see cref="IsLocked"/> is <see langword="true"/>.</remarks>
        public async Task<ReadOnlyDictionary<string, string>> GetSocialChannelsAsync()
        {
            if (IsLocked)
                throw new InvalidOperationException("Group is locked.");

            if (socialChannels == null)
            {
                Dictionary<string, string> dict = [];
                var response = await SendAsync(HttpMethod.Get, $"/v1/groups/{Id}/social-links");
                string rawData = await response.Content.ReadAsStringAsync();
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
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<string>> GetIconAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/groups/icons?groupIds={Id}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            var response = await SendAsync(HttpMethod.Get, url, Constants.URL("thumbnails"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new UnreachableException("Invalid group to get icon for.");
            return new(response, Convert.ToString(data.data[0].imageUrl));
        }

        /// <summary>
        /// Modifies the community's description.
        /// </summary>
        /// <param name="text">The new community's description.</param>
        /// <returns>Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        [Obsolete("Use ModifyAsync")]
        public async Task ModifyDescriptionAsync(string text)
            => await ModifyAsync(new() { Description = text });

        /// <summary>
        /// Sets the community shout. Providing <see langword="null"/> as the text will clear the shout.
        /// </summary>
        /// <param name="text">The text for the shout, or <see langword="null"/> to clear the shout.</param>
        /// <returns>Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ShoutAsync(string? text)
        {
            var message = new HttpMessage(HttpMethod.Patch, $"/v1/groups/{Id}/status", new { message = text ?? string.Empty })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ShoutAsync)
            };
            return new(await SendAsync(message));
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
        public async Task<HttpResult<PageResponse<Id<Experience>>>> GetExperiencesAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/groups/{Id}/games?accessFilter=Public&limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<Id<Experience>>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, url, Constants.URL("games"));

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic exp in data.data)
            {
                list.Add(new Id<Experience>(Convert.ToUInt64(exp.id), session));
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(response, new(list, nextPage, previousPage));
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
        public async Task<HttpResult<PageResponse<CommunityAuditLog>>> GetAuditLogsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v1/groups/{Id}/audit-log?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetAuditLogsAsync)
            };
            
            var response = await SendAsync(message);
            string rawData = await response.Content.ReadAsStringAsync();
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

            return new(response, new PageResponse<CommunityAuditLog>(list, nextPage, previousPage));
        }

        /// <summary>
        /// Gets this community's allies.
        /// </summary>
        /// <param name="limit">The maximum amount of communities to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        [Obsolete("Use GetRelationshipsAsync()")]
        public async Task<HttpResult<PageResponse<Id<Community>>>> GetAlliesAsync(int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
            => await GetRelationshipsAsync(CommunityRelationship.Allies, limit, sortOrder, startRowIndex);

        /// <summary>
        /// Gets this community's allies.
        /// </summary>
        /// <param name="limit">The maximum amount of communities to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        [Obsolete("Use GetRelationshipsAsync()")]
        public async Task<HttpResult<PageResponse<Id<Community>>>> GetEnemiesAsync(int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
            => await GetRelationshipsAsync(CommunityRelationship.Enemies, limit, sortOrder, startRowIndex);

        /// <summary>
        /// Gets communities that are in a relationship (allies or enemies) with this community.
        /// </summary>
        /// <param name="relationshipType">The relationship type.</param>
        /// <param name="limit">The maximum amount of communities to return.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="startRowIndex">The amount of items to skip before returning data, or <c>0</c> to skip none.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        public async Task<HttpResult<PageResponse<Id<Community>>>> GetRelationshipsAsync(CommunityRelationship relationshipType, int limit = 50, RequestSortOrder sortOrder = RequestSortOrder.Desc, int startRowIndex = 0)
        {
            string url = $"/v1/groups/{Id}/relationships/{relationshipType.ToString().ToLower()}?maxRows={limit}&sortOrder={sortOrder}&startRowIndex={startRowIndex}";

            var response = await SendAsync(HttpMethod.Get, url);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            List<Id<Community>> list = [];
            string? nextPage = Convert.ToString(data.nextRowIndex);

            foreach (dynamic item in data.relatedGroups)
            {
                list.Add(new(Convert.ToUInt64(item.id), session));
            }

            return new(response, new PageResponse<Id<Community>>(list, nextPage, null));
        }

        /// <summary>
        /// Sends a relationship request from this community to another.
        /// </summary>
        /// <param name="communityId">The Id of the community to send the request to.</param>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> SendRelationshipRequest(ulong communityId, CommunityRelationship relationshipType = CommunityRelationship.Allies)
        {
            string url = $"/v1/groups/{Id}/relationships/{relationshipType.ToString().ToLower()}/{communityId}";
            return new(await SendAsync(HttpMethod.Post, url, body: new { }));
        }

        /// <summary>
        /// Sends a relationship request from this community to another.
        /// </summary>
        /// <param name="community">The community to send the request to.</param>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> SendRelationshipRequest(Community community, CommunityRelationship relationshipType = CommunityRelationship.Allies)
            => new(await SendRelationshipRequest(community.Id, relationshipType));

        /// <summary>
        /// Removes a relationship between this community and another.
        /// </summary>
        /// <param name="communityId">The Id of the community whose relationship to delete.</param>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> RemoveRelationshipAsync(ulong communityId, CommunityRelationship relationshipType = CommunityRelationship.Allies)
        {
            string url = $"/v1/groups/{Id}/relationships/{relationshipType.ToString().ToLower()}/{communityId}";
            return new(await SendAsync(HttpMethod.Delete, url, body: new { }));
        }

        /// <summary>
        /// Removes a relationship between this community and another.
        /// </summary>
        /// <param name="community">The community whose relationship to delete.</param>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> RemoveRelationshipAsync(Community community, CommunityRelationship relationshipType = CommunityRelationship.Allies)
            => new(await RemoveRelationshipAsync(community.Id, relationshipType));

        /// <summary>
        /// Modifies a relationship request to this group.
        /// </summary>
        /// <param name="communityId">The Id of the community whose request to modify.</param>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <param name="action">Whether to accept or decline the relationship request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> ModifyRelationshipRequest(ulong communityId, JoinRequestAction action, CommunityRelationship relationshipType = CommunityRelationship.Allies)
        {
            string url = $"/v1/groups/{Id}/relationships/{relationshipType.ToString().ToLower()}/requests/{communityId}";

            var message = new HttpMessage(action switch
            {
                JoinRequestAction.Accept => HttpMethod.Post,
                JoinRequestAction.Decline => HttpMethod.Delete,
                _ => throw new UnreachableException("JoinRequestAction must be Accept or Decline.")
            }, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyRelationshipRequest)
            };

            return new(await SendAsync(message));
        }

        /// <summary>
        /// Modifies a relationship request to this group.
        /// </summary>
        /// <param name="community">The community whose request to modify.</param>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <param name="action">Whether to accept or decline the relationship request.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> ModifyRelationshipRequest(Community community, JoinRequestAction action, CommunityRelationship relationshipType = CommunityRelationship.Allies)
            => new(await ModifyRelationshipRequest(community.Id, action, relationshipType));

        /// <summary>
        /// Gets this community's income statistics for the given <paramref name="timeLength"/>.
        /// </summary>
        /// <param name="timeLength">The length of time to use for the breakdown.</param>
        /// <returns>A task containing a <see cref="EconomyBreakdown"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<EconomyBreakdown>> GetIncomeAsync(AnalyticTimeLength timeLength = AnalyticTimeLength.Day)
        {
            var url = $"/v1/groups/{Id}/revenue/summary/{timeLength.ToString().ToLower()}";
            var message = new HttpMessage(HttpMethod.Delete, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetIncomeAsync)
            };

            var response = await SendAsync(message, Constants.URL("economy"));
            string rawData = await response.Content.ReadAsStringAsync();
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

            return new(response, new EconomyBreakdown(timeLength, amount, breakdown, pending));
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
        public async Task<HttpResult<PageResponse<CommunityPost>>> GetPostsAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Desc, string? cursor = null)
        {
            string url = $"/v2/groups/{Id}/wall/posts?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetPostsAsync)
            };

            var list = new List<CommunityPost>();
            string? nextPage;
            string? previousPage;
            HttpResponseMessage response = await SendAsync(message);

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            foreach (dynamic post in data.data)
            {
                list.Add(new CommunityPost()
                {
                    PostId = post.id,
                    PostedAt = post.updated,
                    Text = post.body,
                    RankInCommunity = post.poster == null ? null : post.poster.role.name,
                    PosterId = post.poster == null ? null : new Id<User>(Convert.ToUInt64(post.poster.user.userId)),

                    group = this,
                });
            }
            nextPage = data.nextPageCursor;
            previousPage = data.previousPageCursor;

            return new(response, new(list, nextPage, previousPage));
        }

        /// <summary>
        /// Delete all wall posts from the given user.
        /// </summary>
        /// <param name="userId">The user Id of the posts to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> DeletePostsFromMemberAsync(ulong userId)
        {
            var message = new HttpMessage(HttpMethod.Delete, $"/v1/groups/{Id}/wall/users/{userId}/posts")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(DeletePostsFromMemberAsync)
            };

            return new(await SendAsync(message));
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
        /// Delete all wall posts from the given user.
        /// </summary>
        /// <param name="username">The user name of the posts to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> DeletePostsFromMemberAsync(string username)
            => new(await DeletePostsFromMemberAsync(await UserUtility.GetUserIdAsync(username)));

        /// <summary>
        /// Deletes the wall post with the given Id.
        /// </summary>
        /// <param name="postId">The post Id to delete. Can be obtained from <see cref="GetPostsAsync(FixedLimit, RequestSortOrder, string?)"/>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> DeletePostAsync(ulong postId)
        {
            var message = new HttpMessage(HttpMethod.Delete, $"/v1/groups/{Id}/wall/posts/{postId}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(DeletePostAsync)
            };

            return new(await SendAsync(message));
        }

        /// <summary>
        /// Deletes the specified wall post. Convenience method, equivalent to calling <see cref="DeletePostAsync(ulong)"/> with the <see cref="CommunityPost"/>'s <see cref="CommunityPost.PostId"/>.
        /// </summary>
        /// <param name="post">The post to delete. Can be obtained from <see cref="GetPostsAsync(FixedLimit, RequestSortOrder, string?)"/>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> DeletePostAsync(CommunityPost post)
            => new(await DeletePostAsync(post.PostId));

        /// <summary>
        /// Modifies the community.
        /// </summary>
        /// <param name="options">The options to use.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> ModifyAsync(CommunityModifyOptions options)
        {
            var message = new HttpMessage(HttpMethod.Patch, $"/v1/groups/{Id}/settings", new
            {
                isApprovalRequired = options.ManualApproval,
                areGroupGamesVisible = options.GamesVisible,
                areEnemiesAllowed = options.EnemyDeclarations,
                areGroupFundsVisible = options.FundsVisible,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyAsync),
            };

            var res = await SendAsync(message);

            // Todo: Separate description to its own method
            if (!string.IsNullOrWhiteSpace(options.Description))
            {
                var message2 = new HttpMessage(HttpMethod.Patch, $"/v1/groups/{Id}/description", new { description = options.Description })
                {
                    AuthType = AuthType.RobloSecurity,
                    ApiName = nameof(ModifyDescriptionAsync),
                };
                await SendAsync(message2);
            }
            return new(res);
        }

        /// <summary>
        /// Leaves the community if the authenticated user is in it.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> LeaveAsync()
        {
            HttpMessage payload = new(HttpMethod.Delete, $"/v1/groups/{Id}/users/{session?.AuthUser?.Id}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(LeaveAsync),
            };
            return new(await SendAsync(payload));
        }

        /// <summary>
        /// Gets blocked keywords in this community.
        /// </summary>
        /// <param name="limit">The maximum amount of words to return.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="CommunityBlockedWord"/> upon completion.</returns>
        public async Task<HttpResult<PageResponse<CommunityBlockedWord>>> GetBlockedWordsAsync(FixedLimit limit = FixedLimit.Limit50, string? cursor = null)
        {
            string url = $"/v1/groups/{Id}/blocked-keywords?limit={limit.Limit()}";
            if (cursor != null)
                url += $"&cursor={cursor}";

            var response = await SendAsync(HttpMethod.Get, url);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            List<CommunityBlockedWord> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                CommunityBlockedWord word = new()
                {
                    Id = item.id,
                    Keyword = item.keyword,
                    UserId = new(Convert.ToUInt64(item.createdBy), session),
                    IsPrivate = item.isPrivate,
                    Created = item.createdAt,
                    Updated = item.updatedAt,
                };
                list.Add(word);
            }

            return new(response, new(list, nextPage, previousPage));
        }

        /// <summary>
        /// Adds blocked words to a community.
        /// </summary>
        /// <param name="keywords">The words to block.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        // TODO: Return an Enumerable with the response body.
        public async Task<HttpResult> AddBlockedWordsAsync(params string[] keywords)
        {
            HttpMessage payload = new(HttpMethod.Post, $"/v1/groups/{Id}/blocked-keywords", new
            {
                isPrivate = false,
                keywords = string.Join(',', keywords),
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(AddBlockedWordsAsync),
            };
            return new(await SendAsync(payload));
        }

        /// <summary>
        /// Removes a blocked word with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The unique Id of the blocked word.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        public async Task<HttpResult> RemoveBlockedWordAsync(string id)
        {
            HttpMessage payload = new(HttpMethod.Delete, $"/v1/groups/{Id}/blocked-keywords/{id}")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(RemoveBlockedWordAsync),
            };
            return new(await SendAsync(payload));
        }

        /// <summary>
        /// Removes a blocked word.
        /// </summary>
        /// <param name="word">The blocked word to remove.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        public async Task<HttpResult> RemoveBlockedWordAsync(CommunityBlockedWord word)
            => await RemoveBlockedWordAsync(word.Id);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Community {Name} [{Id}] {{{members}}}{(Verified ? " [V]" : string.Empty)}";
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

    /// <summary>
    /// Specifies the options to use for community modification.
    /// </summary>
    /// <remarks>Any property in this class that is not changed will not modify the website.</remarks>
    public class CommunityModifyOptions
    {
        /// <summary>
        /// Gets or sets the new description of the community.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets and sets whether or not manual approval is enabled, which requires joining users to be approved.
        /// </summary>
        public bool? ManualApproval { get; set; }

        /// <summary>
        /// Gets and sets whether or not group funds are publicly visible.
        /// </summary>
        public bool? FundsVisible { get; set; }

        /// <summary>
        /// Gets and sets whether or not group experiences are publicly visible.
        /// </summary>
        public bool? GamesVisible { get; set; }

        /// <summary>
        /// Gets and sets whether or not the group can have and make enemy declarations.
        /// </summary>
        public bool? EnemyDeclarations { get; set; }
    }
}
