using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Extensions;

namespace RoSharp.API.Communities.Forum
{
    public sealed class ForumPost
    {
        internal CommunityForum manager;

        public string Id { get; init; }
        public string CategoryId { get; init; }
        public ForumCategory Category { get; init; }
        public string Title { get; init; }
        public int Comments { get; init; }
        public bool IsPinned { get; init; }
        public bool IsLocked { get; init; }
        public ForumComment MainComment { get; init; }

        public async Task<PageResponse<ForumComment>> GetCommentsAsync(FixedLimit limit = FixedLimit.Limit10, string? cursor = null)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{CategoryId}/posts/{Id}/comments?limit={limit.Limit()}";
            if (cursor is not null)
                url += $"&cursor={cursor}";

            string rawData = await manager.community.SendStringAsync(HttpMethod.Get, url, Constants.URL("groups"));
            dynamic data = JObject.Parse(rawData);

            string? next = data.nextPageCursor;
            string? previous = data.previousPageCursor;

            List<ForumComment> comments = [];

            foreach (dynamic comment in data.data)
            {
                comments.Add(await ForumComment.Construct(manager, Category, comment));
            }

            return new(comments, next, previous);
        }
    }
}
