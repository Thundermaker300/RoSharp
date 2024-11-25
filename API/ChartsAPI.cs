using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Net;

namespace RoSharp.API
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
        /// <param name="options">Options to filter the charts API by.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="ChartsResponse"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<ChartsResponse> GetFrontPageExperiencesAsync(Session? session = null, ChartsFilterOptions? options = null, string? cursor = null)
        {
            options ??= new ChartsFilterOptions();

            string url = $"/explore-api/v1/get-sorts?sessionId={DateTime.UtcNow.Ticks}";
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

            session ??= session.Global();
            HttpClient client = MakeClient();
            HttpResponseMessage response = await client.GetAsync(url);
            RoUtility.LogHTTP(session, response, client);
            string body = await response.Content.ReadAsStringAsync();
            HttpVerify.ThrowIfNecessary(response, body);

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

        private static HttpClient MakeClient(Session? session = null)
        {
            Uri uri = new(Constants.URL("apis"));

            CookieContainer cookies = new();
            HttpClientHandler handler = new()
            {
                CookieContainer = cookies
            };

            if (session != null && session.RobloSecurity != string.Empty)
                cookies.Add(uri, new Cookie(".ROBLOSECURITY", session?.RobloSecurity));

            HttpClient client = new(handler)
            {
                BaseAddress = uri
            };

            return client;
        }
    }
}
