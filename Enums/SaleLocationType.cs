namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates where a UGC item can be purchased.
    /// </summary>
    public enum SaleLocationType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        NotApplicable = -1,

        /// <summary>
        /// Can be purchased anywhere.
        /// </summary>
        ShopAndAllExperiences = 5,

        /// <summary>
        /// Can only be purchased in certain <see cref="API.Assets.Experiences.Experience"/>s.
        /// </summary>
        AllowedGames = 6,
    }
}
