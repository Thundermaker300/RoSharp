using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using RoSharp.Http;
using RoSharp.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    /// <summary>
    /// Contains miscellaneous development-related APIs.
    /// </summary>
    public static class DeveloperAPI
    {
        private static async Task<HttpResult<string>> getQuotaData(Session? session, AssetType assetType)
        {
            HttpMessage payload = new(HttpMethod.Get, $"{Constants.URL("publish")}/v1/asset-quotas?resourceType=RateLimitUpload&assetType={assetType}");
            var response = await HttpManager.SendAsync(session, payload);
            string rawData = await response.Content.ReadAsStringAsync();

            return new(response, rawData);
        }

        /// <summary>
        /// Gets the maximum amount of the given <see cref="AssetType"/> that can be uploaded by the authenticated user in a specific time frame.
        /// </summary>
        /// <param name="assetType">The asset type to get the quota of.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing the quota upon completion.</returns>
        public static async Task<HttpResult<int>> GetUploadQuotaAsync(AssetType assetType, Session? session)
        {
            var response = await getQuotaData(session, assetType);
            dynamic data = JObject.Parse(response.Value);

            if (data.quotas.Count == 0)
                return new(response, 0);

            dynamic quota = data.quotas[0];
            return new(response, Convert.ToInt32(quota.capacity));
        }

        /// <summary>
        /// Gets the amount of assets the authenticated user has uploaded within the quota's period.
        /// </summary>
        /// <param name="assetType">The asset type to get the quota usage of.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing the quota's usage upon completion.</returns>
        public static async Task<HttpResult<int>> GetUploadQuotaUsageAsync(AssetType assetType, Session? session)
        {
            var response = await getQuotaData(session, assetType);
            dynamic data = JObject.Parse(response.Value);

            if (data.quotas.Count == 0)
                return new(response, 0);

            dynamic quota = data.quotas[0];
            return new(response, Convert.ToInt32(quota.usage));
        }
    }
}
