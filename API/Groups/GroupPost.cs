namespace RoSharp.API.Groups
{
    /// <summary>
    /// Represents a group post.
    /// </summary>
    public struct GroupPost
    {
        internal Group group;

        /// <summary>
        /// Gets the unique Id of the post.
        /// </summary>
        public ulong PostId { get; init; }

        /// <summary>
        /// Gets the unique Id of the poster. Can be <see langword="null"/>.
        /// </summary>
        public GenericId<User>? PosterId { get; init; }

        /// <summary>
        /// Gets the text of the post.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the date the post was created.
        /// </summary>
        public DateTime PostedAt { get; init; }

        /// <summary>
        /// Gets the rank of the user that made the post. Can be <see langword="null"/>.
        /// </summary>
        public string? RankInGroup { get; init; }
    }
}
