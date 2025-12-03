namespace RoSharp.Enums
{
    /// <summary>
    /// Minimum required account age to join a community.
    /// </summary>
    public enum GroupAccountAge
    {
        /// <summary>
        /// Users do not require a specific account age before joining.
        /// </summary>
        None,

        /// <summary>
        /// Minimum 1 day account age before joining.
        /// </summary>
        OneDay,

        /// <summary>
        /// Minimum 3 day account age before joining.
        /// </summary>
        ThreeDays,

        /// <summary>
        /// Minimum 7 day account age before joining.
        /// </summary>
        OneWeek,

        /// <summary>
        /// Minimum 30 day account age before joining.
        /// </summary>
        OneMonth,

        /// <summary>
        /// Minimum 90 day account age before joining.
        /// </summary>
        ThreeMonths,
    }
}
