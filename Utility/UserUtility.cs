﻿using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for users.
    /// </summary>
    public static class UserUtility
    {
        private static HttpClient userUtilityClient { get; } = new HttpClient();

        /// <summary>
        /// Gets the User Id that corresponds with the given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The matching User Id.</returns>
        /// <exception cref="ArgumentException">Invalid username provided.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<ulong> GetUserIdAsync(string username)
        {
            object request = new
            {
                usernames = new[] { username },
            };
            var content = JsonContent.Create(request);
            HttpResponseMessage response = await userUtilityClient.PostAsync($"{Constants.URL("users")}/v1/usernames/users", content);
            string body = await response.Content.ReadAsStringAsync();
            HttpVerify.ThrowIfNecessary(response, body);

            dynamic data = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (data.data.Count == 0)
            {
                throw new ArgumentException("Invalid username provided.");
            }
            return data.data[0].id;
        }

        public static async Task<ReadOnlyDictionary<string, ulong>> GetUserIdsAsync(IEnumerable<string> usernames)
        {
            object request = new
            {
                usernames = usernames.ToArray(),
            };
            var content = JsonContent.Create(request);
            HttpResponseMessage response = await userUtilityClient.PostAsync($"{Constants.URL("users")}/v1/usernames/users", content);
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
            return dict.AsReadOnly();
        }
    }
}
