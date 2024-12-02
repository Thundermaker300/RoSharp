using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Structures;
using RoSharp.Utility;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp.API
{
    /// <summary>
    /// Contains API for viewing the front page of experiences, query experiences, etc.
    /// </summary>
    public static class GameAPI
    {
        // lol
        private static string SessionId => DateTime.UtcNow.Ticks.ToString();

        /// <summary>
        /// Gets experiences currently on the front page and returns a <see cref="ChartsResponse"/> class containing this information.
        /// </summary>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="options">Options to filter the charts API by.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="ChartsResponse"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<ChartsResponse> GetFrontPageExperiencesAsync(Session? session = null, ChartsFilterOptions? options = null, string? cursor = null)
        {
            options ??= new ChartsFilterOptions();

            string url = $"/explore-api/v1/get-sorts?sessionId={SessionId}";
            if (cursor != null)
                url += $"&sortsPageToken={cursor}";

            if (options.Device.HasValue)
                url += "&device=" + options.Device.Value switch
                {
                    Device.Computer => "computer",
                    Device.Console => "console",
                    Device.VR => "vr",
                    Device.Phone => options.IsHighEndDevice ? "high_end_phone" : "low_end_phone",
                    Device.Tablet => options.IsHighEndDevice ? "high_end_tablet" : "low_end_tablet",
                    _ => "all",
                };

            if (!string.IsNullOrWhiteSpace(options.CountryCode))
                url += $"&country={options.CountryCode.ToLower()}";


            HttpRequestMessage message = new(HttpMethod.Get, url);
            string body = await HttpManager.SendStringAsync(session, message);

            List<ChartCategory> categories = [];
            dynamic data = JObject.Parse(body);
            foreach (dynamic sort in data.sorts)
            {
                if (sort.contentType != "Games")
                    continue;

                List<Id<Experience>> list = [];

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

        /// <summary>
        /// Searches Roblox experiences and returns results based on the <paramref name="query"/>.
        /// <para>
        /// Roblox uses an advanced algorithm to determine what results are shown. This algorithm usually combines the <paramref name="query"/> with other factors like visits, active player counts, etc. However, setting <paramref name="exactMatchSearch"/> to <see langword="false"/> will bypass this algorithm and search for experiences strictly by name alone.
        /// </para>
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="exactMatchSearch">Indicates whether or not to perform an 'exact search', which will match results by name alone and no other criteria. In other words, the returned results are guaranteed to contain the <paramref name="query"/> within their title.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<PageResponse<Id<Experience>>> SearchAsync(string query, Session? session = null, bool exactMatchSearch = false, string? cursor = null)
        {
            if (exactMatchSearch)
                query = $"\"{query}\"";
            string url = $"/search-api/omni-search?searchQuery={query}&sessionId={SessionId}&pageType=all";
            if (cursor != null)
                url += $"&pageToken={cursor}";

            HttpRequestMessage message = new(HttpMethod.Get, url);
            string body = await HttpManager.SendStringAsync(session, message);

            List<Id<Experience>> list = [];
            dynamic data = JObject.Parse(body);
            foreach (dynamic item in data.searchResults)
            {
                if (item.contentGroupType != "Game")
                    continue;

                ulong universeId = item.contents[0].universeId;
                list.Add(new(universeId, session));
            }

            string token = data.nextPageToken;
            return new(list, token, null);
        }

        /// <summary>
        /// Gets a list of Roblox selected experiences to show in the "Today's Picks" sort.
        /// </summary>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions (must be authenticated to access).</exception>
        public static async Task<ReadOnlyCollection<Id<Experience>>> GetTodaysPicksAsync(Session? session)
        {
            object payloadBody = new
            {
                pageType = "Home",
                sessionId = SessionId,
            };
            JsonContent payload = JsonContent.Create(payloadBody);
            HttpRequestMessage message = new(HttpMethod.Post, $"/discovery-api/omni-recommendation")
            {
                Content = payload,
            };

            string body = await HttpManager.SendStringAsync(session, message);
            dynamic data = JObject.Parse(body);

            List<Id<Experience>> list = [];
            foreach (dynamic sort in data.sorts)
            {
                if (Convert.ToString(sort.topic).Contains("Today's Picks"))
                {
                    foreach (dynamic item in sort.recommendationList)
                    {
                        if (item.contentType != "Game")
                            continue;

                        ulong contentId = item.contentId;
                        list.Add(new(contentId, session));
                    }
                }
            }

            return list.AsReadOnly();
        }

        /// <summary>
        /// Given a query, returns a list of strings that are recommended by Roblox's experience search algorithm to search for, based on trending searches.
        /// </summary>
        /// <param name="query">The query to return suggestions for.</param>
        /// <param name="session">Logged in session, optional.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="string"/>s upon completion.</returns>
        public static async Task<ReadOnlyCollection<string>> GetAutocompleteSuggestionsAsync(string query, Session? session = null)
        {
            HttpRequestMessage message = new(HttpMethod.Get, $"/games-autocomplete/v1/get-suggestion/{query}");
            string body = await HttpManager.SendStringAsync(session, message);
            dynamic data = JObject.Parse(body);

            List<string> list = [];
            foreach (dynamic item in data.entries)
            {
                string newItem = item.searchQuery;
                list.Add(newItem);
            }
            return list.AsReadOnly();
        }
    }
}
