using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    public static class UserUtility
    {
        public static HttpClient userUtilityClient { get; } = new HttpClient();

        public static async Task<ulong> GetUserIdAsync(string username)
        {
            object request = new
            {
                usernames = new[] { username },
            };
            var content = JsonContent.Create(request);
            HttpResponseMessage response = await userUtilityClient.PostAsync("https://users.roblox.com/v1/usernames/users", content);
            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
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
