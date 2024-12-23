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
        NotApplicable = 0,

        /// <summary>
        /// Can only be purchased in the shop.
        /// </summary>
        ShopOnly = 1,

        /// <summary>
        /// Can only be purchased within owner's experiences.
        /// </summary>
        MyExperiencesOnly = 2,

        /// <summary>
        /// Can be purchased in the shop and owner's experiences.
        /// </summary>
        ShopAndMyExperiences = 3,
        
        /// <summary>
        /// Can only be purchased within certain experiences.
        /// </summary>
        ExperiencesById = 4,

        /// <summary>
        /// Can be purchased anywhere.
        /// </summary>
        ShopAndAllExperiences = 5,

        /// <summary>
        /// Can only be purchased in certain <see cref="API.Assets.Experiences.Experience"/>s.
        /// </summary>
        AllowedGames = 6,

        /// <summary>
        /// Unknown.
        /// </summary>
        ExperiencesDevApiOnly = 6,

        /// <summary>
        /// Can only be purchased in the shop and within certain experiences.
        /// </summary>
        ShopAndExperiencesById = 7,
    }
}
