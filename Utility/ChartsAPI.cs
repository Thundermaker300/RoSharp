using Newtonsoft.Json.Linq;
using RoSharp.API;
using RoSharp.API.Assets;
using RoSharp.API.Misc;
using RoSharp.Exceptions;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.Utility
{
    /// <summary>
    /// Contains API for viewing the front page of experiences.
    /// </summary>
    public static class ChartsAPI
    {
        /// <summary>
        /// Gets experiences currently on the front page and returns a <see cref="ChartsResponse"/> class containing this information.
        /// </summary>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="ChartsResponse"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<ChartsResponse> GetFrontPageExperiencesAsync(Session? session = null, string? cursor = null)
        {
            string url = $"/explore-api/v1/get-sorts?sessionId={DateTime.UtcNow.Ticks}";
            if (cursor != null)
                url += $"&sortsPageToken={cursor}";

            HttpClient client = MakeClient(session.Global());
            HttpResponseMessage response = await client.GetAsync(url);
            string body = await response.Content.ReadAsStringAsync();
            HttpVerify.ThrowIfNecessary(response, body);

            List<ChartCategory> categories = new();
            dynamic data = JObject.Parse(body);
            foreach (dynamic sort in data.sorts)
            {
                if (sort.contentType != "Games")
                    continue;

                List<GenericId<Experience>> list = new();

                foreach (dynamic game in sort.games)
                {
                    ulong id = game.universeId;
                    list.Add(new(id, session));
                }

                ChartCategory category = new()
                {
                    DisplayName = sort.sortDisplayName,
                    Id = sort.sortId,
                    Description = sort.topicLayoutData.infoText,
                    ExperienceIds = list.AsReadOnly(),
                };
                categories.Add(category);
            }

            string token = data.nextSortsPageToken;
            return new()
            {
                NextPageToken = token == string.Empty ? null : token,
                Categories = categories.AsReadOnly(),
            };
        }

        private static HttpClient MakeClient(Session? session = null)
        {
            Uri uri = new Uri(Constants.URL("apis"));

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            if (session != null && session.RobloSecurity != string.Empty)
                cookies.Add(uri, new Cookie(".ROBLOSECURITY", session?.RobloSecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = uri;

            return client;
        }
    }

    /// <summary>
    /// Represents the response for the Charts API.
    /// </summary>
    public sealed class ChartsResponse
    {
        /// <summary>
        /// Gets the token that can be used in the cursor parameter of <see cref="ChartsAPI.GetFrontPageExperiencesAsync(Session?, string?)"/> to advance to the next page.
        /// </summary>
        /// <remarks>Will be <see langword="null"/> if it is already on the last page.</remarks>
        public string? NextPageToken { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="ChartCategory"/> experiences within this request.
        /// </summary>
        public ReadOnlyCollection<ChartCategory> Categories { get; init; }

        internal ChartsResponse() { }
    }

    /// <summary>
    /// Represents a category for experiences within the Charts API.
    /// </summary>
    public sealed class ChartCategory
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the internal Id of the category.
        /// </summary>
        public string Id { get; init; }
        
        /// <summary>
        /// Gets the description of the category.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets a list of experience Ids within this category.
        /// </summary>
        public ReadOnlyCollection<GenericId<Experience>> ExperienceIds { get; init; }

        /// <summary>
        /// Converts <see cref="ExperienceIds"/> to a read-only <see cref="Experience"/> collection.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to convert. Defaults to <c>-1</c> (unlimited).</param>
        /// <param name="startAt">The amount of Ids to skip before converting.</param>
        /// <returns>A task containing the list of experiences upon completion.</returns>
        /// <remarks>This API is very time-consuming as each new experience is an API call, and Too Many Requests is a common error. As such, a limit should be used in conjunction with the <paramref name="startAt"/> parameter to only convert a certain amount of experiences at a time.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<ReadOnlyCollection<Experience>> ToExperienceListAsync(int limit = -1, int startAt = 0)
        {
            List<Experience> experiences = new List<Experience>();
            foreach (GenericId<Experience> id in ExperienceIds.Skip(startAt))
            {
                experiences.Add(await id.GetInstanceAsync());

                if (limit > 0 && experiences.Count >= limit)
                    break;
            }
            return experiences.AsReadOnly();
        }

        /// <summary>
        /// Calls a provided action for each experience in the <see cref="ExperienceIds"/> list, converting each to a <see cref="Experience"/> before doing so.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="limit">The maximum amount of Ids to call the <paramref name="action"/> with. Defaults to <c>-1</c> (unlimited).</param>
        /// <param name="startAt">The amount of Ids to skip before calling the <paramref name="action"/>.</param>
        /// <returns>A task that completes when the process is done.</returns>
        /// <remarks>This API is very time-consuming as each new experience is an API call, and Too Many Requests is a common error. As such, a limit should be used in conjunction with the <paramref name="startAt"/> parameter to only peform the action with a certain amount of experiences at a time.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task ForEachExperienceAsync(Action<Experience> action, int limit = -1, int startAt = 0)
        {
            int count = 0;
            foreach (GenericId<Experience> id in ExperienceIds.Skip(startAt))
            {
                action(await id.GetInstanceAsync());
                count++;

                if (limit > 0 && count >= limit)
                    break;
            }
        }
    }
}
