using Newtonsoft.Json.Linq;
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
        public static async Task<ChartsResponse> GetFrontPageExperiencesAsync(Session? session = null, string? cursor = null)
        {
            string url = $"/explore-api/v1/get-sorts?sessionId={DateTime.UtcNow.Ticks}";
            if (cursor != null)
                url += $"&sortsPageToken={cursor}";

            HttpClient client = MakeClient(session);
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                List<ChartCategory> categories = new();
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (dynamic sort in data.sorts)
                {
                    if (sort.contentType != "Games")
                        continue;

                    List<ulong> list = new();

                    foreach (dynamic game in sort.games)
                    {
                        ulong id = game.universeId;
                        list.Add(id);
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

            throw new RobloxAPIException($"Failed to get front page experiences (HTTP {response.StatusCode}). Try again later.");
        }

        private static HttpClient MakeClient(Session? session = null)
        {
            Uri uri = new Uri("https://apis.roblox.com");

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
        public ReadOnlyCollection<ulong> ExperienceIds { get; init; }

        /// <summary>
        /// Converts <see cref="ExperienceIds"/> to a read-only <see cref="Experience"/> collection.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to convert. Defaults to <c>-1</c> (unlimited).</param>
        /// <param name="startAt">The amount of Ids to skip before converting.</param>
        /// <returns>A task containing the list of experiences upon completion.</returns>
        /// <remarks>This API is very time-consuming as each new experience is an API call, and Too Many Requests is a common error. As such, a limit should be used in conjunction with the <paramref name="startAt"/> parameter to only convert a certain amount of experiences at a time.</remarks>
        public async Task<ReadOnlyCollection<Experience>> ToExperienceListAsync(int limit = -1, int startAt = 0)
        {
            List<Experience> experiences = new List<Experience>();
            foreach (ulong id in ExperienceIds.Skip(startAt))
            {
                experiences.Add(await Experience.FromId(id));

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
        public async Task ForEachExperienceAsync(Action<Experience> action, int limit = -1, int startAt = 0)
        {
            int count = 0;
            foreach (ulong id in ExperienceIds.Skip(startAt))
            {
                action(await Experience.FromId(id));
                count++;

                if (limit > 0 && count >= limit)
                    break;
            }
        }
    }
}
