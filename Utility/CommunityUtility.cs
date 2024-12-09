using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using RoSharp.Structures;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for communities.
    /// </summary>
    public static class CommunityUtility
    {
        /// <summary>
        /// Gets the community Id that corresponds with the given community name.
        /// </summary>
        /// <param name="groupName">The name of the community.</param>
        /// <returns>The matching community Id.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<ulong?> GetCommunityIdAsync(string groupName)
        {
            HttpMessage payload = new(HttpMethod.Get, $"{Constants.URL("groups")}/v1/groups/search/lookup?groupName={groupName}");

            HttpResponseMessage response = await HttpManager.SendAsync(null, payload);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            
            foreach (dynamic item in data.data)
            {
                if (Convert.ToString(item.name).ToLower().Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                    return Convert.ToUInt64(item.id);
            }

            return null;
        }
    }
}
