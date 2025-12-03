namespace RoSharp.Enums
{
    /// <summary>
    /// Minimum required verification level to join a community.
    /// </summary>
    public enum GroupVerificationLevel
    {
        /// <summary>
        /// Users do not require account verification before joining.
        /// </summary>
        None,

        /// <summary>
        /// Users must be phone, email, or ID verified before joining.
        /// </summary>
        Low,

        /// <summary>
        /// Users must be ID verified, or phone and email verified before joining.
        /// </summary>
        Medium,

        /// <summary>
        /// Users must be ID verified before joining.
        /// </summary>
        High,
    }
}
