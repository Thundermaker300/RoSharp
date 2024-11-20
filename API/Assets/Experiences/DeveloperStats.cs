using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Interfaces;
using RoSharp.Structures.DeveloperStats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RoSharp.API.Assets.Experiences
{
    public class DeveloperStats : IRefreshable
    {
        private Experience experience;

        public string AnalyticUrl => $"/analytics-query-gateway/v1/metrics/resource/RESOURCE_TYPE_UNIVERSE/id/{experience.UniverseId}";

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        internal DeveloperStats(Experience experience)
        {
            ArgumentNullException.ThrowIfNull(experience, nameof(experience));
            this.experience = experience;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            history = null;
        }

        private object MakeGenericBody(DateTime date, string metric, object[] breakdown)
        {
            // Remove the time component
            DateTime useDateTime = new(date.Year, date.Month, date.Day);

            return new
            {
                resourceId = experience.UniverseId.ToString(),
                resourceType = "RESOURCE_TYPE_UNIVERSE",
                query = new
                {
                    metric = metric,
                    granularity = "METRIC_GRANULARITY_ONE_DAY",
                    startTime = useDateTime.ToString("O") + "Z",
                    endTime = useDateTime.ToString("O") + "Z",
                    breakdown = breakdown,
                }
            };
        }

        public async Task<MAUData> GetMAUData(DateTime date)
        {
            // Total MAU
            object body = MakeGenericBody(date, "MonthlyActiveUsers", Array.Empty<object>());
            HttpResponseMessage message = await experience.PostAsync(AnalyticUrl, body, Constants.URL("apis"), "DeveloperStats.GetMAUData");
            dynamic data = JObject.Parse(await message.Content.ReadAsStringAsync());
            int totalMau = data.operation.queryResult.values[0].dataPoints[0].value;

            // By Country
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "Country" } }});
            message = await experience.PostAsync(AnalyticUrl, body, Constants.URL("apis"), "DeveloperStats.GetMAUData");
            data = JObject.Parse(await message.Content.ReadAsStringAsync());

            Dictionary<string, int> byCountry = new();
            foreach (var item in data.operation.queryResult.values)
            {
                string key = item.breakdownValue[0].value;
                int value = item.dataPoints[0].value;
                byCountry.Add(key, value);
            }

            // By Gender
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "Gender" } }});
            message = await experience.PostAsync(AnalyticUrl, body, Constants.URL("apis"), "DeveloperStats.GetMAUData");
            data = JObject.Parse(await message.Content.ReadAsStringAsync());

            Dictionary<Gender, int> byGender = new();
            foreach (var item in data.operation.queryResult.values)
            {
                string genderValue = item.breakdownValue[0].value;
                Gender key = Enum.Parse<Gender>(genderValue, true);
                int value = item.dataPoints[0].value;
                byGender.Add(key, value);
            }

            // By AgeGroup
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "AgeGroup" } }});
            message = await experience.PostAsync(AnalyticUrl, body, Constants.URL("apis"), "DeveloperStats.GetMAUData");
            data = JObject.Parse(await message.Content.ReadAsStringAsync());

            Dictionary<string, int> byAgeGroup = new();
            foreach (var item in data.operation.queryResult.values)
            {
                string key = item.breakdownValue[0].value;
                int value = item.dataPoints[0].value;
                byAgeGroup.Add(key, value);
            }

            // By Locale
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "Locale" } }});
            message = await experience.PostAsync(AnalyticUrl, body, Constants.URL("apis"), "DeveloperStats.GetMAUData");
            data = JObject.Parse(await message.Content.ReadAsStringAsync());

            Dictionary<string, int> byLocale = new();
            foreach (var item in data.operation.queryResult.values)
            {
                string key = item.breakdownValue[0].value;
                int value = item.dataPoints[0].value;
                byLocale.Add(key, value);
            }

            return new MAUData()
            {
                TotalMonthlyUsers = totalMau,
                ByCountry = byCountry.AsReadOnly(),
                ByGender = byGender.AsReadOnly(),
                ByAgeRange = byAgeGroup.AsReadOnly(),
                ByLocale = byLocale.AsReadOnly(),
            };
        }

        private ReadOnlyCollection<ExperienceActivityHistory>? history;
        public async Task<ReadOnlyCollection<ExperienceActivityHistory>> GetActivityHistoryAsync()
        {
            if (history == null)
            {
                string rawData = await experience.GetStringAsync("/activity-feed-api/v1/history?clientType=1&universeId=3744484651", "https://apis.roblox.com", "DeveloperStats.GetActivityHistoryAsync");
                dynamic data = JObject.Parse(rawData);

                List<ExperienceActivityHistory> list = new();
                foreach (dynamic ev in data.events)
                {
                    list.Add(new()
                    {
                        Id = ev.id,
                        Type = (ExperienceActivityHistoryType)ev.eventType,
                        UniverseId = new GenericId<Experience>(Convert.ToUInt64(ev.universeId)),
                        PlaceId = ev.placeId,
                        UserId = new GenericId<User>(Convert.ToUInt64(ev.userId)),
                        Time = DateTime.UnixEpoch.AddMilliseconds(Convert.ToInt64(ev.createdUnixTimeMs)),
                    });
                }

                history = list.AsReadOnly();
            }
            return history;
        }
    }
}
