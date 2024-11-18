﻿using Newtonsoft.Json.Linq;
using RoSharp.API.Pooling;
using RoSharp.Exceptions;
using RoSharp.Interfaces;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A class that represents a Roblox badge.
    /// </summary>
    public class Badge : APIMain, IRefreshable, IIdApi<Badge>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("badges");

        public ulong Id { get; }

        private string name;
        public string Name => name;

        private string description;
        public string Description => description;

        private Experience experience;
        public Experience Experience => experience;
        public IAssetOwner Owner => experience.Owner;

        private DateTime created;
        public DateTime Created => created;

        private DateTime lastUpdated;
        public DateTime LastUpdated => lastUpdated;

        private bool isEnabled;
        public bool IsEnabled => isEnabled;

        private int awardedCount;
        public int AwardedCount => awardedCount;

        private int yesterdayAwardedCount;
        public int YesterdayAwardedCount => yesterdayAwardedCount;

        private Asset thumbnailAsset;

        /// <summary>
        /// Gets an <see cref="Asset"/> that is used for this badge.
        /// </summary>
        /// <remarks>This value will be <see langword="null"/> if this <see cref="Badge"/> is created without an authenticated <see cref="Session"/> as Asset instances require an authenticated session.</remarks>
        public Asset ThumbnailAsset => thumbnailAsset;

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
            HttpResponseMessage response = await GetAsync($"/v1/badges/{Id}");
            string raw = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(raw);

            ulong experienceId = Convert.ToUInt64(data.awardingUniverse.id);

            name = data.displayName;
            description = (data.displayDescription == null ? string.Empty : data.displayDescription);
            created = data.created;
            lastUpdated = data.updated;
            experience = await Experience.FromId(experienceId);
            awardedCount = Convert.ToInt32(data.statistics.awardedCount);
            yesterdayAwardedCount = Convert.ToInt32(data.statistics.pastDayAwardedCount);
            isEnabled = data.enabled;

            if (SessionVerify.Verify(session))
            {
                thumbnailAsset = await Asset.FromId(Convert.ToUInt64(data.displayIconImageId), session);
            }

            RefreshedAt = DateTime.Now;
        }

        public async Task<string> GetThumbnailAsync()
        {
            string url = $"/v1/badges/icons?badgeIds={Id}&size=150x150&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new ArgumentException("Invalid badge to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        public async Task ModifyAsync(BadgeModifyOptions options)
        {
            object body = new
            {
                name = options.Name,
                description = options.Description,
                enabled = options.IsEnabled,
            };

            HttpResponseMessage response = await PatchAsync($"/v1/badges/{Id}", body, verifyApiName: "Badge.ModifyAsync");
        }

        /// <summary>
        /// Gets whether or not this badge is owned by the given <see cref="User"/>.
        /// </summary>
        /// <param name="target">The user to check.</param>
        /// <returns>A task which contains a boolean when completed.</returns>
        /// <seealso cref="User.HasBadgeAsync(Badge)"/>
        /// <seealso cref="User.HasBadgeAsync(ulong)"/>
        public async Task<bool> IsOwnedByAsync(User target) => await target.HasBadgeAsync(this);

        /// <summary>
        /// Gets whether or not this badge is owned by the user with the given user Id.
        /// </summary>
        /// <param name="targetId">The user ID to check.</param>
        /// <returns>A task which contains a boolean when completed.</returns>
        /// <seealso cref="User.HasBadgeAsync(Badge)"/>
        /// <seealso cref="User.HasBadgeAsync(ulong)"/>
        public async Task<bool> IsOwnedByAsync(ulong targetId) => await IsOwnedByAsync(await User.FromId(targetId, session));

        /// <summary>
        /// Gets whether or not this badge is owned by the user with the given username.
        /// </summary>
        /// <param name="targetUsername">The username to check.</param>
        /// <returns>A task which contains a boolean when completed.</returns>
        /// <seealso cref="User.HasBadgeAsync(Badge)"/>
        /// <seealso cref="User.HasBadgeAsync(ulong)"/>
        public async Task<bool> IsOwnedByAsync(string targetUsername) => await IsOwnedByAsync(await User.FromUsername(targetUsername, session));

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} [{Id}] {{{(Owner is User ? "@" : string.Empty)}{Owner.Name}}} <{AwardedCount}>";
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

    public class BadgeModifyOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }

        public BadgeModifyOptions(Badge target)
        {
            Name = target.Name;
            Description = target.Description;
            IsEnabled = target.IsEnabled;
        }
    }
}
