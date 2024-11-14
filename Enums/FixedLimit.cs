namespace RoSharp.Enums
{
    /// <summary>
    /// Used for Roblox APIs where the limit parameter must be 10, 25, 50, or 100.
    /// </summary>
    /// <seealso cref="Extensions.UtilityExtensions.Limit(FixedLimit)"/>
    public enum FixedLimit
    {
        /// <summary>
        /// Limit of 10.
        /// </summary>
        Limit10,

        /// <summary>
        /// Limit of 25.
        /// </summary>
        Limit25,

        /// <summary>
        /// Limit of 50.
        /// </summary>
        Limit50,

        /// <summary>
        /// Limit of 100.
        /// </summary>
        Limit100,
    }
}
