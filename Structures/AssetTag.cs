namespace RoSharp.Structures
{
    /// <summary>
    /// Represents an asset tag.
    /// </summary>
    public readonly struct AssetTag
    {
        /// <summary>
        /// The Id of the tag.
        /// </summary>
        public string TagId { get; init; }

        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name { get; init; }
    }
}
