using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Utility
{
    public static class UserUtility
    {
        public static HttpClient userUtilityClient { get; } = new HttpClient();

        public static ulong GetUserId(string username)
        {
            object request = new
            {
                usernames = new[] { username },
            };
            var content = JsonContent.Create(request);
            HttpResponseMessage response = userUtilityClient.PostAsync("https://users.roblox.com/v1/usernames/users", content).Result;
            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (data.data.Count == 0)
                {
                    throw new InvalidOperationException("Invalid username provided.");
                }
                return data.data[0].id;
            }
            return 0;
        }
    }
}
