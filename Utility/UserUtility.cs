using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    public static class UserUtility
    {
        private static HttpClient userUtilityClient { get; } = new HttpClient();

        /// <summary>
        /// Gets the User Id that corresponds with the given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The matching User Id.</returns>
        /// <exception cref="HttpRequestException">Invalid username provided.</exception>
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
                    throw new RobloxAPIException("Invalid username provided.");
                }
                return data.data[0].id;
            }
            return 0;
        }
    }
}
