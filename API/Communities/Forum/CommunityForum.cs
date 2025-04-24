using Newtonsoft.Json.Linq;

namespace RoSharp.API.Communities.Forum
{
    public sealed class CommunityForum
    {
        internal Community community;

        internal CommunityForum(Community community) { this.community = community; }

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
    }
}
