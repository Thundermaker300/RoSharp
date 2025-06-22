using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Http;
using RoSharp.Structures;
using RoSharp.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// Manager class for reading and modifying data within an experience's <c>DataStoreService</c>.
    /// </summary>
    public class DataStoreManager
    {
        internal Experience experience;

        internal DataStoreManager(Experience exp)
        {
            ArgumentNullException.ThrowIfNull(exp, nameof(exp));
            experience = exp;
        }

        /// <summary>
        /// Gets a list of datastores within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:list</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="DataStore"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<EnumerableHttpResult<PageResponse<DataStore>>> ListDataStoresAsync(string? cursor = null, string scope = "global")
        {
            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores?limit=10&scope={scope}";
            if (cursor != null)
                url += $"&cursor={cursor}";

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.ApiKey,
                ApiName = nameof(ListDataStoresAsync),
                ApiKeyPermission = "universe-datastores.objects:list"
            };

            var response = await experience.SendAsync(message, Constants.URL("apis"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);

            string? nextPage = data.nextPageCursor;
            List<DataStore> dataStores = [];

            foreach (dynamic dataStore in data.datastores)
            {
                dataStores.Add(new DataStore(this)
                {
                    Name = dataStore.name,
                    Created = dataStore.createdTime,
                    Scope = scope,
                });
            }
            return new(response, new(dataStores, nextPage, null));
        }

        /// <summary>
        /// Gets a specific datastore within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:list</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="name">The name of the datastore, cannot be <see langword="null"/>.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task containing a <see cref="DataStore"/> upon completion. Can be <see langword="null"/> if there is no data.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="ArgumentNullException">Name cannot be null.</exception>
        public async Task<DataStore?> GetDataStoreAsync(string name, string scope = "global")
        {
            SessionVerify.ThrowAPIKeyIfNecessary(experience.session, "DataStoreManager.GetDataStoreAsync", "universe-datastores.objects:list");
            ArgumentNullException.ThrowIfNullOrWhiteSpace(name, nameof(name));

            DataStore? match = null;
            string? cursor = null;
            while (cursor != string.Empty && match == null)
            {
                PageResponse<DataStore> page = await ListDataStoresAsync(cursor, scope);
                if (page != null)
                {
                    match = page.FirstOrDefault(ds => ds.Name == name);
                    cursor = page.NextPageCursor;
                }
                else
                {
                    cursor = string.Empty;
                }
            }
            return match;
        }

        /// <summary>
        /// Gets a specific key from a datastore within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:read</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="dataStoreName">The name of the datastore.</param>
        /// <param name="key">The key of the data.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task containing <see cref="DataStoreEntry"/> upon completion. Can be <see langword="null"/> if there is no data.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="ArgumentNullException">dataStoreName and key cannot be null.</exception>
        public async Task<HttpResult<DataStoreEntry?>> GetKeyAsync(string dataStoreName, string key, string scope = "global")
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(dataStoreName, nameof(dataStoreName));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores/datastore/entries/entry"
                + $"?dataStoreName={dataStoreName}"
                + $"&entryKey={key}"
                + $"&scope={scope}";

            var message = new HttpMessage(HttpMethod.Get, url)
            {
                AuthType = AuthType.ApiKey,
                ApiName = nameof(GetKeyAsync),
                ApiKeyPermission = "universe-datastores.objects:read"
            };

            HttpResponseMessage res = await experience.SendAsync(message, Constants.URL("apis"));

            if (res.StatusCode == HttpStatusCode.NoContent)
                return new(res, null);

            string rawData = await res.Content.ReadAsStringAsync();

            DateTime created = DateTime.UnixEpoch;
            if (res.Headers.TryGetValues("roblox-entry-created-time", out var createdHeaders))
                created = DateTime.Parse(createdHeaders.First());

            DateTime updated = DateTime.UnixEpoch;
            if (res.Headers.TryGetValues("last-modified", out var updatedHeaders))
                created = DateTime.Parse(updatedHeaders.First());

            List<Id<User>> ids = [];
            if (res.Headers.TryGetValues("roblox-entry-userids", out var userIdHeaders))
            {
                string idsRaw = userIdHeaders.First();
                ulong[]? ulongIds = JsonConvert.DeserializeObject<ulong[]>(idsRaw);
                if (ulongIds != null)
                {
                    foreach (ulong id in ulongIds)
                        ids.Add(new(id, experience.session));
                }
            }

            return new(res, new DataStoreEntry(this)
            {
                Key = key,
                Created = created,
                Updated = updated,
                Version = res.Headers.GetValues("roblox-entry-version")?.FirstOrDefault() ?? "0",
                UserIds = ids.AsReadOnly(),
                Content = rawData,
            });
        }

        /// <summary>
        /// Sets a specific key from a datastore within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:create</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="dataStoreName">The name of the datastore.</param>
        /// <param name="key">The key of the data.</param>
        /// <param name="newContent">The new content to set. Can be any type except <see langword="null"/> and will be converted automatically.</param>
        /// <param name="userIds">A list of user Ids to associate with this datastore.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="ArgumentNullException">dataStoreName, key, and newContent cannot be null.</exception>
        public async Task<HttpResult> SetKeyAsync(string dataStoreName, string key, object newContent, IList<ulong> userIds, string scope = "global")
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(dataStoreName, nameof(dataStoreName));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNull(newContent, nameof(newContent));

            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores/datastore/entries/entry"
                + $"?dataStoreName={dataStoreName}"
                + $"&entryKey={key}"
                + $"&scope={scope}";

            HttpMessage message = new(HttpMethod.Post, url, newContent)
            {
                AuthType = AuthType.ApiKey,
                ApiName = nameof(SetKeyAsync),
                ApiKeyPermission = "universe-datastores.objects:create"
            };

            if (userIds.Count > 0)
                message.Headers.Add("roblox-entry-userids", [JsonConvert.SerializeObject(userIds)] );

            return new(await experience.SendAsync(message, Constants.URL("apis")));
        }

        /// <summary>
        /// Sets a specific key from a datastore within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:create</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="dataStoreName">The name of the datastore.</param>
        /// <param name="key">The key of the data.</param>
        /// <param name="newContent">The new content to set. Can be any type except <see langword="null"/> and will be converted automatically.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="ArgumentNullException">dataStoreName and key cannot be null.</exception>
        public async Task<HttpResult> SetKeyAsync(string dataStoreName, string key, object newContent, string scope = "global")
            => await SetKeyAsync(dataStoreName, key, newContent, new List<ulong>(0), scope);

        /// <summary>
        /// Sets a specific key from a datastore within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:create</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="dataStoreName">The name of the datastore.</param>
        /// <param name="key">The key of the data.</param>
        /// <param name="newContent">The new content to set. Can be any type except <see langword="null"/> and will be converted automatically.</param>
        /// <param name="users">A list of users to associate with this datastore.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="ArgumentNullException">dataStoreName and key cannot be null.</exception>
        public async Task<HttpResult> SetKeyAsync(string dataStoreName, string key, object newContent, IList<User> users, string scope = "global")
            => await SetKeyAsync(dataStoreName, key, newContent, users.Select(u => u.Id).ToList(), scope);

        /// <summary>
        /// Deletes a specific key from a datastore within this experience.
        /// <para>
        /// An attached API key with the <c>universe-datastores.objects:delete</c> permission is required to use this method.
        /// </para>
        /// </summary>
        /// <param name="dataStoreName">The name of the datastore.</param>
        /// <param name="key">The key of the data.</param>
        /// <param name="scope">The scope of the request. Defaults to <c>global</c>.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <exception cref="ArgumentNullException">dataStoreName and key cannot be null.</exception>
        public async Task<HttpResult> DeleteKeyAsync(string dataStoreName, string key, string scope = "global")
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(dataStoreName, nameof(dataStoreName));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores/datastore/entries/entry"
                + $"?dataStoreName={dataStoreName}"
                + $"&entryKey={key}"
                + $"&scope={scope}";

            var message = new HttpMessage(HttpMethod.Delete, url)
            {
                AuthType = AuthType.ApiKey,
                ApiName = nameof(DeleteKeyAsync),
                ApiKeyPermission = "universe-datastores.objects:delete"
            };

            return new(await experience.SendAsync(message, Constants.URL("apis")));
        }
    }

    /// <summary>
    /// Represents a data store within an experience.
    /// </summary>
    public class DataStore
    {
        private DataStoreManager manager;

        /// <summary>
        /// Gets the name of the data store.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the time the data store was created.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// Gets whether or not this data store is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        /// <summary>
        /// Gets the scope of the datastore.
        /// </summary>
        public string Scope { get; init; }

        internal DataStore(DataStoreManager manager) { this.manager = manager; }

        /// <summary>
        /// Gets a key within the datastore.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A task containing a <see cref="DataStoreEntry"/> representing the key. Can be <see langword="null"/>.</returns>
        public async Task<HttpResult<DataStoreEntry?>> GetAsync(string key)
            => await manager.GetKeyAsync(Name, key, Scope);

        /// <summary>
        /// Sets a key within the datastore.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value. Can be any type except <see langword="null"/> and will be converted automatically</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> SetAsync(string key, object value)
            => await manager.SetKeyAsync(Name, key, value, Scope);

        /// <summary>
        /// Sets a key within the datastore.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value.</param>
        /// <param name="userIds">A list of user Ids to associate with the datastore.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> SetAsync(string key, object value, IList<ulong> userIds)
            => await manager.SetKeyAsync(Name, key, value, userIds, Scope);

        /// <summary>
        /// Sets a key within the datastore.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value.</param>
        /// <param name="users">A list of users to associate with the datastore.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> SetAsync(string key, object value, IList<User> users)
            => await manager.SetKeyAsync(Name, key, value, users, Scope);

        /// <summary>
        /// Deletes a key within the datastore.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> DeleteAsync(string key)
            => await manager.DeleteKeyAsync(Name, key, Scope);
    }

    /// <summary>
    /// Represents a key with a value within a <see cref="DataStore"/>.
    /// </summary>
    public class DataStoreEntry
    {
        private DataStoreManager manager;

        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        public string Key { get; init; }

        /// <summary>
        /// Gets when the key was created.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// Gets the last time the key was updated.
        /// </summary>
        public DateTime Updated { get; init; }

        /// <summary>
        /// Gets whether or not this data store entry is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        /// <summary>
        /// Gets the current Roblox-defined version of the key.
        /// </summary>
        public string Version { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="Id{T}"/> of <see cref="User"/>s that are associated with this API key.
        /// </summary>
        public ReadOnlyCollection<Id<User>> UserIds { get; init; }
        
        /// <summary>
        /// Gets the content in this key.
        /// </summary>
        public string Content { get; init; }

        internal DataStoreEntry(DataStoreManager manager) { this.manager = manager; }
    }
}
