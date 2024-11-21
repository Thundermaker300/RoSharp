using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Groups
{
    public struct GroupAuditLog
    {

        /// <summary>
        /// The type of group log.
        /// </summary>
        public GroupAuditLogType Type { get; init; }

        /// <summary>
        /// The time the action occurred.
        /// </summary>
        public DateTime Time { get; init; }

        /// <summary>
        /// The group Id the action occurred in.
        /// </summary>
        public GenericId<Group> GroupId { get; init; }

        /// <summary>
        /// The user Id of the user that performed the action.
        /// </summary>
        public GenericId<User> UserId { get; init; }

        /// <summary>
        /// The rank Id of the user that performed the action.
        /// </summary>
        public ulong RankId { get; init; }

        /// <summary>
        /// Gets the targeted user in this log. Will be <see langword="null"/> if the action isn't targeting a user.
        /// </summary>
        public GenericId<User>? TargetUserId { get; init; }

        /// <summary>
        /// Gets the targeted group in this log. Will be <see langword="null"/> if the action isn't targeting a group.
        /// </summary>
        public GenericId<Group>? TargetGroupId { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Type} [{Time}] UserID: {UserId.Id}";
        }
    }
}
