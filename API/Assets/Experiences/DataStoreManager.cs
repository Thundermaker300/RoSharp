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

            dynamic rawData;
            try
            {
                // Parse if it's an object
                rawData = JObject.Parse(await res.Content.ReadAsStringAsync());
            }
            catch
            {
                rawData = await res.Content.ReadAsStringAsync();
            }

            DateTime created = DateTime.UnixEpoch;
            if (res.Headers.TryGetValues("roblox-entry-created-time", out var createdHeaders))
                created = DateTime.Parse(createdHeaders.First());

            DateTime updated = DateTime.UnixEpoch;
            if (res.Headers.TryGetValues("last-modified", out var updatedHeaders))
                created = DateTime.Parse(updatedHeaders.First());

            return new DataStoreEntry(this)
            {
                Key = key,
                Created = created,
                Updated = updated,
                Version = res.Headers.GetValues("roblox-entry-version")?.FirstOrDefault() ?? "0",
                UserIds = null,
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

    public class DataStore
    {
        private DataStoreManager manager;

        public string Name { get; init; }
        public DateTime Created { get; init; }
        public string Scope { get; init; }

        internal DataStore(DataStoreManager manager) { this.manager = manager; }

        public async Task<DataStoreEntry?> GetAsync(string key)
            => await manager.GetKeyAsync(Name, key, Scope);

        public async Task SetAsync(string key, object value)
            => await manager.SetKeyAsync(Name, key, value, Scope);

        public async Task DeleteAsync(string key)
            => await manager.DeleteKeyAsync(Name, key, Scope);
    }

    public class DataStoreEntry
    {
        private DataStoreManager manager;

        public string Key { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public string Version { get; init; }

        /// <summary>
        /// Not supported. TODO
        /// </summary>
        public ReadOnlyCollection<Id<User>> UserIds { get; init; }
        public dynamic Content { get; init; }

        internal DataStoreEntry(DataStoreManager manager) { this.manager = manager; }
    }
}
