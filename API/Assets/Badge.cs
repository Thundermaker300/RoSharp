using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Interfaces;
using RoSharp.Structures;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A class that represents a Roblox badge.
    /// </summary>
    public class Badge : APIMain, IRefreshable, IIdApi<Badge>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("badges");

        /// <summary>
        /// Gets the unique Id of the badge.
        /// </summary>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/badges/{Id}/";

        private string name;

        /// <summary>
        /// Gets the name of the badge.
        /// </summary>
        public string Name => name;

        private string description;

        /// <summary>
        /// Gets the description of the badge.
        /// </summary>
        public string Description => description;

        private Id<Experience> experience;

        /// <summary>
        /// Gets a <see cref="Id{T}"/> of the experience that owns this badge.
        /// </summary>
        public Id<Experience> Experience => experience;

        private DateTime created;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the creation date of the badge.
        /// </summary>
        public DateTime Created => created;

        private DateTime lastUpdated;

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the badge was updated last.
        /// </summary>
        public DateTime LastUpdated => lastUpdated;

        /// <summary>
        /// Gets whether or not this badge is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        private bool isEnabled;

        /// <summary>
        /// Gets whether or not the badge is enabled and can be awarded.
        /// </summary>
        public bool IsEnabled => isEnabled;

        private int awardedCount;

        /// <summary>
        /// Gets the total amount of times this badge has been awarded.
        /// </summary>
        public int AwardedCount => awardedCount;

        private int yesterdayAwardedCount;

        /// <summary>
        /// Gets the amount of times this badge was awarded yesterday.
        /// </summary>
        public int YesterdayAwardedCount => yesterdayAwardedCount;

        private Id<Asset> thumbnailAsset;

        /// <summary>
        /// Gets a <see cref="Id{T}"/> representing the asset that is used for this badge.
        /// </summary>
        /// <remarks>This value will be <see langword="null"/> if this <see cref="Badge"/> is created without an authenticated <see cref="Session"/> as Asset instances require an authenticated session.</remarks>
        public Id<Asset> ThumbnailAsset => thumbnailAsset;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private Badge(ulong assetId, Session? session = null)
        {
            Id = assetId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<Badge>.Contains(Id))
                RoPool<Badge>.Add(this);
        }

        /// <summary>
        /// Returns a <see cref="Badge"/> given its unique Id.
        /// </summary>
        /// <param name="badgeId">The badge Id.</param>
        /// <param name="session">The session, optional.</param>
        /// <returns>A task containing the <see cref="Badge"/> upon completion.</returns>
        /// <exception cref="ArgumentException">If the asset Id invalid.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public static async Task<Badge> FromId(ulong badgeId, Session? session = null)
        {
            if (RoPool<Badge>.Contains(badgeId))
                return RoPool<Badge>.Get(badgeId, session.Global());

            Badge newUser = new(badgeId, session.Global());
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"/v1/badges/{Id}");
            string raw = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(raw);

            ulong experienceId = Convert.ToUInt64(data.awardingUniverse.id);

            name = data.displayName;
            description = (data.displayDescription == null ? string.Empty : data.displayDescription);
            created = data.created;
            lastUpdated = data.updated;
            experience = new Id<Experience>(experienceId);
            awardedCount = Convert.ToInt32(data.statistics.awardedCount);
            yesterdayAwardedCount = Convert.ToInt32(data.statistics.pastDayAwardedCount);
            isEnabled = data.enabled;

            thumbnailAsset = new Id<Asset>(Convert.ToUInt64(data.displayIconImageId), session);

            RefreshedAt = DateTime.Now;
        }

        /// <summary>
        /// Retrieves a thumbnail URL for this badge.
        /// </summary>
        /// <returns>A task containing a thumbnail URL.</returns>
        /// <exception cref="ArgumentException">Invalid asset to get thumbnail for.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        public async Task<HttpResult<string>> GetThumbnailAsync()
        {
            string url = $"/v1/badges/icons?badgeIds={Id}&size=150x150&format=Png&isCircular=false";
            var response = await SendAsync(HttpMethod.Get, url, Constants.URL("thumbnails"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid badge to get thumbnail for.");
            return new(response, Convert.ToString(data.data[0].imageUrl));
        }

        /// <summary>
        /// Modifies a badge.
        /// </summary>
        /// <param name="options">A class representing the options to use to modify the badge.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyAsync(BadgeModifyOptions options)
        {
            var message = new HttpMessage(HttpMethod.Patch, $"/v1/badges/{Id}", new
            {
                name = options.Name ?? Name,
                description = options.Description ?? Description,
                enabled = options.IsEnabled ?? IsEnabled,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyAsync),
            };

            return new(await SendAsync(message));
        }

        /// <summary>
        /// Gets whether or not this badge is owned by the given <see cref="User"/>.
        /// </summary>
        /// <param name="target">The user to check.</param>
        /// <returns>A task which contains a boolean when completed.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <seealso cref="User.HasBadgeAsync(Badge)"/>
        /// <seealso cref="User.HasBadgeAsync(ulong)"/>
        public async Task<HttpResult<bool>> IsOwnedByAsync(User target) => await target.HasBadgeAsync(this);

        /// <summary>
        /// Gets whether or not this badge is owned by the user with the given user Id.
        /// </summary>
        /// <param name="targetId">The user ID to check.</param>
        /// <returns>A task which contains a boolean when completed.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <seealso cref="User.HasBadgeAsync(Badge)"/>
        /// <seealso cref="User.HasBadgeAsync(ulong)"/>
        public async Task<HttpResult<bool>> IsOwnedByAsync(ulong targetId) => await IsOwnedByAsync(await User.FromId(targetId, session));

        /// <summary>
        /// Gets whether or not this badge is owned by the user with the given username.
        /// </summary>
        /// <param name="targetUsername">The username to check.</param>
        /// <returns>A task which contains a boolean when completed.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <seealso cref="User.HasBadgeAsync(Badge)"/>
        /// <seealso cref="User.HasBadgeAsync(ulong)"/>
        public async Task<HttpResult<bool>> IsOwnedByAsync(string targetUsername) => await IsOwnedByAsync(await User.FromUsername(targetUsername, session));

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Badge {Name} [{Id}] {{EXP:{experience.UniqueId}}} <{AwardedCount}>";
        }
        
        /// <inheritdoc/>
        public Badge AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    /// <summary>
    /// Specifies the options to use for badge modification.
    /// </summary>
    /// <remarks>Any property in this class that is not changed will not modify the website.</remarks>
    public class BadgeModifyOptions
    {
        /// <summary>
        /// The new name for the badge.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The new description for the badge.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The new enabled state for the badge.
        /// </summary>
        public bool? IsEnabled { get; set; }
    }
}
