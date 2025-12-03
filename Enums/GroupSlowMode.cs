namespace RoSharp.Enums
{
    /// <summary>
    /// Community slow mode setting.
    /// </summary>
    public enum GroupSlowMode
    {
        /// <summary>
        /// Default rate limits for community activity.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// 1 post per 2 minutes, 2 comments per minute.
        /// </summary>
        Slow = 1,

        /// <summary>
        /// 1 post per 5 minutes, 1 comment per minute.
        /// </summary>
        Slower = 2,

        /// <summary>
        /// 1 post per 10 minutes, 1 comment per 2 minutes.
        /// </summary>
        Slowest = 3,
    }
}
