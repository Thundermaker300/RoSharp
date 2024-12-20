﻿using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Structures.DeveloperStats;
using System.Collections.ObjectModel;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// An API class that holds experience analytics-related API.
    /// </summary>
    public class DeveloperStats : IRefreshable
    {
        private Experience experience;

        /// <summary>
        /// Gets the primary URL to use for analytics for this experience.
        /// </summary>
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
            await Task.CompletedTask;
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

        /// <summary>
        /// Returns Monthly Active User (MAU) data for the given date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>A <see cref="MAUData"/> containing data about that day's monthly active users (MAU) amount.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<MAUData> GetMAUDataAsync(DateTime date)
        {
            // Total MAU
            object body = MakeGenericBody(date, "MonthlyActiveUsers", []);
            var message = new HttpMessage(HttpMethod.Post, AnalyticUrl, body)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetMAUDataAsync)
            };

            HttpResponseMessage response = await experience.SendAsync(message, Constants.URL("apis"));
            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());

            if (data.operation.queryResult.values.Count == 0)
            {
                return new MAUData()
                {
                    TotalMonthlyUsers = 0,
                    ByCountry = new Dictionary<string, int>(0).AsReadOnly(),
                    ByGender = new Dictionary<Gender, int>(0).AsReadOnly(),
                    ByAgeRange = new Dictionary<string, int>(0).AsReadOnly(),
                    ByLocale = new Dictionary<string, int>(0).AsReadOnly(),
                };
            }

            int totalMau = data.operation.queryResult.values[0].dataPoints[0].value;

            // By Country
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "Country" } }});
            message.Content = body;
            response = await experience.SendAsync(message, Constants.URL("apis"));
            data = JObject.Parse(await response.Content.ReadAsStringAsync());

            Dictionary<string, int> byCountry = [];
            foreach (var item in data.operation.queryResult.values)
            {
                string key = item.breakdownValue[0].value;
                int value = item.dataPoints[0].value;
                byCountry.Add(key, value);
            }

            // By Gender
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "Gender" } }});
            message.Content = body;
            response = await experience.SendAsync(message, Constants.URL("apis"));
            data = JObject.Parse(await response.Content.ReadAsStringAsync());

            Dictionary<Gender, int> byGender = [];
            foreach (var item in data.operation.queryResult.values)
            {
                string genderValue = item.breakdownValue[0].value;
                Gender key = Enum.Parse<Gender>(genderValue, true);
                int value = item.dataPoints[0].value;
                byGender.Add(key, value);
            }

            // By AgeGroup
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "AgeGroup" } }});
            message.Content = body;
            response = await experience.SendAsync(message, Constants.URL("apis"));
            data = JObject.Parse(await response.Content.ReadAsStringAsync());

            Dictionary<string, int> byAgeGroup = [];
            foreach (var item in data.operation.queryResult.values)
            {
                string key = item.breakdownValue[0].value;
                int value = item.dataPoints[0].value;
                byAgeGroup.Add(key, value);
            }

            // By Locale
            body = MakeGenericBody(date, "MonthlyActiveUsers", new[] { new { dimensions = new[] { "Locale" } }});
            message.Content = body;
            response = await experience.SendAsync(message, Constants.URL("apis"));
            data = JObject.Parse(await response.Content.ReadAsStringAsync());

            Dictionary<string, int> byLocale = [];
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

        private ReadOnlyCollection<ExperienceAuditLog>? history;

        /// <summary>
        /// Gets recent audit logs for an experience.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="ExperienceAuditLog"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<ReadOnlyCollection<ExperienceAuditLog>> GetAuditLogsAsync()
        {
            if (history == null)
            {
                var message = new HttpMessage(HttpMethod.Get, $"/activity-feed-api/v1/history?clientType=1&universeId={experience.UniverseId}")
                {
                    AuthType = AuthType.RobloSecurity,
                    ApiName = nameof(GetAuditLogsAsync),
                };
                string rawData = await experience.SendStringAsync(message, Constants.URL("apis"));
                dynamic data = JObject.Parse(rawData);

                List<ExperienceAuditLog> list = [];
                foreach (dynamic ev in data.events)
                {
                    list.Add(new()
                    {
                        Id = ev.id,
                        Type = (ExperienceAuditLogType)ev.eventType,
                        UniverseId = new Id<Experience>(Convert.ToUInt64(ev.universeId)),
                        PlaceId = ev.placeId,
                        UserId = new Id<User>(Convert.ToUInt64(ev.userId)),
                        Time = DateTime.UnixEpoch.AddMilliseconds(Convert.ToInt64(ev.createdUnixTimeMs)),
                    });
                }

                history = list.AsReadOnly();
            }
            return history;
        }
    }
}
