namespace RoSharp.API.Groups
{
    /// <summary>
    /// Contains info regarding a group shout.
    /// </summary>
    public struct GroupShout
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
    }
}
