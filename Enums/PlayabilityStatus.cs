namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates the playability status of an experience.
    /// </summary>
    public enum PlayabilityStatus
    {
        /// <summary>
        /// Unknown playability status.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Experience is playable.
        /// </summary>
        Playable,

        /// <summary>
        /// Experience is private, or the authenticated user does not have permission to join.
        /// </summary>
        UniverseRootPlaceIsPrivate,

        /// <summary>
        /// Un-authenticated users cannot play this experience.
        /// </summary>
        GuestProhibited,

        /// <summary>
        /// Experience is unavailable on this device.
        /// </summary>
        DeviceRestricted,

        /// <summary>
        /// Experience is under moderation review.
        /// </summary>
        UnderReview,

        /// <summary>
        /// Experience cannot be played (unknown reason, likely moderation related).
        /// </summary>
        GameUnapproved,

        /// <summary>
        /// Experience is 17+ and authenticated user is not age verified.
        /// </summary>
        ContextualPlayabilityUnverifiedSeventeenPlusUser,
    }
}
