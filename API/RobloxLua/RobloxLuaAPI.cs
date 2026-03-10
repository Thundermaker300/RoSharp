using RoSharp.Http;
using RoSharp.Structures;

namespace RoSharp.API.RobloxLua
{
    public static class RobloxLuaAPI
    {
        public static async Task<HttpResult<string>> GetLiveAPIVersionAsync()
        {
            HttpMessage payload = new(HttpMethod.Get, $"https://raw.githubusercontent.com/MaximumADHD/Roblox-Client-Tracker/roblox/version-guid.txt");
            var response = await HttpManager.SendAsync(null, payload);
            string rawData = await response.Content.ReadAsStringAsync();

            return new(response, rawData);
        }

        public static async Task<HttpResult<APIDumpSnapshot>> GetAPIDump(string? version = null)
        {
            version ??= await GetLiveAPIVersionAsync();

            HttpMessage payload = new(HttpMethod.Get, $"https://setup.rbxcdn.com/{version}-API-Dump.json");
            var response = await HttpManager.SendAsync(null, payload);
            string rawData = await response.Content.ReadAsStringAsync();

            return new(response, new APIDumpSnapshot(version, rawData));
        }
    }
}
