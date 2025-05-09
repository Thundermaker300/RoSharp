namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates non-owner access permissions to an <see cref="API.Assets.Asset"/>.
    /// </summary>
    public enum AssetPermissionType
    {
        /// <summary>
        /// Access to use the asset within experiences.
        /// </summary>
        Use,

        /// <summary>
        /// Access to modify the asset.
        /// </summary>
        Edit,
    }
}
