using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Structures;
using RoSharp.Utility;

namespace RoSharp.Extensions
{
    /// <summary>
    /// Utility extensions.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Converts a <see cref="FixedLimit"/> to the appropriate string to use within the Roblox API.
        /// </summary>
        /// <param name="fixedLimit">The <see cref="FixedLimit"/> to convert.</param>
        /// <returns>Roblox-API compatible string.</returns>
        public static int Limit(this FixedLimit fixedLimit)
        {
            return Convert.ToInt32(fixedLimit.ToString().Replace("Limit", string.Empty));
        }

        public static async Task<int?> GetCategoryIdAsync(this AssetType assetType)
        {
            int assetId = (int)assetType;
            HttpMessage message = new(HttpMethod.Get, $"{Constants.URL("catalog")}/v1/asset-to-category");
            string rawData = await HttpManager.SendStringAsync(null, message);
            JObject data = JObject.Parse(rawData);
            if (data.TryGetValue(assetId.ToString(), out JToken? val))
                return (int)val;
            return null;
        }
    }
}
