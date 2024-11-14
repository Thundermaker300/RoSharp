namespace RoSharp.Interfaces
{
    /// <summary>
    /// Indicates a type that can be considered the owner of an <see cref="API.Assets.Asset"/>.
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
    }
}
