namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates where a user is located, mainly for <see cref="Structures.UserPresence"/>.
    /// </summary>
    public enum UserLocationType
    {
        /// <summary>
        /// User is offline.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// User is online on the website.
        /// </summary>
        Website = 1,

        /// <summary>
        /// User is online and in an experience.
        /// </summary>
        Experience = 2,

        /// <summary>
        /// User is online and in Roblox Studio.
        /// </summary>
        Studio = 3,
    }
}
