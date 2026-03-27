using Newtonsoft.Json.Linq;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Interfaces;
using RoSharp.Structures.PurchaseTypes;
using System.Collections.ObjectModel;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// Represents a Roblox bundle.
    /// </summary>
    public class Bundle : APIMain, IRefreshable, IIdApi<Bundle>
    {
        /// <inheritdoc/>
        public ulong Id { get; }

        private string name;

        /// <summary>
        /// Gets the name of the bundle.
        /// </summary>
        public string Name => name;

        private BundleType bundleType;

        /// <summary>
        /// Gets the <see cref="Enums.BundleType"/> of this bundle.
        /// </summary>
        public BundleType BundleType => bundleType;

        private string description;

        /// <summary>
        /// Gets the description of the bundle.
        /// </summary>
        public string Description => description;

        private ReadOnlyCollection<Id<Asset>> assets;

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="Asset"/>s that are contained in the bundle.
        /// </summary>
        public ReadOnlyCollection<Id<Asset>> Assets => assets;

        private ulong ownerId;
        private string ownerName;
        private bool isCommunityOwned;

        /// <summary>
        /// Gets the unique Id (community or user) of the owner of this asset.
        /// </summary>
        public ulong OwnerId => ownerId;

        /// <summary>
        /// Gets the name (community or user) of the owner of this asset.
        /// </summary>
        public string OwnerName => ownerName;

        /// <summary>
        /// Gets whether or not this asset is owned by a community.
        /// </summary>
        /// <seealso cref="GetOwnerAsync"/>
        public bool IsCommunityOwned => isCommunityOwned;

        private DateTime created;
        private DateTime lastUpdated;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time this bundle was created.
        /// </summary>
        public DateTime Created => created;
        // TODO: Figure out how to get this added : public DateTime LastUpdated => lastUpdated;

        /// <summary>
        /// Gets whether or not this bundle is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        private ulong favorites;

        /// <summary>
        /// Gets this bundle's total amount of favorites.
        /// </summary>
        public ulong Favorites => favorites;

        private string thumbnailUrl;

        /// <summary>
        /// Returns this bundle's thumbnail. Equivalent to calling <see cref="GetThumbnailAsync(ThumbnailSize)"/> with <see cref="ThumbnailSize.S420x420"/>, except this value is cached.
        /// </summary>
        public string ThumbnailUrl => thumbnailUrl;


        private bool isLimited;

        /// <summary>
        /// Gets whether or not this asset is limited.
        /// </summary>
        public bool IsLimited => isLimited;

        private bool isLimitedUnique;

        /// <summary>
        /// Gets whether or not this asset is limited unique.
        /// </summary>
        public bool IsLimitedUnique => isLimitedUnique;

        private IPurchaseType purchaseInfo;

        /// <inheritdoc cref="IPurchaseType"/>
        public IPurchaseType PurchaseInfo => purchaseInfo;

        private SaleLocationType saleLocation;

        /// <summary>
        /// Gets the <see cref="Enums.SaleLocationType"/> of this bundle, indicating where it can be purchased.
        /// </summary>
        public SaleLocationType SaleLocation => saleLocation;

        private bool isRecolorable;

        /// <summary>
        /// Indicates whether or not parts of the bundle can be re-colored.
        /// </summary>
        public bool IsRecolorable => isRecolorable;


        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        /// <inheritdoc/>
        public string Url => $"https://www.roblox.com/bundles/{Id}/";

        private Bundle(ulong assetId, Session? session = null)
        {
            Id = assetId;

            if (session != null)
                AttachSession(session);
        }

        public static async Task<Bundle> FromId(ulong bundleId, Session? session = null)
        {
            if (RoPool<Bundle>.Contains(bundleId))
                return RoPool<Bundle>.Get(bundleId, session.Global());

            Bundle newUser = new(bundleId, session.Global());
            await newUser.RefreshAsync();

            RoPool<Bundle>.Add(newUser);

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            // 
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"/v1/catalog/items/{Id}/details?itemType=Bundle", Constants.URL("catalog"));

            string raw = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(raw);

            name = data.name;
            description = data.description;
            bundleType = (BundleType)Convert.ToInt32(data.bundleType);
            isRecolorable = (data.isRecolorable ?? false);
            favorites = data.favoriteCount;

            ownerId = data.creatorTargetId;
            ownerName = data.creatorName;
            isCommunityOwned = data.creatorType == "Group";

            thumbnailUrl = await GetThumbnailAsync(ThumbnailSize.S420x420);

            created = data.itemCreatedUtc;

            List<string> restrictions = [];
            foreach (dynamic item in data.itemRestrictions)
                restrictions.Add(Convert.ToString(item));

            isLimited = restrictions.Contains("Limited");
            isLimitedUnique = restrictions.Contains("LimitedUnique");

            List<Id<Asset>> items = [];

            foreach (dynamic item in data.bundledItems)
            {
                string type = item.type;
                if (type.Equals("Asset"))
                {
                    Id<Asset> id = new(Convert.ToUInt64(item.id), session);
                    items.Add(id);
                }
            }

            assets = items.AsReadOnly();

            if (data.priceStatus == "Free")
                purchaseInfo = new FreePurchase();
            else if (data.price != null)
                purchaseInfo = new RobuxPurchase() { Price = data.price };
            else
                purchaseInfo = new NotForSalePurchase();


            if (data.saleLocationType == null)
                saleLocation = SaleLocationType.NotApplicable;
            else
                saleLocation = Enum.Parse<SaleLocationType>(Convert.ToString(data.saleLocationType));
        }

        /// <summary>
        /// Returns the owner of this asset.
        /// </summary>
        /// <returns>A task containing the owner of this asset.</returns>
        public async Task<IAssetOwner> GetOwnerAsync()
        {
            if (IsCommunityOwned)
            {
                return await Community.FromId(OwnerId);
            }
            return await User.FromId(OwnerId);
        }

        /// <summary>
        /// Retrieves a thumbnail URL for this bundle.
        /// </summary>
        /// <param name="size">The size of the thumbnail.</param>
        /// <returns>A task containing a thumbnail URL.</returns>
        /// <exception cref="ArgumentException">Invalid bundle to get thumbnail for.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<string>> GetThumbnailAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/bundles/thumbnails?bundleIds={Id}&format=png&isCircular=false&size={size.ToString().Substring(1)}";
            var response = await SendAsync(HttpMethod.Get, url, Constants.URL("thumbnails"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid bundle to get thumbnail for.");
            return new(response, Convert.ToString(data.data[0].imageUrl));
        }

        /// <inheritdoc/>
        public Bundle AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Bundle {Name} [{Id}] ({BundleType}) {{{(!IsCommunityOwned ? "@" : string.Empty)}{OwnerName}}} <{PurchaseInfo}>";
        }
    }
}
