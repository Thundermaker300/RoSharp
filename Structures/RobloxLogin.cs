namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a Roblox authentication.
    /// </summary>
    public struct RobloxLogin
    {
        /// <summary>
        /// The user Id.
        /// </summary>
        public ulong id { get; set; }

        /// <summary>
        /// The user name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The display name.
        /// </summary>
        public string displayName { get; set; }
    }
}
