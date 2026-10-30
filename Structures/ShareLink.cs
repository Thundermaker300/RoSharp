using RoSharp.API;
using RoSharp.API.Assets;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a unique share link to an item.
    /// </summary>
    public struct ShareLink
    {
        /// <summary>
        /// The name of the share link.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The time that the share link was created.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// The unique ID of the share link.
        /// </summary>
        public string LinkId { get; init; }

        /// <summary>
        /// The type of share link.
        /// </summary>
        public string LinkType { get; init; }

        /// <summary>
        /// The URL of the share link.
        /// </summary>
        public string Url { get; init; }

        /// <summary>
        /// The current state of the share link.
        /// </summary>
        public string State { get; init; }

        /// <summary>
        /// The <see cref="Asset"/> that this share link refers to.
        /// </summary>
        public Id<Asset> TargetAsset { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ShareLink {LinkId} [{Name}] {LinkType} <{State}> <{TargetAsset.UniqueId}> [{Created}] {Url}";
        }

    }
}
