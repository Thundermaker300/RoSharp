using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Misc;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.Utility
{
    public static class ChartsAPI
    {
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

            throw new HttpRequestException("Failed to get front page experiences. Try again later.");
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

    public sealed class ChartsResponse
    {
        public string? NextPageToken { get; init; }
        public ReadOnlyCollection<ChartCategory> Categories { get; init; }
    }

    public sealed class ChartCategory
    {
        public string DisplayName { get; init; }
        public string Id { get; init; }
        public string Description { get; init; }
        public ReadOnlyCollection<ulong> ExperienceIds { get; init; }
    }
}
