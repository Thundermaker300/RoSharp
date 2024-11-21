namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates a group audit log type. Used in <see cref="API.Groups.Group.GetAuditLogsAsync"/>.
    /// </summary>
    public enum GroupAuditLogType
    {
        // Group

        /// <summary>
        /// The group's description is changed.
        /// </summary>
        ChangeDescription,

        /// <summary>
        /// The group's status is changed.
        /// </summary>
        PostStatus,

        /// <summary>
        /// The group is abandoned by its owner.
        /// </summary>
        /// <seealso cref="Claim"/>
        /// <seealso cref="ChangeOwner"/>
        Abandon,

        /// <summary>
        /// The group is claimed after being abandoned.
        /// </summary>
        /// <seealso cref="Abandon"/>
        /// <seealso cref="ChangeOwner"/>
        Claim,

        /// <summary>
        /// The group is renamed.
        /// </summary>
        Rename,

        /// <summary>
        /// The group is deleted by Roblox.
        /// </summary>
        Delete,

        /// <summary>
        /// The group is locked by Roblox.
        /// </summary>
        Lock,

        /// <summary>
        /// The group is unlocked by Roblox.
        /// </summary>
        Unlock,

        /// <summary>
        /// The group's owner is changed from one person to another without being abandoned.
        /// </summary>
        /// <seealso cref="Abandon"/>
        /// <seealso cref="Claim"/>
        ChangeOwner,

        // Group Assets/Funds

        /// <summary>
        /// A group place is added.
        /// </summary>
        AddGroupPlace,

        /// <summary>
        /// A group place is removed.
        /// </summary>
        RemoveGroupPlace,

        /// <summary>
        /// A group item (such as a shirt) is created.
        /// </summary>
        CreateItems,

        /// <summary>
        /// A group asset (such as a model) is created.
        /// </summary>
        CreateGroupAsset,

        /// <summary>
        /// A group developer product is created.
        /// </summary>
        CreateGroupDeveloperProduct,

        /// <summary>
        /// A group Game Pass is created.
        /// </summary>
        CreateGamePass,

        /// <summary>
        /// A group developer subscription is created.
        /// </summary>
        CreateGroupDeveloperSubscriptionProduct,

        /// <summary>
        /// A group asset (such as a model) is modified.
        /// </summary>
        ConfigureGroupAsset,

        /// <summary>
        /// A group experience is modified.
        /// </summary>
        ConfigureGroupGame,

        /// <summary>
        /// A group badge is modified.
        /// </summary>
        ConfigureBadge,

        /// <summary>
        /// A group item (such as a shirt) is modified.
        /// </summary>
        ConfigureItems,

        /// <summary>
        /// A group asset is updated.
        /// </summary>
        UpdateGroupAsset,

        /// <summary>
        /// A group place is saved.
        /// </summary>
        SavePlace,

        /// <summary>
        /// A new version of a group place is published.
        /// </summary>
        PublishPlace,

        /// <summary>
        /// Group funds are spent.
        /// </summary>
        SpendGroupFunds,

        /// <summary>
        /// Currency amounts are adjusted by Roblox.
        /// </summary>
        AdjustCurrencyAmounts,

        /// <summary>
        /// A group asset's version has been reverted.
        /// </summary>
        RevertGroupAsset,

        /// <summary>
        /// An ad for the group has been purchased.
        /// </summary>
        BuyAd,

        // Posts & Clan
        /// <summary>
        /// A post on the wall has been deleted.
        /// </summary>
        DeletePost,

        /// <summary>
        /// A user has been invited to the group's clan.
        /// </summary>
        InviteToClan,

        /// <summary>
        /// A user has been kicked from the group's clan.
        /// </summary>
        KickFromClan,

        /// <summary>
        /// A user's invite to the group's clan has been revoked.
        /// </summary>
        CancelClanInvite,

        /// <summary>
        /// A clan was purchased for the group.
        /// </summary>
        BuyClan,

        // Alliances

        /// <summary>
        /// An ally request to a different group has been sent.
        /// </summary>
        SendAllyRequest,

        /// <summary>
        /// An ally request to this group has been accepted.
        /// </summary>
        AcceptAllyRequest,

        /// <summary>
        /// An ally request to this group has been declined.
        /// </summary>
        DeclineAllyRequest,

        /// <summary>
        /// An ally has been removed from this group.
        /// </summary>
        DeleteAlly,

        /// <summary>
        /// An enemy has been declared from this group.
        /// </summary>
        CreateEnemy,

        /// <summary>
        /// An enemy has been deleted.
        /// </summary>
        DeleteEnemy,

        // Members

        /// <summary>
        /// A member's join request has been accepted.
        /// </summary>
        AcceptJoinRequest,

        /// <summary>
        /// A member's join request has been declined.
        /// </summary>
        DeclineJoinRequest,

        /// <summary>
        /// A member has been kicked.
        /// </summary>
        RemoveMember,

        /// <summary>
        /// A member has been banned.
        /// </summary>
        BanMember,

        /// <summary>
        /// A member has been unbanned.
        /// </summary>
        UnbanMember,

        /// <summary>
        /// A member's rank has changed.
        /// </summary>
        ChangeRank,
    }
}
