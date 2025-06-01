using Newtonsoft.Json.Linq;
using RoSharp.Http;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Collections.ObjectModel;

namespace RoSharp.API.DevForum
{
    /// <summary>
    /// Read-only API for getting information from the Roblox Developer Forum.
    /// </summary>
    /// <remarks>This API is not built with authentication, so it is read-only.</remarks>
    public class DevForumAPI
    {
        internal static async Task<List<DevForumPost>> ConvertRawData(dynamic data)
        {
            var usernames = new List<string>();
            foreach (dynamic item in data.Children())
            {
                if (item.username != null)
                    usernames.Add(Convert.ToString(item.username));
            }
            var ids = await UserUtility.GetUserIdsAsync(usernames);

            List<DevForumPost> posts = new();
            foreach (dynamic item in data.Children())
            {
                string username = item.username;
                ulong userId = ids[username];
                int likes = 0;

                foreach (dynamic action in item.actions_summary)
                {
                    if (action.id == 2)
                        likes = action.count;
                }

                posts.Add(new()
                {
                    PostId = item.id,
                    PosterName = item.username,
                    Poster = Convert.ToString(item.username) == "system" ? null : new(userId, null),
                    Content = item.cooked, // lol?
                    Created = item.created_at,
                    LastUpdated = item.updated_at,
                    PostNumber = item.post_number,
                    Likes = likes,
                    Reads = item.reads,
                    ReplyCount = item.reply_count,
                    EditVersion = item.version,
                    Hidden = item.hidden,
                    UserTitle = item.user_title,
                    AcceptedAnswer = item.accepted_answer,

                });
            }
            return posts;
        }

        /// <summary>
        /// Returns a topic given its Id.
        /// </summary>
        /// <param name="topicId">The Id of the topic.</param>
        /// <param name="excludeSystemReplies">Exclude system replies within topics.</param>
        /// <returns>A task containing a <see cref="DevForumTopic"/> upon completion.</returns>
        /// <exception cref="ArgumentException">Invalid topic Id.</exception>
        public static async Task<DevForumTopic> GetTopicAsync(ulong topicId, bool excludeSystemReplies = true)
        {
            HttpMessage message = new(HttpMethod.Get, $"https://devforum.roblox.com/t/{topicId}.json");
            HttpResponseMessage response = await HttpManager.SendAsync(null, message);

            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());

                List<DevForumPost> replies = await ConvertRawData(data.post_stream.posts);
                DevForumPost op = replies[0];
                replies.RemoveAt(0);

                List<string> participants = new();
                foreach (dynamic item in data.details.participants)
                {
                    participants.Add(Convert.ToString(item.username));
                }

                List<ulong> recommended = new();
                if (data.suggested_topics != null)
                {
                    foreach (dynamic item in data.suggested_topics)
                    {
                        recommended.Add(Convert.ToUInt64(item.id));
                    }
                }

                List<string> tags = new();
                if (data.tags != null)
                {
                    foreach (dynamic item in data.tags)
                    {
                        tags.Add(Convert.ToString(item));
                    }
                }

                if (excludeSystemReplies)
                    replies.RemoveAll(reply => reply.PosterName == "system");

                return new()
                {
                    Title = data.title,
                    CategoryId = data.category_id,
                    Id = data.id,
                    Views = data.views,
                    OriginalPost = op,
                    Pinned = data.pinned,
                    PinnedGlobally = data.pinned_globally,
                    Closed = data.closed,
                    Archived = data.archived,
                    Visible = data.visible,
                    Replies = replies.AsReadOnly(),
                    Participants = participants.AsReadOnly(),
                    RecommendedTopicsIds = recommended.AsReadOnly(),
                    Tags = tags.AsReadOnly(),
                };

            }

            throw new ArgumentException("Invalid topic Id.", nameof(topicId));
        }

        private static DevForumCategory MakeCat(dynamic item)
        {
            return new()
            {
                Title = item.name,
                Id = item.id,
                Color = item.color,
                Position = item.position,
                Description = item.description,
                DisplayTopics = item.num_featured_topics,
            };
        }

        private static List<DevForumCategory> catCache;

        /// <summary>
        /// Returns a list of DevForum categories.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="DevForumCategory"/> upon completion.</returns>
        /// <exception cref="ArgumentException">Unknown error.</exception>
        /// <remarks>This API does not include subcategories, use <see cref="DevForumCategory.SubcategoryIds"/> in conjunction with <see cref="GetCategoryAsync(ushort)"/>.</remarks>
        public static async Task<ReadOnlyCollection<DevForumCategory>> GetCategoriesAsync()
        {
            if (catCache == null)
            {
                HttpMessage message = new(HttpMethod.Get, $"https://devforum.roblox.com/categories.json");
                HttpResponseMessage response = await HttpManager.SendAsync(null, message);

                if (!response.IsSuccessStatusCode)
                    throw new ArgumentException($"Unknown error. HTTP {response.StatusCode}");

                List<DevForumCategory> list = new();
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (dynamic item in data.category_list.categories)
                {
                    List<ushort> subcats = new();
                    foreach (dynamic item2 in item.subcategory_ids)
                    {
                        subcats.Add(Convert.ToUInt16(item2));
                    }
                    List<ulong> topicIds = new();
                    foreach (dynamic item3 in item.topics)
                    {
                        topicIds.Add(Convert.ToUInt64(item3.id));
                    }
                    DevForumCategory cat = MakeCat(item);
                    cat.SubcategoryIds = subcats.AsReadOnly();
                    cat.TopicIds = topicIds.AsReadOnly();
                    cat.IsSubcategory = false;

                    list.Add(cat);
                }

                catCache = list;
            }
            return catCache.AsReadOnly();
        }

        /// <summary>
        /// Gets a category given its unique Id.
        /// </summary>
        /// <param name="categoryId">The Id of the category.</param>
        /// <returns>A task containing a <see cref="DevForumCategory"/> upon completion.</returns>
        /// <exception cref="ArgumentException">Invalid category Id.</exception>
        public static async Task<DevForumCategory> GetCategoryAsync(ushort categoryId)
        {
            // Main cats
            foreach (var category in await GetCategoriesAsync())
            {
                if (category.Id == categoryId)
                    return category;
            }

            // Subcats

            HttpMessage message = new(HttpMethod.Get, $"https://devforum.roblox.com/c/{categoryId}/show.json");
            HttpResponseMessage response = await HttpManager.SendAsync(null, message);

            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                DevForumCategory sub = MakeCat(data.category);
                sub.IsSubcategory = true;
                catCache.Add(sub);
                return sub;
            }

            throw new ArgumentException("Invalid category Id.", nameof(categoryId));
        }
    }
}
