using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace RoSharp.API.Communities.Forum
{
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

                nextPage = response.NextPageCursor;
            }

            return categories;
        }

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
        public async Task<ForumEmote?> GetEmoteAsync(string name)
        {
            var emotes = await GetEmotesAsync();
            return emotes.FirstOrDefault(emote => emote.Name.Equals(name));
        }

        public async Task<ForumEmote?> GetEmoteByIdAsync(string id)
        {
            var emotes = await GetEmotesAsync();
            return emotes.FirstOrDefault(emote => emote.Id.Equals(id));
        }

        public async Task<PageResponse<ForumCategory>> GetCategoriesAsync(string? cursor = null)
        {
            string url = $"/v1/groups/{community.Id}/forums";
            if (cursor is not null)
                url += $"?cursor={cursor}";

            string rawData = await community.SendStringAsync(HttpMethod.Get, url, Constants.URL("groups"));
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

            return new(categories, next, previous);
        }


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
