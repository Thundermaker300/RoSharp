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
                comments.Add(new ForumPost
                {
                    Id = comment.id,
                    CategoryId = comment.categoryId,
                    Category = this,
                    Title = comment.name,
                    Comments = comment.commentCount - 1,
                    IsPinned = comment.isPinned,
                    IsLocked = comment.isLocked,
                    MainComment = ForumComment.Construct(this.manager, this, comment.firstComment),
                    
                    manager = this.manager
                });
            }

            return new(comments, next, previous);
        }
    }
}
