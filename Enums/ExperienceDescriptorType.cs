namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates certain age-restricting descriptors for experiences.
    /// </summary>
    public enum ExperienceDescriptorType
    {
        /// <summary>
        /// Indicates that users can interact with AI in an experience.
        /// </summary>
        AIInteraction,

        /// <summary>
        /// Indicates that alcohol depictions are present in an experience.
        /// </summary>
        Alcohol,

        /// <summary>
        /// Indicates that blood depictions are present in an experience.
        /// </summary>
        Blood,

        /// <summary>
        /// Indicates that 'continuous media feed(s)' are present in an experience.
        /// </summary>
        ContinuousMediaFeed,

        /// <summary>
        /// Indicates that crude humor is present in an experience.
        /// </summary>
        CrudeHumor,

        /// <summary>
        /// Indicates that fear factors are present in an experience.
        /// </summary>
        Fear,

        /// <summary>
        /// Indicates that free-form user creation is enabled in an experience.
        /// </summary>
        FreeFormUserCreation,

        /// <summary>
        /// Indicates that unplayable gambling content is present in an experience.
        /// </summary>
        Gambling,

        /// <summary>
        /// Indicates that cross-experience media sharing is present in an experience.
        /// </summary>
        MediaSharing,

        /// <summary>
        /// Indicates that cross-experience media viewing is present in an experience.
        /// </summary>
        MediaViewing,

        /// <summary>
        /// Indicates that paid item trading is present in an experience.
        /// </summary>
        PaidItemTrading,

        /// <summary>
        /// Indicates that paid random items are present in an experience.
        /// </summary>
        PaidRandomItems,

        /// <summary>
        /// Indicates that romance and/or romantic themes are present in an experience.
        /// </summary>
        Romance,

        /// <summary>
        /// Indicates that sensitive issues are present in an experience.
        /// </summary>
        SensitiveIssues,

        /// <summary>
        /// Indicates that strong language is present in an experience.
        /// </summary>
        /// <remarks>This indicates that strong language is present; this does not necessarily mean that users can use strong language. See <see cref="API.Assets.Experiences.Experience.ProfanityAllowed"/> instead.</remarks>
        StrongLanguage,

        /// <summary>
        /// Indicates that social hangout themes are present in an experience.
        /// </summary>
        SocialHangout,

        /// <summary>
        /// Indicates that violence is present in an experience.
        /// </summary>
        Violence,

        /// <summary>
        /// Indicates that an experience is for all ages and suitable for everyone.
        /// </summary>
        AllAges,
    }
}
