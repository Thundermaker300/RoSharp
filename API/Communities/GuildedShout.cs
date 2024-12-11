namespace RoSharp.API.Communities
{
    /// <summary>
    /// Represents a group shout from Guilded.
    /// </summary>
    public readonly struct GuildedShout
    {
        /// <summary>
        /// Gets the Id of the Guilded server that the announcement was posted in.
        /// </summary>
        public string GuildedId { get; init; }

        /// <summary>
        /// Gets the Id of the announcement.
        /// </summary>
        public string ShoutId { get; init; }

        /// <summary>
        /// Gets the Id of the channel that the announcement was posted in.
        /// </summary>
        public string ShoutChannelId { get; init; }

        /// <summary>
        /// Gets the title of the announcement.
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Gets the image url on the announcement, if present.
        /// </summary>
        public string ImageUrl { get; init; }

        /// <summary>
        /// Gets the amount of likes on the post, if visible.
        /// </summary>
        public ulong LikeCount { get; init; }
        /// <summary>
        /// Gets the content of the shout.
        /// </summary>
        public string Content { get; init; }

        /// <summary>
        /// Gets the poster of the shout.
        /// </summary>
        public User Poster { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the shout was posted.
        /// </summary>
        public DateTime PostedAt { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the shout was updated.
        /// </summary>
        public DateTime UpdatedAt { get; init; }

        /// <summary>
        /// Gets whether or not <see cref="LikeCount"/> is visible to the public.
        /// </summary>
        public bool ReactionsVisible { get; init; }
    }
}
