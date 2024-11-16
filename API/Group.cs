using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Misc;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RoSharp.API
{
    public class Group : APIMain, IRefreshable, IAssetOwner, IIdApi<Group>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("groups");

        /// <summary>
        /// Gets the unique Id of the group.
        /// </summary>
        public ulong Id { get; }

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


        private RoleManager roleManager;

        /// <summary>
        /// Gets a <see cref="API.RoleManager"/> class that has additional API to manage group roles.
        /// </summary>
        public RoleManager RoleManager => roleManager;

        private MemberManager memberManager;

        /// <summary>
        /// Gets a <see cref="API.MemberManager"/> class that has additional API to manage group members.
        /// </summary>
        public MemberManager MemberManager => memberManager ?? new MemberManager(this);

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

        public static async Task<Group> FromId(ulong groupId, Session? session = null)
        {
            if (RoPool<Group>.Contains(groupId))
                return RoPool<Group>.Get(groupId, session);

            Group newGroup = new(groupId, session);
            await newGroup.RefreshAsync();

            newGroup.roleManager = new RoleManager(newGroup);
            await newGroup.roleManager.RefreshAsync();

            return newGroup;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await GetAsync($"/v1/groups/{Id}");
            if (response.IsSuccessStatusCode)
            {
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
            }
            else
            {
                throw new ArgumentException($"Invalid group ID '{Id}'. HTTP {response.StatusCode}");
            }

            // Reset properties
            shout = null;
            socialChannels = null;

            RefreshedAt = DateTime.Now;
        }

        private GroupShoutInfo? shout;
        public async Task<GroupShoutInfo?> GetShoutAsync()
        {
            if (shout == null)
            {
                string rawData = await GetStringAsync($"/v1/groups/{Id}");
                dynamic data = JObject.Parse(rawData);
                if (data.shout != null)
                {
                    ulong posterId = Convert.ToUInt64(data.shout.poster.userId);

                    shout = new GroupShoutInfo
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
            if (response.IsSuccessStatusCode)
            {
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
            }

            return new(list, nextPage, previousPage);
        }

        [UsesSession]
        public async Task<PageResponse<GenericId<User>>> GetMembersAsync(FixedLimit limit = FixedLimit.Limit100, RequestSortOrder sortOrder = RequestSortOrder.Asc, string? cursor = null)
        {
            string url = $"/v1/groups/{Id}/users?limit={limit.Limit()}&sortOrder={sortOrder}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            var list = new List<GenericId<User>>();
            string? nextPage = null;
            string? previousPage = null;
            HttpResponseMessage response = await GetAsync(url, verifyApiName: "Group.GetMembersAsync");
            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (dynamic user in data.data)
                {
                    ulong userId = Convert.ToUInt64(user.user.userId);
                    list.Add(new GenericId<User>(userId, session));
                }
                nextPage = data.nextPageCursor;
                previousPage = data.previousPageCursor;
            }

            return new(list, nextPage, previousPage);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} [{Id}] {(Verified ? "[V]" : string.Empty)}";
        }

        public Group AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    /// <summary>
    /// Contains info regarding a group shout.
    /// </summary>
    public class GroupShoutInfo
    {
        /// <summary>
        /// Gets the text of the shout.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets the poster of the shout.
        /// </summary>
        public User Poster { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the shout was posted.
        /// </summary>
        public DateTime PostedAt { get; init; }
    }

    /// <summary>
    /// Represents a group post.
    /// </summary>
    public class GroupPost
    {
        internal Group group;

        /// <summary>
        /// Gets the unique Id of the post.
        /// </summary>
        public ulong PostId { get; init; }

        /// <summary>
        /// Gets the unique Id of the poster. Can be <see langword="null"/>.
        /// </summary>
        public GenericId<User>? PosterId { get; init; }

        /// <summary>
        /// Gets the text of the post.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the date the post was created.
        /// </summary>
        public DateTime PostedAt { get; init; }

        /// <summary>
        /// Gets the rank of the user that made the post. Can be <see langword="null"/>.
        /// </summary>
        public string? RankInGroup { get; init; }
    }
}
