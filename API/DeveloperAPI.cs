using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using RoSharp.Structures;
using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public static class DeveloperAPI
    {
        private static async Task<string> getQuotaData(Session? session, AssetType assetType)
        {
            HttpMessage payload = new(HttpMethod.Get, $"{Constants.URL("publish")}/v1/asset-quotas?resourceType=RateLimitUpload&assetType={assetType}");
            string rawData = await HttpManager.SendStringAsync(session, payload);

            return rawData;
        }

        /// <summary>
        /// Gets the maximum amount of the given <see cref="AssetType"/> that can be uploaded by the authenticated user in a specific time frame.
        /// </summary>
        /// <param name="assetType">The asset type to get the quota of.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing the quota upon completion.</returns>
        public static async Task<int> GetUploadQuotaAsync(AssetType assetType, Session? session)
        {
            string rawData = await getQuotaData(session, assetType);
            dynamic data = JObject.Parse(rawData);

            if (data.quotas.Count == 0)
                return 0;

            dynamic quota = data.quotas[0];
            return quota.capacity;
        }

        /// <summary>
        /// Gets the amount of assets the authenticated user has uploaded within the quota's period.
        /// </summary>
        /// <param name="assetType">The asset type to get the quota usage of.</param>
        /// <param name="session">Logged in session. Required but can be replaced with <see langword="null"/> if there is a global session assigned.</param>
        /// <returns>A task containing the quota's usage upon completion.</returns>
        public static async Task<int> GetUploadQuotaUsageAsync(AssetType assetType, Session? session)
        {
            string rawData = await getQuotaData(session, assetType);
            dynamic data = JObject.Parse(rawData);

            if (data.quotas.Count == 0)
                return 0;

            dynamic quota = data.quotas[0];
            return quota.usage;
        }
    }
}
