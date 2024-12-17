using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.Exceptions;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Collections.ObjectModel;

namespace RoSharp.API
{
    /// <summary>
    /// API for getting data and interacting with the Roblox Marketplace.
    /// </summary>
    public static class MarketplaceAPI
    {
        private static ReadOnlyCollection<MarketplaceCategory> categories;

        /// <summary>
        /// Gets all the categories on the marketplace.
        /// </summary>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="MarketplaceCategory"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<ReadOnlyCollection<MarketplaceCategory>> GetCategoriesAsync()
        {
            if (categories == null)
            {
                HttpMessage message = new(HttpMethod.Get, $"{Constants.URL("catalog")}/v1/search/navigation-menu-items");
                string rawData = await HttpManager.SendStringAsync(null, message);
                dynamic data = JObject.Parse(rawData);

                List<MarketplaceCategory> cats = [];
                foreach (dynamic item in data.categories)
                {
                    List<MarketplaceCategory> subcats = [];
                    foreach (dynamic subitem in item.subcategories)
                    {
                        MarketplaceCategory cat2 = new()
                        {
                            Category = subitem.subcategory,
                            Name = subitem.name,
                            ShortName = subitem.shortName,
                            Id = subitem.subcategoryId,
                            IsSubcategory = true,
                            Subcategories = new List<MarketplaceCategory>(0).AsReadOnly(),
                            ParentCategoryId = item.categoryId,
                        };

                        subcats.Add(cat2);
                    }
                    MarketplaceCategory cat = new()
                    {
                        Category = item.category,
                        Name = item.name,
                        ShortName = null,
                        ParentCategoryId = null,
                        Id = item.categoryId,
                        IsSubcategory = false,
                        Subcategories = subcats.AsReadOnly(),
                    };
                    cats.Add(cat);
                }
                categories = cats.AsReadOnly();
            }
            return categories;
        }

        /// <summary>
        /// Gets the category with the given Id.
        /// </summary>
        /// <param name="categoryId">The unique category Id.</param>
        /// <returns>A task containing a <see cref="MarketplaceCategory"/> if found successfully.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<MarketplaceCategory?> GetCategoryAsync(int categoryId)
        {
            foreach (MarketplaceCategory category in await GetCategoriesAsync())
            {
                if (category.Id == categoryId)
                    return category;
            }
            return null;
        }

        /// <summary>
        /// Gets the category with the given name.
        /// </summary>
        /// <param name="categoryName">The unique category name.</param>
        /// <returns>A task containing a <see cref="MarketplaceCategory"/> if found successfully.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<MarketplaceCategory?> GetCategoryAsync(string categoryName)
        {
            foreach (MarketplaceCategory category in await GetCategoriesAsync())
            {
                if (category.Name == categoryName || category.Category == categoryName)
                    return category;
            }
            return null;
        }

        /// <summary>
        /// Gets the subcategory with the given Id.
        /// </summary>
        /// <param name="categoryId">The unique subcategory Id.</param>
        /// <returns>A task containing a <see cref="MarketplaceCategory"/> if found successfully.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<MarketplaceCategory?> GetSubCategoryAsync(int categoryId)
        {
            foreach (MarketplaceCategory category in await GetCategoriesAsync())
            {
                foreach (MarketplaceCategory sub in category.Subcategories)
                {
                    if (sub.Id == categoryId)
                        return sub;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the subcategory with the given name.
        /// </summary>
        /// <param name="categoryName">The unique subcategory name.</param>
        /// <returns>A task containing a <see cref="MarketplaceCategory"/> if found successfully.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<MarketplaceCategory?> GetSubCategoryAsync(string categoryName)
        {
            foreach (MarketplaceCategory category in await GetCategoriesAsync())
            {
                foreach (MarketplaceCategory sub in category.Subcategories)
                {
                    if (sub.Name == categoryName || sub.Category == categoryName)
                        return sub;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Represents a category on the Roblox marketplace.
    /// </summary>
    public class MarketplaceCategory
    {
        /// <summary>
        /// Gets the URL that is used in <see cref="SearchAsync(string, Session?, byte, string?)"/>.
        /// </summary>
        public string SearchUrl
        {
            get
            {
                if (IsSubcategory)
                    return $"{Constants.URL("catalog")}/v1/search/items?category={ParentCategoryId}&subcategory={Id}";
                else
                    return $"{Constants.URL("catalog")}/v1/search/items?category={Id}";
            }
        }

        /// <summary>
        /// Gets the internal name of the category.
        /// </summary>
        public string Category { get; init; }

        /// <summary>
        /// Gets the display name of the category.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the short name of the category. Not all categories have short names.
        /// </summary>
        public string? ShortName { get; init; }

        /// <summary>
        /// Gets the Id of the parent category, if <see cref="IsSubcategory"/> is <see langword="true"/>.
        /// </summary>
        public int? ParentCategoryId { get; init; }

        /// <summary>
        /// Gets the Id of this category. Note that categories and subcategories can have the same Id, but no two categories and no two subcategories will have the same Id.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets whether or not this is a subcategory.
        /// </summary>
        public bool IsSubcategory { get; init; }

        /// <summary>
        /// Gets this category's subcategories. Will always be empty if <see cref="IsSubcategory"/> is <see langword="false"/>.
        /// </summary>
        public ReadOnlyCollection<MarketplaceCategory> Subcategories { get; init; }

        /// <summary>
        /// Searches the Roblox marketplace in this category. Equivalent to <see cref="SearchAPI.SearchMarketplaceAsync(MarketplaceCategory, string, Session?, byte, string?)"/> with this category.
        /// </summary>
        /// <param name="query">The query to search. Can be <see langword="null"/> to show the top items of the category instead.</param>
        /// <param name="session">The logged-in session, optional.</param>
        /// <param name="limit">The limit of items to return. Must be one of the following values or the Roblox API will error: 10, 28, 30, 50, 60, 100, 120.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or invalid <paramref name="limit"/>.</exception>
        /// <seealso cref="MarketplaceAPI.GetCategoriesAsync"/>
        /// <seealso cref="MarketplaceAPI.GetCategoryAsync(string)"/>
        /// <seealso cref="SearchAPI.SearchMarketplaceAsync(MarketplaceCategory, string, Session?, byte, string?)"/>
        public async Task<PageResponse<Id<Asset>>> SearchAsync(string? query, Session? session = null, byte limit = 30, string? cursor = null)
            => await SearchAPI.SearchMarketplaceAsync(this, query, session, limit, cursor);

        internal MarketplaceCategory() { }
    }
}
