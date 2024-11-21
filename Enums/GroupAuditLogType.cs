namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates a community audit log type. Used in <see cref="API.Communities.Community.GetAuditLogsAsync"/>.
    /// </summary>
    public enum CommunityAuditLogType
    {
        // Community

        /// <summary>
        /// The community's description is changed.
        /// </summary>
        ChangeDescription,

        /// <summary>
        /// The community's status is changed.
        /// </summary>
        PostStatus,

        /// <summary>
        /// The community is abandoned by its owner.
        /// </summary>
        /// <seealso cref="Claim"/>
        /// <seealso cref="ChangeOwner"/>
        Abandon,

        /// <summary>
        /// The community is claimed after being abandoned.
        /// </summary>
        /// <seealso cref="Abandon"/>
        /// <seealso cref="ChangeOwner"/>
        Claim,

        /// <summary>
        /// The community is renamed.
        /// </summary>
        Rename,

        /// <summary>
        /// The community is deleted by Roblox.
        /// </summary>
        Delete,

        /// <summary>
        /// The community is locked by Roblox.
        /// </summary>
        Lock,

        /// <summary>
        /// The community is unlocked by Roblox.
        /// </summary>
        Unlock,

        /// <summary>
        /// The community's owner is changed from one person to another without being abandoned.
        /// </summary>
        /// <seealso cref="Abandon"/>
        /// <seealso cref="Claim"/>
        ChangeOwner,

        // community Assets/Funds

        /// <summary>
        /// A community place is added.
        /// </summary>
        AddcommunityPlace,

        /// <summary>
        /// A community place is removed.
        /// </summary>
        RemovecommunityPlace,

        /// <summary>
        /// A community item (such as a shirt) is created.
        /// </summary>
        CreateItems,

        /// <summary>
        /// A community asset (such as a model) is created.
        /// </summary>
        CreatecommunityAsset,

        /// <summary>
        /// A community developer product is created.
        /// </summary>
        CreatecommunityDeveloperProduct,

        /// <summary>
        /// A community Game Pass is created.
        /// </summary>
        CreateGamePass,

        /// <summary>
        /// A community developer subscription is created.
        /// </summary>
        CreatecommunityDeveloperSubscriptionProduct,

        /// <summary>
        /// A community asset (such as a model) is modified.
        /// </summary>
        ConfigurecommunityAsset,

        /// <summary>
        /// A community experience is modified.
        /// </summary>
        ConfigurecommunityGame,

        /// <summary>
        /// A community badge is modified.
        /// </summary>
        ConfigureBadge,

        /// <summary>
        /// A community item (such as a shirt) is modified.
        /// </summary>
        ConfigureItems,

        /// <summary>
        /// A community asset is updated.
        /// </summary>
        UpdatecommunityAsset,

        /// <summary>
        /// A community place is saved.
        /// </summary>
        SavePlace,

        /// <summary>
        /// A new version of a community place is published.
        /// </summary>
        PublishPlace,

        /// <summary>
        /// community funds are spent.
        /// </summary>
        SpendcommunityFunds,

        /// <summary>
        /// Currency amounts are adjusted by Roblox.
        /// </summary>
        AdjustCurrencyAmounts,

        /// <summary>
        /// A community asset's version has been reverted.
        /// </summary>
        RevertcommunityAsset,

        /// <summary>
        /// An ad for the community has been purchased.
        /// </summary>
        BuyAd,

        // Posts & Clan
        /// <summary>
        /// A post on the wall has been deleted.
        /// </summary>
        DeletePost,

        /// <summary>
        /// A user has been invited to the community's clan.
        /// </summary>
        InviteToClan,

        /// <summary>
        /// A user has been kicked from the community's clan.
        /// </summary>
        KickFromClan,

        /// <summary>
        /// A user's invite to the community's clan has been revoked.
        /// </summary>
        CancelClanInvite,

        /// <summary>
        /// A clan was purchased for the community.
        /// </summary>
        BuyClan,

        // Alliances

        /// <summary>
        /// An ally request to a different community has been sent.
        /// </summary>
        SendAllyRequest,

        /// <summary>
        /// An ally request to this community has been accepted.
        /// </summary>
        AcceptAllyRequest,

        /// <summary>
        /// An ally request to this community has been declined.
        /// </summary>
        DeclineAllyRequest,

        /// <summary>
        /// An ally has been removed from this community.
        /// </summary>
        DeleteAlly,

        /// <summary>
        /// An enemy has been declared from this community.
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
