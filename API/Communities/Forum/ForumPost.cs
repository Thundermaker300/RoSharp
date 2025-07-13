using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;

namespace RoSharp.API.Communities.Forum
{
    /// <summary>
    /// Represents a community forum post. A post consists of a main <see cref="ForumComment"/> with a title that users can reply to. All <see cref="ForumComment"/>s belong to no more than one <see cref="ForumPost"/>.
    /// </summary>
    public sealed class ForumPost
    {
        internal CommunityForum manager;

        /// <summary>
        /// Gets the unique Id of the post.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the unique Id of the category this post is posted in.
        /// </summary>
        public string CategoryId { get; init; }

        /// <summary>
        /// Gets a <see cref="ForumCategory"/> representing the category that this post is posted in.
        /// </summary>
        public ForumCategory Category { get; init; }

        /// <summary>
        /// Gets the title of the post.
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Gets the amount of <see cref="ForumComment"/>s replying to this post.
        /// </summary>
        public int Comments { get; init; }

        /// <summary>
        /// Gets whether or not the post is pinned in the category.
        /// </summary>
        public bool IsPinned { get; init; }

        /// <summary>
        /// Gets whether or not the post is locked (still visible but no more replies allowed).
        /// </summary>
        public bool IsLocked { get; init; }

        /// <summary>
        /// Gets the <see cref="ForumComment"/> associated with this post. This contains the author, text, creation date, etc of the original comment.
        /// </summary>
        public ForumComment MainComment { get; init; }

        /// <summary>
        /// Gets replies to the post.
        /// </summary>
        /// <param name="limit">The maximum amount of comments to return.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="ForumComment"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<ForumComment>>> GetCommentsAsync(FixedLimit limit = FixedLimit.Limit10, string? cursor = null)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{CategoryId}/posts/{Id}/comments?limit={limit.Limit()}";
            if (cursor is not null)
                url += $"&cursor={cursor}";

            var response = await manager.community.SendAsync(HttpMethod.Get, url, Constants.URL("groups"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            string? next = data.nextPageCursor;
            string? previous = data.previousPageCursor;

            List<ForumComment> comments = [];

            foreach (dynamic comment in data.data)
            {
                comments.Add(await ForumComment.Construct(manager, Category, comment));
            }

            return new(response, new(comments, next, previous));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ForumPost {Title} [{Id}] {{COMMUNITY:{manager.community.Id}}} <CREATOR:{MainComment.Poster.UniqueId}>";
        }
    }
}
