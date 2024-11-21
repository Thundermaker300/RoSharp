using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates an experience activity history type. Used in <see cref="API.Groups.Group.GetAuditLogsAsync"/>.
    /// </summary>
    public enum GroupAuditLogType
    {
        // Group
        ChangeDescription,
        PostStatus,
        Abandon,
        Claim,
        Rename,
        Delete,
        Lock,
        Unlock,
        ChangeOwner,

        // Group Assets/Funds
        AddGroupPlace,
        RemoveGroupPlace,
        CreateItems,
        CreateGroupAsset,
        CreateGroupDeveloperProduct,
        CreateGamePass,
        CreateGroupDeveloperSubscriptionProduct,
        ConfigureGroupAsset,
        ConfigureGroupGame,
        ConfigureBadge,
        ConfigureItems,
        UpdateGroupAsset,
        SavePlace,
        PublishPlace,
        SpendGroupFunds,
        AdjustCurrencyAmounts,
        RevertGroupAsset,
        BuyAd,

        // Posts & Clan
        DeletePost,
        InviteToClan,
        KickFromClan,
        CancelClanInvite,
        BuyClan,

        // Alliances
        SendAllyRequest,
        AcceptAllyRequest,
        DeclineAllyRequest,
        DeleteAlly,
        CreateEnemy,
        DeleteEnemy,

        // Members
        AcceptJoinRequest,
        DeclineJoinRequest,
        RemoveMember,
        ChangeRank,
    }
}
