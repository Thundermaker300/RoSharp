using RoSharp.API.Assets;

namespace RoSharp.API.Communities
{
    /// <summary>
    /// Represents a community announcement.
    /// </summary>
    public readonly struct CommunityAnnouncement
    {
        /// <summary>
        /// Gets the Id of the Guilded server that the announcement was posted in.
        /// </summary>
        /// <remarks>Guilded has been discontinued, so this value will always be: <c>XCOMM-ID</c>.</remarks>
        public string GuildedId { get; init; }

        /// <summary>
        /// Gets the Id of the announcement.
        /// </summary>
        public string ShoutId { get; init; }

        /// <summary>
        /// Gets the Id of the Guilded channel that the announcement was posted in.
        /// </summary>
        /// <remarks>Guilded has been discontinued, so this value will always be: <c>CHAN-ID-000000000000000000000</c> with the Id of the community appended to the end.</remarks>
        public string ShoutChannelId { get; init; }

        /// <summary>
        /// Gets the title of the announcement.
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Gets the image asset on the announcement, if present.
        /// </summary>
        public Id<Asset>? ImageUrl { get; init; }

        /// <summary>
        /// Gets the amount of likes on the post, if visible.
        /// </summary>
        public ulong LikeCount { get; init; }
        /// <summary>
        /// Gets the content of the announcement.
        /// </summary>
        public string Content { get; init; }

        /// <summary>
        /// Gets the poster of the announcement.
        /// </summary>
        public Id<User> Poster { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the announcement was posted.
        /// </summary>
        public DateTime PostedAt { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the announcement was updated.
        /// </summary>
        public DateTime UpdatedAt { get; init; }

        /// <summary>
        /// Gets whether or not <see cref="LikeCount"/> is visible to the public.
        /// </summary>
        public bool ReactionsVisible { get; init; }

        /// <summary>
        /// Gets whether or not this announcement is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - PostedAt) < TimeSpan.FromDays(3);
    }
}
