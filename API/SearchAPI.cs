using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Communities;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Structures;
using RoSharp.Utility;

namespace RoSharp.API
{
    /// <summary>
    /// API for searching through Roblox's database.
    /// </summary>
    public static class SearchAPI
    {
        /// <summary>
        /// Searches Roblox experiences and returns results based on the <paramref name="query"/>.
        /// <para>
        /// Roblox uses an advanced algorithm to determine what results are shown. This algorithm usually combines the <paramref name="query"/> with other factors like visits, active player counts, etc. However, setting <paramref name="exactMatchSearch"/> to <see langword="false"/> will bypass this algorithm and search for experiences strictly by name alone.
        /// </para>
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="exactMatchSearch">Indicates whether or not to perform an 'exact search', which will match results by name alone and no other criteria. In other words, the returned results are guaranteed to contain the <paramref name="query"/> within their title.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<PageResponse<Id<Experience>>> SearchExperiencesAsync(string query, Session? session = null, bool exactMatchSearch = false, string? cursor = null)
        {
            if (exactMatchSearch)
                query = $"\"{query}\"";

            string url = $"{Constants.URL("apis")}/search-api/omni-search?verticalType=game&searchQuery={query}&sessionId={GameAPI.SessionId}&pageType=all";
            if (cursor != null)
                url += $"&pageToken={cursor}";

            HttpMessage message = new(HttpMethod.Get, url);
            string body = await HttpManager.SendStringAsync(session, message);

            List<Id<Experience>> list = [];
            dynamic data = JObject.Parse(body);
            foreach (dynamic item in data.searchResults)
            {
                if (item.contentGroupType != "Game")
                    continue;

                ulong universeId = item.contents[0].universeId;
                list.Add(new(universeId, session));
            }

            string token = data.nextPageToken;
            return new(list, token, null);
        }

        /// <summary>
        /// Searches Roblox users and returns results based on the <paramref name="query"/>.
        /// <para>
        /// Roblox uses an advanced algorithm to determine what results are shown. This algorithm usually combines the <paramref name="query"/> with other factors like visits, verified status, etc.
        /// </para>
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<PageResponse<Id<User>>> SearchUsersAsync(string query, Session? session = null, string? cursor = null)
        {

            string url = $"{Constants.URL("apis")}/search-api/omni-search?verticalType=user&searchQuery={query}&sessionId={GameAPI.SessionId}";
            if (cursor != null)
                url += $"&pageToken={cursor}";

            HttpMessage message = new(HttpMethod.Get, url);
            string body = await HttpManager.SendStringAsync(session, message);

            List<Id<User>> list = [];
            dynamic data = JObject.Parse(body);

            if (data.searchResults.Count == 0)
                return PageResponse<Id<User>>.Empty;

            dynamic results = data.searchResults[0];
            foreach (dynamic item in results.contents)
            {
                if (item.contentType != "User")
                    continue;

                ulong universeId = item.contentId;
                list.Add(new(universeId, session));
            }

            string token = data.nextPageToken;
            return new(list, token, null);
        }

