namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates certain age-restricting descriptors for experiences.
    /// </summary>
    public enum ExperienceDescriptorType
    {
        /// <summary>
        /// Indicates that alcohol depictions are present in an experience.
        /// </summary>
        Alcohol,

        /// <summary>
        /// Indicates that blood depictions are present in an experience.
        /// </summary>
        Blood,

        /// <summary>
        /// Indicates that crude humor is present in an experience.
        /// </summary>
        CrudeHumor,

        /// <summary>
        /// Indicates that fear factors are present in an experience.
        /// </summary>
        Fear,

        /// <summary>
        /// Indicates that unplayable gambling content is present in an experience.
        /// </summary>
        Gambling,

        /// <summary>
        /// Indicates that romance and/or romantic themes are present in an experience.
        /// </summary>
        Romance,

        /// <summary>
        /// Indicates that strong language is present in an experience.
        /// </summary>
        /// <remarks>This indicates that strong language is present; this does not necessarily mean that users can use strong language. See <see cref="API.Assets.Experience.ProfanityAllowed"/> instead.</remarks>
        StrongLanguage,

        /// <summary>
        /// Indicates that social hangout themes are present in an experience.
        /// </summary>
        SocialHangout,

        /// <summary>
        /// Indicates that violence is present in an experience.
        /// </summary>
        Violence,
    }
}
