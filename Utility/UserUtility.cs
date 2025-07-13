using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using RoSharp.Http;
using RoSharp.Structures;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for users.
    /// </summary>
    public static class UserUtility
    {
        /// <summary>
        /// Gets the User Id that corresponds with the given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The matching User Id.</returns>
        /// <exception cref="ArgumentException">Invalid username provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<HttpResult<ulong>> GetUserIdAsync(string username)
        {
            var list = await GetUserIdsAsync([username]);
            return new(list, list.Value.First().Value);
        }

        /// <summary>
        /// Converts an array of usernames to a dictionary, mapping the usernames to their user Id.
        /// </summary>
        /// <param name="usernames">The username list.</param>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> upon completion.</returns>
        /// <exception cref="ArgumentException">No valid usernames.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<HttpResult<ReadOnlyDictionary<string, ulong>>> GetUserIdsAsync(params string[] usernames)
        {
            object content = new
            {
                usernames = usernames.ToArray(),
            };
            HttpMessage payload = new(HttpMethod.Post, $"{Constants.URL("users")}/v1/usernames/users", content);

            HttpResponseMessage response = await HttpManager.SendAsync(null, payload);
            string body = await response.Content.ReadAsStringAsync();
            HttpVerify.ThrowIfNecessary(response, body);

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (data.data.Count == 0)
            {
                throw new ArgumentException("No valid usernames provided.");
            }
            Dictionary<string, ulong> dict = new(usernames.Count());
            foreach (dynamic item in data.data)
            {
                string name = item.requestedUsername;
                ulong id = item.id;
                dict.Add(name, id);
            }
            return new(response, dict.AsReadOnly());
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{T}"/> of usernames to a dictionary, mapping the usernames to their user Id.
        /// </summary>
        /// <param name="usernames">The username list.</param>
        /// <returns>A task containing a <see cref="ReadOnlyDictionary{TKey, TValue}"/> upon completion.</returns>
        /// <exception cref="ArgumentException">No valid usernames.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<HttpResult<ReadOnlyDictionary<string, ulong>>> GetUserIdsAsync(IEnumerable<string> usernames)
            => await GetUserIdsAsync(usernames.ToArray());

        private static async Task<HttpResult<ReadOnlyCollection<Color>>> GetColors(string key)
        {
            HttpMessage payload = new(HttpMethod.Get, $"{Constants.URL("avatar")}/v1/avatar-rules");
            var response = await HttpManager.SendAsync(null, payload);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            
            List<Color> colors = new List<Color>();
            foreach (dynamic item in data[key])
            {
                colors.Add(Color.FromHex(Convert.ToString(item.hexColor)));
            }
            return new(response, colors.AsReadOnly());
        }

        /// <summary>
        /// Gets a list of colors that are listed in the basic body colors in the avatar editor.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Color"/> upon completion.</returns>
        public static async Task<HttpResult<ReadOnlyCollection<Color>>> GetBasicBodyColorOptionsAsync()
            => await GetColors("basicBodyColorsPalette");

        /// <summary>
        /// Gets a list of colors that are listed in the advanced body colors in the avatar editor.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="Color"/> upon completion.</returns>
        public static async Task<HttpResult<ReadOnlyCollection<Color>>> GetAdvancedBodyColorOptionsAsync()
            => await GetColors("bodyColorsPalette");
    }
}
