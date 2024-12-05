using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<PageResponse<DataStore>> ListDataStoresAsync(string? cursor = null, string scope = "global")
        {
            SessionVerify.ThrowAPIKeyIfNecessary(experience.session, "DataStoreManager.ListDataStoresAsync", "universe-datastores.objects:list");
            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores?limit=10&scope={scope}";
            if (cursor != null)
                url += $"&cursor={cursor}";

            string rawData = await experience.GetStringAsync(url, Constants.URL("apis"));
            dynamic data = JObject.Parse(rawData);

            string? nextPage = data.nextPageCursor;
            List<DataStore> dataStores = new List<DataStore>();

            foreach (dynamic dataStore in data.datastores)
            {
                dataStores.Add(new DataStore(this)
                {
                    Name = dataStore.name,
                    Created = dataStore.createdTime,
                    Scope = scope,
                });
            }
            return new(dataStores, nextPage, null);
        }

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

        public async Task<DataStoreEntry?> GetKeyAsync(string dataStoreName, string key, string scope = "global")
        {
            SessionVerify.ThrowAPIKeyIfNecessary(experience.session, "DataStore.GetAsync", "universe-datastores.objects:read");
            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores/datastore/entries/entry"
                + $"?dataStoreName={dataStoreName}"
                + $"&entryKey={key}"
                + $"&scope={scope}";

            HttpResponseMessage res = await experience.GetAsync(url, Constants.URL("apis"));

            if (res.StatusCode == HttpStatusCode.NoContent)
                return null;

            string rawData = await res.Content.ReadAsStringAsync();

            DateTime created = DateTime.UnixEpoch;
            if (res.Headers.TryGetValues("roblox-entry-created-time", out var createdHeaders))
                created = DateTime.Parse(createdHeaders.First());

            DateTime updated = DateTime.UnixEpoch;
            if (res.Headers.TryGetValues("last-modified", out var updatedHeaders))
                created = DateTime.Parse(updatedHeaders.First());

            List<Id<User>> ids = new();
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

            return new DataStoreEntry(this)
            {
                Key = key,
                Created = created,
                Updated = updated,
                Version = res.Headers.GetValues("roblox-entry-version")?.FirstOrDefault() ?? "0",
                UserIds = ids.AsReadOnly(),
                Content = rawData,
            };
        }

        public async Task SetKeyAsync(string dataStoreName, string key, object newContent, string scope = "global")
        {
            SessionVerify.ThrowAPIKeyIfNecessary(experience.session, "DataStore.SetAsync", "universe-datastores.objects:create");
            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores/datastore/entries/entry"
                + $"?dataStoreName={dataStoreName}"
                + $"&entryKey={key}"
                + $"&scope={scope}";

            await experience.PostAsync(url, newContent, Constants.URL("apis"));
        }

        public async Task DeleteKeyAsync(string dataStoreName, string key, string scope = "global")
        {
            SessionVerify.ThrowAPIKeyIfNecessary(experience.session, "DataStore.DeleteAsync", "universe-datastores.objects:delete");
            string url = $"/datastores/v1/universes/{experience.UniverseId}/standard-datastores/datastore/entries/entry"
                + $"?dataStoreName={dataStoreName}"
                + $"&entryKey={key}"
                + $"&scope={scope}";

            await experience.DeleteAsync(url, Constants.URL("apis"));
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
        /// Gets the scope of the datastore.
        /// </summary>
        public string Scope { get; init; }

        internal DataStore(DataStoreManager manager) { this.manager = manager; }

        /// <summary>
        /// Gets a key within the datastore.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A task containing a <see cref="DataStoreEntry"/> representing the key. Can be <see langword="null"/>.</returns>
        public async Task<DataStoreEntry?> GetAsync(string key)
            => await manager.GetKeyAsync(Name, key, Scope);

        /// <summary>
        /// Sets a key within the datastore.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The new value.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task SetAsync(string key, object value)
            => await manager.SetKeyAsync(Name, key, value, Scope);

        /// <summary>
        /// Deletes a key within the datastore.
        /// </summary>
        /// <param name="key">The key to delete.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task DeleteAsync(string key)
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
