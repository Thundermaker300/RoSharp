using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using System.Collections.ObjectModel;
using System.Net;

namespace RoSharp.Utility
{
    public static class PriceFloorAPI
    {
        public static async Task<ReadOnlyDictionary<AssetType, int>> GetPriceFloorsAsync(Session session)
        {
            HttpClient client = MakeClient(session);
            HttpResponseMessage response = await client.GetAsync("/v1/collectibles/metadata");
            HttpVerify.ThrowIfNecessary(response);
            var dict = new Dictionary<AssetType, int>();
            if (response.IsSuccessStatusCode)
            {

                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                foreach (JProperty item in data.unlimitedItemPriceFloors)
                {
                    string name = item.Name;
                    JToken jToken = item.Value;
                    int value = Convert.ToInt32(jToken["priceFloor"]);
                    if (Enum.TryParse(name, out AssetType result))
                        dict.Add(result, value);
                }
            }

            return dict.AsReadOnly();
        }

        public static async Task<int?> GetPriceFloorForTypeAsync(AssetType assetType, Session session)
        {
            var priceFloors = await GetPriceFloorsAsync(session);
            if (priceFloors.TryGetValue(assetType, out int value))
                return value;

            return null;
        }

        private static HttpClient MakeClient(Session session)
        {
            SessionVerify.ThrowIfNecessary(session, "PriceFloorAPI");

            Uri uri = new Uri(Constants.URL("itemconfiguration"));

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            cookies.Add(uri, new Cookie(".ROBLOSECURITY", session.RobloSecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = uri;

            return client;
        }
    }
}
