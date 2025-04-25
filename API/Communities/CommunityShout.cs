namespace RoSharp.API.Communities
{
    /// <summary>
    /// Contains info regarding a community shout.
    /// </summary>
    public readonly struct CommunityShout
    {
        /// <summary>
        /// Gets the text of the shout.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets the poster of the shout.
        /// </summary>
        public User Poster { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the time the shout was posted.
        /// </summary>
        public DateTime PostedAt { get; init; }

        /// <summary>
        /// Gets whether or not this shout is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - PostedAt) < TimeSpan.FromDays(3);
    }
}
