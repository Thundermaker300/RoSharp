namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates the action to perform for a join request. Used in <see cref="API.MemberManager.ModifyJoinRequestAsync(API.User, JoinRequestAction)"/>.
    /// </summary>
    public enum JoinRequestAction
    {
        /// <summary>
        /// Accept the join request.
        /// </summary>
        Accept,

        /// <summary>
        /// Decline the join request.
        /// </summary>
        Decline,
    }
}
