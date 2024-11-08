﻿using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Utility
{
    public static class PriceFloorAPI
    {
        public static async Task<ReadOnlyDictionary<AssetType, int>> GetPriceFloorsAsync(Session session)
        {
            ArgumentNullException.ThrowIfNull(session);

            HttpClient client = MakeClient(session);
            HttpResponseMessage response = await client.GetAsync("/v1/collectibles/metadata");
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

        public static async Task<int?> GetPriceFloorForTypeAsync(Session session, AssetType assetType)
        {
            var priceFloors = await GetPriceFloorsAsync(session);
            if (priceFloors.TryGetValue(assetType, out int value))
                return value;

            return null;
        }

        private static HttpClient MakeClient(Session session)
        {
            SessionErrors.Verify(session);

            Uri uri = new Uri("https://itemconfiguration.roblox.com");

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
