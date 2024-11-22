using RoSharp.Enums;

namespace RoSharp.API.Communities
{
    /// <summary>
    /// Represents a single community log.
    /// </summary>
    public readonly struct CommunityAuditLog
    {

        /// <summary>
        /// The type of community log.
        /// </summary>
        public CommunityAuditLogType Type { get; init; }

        /// <summary>
        /// The time the action occurred.
        /// </summary>
        public DateTime Time { get; init; }

        /// <summary>
        /// The community Id the action occurred in.
        /// </summary>
        public Id<Community> CommunityId { get; init; }

        /// <summary>
        /// The user Id of the user that performed the action.
        /// </summary>
        public Id<User> UserId { get; init; }

        /// <summary>
        /// The rank Id of the user that performed the action.
        /// </summary>
        public ulong RankId { get; init; }

        /// <summary>
        /// Gets the targeted user in this log. Will be <see langword="null"/> if the action isn't targeting a user.
        /// </summary>
        public Id<User>? TargetUserId { get; init; }

        /// <summary>
        /// Gets the targeted community in this log. Will be <see langword="null"/> if the action isn't targeting a community.
        /// </summary>
        public Id<Community>? TargetCommunityId { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Type} [{Time}] CommunityId: {CommunityId.ItemId} UserID: {UserId.ItemId}";
        }
    }
}
