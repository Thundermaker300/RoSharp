namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates where a connection request originated from.
    /// </summary>
    public enum FriendshipOriginSourceType
    {
        /// <summary>
        /// Unknown friendship origin.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Connection request originated from user's profile.
        /// </summary>
        UserProfile,

        /// <summary>
        /// Connection request originated from a search to the user's profile.
        /// </summary>
        PlayerSearch,

        /// <summary>
        /// Connection request originated from importing phone contacts.
        /// </summary>
        PhoneContactImporter,

        /// <summary>
        /// Connection request originated from within an experience.
        /// </summary>
        InGame,
    }
}
