namespace RoSharp.Enums
{
    /// <summary>
    /// Represents the permissions a community role can have.
    /// </summary>
    public enum GroupPermission
    {
        /// <summary>
        /// View the community wall.
        /// </summary>
        ViewWall,

        /// <summary>
        /// Post on the community wall.
        /// </summary>
        PostToWall,

        /// <summary>
        /// Delete posts from other users off the community wall.
        /// </summary>
        DeleteFromWall,

        /// <summary>
        /// View the community's status (shout).
        /// </summary>
        ViewStatus,

        /// <summary>
        /// Post to the community's status (shout).
        /// </summary>
        PostToStatus,

        /// <summary>
        /// Change other users' rank.
        /// </summary>
        ChangeRank,

        /// <summary>
        /// Accept/decline join requests to the community.
        /// </summary>
        InviteMembers,

        /// <summary>
        /// Remove (kick) members from the community.
        /// </summary>
        RemoveMembers,

        /// <summary>
        /// Ban/Unban members from the community.
        /// </summary>
        BanMembers,

        /// <summary>
        /// Manage community relationships (allies and enemies).
        /// </summary>
        ManageRelationships,

        /// <summary>
        /// Manage the community's clan (no longer possible).
        /// </summary>
        ManageClan,

        /// <summary>
        /// View the community's audit logs.
        /// </summary>
        ViewAuditLogs,

        /// <summary>
        /// Spend community funds (badges, ads, etc).
        /// </summary>
        SpendGroupFunds,

        /// <summary>
        /// Advertise the community.
        /// </summary>
        AdvertiseGroup,

        /// <summary>
        /// Create community items (such as shirts).
        /// </summary>
        CreateItems,

        /// <summary>
        /// Manage and modify community items (such as shirts).
        /// </summary>
        ManageItems,

        /// <summary>
        /// Add community places.
        /// </summary>
        AddGroupPlaces,

        /// <summary>
        /// Manage and modify community experiences.
        /// </summary>
        ManageGroupGames,

        /// <summary>
        /// View community payouts.
        /// </summary>
        ViewGroupPayouts,

        /// <summary>
        /// View community analytics.
        /// </summary>
        ViewAnalytics,

        /// <summary>
        /// Use cloud authentication.
        /// </summary>
        UseCloudAuthentication,

        /// <summary>
        /// Grant cloud authentication permissions.
        /// </summary>
        AdministerCloudAuthentication,
    }
}