        /// <summary>
        /// Searches Roblox communities and returns results based on the <paramref name="query"/>.
        /// <para>
        /// Roblox uses an advanced algorithm to determine what results are shown. This algorithm usually combines the <paramref name="query"/> with other factors like members, activity, etc. Setting <paramref name="prioritizeExactMatch"/> will prioritize finding a community by name alone first.
        /// </para>
        /// </summary>
        /// <param name="query">The query to search.</param>
        /// <param name="session">Logged in session, optional.</param>
        /// <param name="limit">The limit of communities to return.</param>
        /// <param name="prioritizeExactMatch">Indicates whether or not to prioritize exact matches first.</param>
        /// <param name="cursor">The cursor to use to advance to the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Error from the Roblox API.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public static async Task<PageResponse<Id<Community>>> SearchCommunitiesAsync(string query, Session? session = null, FixedLimit limit = FixedLimit.Limit100, bool prioritizeExactMatch = false, string? cursor = null)
        {
            string url = $"{Constants.URL("groups")}/v1/groups/search?keyword={query}&prioritizeExactMatch={prioritizeExactMatch}&limit={limit.Limit()}";
            if (cursor != null)
                url += "&cursor=" + cursor;

            HttpMessage message = new(HttpMethod.Get, url);
            string rawData = await HttpManager.SendStringAsync(session, message);
            dynamic data = JObject.Parse(rawData);

            List<Id<Community>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<Community>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Searches the Roblox marketplace.
        /// </summary>
        /// <param name="category">The category to search in.</param>
        /// <param name="query">The query to search. Can be <see langword="null"/> to show the top items of the category instead.</param>
        /// <param name="session">The logged-in session, optional.</param>
        /// <param name="limit">The limit of items to return. Must be one of the following values or the Roblox API will error: 10, 28, 30, 50, 60, 100, 120.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or invalid <paramref name="limit"/>.</exception>
        /// <seealso cref="MarketplaceAPI.GetCategoriesAsync"/>
        /// <seealso cref="MarketplaceAPI.GetCategoryAsync(string)"/>
        /// <seealso cref="MarketplaceCategory.SearchAsync(string, Session?, byte, string?)"/>
        public static async Task<PageResponse<Id<Asset>>> SearchMarketplaceAsync(MarketplaceCategory category, string? query, Session? session = null, byte limit = 30, string? cursor = null)
        {
            ArgumentNullException.ThrowIfNull(category, nameof(category));

            string url = category.SearchUrl + $"&limit={limit}";
            if (query != null)
                url += $"&keyword={query}";
            if (cursor != null)
                url += $"&cursor={cursor}";

            HttpMessage message = new(HttpMethod.Get, url);
            string rawData = await HttpManager.SendStringAsync(session, message);
            dynamic data = JObject.Parse(rawData);

            List<Id<Asset>> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                ulong id = Convert.ToUInt64(item.id);
                list.Add(new(id, session));
            }

            return new PageResponse<Id<Asset>>(list, nextPage, previousPage);
        }

        /// <summary>
        /// Searches the Roblox marketplace.
        /// </summary>
        /// <param name="category">The name of the category or subcategory to search in.</param>
        /// <param name="query">The query to search. Can be <see langword="null"/> to show the top items of the category instead.</param>
        /// <param name="session">The logged-in session, optional.</param>
        /// <param name="limit">The limit of items to return. Must be one of the following values or the Roblox API will error: 10, 28, 30, 50, 60, 100, 120.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or invalid <paramref name="limit"/>.</exception>
        /// <exception cref="ArgumentException">Invalid category.</exception>
        public static async Task<PageResponse<Id<Asset>>> SearchMarketplaceAsync(string category, string? query, Session? session = null, byte limit = 30, string? cursor = null)
        {
            MarketplaceCategory categoryObject = await MarketplaceAPI.GetCategoryAsync(category) ?? await MarketplaceAPI.GetSubCategoryAsync(category) ?? throw new ArgumentException("Invalid category.");
            return await SearchMarketplaceAsync(categoryObject, query, session, limit, cursor);
        }

        /// <summary>
        /// Searches the Roblox marketplace.
        /// </summary>
        /// <param name="assetType">The type of assets to search.</param>
        /// <param name="query">The query to search. Can be <see langword="null"/> to show the top items of the category instead.</param>
        /// <param name="session">The logged-in session, optional.</param>
        /// <param name="limit">The limit of items to return. Must be one of the following values or the Roblox API will error: 10, 28, 30, 50, 60, 100, 120.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or invalid <paramref name="limit"/>.</exception>
        /// <exception cref="ArgumentException">Invalid category.</exception>
        public static async Task<PageResponse<Id<Asset>>> SearchMarketplaceAsync(AssetType assetType, string? query, Session? session = null, byte limit = 30, string? cursor = null)
        {
            MarketplaceCategory categoryObject = await MarketplaceAPI.GetCategoryAsync(assetType) ?? throw new ArgumentException("Invalid asset type.");
            return await SearchMarketplaceAsync(categoryObject, query, session, limit, cursor);
        }
    }
}
