namespace RoSharp.Interfaces
{
    /// <summary>
    /// Inherited by API that represents assets or bundles that have a favorites counter.
    /// </summary>
    public interface IFavorites
    {
        /// <summary>
        /// Gets the amount of favorites this item has.
        /// </summary>
        public ulong Favorites { get; }
    }
}
