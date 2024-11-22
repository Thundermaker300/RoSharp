using RoSharp.API;

namespace RoSharp.Structures
{
    /// <summary>
    /// Gets a user's review on an asset.
    /// </summary>
    public readonly struct AssetReview
    {
        /// <summary>
        /// Gets the unique Id of the review.
        /// </summary>
        public string ReviewId { get; init; }

        /// <summary>
        /// Gets a <see cref="Id{T}"/> representing the poster of the review.
        /// </summary>
        public Id<User> Poster { get; init; }

        /// <summary>
        /// Gets the text of the review.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets whether or not the review recommended the asset. Can be <see langword="null"/> if the user didn't recommend or not-recommend.
        /// </summary>
        public bool? IsRecommended { get; init; }
    }
}
