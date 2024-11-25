namespace RoSharp.API.DevForum
{
    /// <summary>
    /// Represents an individual post on a topic on the Roblox Developer Forum.
    /// </summary>
    public struct DevForumPost
    {
        /// <summary>
        /// Gets the unique Id of the post.
        /// </summary>
        public ulong PostId { get; init; }

        /// <summary>
        /// Gets the content of the post.
        /// </summary>
        public string Content { get; init; }

        /// <summary>
        /// Gets the poster.
        /// </summary>
        public Id<User>? Poster { get; init; }

        /// <summary>
        /// Gets the username of the poster.
        /// </summary>
        public string PosterName { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the creation date of this post.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the last time the post was updated.
        /// </summary>
        public DateTime LastUpdated { get; init; }

        /// <summary>
        /// Gets the amount of likes this post has.
        /// </summary>
        public int Likes { get; init; }

        /// <summary>
        /// Gets the position from the top this post is.
        /// </summary>
        public int PostNumber { get; init; }

        /// <summary>
        /// Gets the amount of replies this post has.
        /// </summary>
        public int ReplyCount { get; init; }

        /// <summary>
        /// Gets the amount of reads this post has.
        /// </summary>
        public int Reads { get; init; }

        /// <summary>
        /// Gets the edit version of this post, starting at 1 and incrementing by 1 every time it is edited.
        /// </summary>
        public int EditVersion { get; init; }
        
        /// <summary>
        /// Gets whether or not this post is hidden.
        /// </summary>
        public bool Hidden { get; init; }

        /// <summary>
        /// Gets the user's chosen title. Can be <see langword="null"/>.
        /// </summary>
        public string? UserTitle { get; init; }

        /// <summary>
        /// Gets whether or not this post was marked as the accepted answer.
        /// </summary>
        public bool AcceptedAnswer { get; init; }
    }
}
