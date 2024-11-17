using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for users.
    /// </summary>
    public static class UserUtility
    {
        private static HttpClient userUtilityClient { get; } = new HttpClient();

        /// <summary>
        /// Gets the User Id that corresponds with the given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The matching User Id.</returns>
        /// <exception cref="ArgumentException">Invalid username provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<ulong> GetUserIdAsync(string username)
        {
            object request = new
            {
                usernames = new[] { username },
            };
            var content = JsonContent.Create(request);
            HttpResponseMessage response = await userUtilityClient.PostAsync($"{Constants.URL("users")}/v1/usernames/users", content);
            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (data.data.Count == 0)
                {
                    throw new ArgumentException("Invalid username provided.");
                }
                return data.data[0].id;
            }
            HttpVerify.ThrowIfNecessary(response);
            return 0;
        }
    }
}
