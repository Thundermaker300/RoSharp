using RoSharp.Http;
using RoSharp.Structures;

namespace RoSharp.API.RobloxLua
{
    /// <summary>
    /// Static API class to access information about Roblox Lua and create <see cref="APIDumpSnapshot"/>s.
    /// </summary>
    public static class RobloxLuaAPI
    {
        /// <summary>
        /// Gets the current live version hash.
        /// </summary>
        /// <returns>A <see cref="string"/> representing the current live version hash.</returns>
        public static async Task<HttpResult<string>> GetLiveAPIVersionAsync()
        {
            HttpMessage payload = new(HttpMethod.Get, $"https://raw.githubusercontent.com/MaximumADHD/Roblox-Client-Tracker/roblox/version-guid.txt");
            var response = await HttpManager.SendAsync(null, payload);
            string rawData = await response.Content.ReadAsStringAsync();

            return new(response, rawData);
        }

        /// <summary>
        /// Gets a <see cref="APIDumpSnapshot"/> containing the Roblox API dump with a specified <paramref name="version"/> hash, or <see langword="null"/> to retrieve the currently live API dump.
        /// </summary>
        /// <param name="version">The specific API Dump version or <see langword="null"/> for the live version.</param>
        /// <returns>A <see cref="APIDumpSnapshot"/> representing the API dump.</returns>
        public static async Task<HttpResult<APIDumpSnapshot>> GetAPIDumpAsync(string? version = null)
        {
            version ??= await GetLiveAPIVersionAsync();

            HttpMessage payload = new(HttpMethod.Get, $"https://setup.rbxcdn.com/{version}-API-Dump.json");
            var response = await HttpManager.SendAsync(null, payload);
            string rawData = await response.Content.ReadAsStringAsync();

            return new(response, new APIDumpSnapshot(version, rawData));
        }
    }
}
