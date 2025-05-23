using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Extensions;

namespace RoSharp.API.Communities.Forum
{
    public sealed class ForumCategory
    {
        internal CommunityForum manager;


        public string Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public Id<User> Creator { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }

        // Todo: Other API as they are implemented

        private async Task<ForumPost> ConstructPost(dynamic comment)
        {
            return new ForumPost
            {
                Id = comment.id,
                CategoryId = comment.categoryId,
                Category = this,
                Title = comment.name,
                Comments = comment.commentCount - 1,
                IsPinned = comment.isPinned,
                IsLocked = comment.isLocked,
                MainComment = await ForumComment.Construct(this.manager, this, comment.firstComment),

                manager = this.manager
            };
        }

        public async Task<PageResponse<ForumPost>> GetPostsAsync(FixedLimit limit = FixedLimit.Limit10, string? cursor = null)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{Id}/posts?includeCommentCount=true&limit={limit.Limit()}";
            if (cursor is not null)
                url += $"&cursor={cursor}";

            string rawData = await manager.community.SendStringAsync(HttpMethod.Get, url, Constants.URL("groups"));
            dynamic data = JObject.Parse(rawData);

            string? next = data.nextPageCursor;
            string? previous = data.previousPageCursor;

            List<ForumPost> comments = [];

            foreach (var comment in data.data)
            {
                comments.Add(await ConstructPost(comment));
            }

            return new(comments, next, previous);
        }

        public async Task<ForumPost?> GetPostAsync(string postId)
        {
            string rawData = await manager.community.SendStringAsync(HttpMethod.Get, $"/v1/groups/{manager.community.Id}/forums/{Id}/posts?postIds={postId}");
            dynamic data = JObject.Parse(rawData);

            if (data.data.Count > 0)
            {
                var comment = data.data[0];
                return await ConstructPost(comment);
            }

            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ForumCategory {Name} [{Id}] {{COMMUNITY:{manager.community.Id}}}";
        }
    }
}
