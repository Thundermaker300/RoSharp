using RoSharp.Enums;

namespace RoSharp.Interfaces
{
    /// <summary>
    /// Indicates a type that can be considered the owner of an <see cref="API.Assets.Asset"/>.
    /// <para>
    /// This interface can either be casted to a <see cref="API.User"/> or <see cref="API.Communities.Community"/>. The type of owner can be verified before performing any casts by checking the <see cref="OwnerType"/> property.
    /// </para>
    /// </summary>
    public interface IAssetOwner
    {
        /// <summary>
        /// The name of the asset owner.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The Id of the asset owner.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// The type of the owner.
        /// </summary>
        public AssetOwnerType OwnerType { get; }

        /// <summary>
        /// Indicates whether or not this user/community has the blue verified checkmark badge.
        /// </summary>
        public bool Verified { get; }
    }
}
