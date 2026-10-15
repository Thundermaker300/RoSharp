using Newtonsoft.Json.Linq;
using RoSharp.Http;
using RoSharp.Structures;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for text on Roblox.
    /// </summary>
    public static class TextUtility
    {
        /// <summary>
        /// Gets a boolean indicating whether or not the provided <paramref name="input"/> was modified by the Roblox text filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing a boolean upon completion.</returns>
        public static async Task<HttpResult<bool>> IsFilteredAsync(string input, Session? session)
        {

            HttpMessage payload = new(HttpMethod.Post, $"{Constants.URL("apis")}/game-update-notifications/v1/filter", input);

            HttpResponseMessage response = await HttpManager.SendAsync(session, payload);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            bool filtered = data.isFiltered;
            return new(response, filtered);
        }

        /// <summary>
        /// Passes the provided <paramref name="input"/> through the Roblox text filter and returns its output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing a string of the new text upon completion.</returns>
        public static async Task<HttpResult<string>> FilterAsync(string input, Session? session)
        {

            HttpMessage payload = new(HttpMethod.Post, $"{Constants.URL("apis")}/game-update-notifications/v1/filter", input);

            HttpResponseMessage response = await HttpManager.SendAsync(session, payload);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            string filtered = data.filteredGameUpdateText;
            return new(response, filtered);
        }
    }
}
