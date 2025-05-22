using RoSharp.Enums;
using RoSharp.Structures;
using System.Collections.ObjectModel;

namespace RoSharp.API.Communities.Forum
{
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

        public string Id { get; init; }
        public string ParentId { get; init; }
        public ForumCategory Category { get; init; }
        public string Text { get; init; }
        public Id<User> Poster { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public int ReplyCount { get; init; }

        public ReadOnlyDictionary<ForumEmote, int> Reactions { get; init; }

        public bool IsEdited => Created.Ticks != Updated.Ticks;

        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        public async Task ReactAsync(ForumEmote emote)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/channels/{ParentId}/comments/{Id}/reactions/{emote.Id}";
            HttpMessage message = new(HttpMethod.Post, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ReactAsync),
            };
            await manager.community.SendAsync(message, Constants.URL("groups"));
        }

        public async Task ReactAsync(string reactionName)
        {
            ForumEmote? emote = await manager.GetEmoteAsync(reactionName);
            if (emote != null)
            {
                await ReactAsync(emote);
                return;
            }
            throw new ArgumentException("Invalid emote name.", nameof(reactionName));
        }

        public async Task RemoveReactionAsync(ForumEmote emote)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/channels/{ParentId}/comments/{Id}/reactions/{emote.Id}";
            HttpMessage message = new(HttpMethod.Delete, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ReactAsync),
            };
            await manager.community.SendAsync(message, Constants.URL("groups"));
        }

        public async Task RemoveReactionAsync(string reactionName)
        {
            ForumEmote? emote = await manager.GetEmoteAsync(reactionName);
            if (emote != null)
            {
                await RemoveReactionAsync(emote);
                return;
            }
            throw new ArgumentException("Invalid emote name.", nameof(reactionName));
        }

        // Todo: Implement when I figure out how the cursor system for forum comment replies works
        /*
        public async Task<PageResponse<ForumComment>> GetRepliesAsync(string? cursor = null)
        {
            return null;
        }*/

    }
}
