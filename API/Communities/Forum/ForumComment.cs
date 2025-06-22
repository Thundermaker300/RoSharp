using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Http;
using RoSharp.Structures;
using System.Collections.ObjectModel;

namespace RoSharp.API.Communities.Forum
{
    /// <summary>
    /// Represents an individual comment left by a single user. This can be the main comment of a <see cref="ForumPost"/> (the comment that started the post), or a reply to a post or any of its comments.
    /// </summary>
    public sealed class ForumComment
    {
        internal static async Task<ForumComment> Construct(CommunityForum manager, ForumCategory category, dynamic data)
        {
            Dictionary<ForumEmote, int> reactions = [];

            foreach (dynamic emoteData in data.reactions)
            {
                var emote = await manager.GetEmoteByIdAsync(Convert.ToString(emoteData.emoteId));
                reactions.Add(emote, Convert.ToInt32(emoteData.reactionCount));
            }

            return new ForumComment
            {
                Id = data.id,
                ParentId = data.parentId,
                Category = category,
                Text = data.content.plainText,
                Poster = new(Convert.ToUInt64(data.createdBy), manager.community.session),
                Created = data.createdAt,
                Updated = data.updatedAt,
                ReplyCount = data.threadCommentCount == null ? 0 : Convert.ToUInt64(data.threadCommentCount),
                Reactions = reactions.AsReadOnly(),
                
                manager = manager
            };
        }

        internal CommunityForum manager;

        /// <summary>
        /// Gets the unique Id of the comment.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the unique Id of the parent comment.
        /// </summary>
        public string ParentId { get; init; }

        /// <summary>
        /// Gets the category this comment was posted in.
        /// </summary>
        public ForumCategory Category { get; init; }

        /// <summary>
        /// Gets the text of the comment.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets the Id of the poster.
        /// </summary>
        public Id<User> Poster { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the creation date of the comment.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the date this comment was updated last.
        /// </summary>
        public DateTime Updated { get; init; }

        /// <summary>
        /// Gets the amount of replies to this comment.
        /// </summary>
        public int ReplyCount { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> of <see cref="ForumEmote"/> and the amount of times they were reacted with on this comment.
        /// </summary>
        public ReadOnlyDictionary<ForumEmote, int> Reactions { get; init; }

        /// <summary>
        /// Gets whether or not this comment has been edited at least once from its original post content.
        /// </summary>
        public bool IsEdited => Created.Ticks != Updated.Ticks;

        /// <summary>
        /// Gets whether or not this comment is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        /// <summary>
        /// Reacts to the comment as the authenticated user.
        /// </summary>
        /// <param name="emote">The emote to react with.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ReactAsync(ForumEmote emote)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/channels/{ParentId}/comments/{Id}/reactions/{emote.Id}";
            HttpMessage message = new(HttpMethod.Post, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ReactAsync),
            };
            return new(await manager.community.SendAsync(message, Constants.URL("groups")));
        }

        /// <summary>
        /// Reacts to the comment as the authenticated user.
        /// </summary>
        /// <param name="reactionName">The name of the emote to react with.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ReactAsync(string reactionName)
        {
            ForumEmote? emote = await manager.GetEmoteAsync(reactionName);
            if (emote != null)
            {
                return new(await ReactAsync(emote));
            }
            throw new ArgumentException("Invalid emote name.", nameof(reactionName));
        }

        /// <summary>
        /// Removes a reaction that was previously added by the authenticated user.
        /// </summary>
        /// <param name="emote">The emote to remove.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> RemoveReactionAsync(ForumEmote emote)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/channels/{ParentId}/comments/{Id}/reactions/{emote.Id}";
            HttpMessage message = new(HttpMethod.Delete, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ReactAsync),
            };
            return new(await manager.community.SendAsync(message, Constants.URL("groups")));
        }

        /// <summary>
        /// Removes a reaction that was previously added by the authenticated user.
        /// </summary>
        /// <param name="reactionName">The name of the emote to remove.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> RemoveReactionAsync(string reactionName)
        {
            ForumEmote? emote = await manager.GetEmoteAsync(reactionName);
            if (emote != null)
            {
                return new(await RemoveReactionAsync(emote));
            }
            throw new ArgumentException("Invalid emote name.", nameof(reactionName));
        }

        // Todo: Implement when I figure out how the cursor system for forum comment replies works
        /*
        public async Task<PageResponse<ForumComment>> GetRepliesAsync(string? cursor = null)
        {
            return null;
        }*/

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ForumComment [{Id}] {{COMMUNITY:{manager.community.Id}}} <CREATOR:{Poster.UniqueId}> || {Text}";
        }

    }
}
