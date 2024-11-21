namespace RoSharp.Enums
{
    /// <summary>
    /// Represents the permissions a group role can have.
    /// </summary>
    public enum GroupPermission
    {
        /// <summary>
        /// View the group wall.
        /// </summary>
        ViewWall,

        /// <summary>
        /// Post on the group wall.
        /// </summary>
        PostToWall,

        /// <summary>
        /// Delete posts from other users off the group wall.
        /// </summary>
        DeleteFromWall,

        /// <summary>
        /// View the group's status (shout).
        /// </summary>
        ViewStatus,

        /// <summary>
        /// Post to the group's status (shout).
        /// </summary>
        PostToStatus,

        /// <summary>
        /// Change other users' rank.
        /// </summary>
        ChangeRank,

        /// <summary>
        /// Accept/decline join requests to the group.
        /// </summary>
        InviteMembers,

        /// <summary>
        /// Remove (kick) members from the group.
        /// </summary>
        RemoveMembers,

        /// <summary>
        /// Ban/Unban members from the group.
        /// </summary>
        BanMembers,

        /// <summary>
        /// Manage group relationships (allies and enemies).
        /// </summary>
        ManageRelationships,

        /// <summary>
        /// Manage the group's clan (no longer possible).
        /// </summary>
        ManageClan,

        /// <summary>
        /// View the group's audit logs.
        /// </summary>
        ViewAuditLogs,

        /// <summary>
        /// Spend group funds (badges, ads, etc).
        /// </summary>
        SpendGroupFunds,

        /// <summary>
        /// Advertise the group.
        /// </summary>
        AdvertiseGroup,

        /// <summary>
        /// Create group items (such as shirts).
        /// </summary>
        CreateItems,

        /// <summary>
        /// Manage and modify group items (such as shirts).
        /// </summary>
        ManageItems,

        /// <summary>
        /// Add group places.
        /// </summary>
        AddGroupPlaces,

        /// <summary>
        /// Manage and modify group experiences.
        /// </summary>
        ManageGroupGames,

        /// <summary>
        /// View group payouts.
        /// </summary>
        ViewGroupPayouts,

        /// <summary>
        /// View group analytics.
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
