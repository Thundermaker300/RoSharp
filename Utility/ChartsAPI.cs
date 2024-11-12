using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.Utility
{
    public static class ChartsAPI
    {
        // TODO: Sort of works. help
        public static async Task<ReadOnlyCollection<Experience>> GetFrontPageExperiencesAsync(string category, Session? session = null)
        {
            HttpClient client = MakeClient(session);
            HttpResponseMessage response = await client.GetAsync("/explore-api/v1/get-sorts?sessionId=1");
            var dict = new List<Experience>();
            if (response.IsSuccessStatusCode)
            {

                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (dynamic item in data.sorts)
                {
                    if (item.sortDisplayName == category)
                    {
                        foreach (dynamic game in item.games)
                        {
                            ulong id = game.universeId;
                            dict.Add(await Experience.FromId(id));
                        }
                    }
                }
            }

            return dict.AsReadOnly();
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
}
