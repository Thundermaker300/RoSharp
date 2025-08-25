using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Structures;

namespace RoSharp.API.Communities.Forum
{
    /// <summary>
    /// Represents a category within a community forum.
    /// </summary>
    public sealed class ForumCategory
    {
        internal CommunityForum manager;


        /// <summary>
        /// Gets the unique Id of the category.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the category.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the Id of the user that created the category.
        /// </summary>
        public Id<User> Creator { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing when the category was created.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing when the category was updated.
        /// </summary>
        public DateTime Updated { get; init; }

        /// <summary>
        /// Gets whether or not the category is archived.
        /// </summary>
        public bool IsArchived { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing when the category was archived.
        /// </summary>
        public DateTime? ArchivedAt { get; init; }

        /// <summary>
        /// Gets the Id of the user that archived the category.
        /// </summary>
        public Id<User>? ArchivedBy { get; init; }

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

        /// <summary>
        /// Renames the category.
        /// </summary>
        /// <param name="newName">The new name for the category.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> RenameAsync(string newName)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{Id}";
            HttpMessage message = new(HttpMethod.Patch, url, new
            {
                name = newName,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(RenameAsync),
            };
            return new(await manager.community.SendAsync(message, Constants.URL("groups")));
        }

        /// <summary>
        /// Toggles the archived state of the category.
        /// </summary>
        /// <param name="archived">Whether or not the category should be archived.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <seealso cref="DeleteAsync"/>
        public async Task<HttpResult> SetArchivedAsync(bool archived)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{Id}/archive";
            HttpMessage message = new(HttpMethod.Patch, url, new
            {
                isArchived = archived,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(SetArchivedAsync),
            };
            return new(await manager.community.SendAsync(message, Constants.URL("groups")));
        }

        /// <summary>
        /// Deletes the category and all of its posts. THIS IS NOT REVERSIBLE!!
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <seealso cref="SetArchivedAsync(bool)"/>
        public async Task<HttpResult> DeleteAsync()
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{Id}";
            HttpMessage message = new(HttpMethod.Delete, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(DeleteAsync),
            };
            return new(await manager.community.SendAsync(message, Constants.URL("groups")));
        }

        /// <summary>
        /// Gets posts within the category.
        /// </summary>
        /// <param name="limit">The maximum amount of posts to get.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="ForumPost"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<ForumPost>>> GetPostsAsync(FixedLimit limit = FixedLimit.Limit10, string? cursor = null)
        {
            string url = $"/v1/groups/{manager.community.Id}/forums/{Id}/posts?includeCommentCount=true&limit={limit.Limit()}";
            if (cursor is not null)
                url += $"&cursor={cursor}";

            var response = await manager.community.SendAsync(HttpMethod.Get, url, Constants.URL("groups"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            string? next = data.nextPageCursor;
            string? previous = data.previousPageCursor;

            List<ForumPost> comments = [];

            foreach (var comment in data.data)
            {
                comments.Add(await ConstructPost(comment));
            }

            return new(response, new(comments, next, previous));
        }

        /// <summary>
        /// Gets a specific post by Id. Can be <see langword="null"/>.
        /// </summary>
        /// <param name="postId">The unique Id of the post.</param>
        /// <returns>A task containing a <see cref="ForumPost"/> upon completion. Can be <see langword="null"/> if no matches are found.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<ForumPost?>> GetPostAsync(string postId)
        {
            var response = await manager.community.SendAsync(HttpMethod.Get, $"/v1/groups/{manager.community.Id}/forums/{Id}/posts?postIds={postId}");
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            if (data.data.Count > 0)
            {
                var comment = data.data[0];
                return new(response, await ConstructPost(comment));
            }

            return new(response, null);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ForumCategory {Name} [{Id}] {{COMMUNITY:{manager.community.Id}}}";
        }
    }
}
