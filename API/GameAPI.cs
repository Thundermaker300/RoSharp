﻿using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Structures;
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
        internal static string SessionId => DateTime.UtcNow.Ticks.ToString();

        /// <summary>
        /// Gets experiences currently on the front page and returns a <see cref="ChartsResponse"/> class containing this information.
        /// </summary>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="options">Options to filter the charts API by.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="ChartsResponse"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<HttpResult<ChartsResponse>> GetFrontPageExperiencesAsync(Session? session = null, ChartsFilterOptions? options = null, string? cursor = null)
        {
            options ??= new ChartsFilterOptions();

            string url = $"{Constants.URL("apis")}/explore-api/v1/get-sorts?sessionId={SessionId}";
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


            HttpMessage message = new(HttpMethod.Get, url);
            var response = await HttpManager.SendAsync(session, message);
            string body = await response.Content.ReadAsStringAsync();

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
            return new(response, new()
            {
                NextPageToken = token == string.Empty ? null : token,
                Categories = categories.AsReadOnly(),
            });
        }

        /// <summary>
        /// Gets a list of Roblox selected experiences to show in the "Today's Picks" sort.
        /// </summary>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions (must be authenticated to access).</exception>
        public static async Task<HttpResult<ReadOnlyCollection<Id<Experience>>>> GetTodaysPicksAsync(Session? session)
        {
            object payload = new
            {
                pageType = "Home",
                sessionId = SessionId,
            };
            HttpMessage message = new(HttpMethod.Post, $"{Constants.URL("apis")}/discovery-api/omni-recommendation", payload);

            var response = await HttpManager.SendAsync(session, message);
            string body = await response.Content.ReadAsStringAsync();
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

            return new(response, list.AsReadOnly());
        }

        /// <summary>
        /// Given a query, returns a list of strings that are recommended by Roblox's experience search algorithm to search for, based on trending searches.
        /// </summary>
        /// <param name="query">The query to return suggestions for.</param>
        /// <param name="session">Logged in session, optional.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="string"/>s upon completion.</returns>
        public static async Task<HttpResult<ReadOnlyCollection<string>>> GetAutocompleteSuggestionsAsync(string query, Session? session = null)
        {
            HttpMessage message = new(HttpMethod.Get, $"/games-autocomplete/v1/get-suggestion/{query}");
            var response = await HttpManager.SendAsync(session, message);
            string body = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(body);

            List<string> list = [];
            foreach (dynamic item in data.entries)
            {
                string newItem = item.searchQuery;
                list.Add(newItem);
            }
            return new(response, list.AsReadOnly());
        }
    }
}
