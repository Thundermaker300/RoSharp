using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using RoSharp.Http;
using System.Collections.ObjectModel;

namespace RoSharp.API.Communities.Forum
{
    /// <summary>
    /// Represents a <see cref="Communities.Community"/>'s public community forum.
    /// </summary>
    public sealed class CommunityForum
    {
        internal Community community;

        private List<ForumEmote> emotes;

        internal CommunityForum(Community community) { this.community = community; }


        private async Task<List<ForumCategory>> getAllCategories()
        {
            List<ForumCategory> categories = [];

            string? nextPage = string.Empty;
            while (nextPage != null)
            {
                var response = await GetCategoriesAsync(nextPage);

                foreach (ForumCategory cat in response)
                {
                    categories.Add(cat);
                }

                nextPage = response.Value.NextPageCursor;
            }

            return categories;
        }

        /// <summary>
        /// Gets this forum's emote list.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="ForumEmote"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<ReadOnlyCollection<ForumEmote>> GetEmotesAsync()
        {
            if (emotes == null)
            {
                string rawData = await community.SendStringAsync(HttpMethod.Get, $"/v1/groups/{community.Id}/emotes", Constants.URL("groups"));
                dynamic data = JObject.Parse(rawData);

                List<ForumEmote> list = [];
                foreach (dynamic set in data.emoteSets)
                {
                    foreach (dynamic emote in set.emotes)
                    {
                        list.Add(new ForumEmote()
                        {
                            Id = emote.id,
                            Name = emote.name,

                            SetId = set.id,
                            SetName = set.name,
                        });
                    }
                }

                emotes = list;
            }
            return emotes.AsReadOnly();
        }

        /// <summary>
        /// Gets a specified <see cref="ForumEmote"/> by name. Can be <see langword="null"/>.
        /// </summary>
        /// <param name="name">The name of the emote.</param>
        /// <returns>A task containing a <see cref="ForumEmote"/> upon completion. Will be <see langword="null"/> if no emote with the given <paramref name="name"/> matches.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<ForumEmote?> GetEmoteAsync(string name)
        {
            var emotes = await GetEmotesAsync();
            return emotes.FirstOrDefault(emote => emote.Name.Equals(name));
        }

        /// <summary>
        /// Gets a specified <see cref="ForumEmote"/> by internal ID. Can be <see langword="null"/>.
        /// </summary>
        /// <param name="id">The ID of the emote.</param>
        /// <returns>A task containing a <see cref="ForumEmote"/> upon completion. Will be <see langword="null"/> if no emote with the given <paramref name="id"/> matches.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<ForumEmote?> GetEmoteByIdAsync(string id)
        {
            var emotes = await GetEmotesAsync();
            return emotes.FirstOrDefault(emote => emote.Id.Equals(id));
        }

        /// <summary>
        /// Gets the categories contained in this forum.
        /// </summary>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="ForumCategory"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<EnumerableHttpResult<PageResponse<ForumCategory>>> GetCategoriesAsync(string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/forums";
            if (cursor is not null)
                url += $"?cursor={cursor}";

            var response = await community.SendAsync(HttpMethod.Get, url, Constants.URL("groups"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            string? next = data.nextPageCursor;
            string? previous = data.previousPageCursor;

            List<ForumCategory> categories = [];

            foreach (dynamic cat in data.data)
            {
                categories.Add(new ForumCategory
                {
                    Id = cat.id,
                    Name = cat.name,
                    Description = cat.description,
                    Creator = new(Convert.ToUInt64(cat.createdBy), community.session),
                    Created = cat.createdAt,
                    Updated = cat.updatedAt,

                    manager = this
                });
            }

            return new(response, new(categories, next, previous));
        }

        /// <summary>
        /// Gets a <see cref="ForumCategory"/> by name. Can be <see langword="null"/>.
        /// </summary>
        /// <param name="categoryName">The name of the category.</param>
        /// <param name="comparisonType">The comparison method to use when comparing the <paramref name="categoryName"/> with the actual name of the category.</param>
        /// <returns>A task containing a <see cref="ForumCategory"/> upon completion. Will be <see langword="null"/> if no matches were found.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<ForumCategory?> GetCategoryAsync(string categoryName, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            List<ForumCategory> cats = await getAllCategories();
            foreach (ForumCategory cat in cats)
            {
                if (cat.Name.Equals(categoryName, comparisonType))
                {
                    return cat;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a <see cref="ForumCategory"/> by internal ID. Can be <see langword="null"/>.
        /// </summary>
        /// <param name="categoryId">The internal ID of the category.</param>
        /// <returns>A task containing a <see cref="ForumCategory"/> upon completion. Will be <see langword="null"/> if no matches were found.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<ForumCategory?> GetCategoryByIdAsync(string categoryId)
        {
            List<ForumCategory> cats = await getAllCategories();
            foreach (ForumCategory cat in cats)
            {
                if (cat.Id.Equals(categoryId))
                {
                    return cat;
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"CommunityForum {{COMMUNITY:{community.Id}}}";
        }
    }
}
