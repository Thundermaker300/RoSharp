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
        /// Game is playable but must be purchased first.
        /// </summary>
        /// <seealso cref="API.Assets.Experiences.Experience.Cost"/>
        PurchaseRequired,

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
        /// Experience is Friends-Only and the authenticated user is not friends with the creator.
        /// </summary>
        InsufficientPermissionFriendsOnly,

        /// <summary>
        /// Experience is 17+ and authenticated user is not age verified.
        /// </summary>
        /// <remarks>Seemingly unused and replaced by <see cref="ContextualPlayabilityAgeGatedByDescriptor"/>.</remarks>
        ContextualPlayabilityUnverifiedSeventeenPlusUser,

        /// <summary>
        /// Experience is 17+ and authenticated user is not age verified.
        /// </summary>
        ContextualPlayabilityAgeGatedByDescriptor,

        /// <summary>
        /// Experience is marked with a higher "maturity level" than is allowed in the authenticated user's content maturity settings, OR the experience's content maturity is "N/A" and the user is under 13.
        /// </summary>
        ContextualPlayabilityAgeRecommendationParentalControls,

    }
}
