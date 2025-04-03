namespace RoSharp.Enums
{
    /// <summary>
    /// Represents the visibility of a virtual event.
    /// </summary>
    public enum VirtualEventVisibility
    {
        /// <summary>
        /// Anybody can see the event.
        /// </summary>
        Public,

        /// <summary>
        /// Only the experience owner (or group members with edit permissions) can see the virtual event.
        /// </summary>
        Private,
    }
}
