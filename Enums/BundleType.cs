namespace RoSharp.Enums
{
    // TODO: Add AvatarAnimations

    /// <summary>
    /// Indicates the type of a bundle. A bundle is a single item that consits of one or more individual <see cref="AssetType"/>s that are all granted to the user upon purchase.
    /// </summary>
    public enum BundleType
    {
        /// <summary>
        /// Unknown bundle type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents a classic bundle, typically consisting of seven total items: One of each <see cref="AssetType.Torso"/>, <see cref="AssetType.LeftArm"/>, <see cref="AssetType.RightArm"/>, <see cref="AssetType.LeftLeg"/>, <see cref="AssetType.RightLeg"/>, <see cref="AssetType.Head"/> OR <see cref="AssetType.DynamicHead"/>, and a <see cref="AssetType.MoodAnimation"/>.
        /// </summary>
        BodyParts = 1,

        /// <summary>
        /// Represents an avatar animations bundle, typically consisting of seven total items: One of each <see cref="AssetType.ClimbAnimation"/>, <see cref="AssetType.FallAnimation"/>, <see cref="AssetType.IdleAnimation"/>, <see cref="AssetType.JumpAnimation"/>, <see cref="AssetType.RunAnimation"/>, <see cref="AssetType.SwimAnimation"/>, <see cref="AssetType.WalkAnimation"/>, 
        /// </summary>
        AvatarAnimations = 2,

        /// <summary>
        /// Represents a shoe bundle, typically consisting of two total items: A <see cref="AssetType.LeftShoeAccessory"/> and a <see cref="AssetType.RightShoeAccessory"/>.
        /// </summary>
        Shoes = 3,

        /// <summary>
        /// Represents a dynamic head bundle, typically consisting of two total items: A <see cref="AssetType.DynamicHead"/> and a <see cref="AssetType.MoodAnimation"/>.
        /// </summary>
        DynamicHead = 4,
    }
}
